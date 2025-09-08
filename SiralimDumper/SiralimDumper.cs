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
        public const string VERSION = "0.1.0";
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
                SaveCombinedDatabaseJSON();
                SaveIndividualDatabaseJSON();
                SaveAggregateDatabaseJSON();
                SaveImageMappingJSON();

                // exit
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

        public static Dictionary<string, List<ImageInfo>> ImageMappingJSON
        {
            get
            {
                Dictionary<string, List<ImageInfo>> result = new();

                foreach (var item in Creature.Database.Values)
                {
                    result.GetAndAppend(item.BattleSprite.Name, new ImageInfo(item.BattleSpriteIndex, item.BattleSpriteFilename));
                    result.GetAndAppend(item.OverworldSprite.Name, values: ImagesForOWSprite(item.OverworldSprite, item.OverworldSpriteFilenamePrefix));
                }

                foreach (var item in Race.Database.Values)
                {
                    Sprite? icon = item.Icon;
                    if (icon != null)
                    {
                        result.GetAndAppend(icon.Name, new ImageInfo(0, item.IconFilename));
                    }
                }

                foreach (var item in SpellProperty.Database.Values)
                {
                    Sprite? icon = item.Icon;
                    if (icon != null)
                    {
                        result.GetAndAppend(icon.Name, new ImageInfo(item.IconIndex, item.IconFilename));
                    }
                }

                foreach (var item in ItemSpellProperty.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndex, item.IconFilename));
                }

                foreach (var item in ItemMaterial.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndex, item.IconFilename));
                }

                foreach (var item in ItemArtifact.Database.Values)
                {
                    for (int i = 0; i < ItemArtifact.N_TIERS; i++)
                    {
                        result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndexEx(i * 10), item.IconFilename(i)));
                    }
                }

                foreach (var item in Personality.Database.Values)
                {
                    result.GetAndAppend("icons", new ImageInfo(item.TomeIconIndex, item.TomeIconFilename));
                }

                foreach (var item in Skin.Database.Values)
                {
                    result.GetAndAppend(item.BattleSprite.Name, new ImageInfo(item.BattleSpriteIndex, item.BattleSpriteFilename));
                    result.GetAndAppend(item.OverworldSprite.Name, values: ImagesForOWSprite(item.OverworldSprite, item.OverworldSpriteFilenamePrefix));
                }

                foreach (var item in Costume.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, values: ImagesForOWSprite(item.Sprite, item.SpriteFilenamePrefix));
                }

                foreach (var item in Decoration.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, item.SpriteFilename));
                }

                foreach (var item in DecorationWalls.Database.Values)
                {
                    result.GetAndAppend(item.Tileset.Name, new ImageInfo(0, item.SpriteFilename));
                }

                foreach (var item in DecorationFloors.Database.Values)
                {
                    result.GetAndAppend(item.Tileset.Name, new ImageInfo(0, item.SpriteFilename));
                }

                foreach (var item in DecorationBackground.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, item.SpriteFilename));
                }

                foreach (var item in God.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                    if (item.EmblemIcon != null)
                    {
                        result.GetAndAppend(item.EmblemIcon.Name, new ImageInfo(0, item.EmblemIconFilename));
                    }
                }

                foreach (var item in Realm.Database.Values)
                {
                    for (var i = 1; i <= 100; i++)
                    {
                        string icon = item.BlessingIcon(i).Name;
                        if (!result.ContainsKey(icon))
                        {
                            result.GetAndAppend(icon, new ImageInfo(0, item.BlessingIconFilename(i)));
                        }
                    }
                }

                foreach (var item in Condition.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                    if (item.IconID != item.ResistantIconID)
                    {
                        result.GetAndAppend(item.ResistantIcon.Name, new ImageInfo(0, item.IconResistedFilename));
                    }
                }

                foreach (var item in Specialization.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                }

                foreach (var item in Perk.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                }

                foreach (var item in RealmProperty.Database.Values)
                {
                    Sprite? icon = item.Icon;
                    if (icon != null)
                    {
                        result.GetAndAppend(icon.Name, new ImageInfo(0, item.IconFilename));
                    }
                }

                foreach (var item in FalseGod.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                    result.GetAndAppend(item.OverworldSprite.Name, new ImageInfo(0, item.SpriteFilename0));
                    result.GetAndAppend(item.OverworldSprite.Name, new ImageInfo(1, item.SpriteFilename1));
                }

                foreach (var item in FalseGodRune.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, item.SpriteFilename));
                }

                foreach (var item in Project.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, item.IconFilename));
                }

                foreach (var item in ProjectItem.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, item.SpriteFilename));
                }

                result.GetAndAppend("project_creatureparts", new ImageInfo(0, $@"{SiralimEntityInfo.PROJECT_ITEMS.Path}\CreatureParts.png"));
                result.GetAndAppend("project_arcanedust", new ImageInfo(0, $@"{SiralimEntityInfo.PROJECT_ITEMS.Path}\ArcaneDust.png"));

                foreach (var clazz in Enum.GetValues<SiralimClass>())
                {
                    using (var tsi = new TempSpellInstance(Spell.Database.Values.First(s => s.Class == clazz)))
                    {
                        for (int level = 0; level <= 3; level++)
                        {
                            tsi.Instance.GetRefInstance()["tier"] = level * 5;
                            result.GetAndAppend("icons", new ImageInfo(Game.Engine.CallScript("gml_Script_inv_SpellGemIcon", tsi.Instance).GetInt32(), $@"item\spellgem\{EnumUtil.Name(clazz)}_{level}.png"));
                        }
                    }
                }

                foreach (var item in Relic.Database.Values)
                {
                    result.GetAndAppend(item.BigIcon.Name, new ImageInfo(0, item.BigIconFilename));
                    result.GetAndAppend(item.SmallIcon.Name, new ImageInfo(0, item.SmallIconFilename));
                }

                foreach (var item in Accessory.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, item.SpriteFilename));
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
                    result.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\slots\{info.Name}.png"));
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
                    result.GetAndAppend(info.Sprite, new ImageInfo(info.Index, $@"item\resource\{info.Name}.png"));
                }

                foreach (var info in new (int Index, string Name)[]{
                        (1991, "chaos"),
                        (1992, "death"),
                        (1993, "life"),
                        (1994, "nature"),
                        (1995, "sorcery"),
                    })
                {
                    result.GetAndAppend("icons", new ImageInfo(info.Index, $@"item\card\{info.Name}.png"));
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
                    result.GetAndAppend("icons", new ImageInfo(info.Index, $@"item\scroll\{info.Name}.png"));
                }

                foreach (var info in new (int Index, string Name)[]{
                        (1859, "chaos"),
                        (1860, "death"),
                        (1861, "life"),
                        (1862, "nature"),
                        (1863, "sorcery"),
                    })
                {
                    result.GetAndAppend("icons", new ImageInfo(info.Index, $@"misc\class\{info.Name}.png"));
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
                    result.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\stat\{info.Name}.png"));
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
                    result.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\action\{info.Name}.png"));
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
                    result.GetAndAppend(info.Sprite, new ImageInfo(0, $@"misc\category\{info.Name}.png"));
                }

                return result;
            }
        }

        public static void SaveImageMappingJSON()
        {
            Framework.Print("[SiralimDumper] writing image mapping JSON...");
            File.WriteAllText(@"..\SiralimDumperImageMappings.json", JsonSerializer.Serialize(ImageMappingJSON, new JsonSerializerOptions()
            {
                IndentSize = 2,
                IndentCharacter = ' ',
                WriteIndented = true,
            }));
        }

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

        public static QuickType.SiralimUltimateDatabase CompleteDatabaseJSON => new()
        {
            Metadata = MetadataJSON,
            Accessories = SiralimEntityInfo.ACCESSORIES.AllAsJSON<QuickType.Accessory>(),
            Artifacts = SiralimEntityInfo.ARTIFACTS.AllAsJSON<QuickType.Artifact>(),
            Backgrounds = SiralimEntityInfo.BGS.AllAsJSON<QuickType.Background>(),
            Conditions = SiralimEntityInfo.CONDITIONS.AllAsJSON<QuickType.Condition>(),
            Costumes = SiralimEntityInfo.COSTUMES.AllAsJSON<QuickType.Costume>(),
            Creatures = SiralimEntityInfo.CREATURES.AllAsJSON<QuickType.Creature>(),
            Decorations = SiralimEntityInfo.DECOR.AllAsJSON<QuickType.Decoration>(),
            FalseGods = SiralimEntityInfo.FALSE_GODS.AllAsJSON<QuickType.FalseGod>(),
            FloorStyles = SiralimEntityInfo.FLOORS.AllAsJSON<QuickType.FloorStyle>(),
            Gods = SiralimEntityInfo.GODS.AllAsJSON<QuickType.God>(),
            Materials = SiralimEntityInfo.MATERIALS.AllAsJSON<QuickType.Material>(),
            Music = SiralimEntityInfo.MUSIC.AllAsJSON<QuickType.Music>(),
            NetherBosses = SiralimEntityInfo.NETHER_BOSSES.AllAsJSON<QuickType.NetherBoss>(),
            Perks = SiralimEntityInfo.PERKS.AllAsJSON<QuickType.Perk>(),
            Personalities = SiralimEntityInfo.PERSONALITIES.AllAsJSON<QuickType.Personality>(),
            ProjectItems = SiralimEntityInfo.PROJECT_ITEMS.AllAsJSON<QuickType.ProjectItem>(),
            Projects = SiralimEntityInfo.PROJECTS.AllAsJSON<QuickType.Project>(),
            Races = SiralimEntityInfo.RACES.AllAsJSON<QuickType.Race>(),
            RealmProperties = SiralimEntityInfo.REALM_PROPS.AllAsJSON<QuickType.RealmProperty>(),
            Realms = SiralimEntityInfo.REALMS.AllAsJSON<QuickType.Realm>(),
            Runes = SiralimEntityInfo.RUNES.AllAsJSON<QuickType.Rune>(),
            Skins = SiralimEntityInfo.SKINS.AllAsJSON<QuickType.Skin>(),
            Specializations = SiralimEntityInfo.SPECS.AllAsJSON<QuickType.Specialization>(),
            SpellProperties = SiralimEntityInfo.SPELLPROP_ITEMS.AllAsJSON<QuickType.SpellProperty>(),
            SpellPropertyItems = SiralimEntityInfo.SPELL_PROPS.AllAsJSON<QuickType.SpellPropertyItem>(),
            Spells = SiralimEntityInfo.SPELLS.AllAsJSON<QuickType.Spell>(),
            Traits = SiralimEntityInfo.TRAITS.AllAsJSON<QuickType.Trait>(),
            WallStyles = SiralimEntityInfo.WALLS.AllAsJSON<QuickType.WallStyle>(),
            Weather = SiralimEntityInfo.WEATHER.AllAsJSON <QuickType.Weather>(),
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

        public static void SaveCombinedDatabaseJSON()
        {
            Framework.Print("[SiralimDumper] writing combined database JSON...");

            const string Filename = @"..\exported\combined\combined.json";

            EnsureFileDirExists(Filename);
            File.WriteAllText(Filename, JsonSerializer.Serialize(CompleteDatabaseJSON, JSONSettings));
        }

        private const string IndividualsDir = $@"..\exported\individual";
        public static void SaveIndividualDatabaseJSON()
        {
            // Quicktype sucks, and can't handle $refs correctly, so we have to do this ourselves
            Framework.Print("[SiralimDumper] writing individual database JSON...");

            const string DBFilename = $@"{IndividualsDir}\individual.json";
            var result = new Dictionary<string, object>() {
                ["metadata"] = JsonSerializer.Serialize(MetadataJSON, JSONSettings)
            };

            foreach (var category in SiralimEntityInfo.ALL)
            {
                result[category.FieldName] = category.Keys.Select(k => new KeyValuePair<string, object>($"{k}", category.IndividualFilePath(category.GetEntity(k)))).ToDictionary();
                foreach (var k in category.Keys)
                {
                    var v = category.GetEntity(k);
                    string filename = $@"{IndividualsDir}\{category.IndividualFilePath(v)}";
                    Framework.Print($"[SiralimDumper] writing individual {category.Path} '{v.Name}' to {filename}...");
                    File.WriteAllText(filename, JsonSerializer.Serialize(v.AsJSON, JSONSettings));
                }
            }

            Framework.Print("[SiralimDumper] writing individual database index JSON...");
            EnsureFileDirExists(DBFilename);
            File.WriteAllText(DBFilename, JsonSerializer.Serialize(result, JSONSettings));
        }

        private delegate object AggregateJSONGetter<V>(V item);
        private const string AggregateDir = @"..\exported\aggregate";
        public static void SaveAggregateDatabaseJSON()
        {
            // Quicktype sucks, and can't handle $refs correctly, so we have to do this ourselves
            Framework.Print("[SiralimDumper] writing aggregate database JSON...");

            const string DBFilename = $@"{AggregateDir}\metadata.json";
            EnsureFileDirExists(DBFilename);
            File.WriteAllText(DBFilename, JsonSerializer.Serialize(MetadataJSON, JSONSettings));

            foreach (var category in SiralimEntityInfo.ALL)
            {
                string filename = $@"{AggregateDir}\{category.FieldName}.json";
                Framework.Print($"[SiralimDumper] writing aggregate of {category.FieldName} to {filename.Escape()}...");
                EnsureFileDirExists(filename);
                File.WriteAllText(filename, JsonSerializer.Serialize(category.AllAsJSON<object>().ToList(), JSONSettings));
            }
        }
    }
}
