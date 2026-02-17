using AurieSharpInterop;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using YYTKInterop;

namespace SiralimDumper
{
    public static class SiralimDumper
    {
        public const string VERSION = "0.2.0";
        public const int SCHEMA_VERSION = 1;

        public static AurieStatus InitializeMod(AurieManagedModule Module)
        {
            Framework.Print("[SiralimDumper] Hello, SiralimDumper!");

            Game.Events.OnFrame += OnFrame;

            return AurieStatus.Success;
        }

        public static void UnloadMod(AurieManagedModule Module)
        {
            Framework.Print("[SiralimDumper] Goodbye, SiralimDumper!");
        }

        public static void OnFrame(int FrameNumber, double DeltaTime)
        {
            if (Game.Engine.GetGlobalObject().Members.ContainsKey("creature"))
            {
                Framework.Print($"[SiralimDumper] Found databases! Dumping...");

                // set up text engine to preserve special values
                Game.Engine.GetGlobalObject()["playername"] = "{PLAYERNAME}";
                Game.Engine.GetGlobalObject()["castlename"] = "{CASTLENAME}";

                // output JSON
                Framework.Print($"[SiralimDumper] Loading existing image mappings...");
                var mappings = ImageMappingsFromFile;

                Framework.Print($"[SiralimDumper] Loading existing progress info...");
                for (var step = CurrentStep; step < ProgressStep.DONE; step++)
                {
                    DumpJSON(step, mappings);
                }

                // exit
                WriteProgress(ProgressStep.DONE);
                Environment.Exit(0);
            }
            else
            {
                // simulate the first E press that activates the loading of global databases
                Game.Engine.CallFunction("keyboard_key_press", (int)'E');
                Game.Engine.CallFunction("keyboard_key_release", (int)'E');
            }
        }

        public static void CompareObjectMembers(IReadOnlyDictionary<string, string> d1, IReadOnlyDictionary<string, GameVariable> d2)
        {
            Framework.Print($"[SiralimDumper] BEGIN DIFF");
            var onlyInD1 = d1.Where(kv => !kv.Key.StartsWith("gml_") && !d2.ContainsKey(kv.Key)).ToDictionary();
            var onlyInD2 = d2.Where(kv => !kv.Key.StartsWith("gml_") && !d1.ContainsKey(kv.Key)).ToDictionary();
            var mutual = d1.Keys.Where(k => !k.StartsWith("gml_") && !onlyInD1.ContainsKey(k)).ToHashSet();

            foreach (var k in mutual)
            {
                string d1pp = d1[k];
                string d2pp = d2[k].PrettyPrint();
                if (!d1pp.Equals(d2pp))
                {
                    Framework.Print($"[SiralimDumper] {k}: was: {d1pp.EscapeNonWS().Truncate()} ; is: {d2pp.EscapeNonWS().Truncate()} ;");
                }
            }

            foreach (var k in onlyInD1.Keys)
            {
                Framework.Print($"[SiralimDumper] {k}: removed");
            }

            foreach (var k in onlyInD2.Keys)
            {
                Framework.Print($"[SiralimDumper] {k}: added: {d2[k].PrettyPrint().EscapeNonWS().Truncate()} ;");
            }

            Framework.Print($"[SiralimDumper] END DIFF");
        }

        public enum ProgressStep
        {
            STARTUP,
            METADATA,
            ACCESSORIES,
            ARTIFACTS,
            BACKGROUNDS,
            CONDITIONS,
            COSTUMES,
            CREATURES,
            DECORATIONS,
            FALSE_GODS,
            FLOOR_STYLES,
            GODS,
            MATERIALS,
            MUSIC,
            NETHER_BOSSES,
            PERKS,
            PERSONALITIES,
            PROJECT_ITEMS,
            PROJECTS,
            RACES,
            REALM_PROPERTIES,
            REALMS,
            RUNES,
            SKINS,
            SPECIALIZATIONS,
            SPELL_PROPERTIES,
            SPELL_PROPERTY_ITEMS,
            SPELLS,
            TRAITS,
            WALL_STYLES,
            WEATHER,
            OTHER,
            DONE,
        }

        public static SiralimEntityInfo? EntityInfo(this ProgressStep step)
        {
            if (step <= ProgressStep.METADATA || step >= ProgressStep.OTHER)
            {
                return null;
            }
            return SiralimEntityInfo.ALL[((uint)step) - 2];
        }

        public static string RootDirectory => @"..\dumped";

        public static string ProgressLogFile => @$"{RootDirectory}\progress.log";

        public static ProgressStep CurrentStep
        {
            get
            {
                try
                {
                    return (ProgressStep)uint.Parse(File.ReadAllText(ProgressLogFile));
                }
                catch (IOException)
                {
                    return ProgressStep.STARTUP;
                }
                catch (FormatException)
                {
                    return ProgressStep.STARTUP;
                }
            }
        }

        public static void WriteProgress(ProgressStep step)
        {
            var logfile = ProgressLogFile;
            EnsureFileDirExists(logfile);
            File.WriteAllText(logfile, ((uint)step).ToString());
        }

        public class ImageInfo
        {
            public int Frame { get; set; }
            public string Output { get; set; }

            public ImageInfo(int frame, string output)
            {
                Frame = frame;
                Output = output;
            }
        }

        public static List<ImageInfo> ImagesForOWSprite(Sprite sprite, string prefix)
        {
            if (sprite.Frames < 8)
            {
                return [new ImageInfo(0, $@"{prefix}.png")];
            }
            else
            {
                return [
                    new ImageInfo(0, $@"{prefix}_s0.png"),
                    new ImageInfo(1, $@"{prefix}_s1.png"),

                    new ImageInfo(2, $@"{prefix}_n0.png"),
                    new ImageInfo(3, $@"{prefix}_n1.png"),

                    new ImageInfo(4, $@"{prefix}_e0.png"),
                    new ImageInfo(5, $@"{prefix}_e1.png"),

                    new ImageInfo(6, $@"{prefix}_w0.png"),
                    new ImageInfo(7, $@"{prefix}_w1.png"),
                ];
            }
        }

        public static void MapMiscImages(Dictionary<string, List<ImageInfo>> mappings)
        {
            mappings.GetAndAppend("project_creatureparts", new ImageInfo(0, $@"{SiralimEntityInfo.PROJECT_ITEMS.Path}\CreatureParts.png"));
            mappings.GetAndAppend("project_arcanedust", new ImageInfo(0, $@"{SiralimEntityInfo.PROJECT_ITEMS.Path}\ArcaneDust.png"));

            foreach (var clazz in Enum.GetValues<SiralimClass>())
            {
                using (var tsi = new TempSpellInstance(Spell.Database.Values.First(s => s.Class == clazz)))
                {
                    for (int level = 0; level <= 3; level++)
                    {
                        tsi.Instance.GetRefInstance()["tier"] = level * 5;
                        mappings.GetAndAppend("icons", new ImageInfo(Game.Engine.CallScript("gml_Script_inv_SpellGemIcon", tsi.Instance).GetInt32(), $@"item\spellgem\{EnumUtil.Name(clazz)}_{level}.png"));
                    }
                }
            }

            foreach (var info in new (string Sprite, string Name)[]{
                        ("gem_slot", "spellprop"),
                        ("slot_nether", "nether"),
                        ("slot_spell", "spell"),
                        ("slot_stat", "stat"),
                        ("slot_trait", "trait"),
                        ("slot_trick", "trick"),
                    })
            {
                mappings.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\slots\{info.Name}.png"));
            }

            foreach (var info in new (string Sprite, int Index, string Name)[]{
                        ("resource_brimstone", 0, "brimstone"),
                        ("resource_crystal", 0, "crystal"),
                        ("resource_essence", 0, "essence"),
                        ("resource_granite", 0, "granite"),
                        ("resource_power", 0, "power"),
                        ("icons", 2001, "glamor"),
                        ("icons", 2002, "glory"),
                        ("icons", 2003, "notoriety"),
                        ("icons", 2004, "piety"),
                        ("icons", 2141, "stardust"),
                        ("icons", 2143, "gotg_key"),
                        ("icons", 2165, "gotg_key_fragment"),
                        ("icons", 1895, "ticket_siropoly"),
                        ("icons", 1896, "ticket_keno"),
                        ("icons", 1897, "ticket_scratch"),
                        ("icons", 1898, "ticket_slots"),
                    })
            {
                mappings.GetAndAppend(info.Sprite, new ImageInfo(info.Index, $@"item\resource\{info.Name}.png"));
            }

            foreach (var info in new (int Index, string Name)[]{
                        (1991, "chaos"),
                        (1992, "death"),
                        (1993, "life"),
                        (1994, "nature"),
                        (1995, "sorcery"),
                    })
            {
                mappings.GetAndAppend("icons", new ImageInfo(info.Index, $@"item\card\{info.Name}.png"));
            }

            foreach (var info in new (int Index, string Name)[]{
                        (2005, "attack"),
                        (2006, "defense"),
                        (2007, "health"),
                        (2008, "intelligence"),
                        (2009, "speed"),
                        (2178, "reset"),
                    })
            {
                mappings.GetAndAppend("icons", new ImageInfo(info.Index, $@"item\scroll\{info.Name}.png"));
            }

            foreach (var info in new (int Index, string Name)[]{
                        (1859, "chaos"),
                        (1860, "death"),
                        (1861, "life"),
                        (1862, "nature"),
                        (1863, "sorcery"),
                    })
            {
                mappings.GetAndAppend("icons", new ImageInfo(info.Index, $@"misc\class\{info.Name}.png"));
            }

            foreach (var info in new (string Sprite, string Name)[]{
                        ("stat_atk", "attack"),
                        ("stat_def", "defense"),
                        ("stat_hlt", "health"),
                        ("stat_int", "intelligence"),
                        ("stat_spd", "speed"),
                        ("stat_mna", "charges"),
                    })
            {
                mappings.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\stat\{info.Name}.png"));
            }

            foreach (var info in new (string Sprite, string Name)[]{
                        ("battle_attack", "attack"),
                        ("battle_cast", "cast"),
                        ("battle_defend", "defend"),
                        ("battle_provoke", "provoke"),
                        ("battle_damageincreased", "statup"),
                        ("battle_damagedecreased", "statdown"),
                        ("battle_defending", "defending"),
                        ("battle_provoking", "provoking"),
                        ("battle_dead", "dead"),
                    })
            {
                mappings.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\action\{info.Name}.png"));
            }

            foreach (var info in new (string Sprite, string Name)[]{
                        ("menu_creatures", "creature"),
                        ("menu_traits", "trait"),
                        ("menu_bestiary", "race"),
                        ("menu_gems", "spell"),
                        ("codex_gems", "spell_property"),
                        ("menu_dust", "item_spell_property"),
                        ("menu_materials", "item_material"),
                        ("codex_artifacts", "item_artifact"),
                        ("statustext", "personality"),
                        ("codex_skins", "skin"),
                        ("cred_costume", "costume"),
                        ("furn_tableschairs", "decoration"),
                        ("setwall", "walls"),
                        ("setfloor", "floors"),
                        ("changebackground", "background"),
                        ("icon_castleweather", "weather"),
                        ("loot_music", "music"),
                        ("cred_godrealm", "god"),
                        ("library_realms", "realm"),
                        ("furn_paintings", "condition"),
                        ("menu_library_changetitle", "specialization"),
                        ("codex_anointments", "perk"),
                        ("codex_realmprops", "realm_property"),
                        ("codex_worldboss", "false_god"),
                        ("rune_p", "rune"),
                        ("codex_nether", "nether_boss"),
                        ("codex_projects", "project"),
                        ("project_special", "item_project"),
                        ("codex_relics", "relic"),
                        ("menu_allresources", "item_resource"),
                        ("moreemblems", "item_emblem"),
                        ("menu_cards", "item_card"),
                        ("item_netherstone", "item_nether_stone"),
                        ("menu_fielditems", "item_misc"),
                        ("accessory_add", "accessory"),
                        ("menu_items", "item"),
                    })
            {
                mappings.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\category\{info.Name}.png"));
            }
        }

        public static string ImageMappingFile => @$"{RootDirectory}\imageMappings.json";
        public static readonly JsonSerializerOptions ImageMappingOptions = new()
        {
            IndentSize = 2,
            IndentCharacter = ' ',
            WriteIndented = true,
        };

        public static QuickType.OverworldSprite OverworldSpriteJSON(Sprite sprite, string prefix)
        {
            var ow = ImagesForOWSprite(sprite, prefix);
            return new()
            {
                South = [
                            $@"images\{ow[0].Output}".Replace("\\", "/"),
                            $@"images\{ow[1].Output}".Replace("\\", "/"),
                        ],
                North = [
                            $@"images\{ow[2].Output}".Replace("\\", "/"),
                            $@"images\{ow[3].Output}".Replace("\\", "/"),
                        ],
                East = [
                            $@"images\{ow[4].Output}".Replace("\\", "/"),
                            $@"images\{ow[5].Output}".Replace("\\", "/"),
                        ],
                West = [
                            $@"images\{ow[6].Output}".Replace("\\", "/"),
                            $@"images\{ow[7].Output}".Replace("\\", "/"),
                        ],
            };
        }

        public static QuickType.Metadata MetadataJSON => new()
        {
            GameVersion = Game.Engine.CallScript("gml_Script_scr_GetCurrentVersion").GetString(),
            Version = VERSION,
            SchemaVersion = SCHEMA_VERSION,
        };

        private static readonly JsonSerializerOptions JSONSettings = new()
        {
            IndentSize = 4,
            IndentCharacter = ' ',
            WriteIndented = true,
            Converters = {
                    QuickType.Converter.Settings.Converters
                },
        };

        private static void EnsureFileDirExists(string path)
        {
            string? dirname = Path.GetDirectoryName(path);
            if (dirname == null) return;
            Directory.CreateDirectory(dirname);
        }

        public static Dictionary<string, List<ImageInfo>> ImageMappingsFromFile
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, List<ImageInfo>>>(File.ReadAllText(ImageMappingFile)) ?? new();
                }
                catch (IOException)
                {
                    return new();
                }
            }
        }

        public static void DumpJSON(ProgressStep step, Dictionary<string, List<ImageInfo>> mappings)
        {
            Framework.Print($"[SiralimDumper] Working on step '{Enum.GetName(step)}'...");

            switch (step)
            {
                case ProgressStep.METADATA:
                    string DBFilename = $@"{RootDirectory}\metadata.json";
                    EnsureFileDirExists(DBFilename);
                    File.WriteAllText(DBFilename, JsonSerializer.Serialize(MetadataJSON, JSONSettings));
                    break;
                case ProgressStep.OTHER:
                    MapMiscImages(mappings);
                    break;
                default:
                    var category = step.EntityInfo();
                    if (category != null)
                    {
                        foreach (var k in category.Keys)
                        {
                            var v = category.GetEntity(k);
                            string filename = $@"{RootDirectory}\{category.IndividualFilePath(v)}";
                            Framework.Print($"[SiralimDumper] writing individual {category.Path} '{v.Name}' to {filename}...");
                            EnsureFileDirExists(filename);
                            File.WriteAllText(filename, JsonSerializer.Serialize(v.AsJSON, JSONSettings));
                            v.MapImages(mappings);
                        }
                    }
                    break;
            }

            Framework.Print($"[SiralimDumper] Saving image mappings...");
            EnsureFileDirExists(ImageMappingFile);
            File.WriteAllText(ImageMappingFile, JsonSerializer.Serialize(mappings, ImageMappingOptions));
            Framework.Print($"[SiralimDumper] Saving progress info...");
            WriteProgress(step);
            Framework.Print($"[SiralimDumper] Done with step '{Enum.GetName(step)}'.");
        }
    }
}
