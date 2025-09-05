using AurieSharpInterop;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                // DebugPrintAllEntities();
                SaveDatabaseJSON();
                //SaveImageMappingJSON();

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

        public static void DebugPrintAllEntities()
        {
            Framework.Print($"[SiralimDumper] creatures: [{string.Join(", ", Creature.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] traits: [{string.Join(", ", Trait.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] races: [{string.Join(", ", Race.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] spells: [{string.Join(", ", Spell.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] spell properties: [{string.Join(", ", SpellProperty.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] spell property items: [{string.Join(", ", ItemSpellProperty.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] material items: [{string.Join(", ", ItemMaterial.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] artifacts: [{string.Join(", ", ItemArtifact.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] personalities: [{string.Join(", ", Personality.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] skins: [{string.Join(", ", Skin.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] costumes: [{string.Join(", ", Costume.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] decorations: [{string.Join(", ", Decoration.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] walls: [{string.Join(", ", DecorationWalls.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] floors: [{string.Join(", ", DecorationFloors.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] backgrounds: [{string.Join(", ", DecorationBackground.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] weather: [{string.Join(", ", DecorationWeather.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] music: [{string.Join(", ", DecorationMusic.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] gods: [{string.Join(", ", God.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] realms: [{string.Join(", ", Realm.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] conditions: [{string.Join(", ", Condition.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] specializations: [{string.Join(", ", Specialization.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] perks: [{string.Join(", ", Perk.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] realm properties: [{string.Join(", ", RealmProperty.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] false gods: [{string.Join(", ", FalseGod.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] false god runes: [{string.Join(", ", FalseGodRune.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] nether bosses: [{string.Join(", ", NetherBoss.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] projects: [{string.Join(", ", Project.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] project items: [{string.Join(", ", ProjectItem.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] relics: [{string.Join(", ", Relic.Database.Values).EscapeNonWS()}]");
            Framework.Print($"[SiralimDumper] accessories: [{string.Join(", ", Accessory.Database.Values).EscapeNonWS()}]");
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

        /// <summary>
        /// The sprite mapping file to generate.
        /// Later, a UTMT script is called to extract these images.
        /// </summary>
        public const string ImageMappingFile = @"..\SiralimDumperImageMappings.json";

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
                        result.GetAndAppend(icon.Name, new ImageInfo(item.IconIndex, $@"spellprop\{item.ID}.png"));
                    }
                }

                foreach (var item in ItemSpellProperty.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndex, $@"item\spellprop\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in ItemMaterial.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndex, $@"item\material\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in ItemArtifact.Database.Values)
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        result.GetAndAppend(item.Icon.Name, new ImageInfo(item.IconIndexEx(i * 10), $@"item\artifact\{item.Name.EscapeForFilename()}_t{i}.png"));
                    }
                }

                foreach (var item in Personality.Database.Values)
                {
                    result.GetAndAppend("icons", new ImageInfo(item.TomeIconIndex, $@"item\tome\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in Skin.Database.Values)
                {
                    result.GetAndAppend(item.BattleSprite.Name, new ImageInfo(item.BattleSpriteIndex, $@"skin\{item.Name.EscapeForFilename()}\battle.png"));
                    result.GetAndAppend(item.OverworldSprite.Name, values: ImagesForOWSprite(item.OverworldSprite, $@"skin\{item.Name.EscapeForFilename()}\overworld"));
                }

                foreach (var item in Costume.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, values: ImagesForOWSprite(item.Sprite, $@"costume\{item.Name.EscapeForFilename()}\overworld"));
                }

                foreach (var item in Decoration.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"decor\object\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in DecorationWalls.Database.Values)
                {
                    result.GetAndAppend(item.Tileset.Name, new ImageInfo(0, $@"decor\wall\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in DecorationFloors.Database.Values)
                {
                    result.GetAndAppend(item.Tileset.Name, new ImageInfo(0, $@"decor\floor\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in DecorationBackground.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"decor\bg\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in God.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"god\{item.Name.EscapeForFilename()}.png"));
                    if (item.EmblemIcon != null)
                    {
                        result.GetAndAppend(item.EmblemIcon.Name, new ImageInfo(0, $@"item\emblem\{item.Name.EscapeForFilename()}.png"));
                    }
                }

                foreach (var item in Condition.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"condition\{item.Name.EscapeForFilename()}.png"));
                    if (item.IconID != item.ResistantIconID)
                    {
                        result.GetAndAppend(item.ResistantIcon.Name, new ImageInfo(0, $@"condition\{item.Name.EscapeForFilename()}_resist.png"));
                    }
                }

                foreach (var item in Specialization.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"spec\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in Perk.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"perk\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in RealmProperty.Database.Values)
                {
                    Sprite? icon = item.Icon;
                    if (icon != null)
                    {
                        result.GetAndAppend(icon.Name, new ImageInfo(0, $@"realmprop\{item.ID}.png"));
                    }
                }

                foreach (var item in FalseGod.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"falsegod\{item.Name.EscapeForFilename()}\icon.png"));
                    result.GetAndAppend(item.OverworldSprite.Name, new ImageInfo(0, $@"falsegod\{item.Name.EscapeForFilename()}\overworld_0.png"));
                    result.GetAndAppend(item.OverworldSprite.Name, new ImageInfo(1, $@"falsegod\{item.Name.EscapeForFilename()}\overworld_1.png"));
                }

                foreach (var item in FalseGodRune.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"rune\{item.ID}.png"));
                    result.GetAndAppend(item.InactiveSprite.Name, new ImageInfo(0, $@"rune\{item.ID}_inactive.png"));
                }

                foreach (var item in Project.Database.Values)
                {
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"project\{item.Name.EscapeForFilename()}.png"));
                }

                foreach (var item in ProjectItem.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"item\project\{item.Name.EscapeForFilename()}.png"));
                }

                result.GetAndAppend("project_creatureparts", new ImageInfo(0, $@"item\project\CreatureParts.png"));
                result.GetAndAppend("project_arcanedust", new ImageInfo(0, $@"item\project\ArcaneDust.png"));

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
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"relic\{item.Name.EscapeForFilename()}\relic.png"));
                    result.GetAndAppend(item.Icon.Name, new ImageInfo(0, $@"relic\{item.Name.EscapeForFilename()}\icon.png"));
                }

                foreach (var item in Accessory.Database.Values)
                {
                    result.GetAndAppend(item.Sprite.Name, new ImageInfo(0, $@"accessory\{item.Name.EscapeForFilename()}.png"));
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
            File.WriteAllText(ImageMappingFile, JsonSerializer.Serialize(ImageMappingJSON, new JsonSerializerOptions()
            {
                IndentSize = 2,
                IndentCharacter = ' ',
                WriteIndented = true,
            }));
        }

        /// <summary>
        /// The database file to generate.
        /// </summary>
        public const string DatabaseFile = @"..\exported\combined.json";

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

        public static QuickType.SiralimUltimateDatabase DatabaseJSON => new()
        {
            Metadata = new()
            {
                GameVersion = Game.Engine.CallScript("gml_Script_scr_GetCurrentVersion").GetString(),
                Version = VERSION,
                SchemaVersion = SCHEMA_VERSION,
            },
            Creatures = Creature.Database.Values.Select(item => item.AsJSON).ToArray(),
            Races = Race.Database.Values.Select(item => item.AsJSON).ToArray(),
        };

        public static void SaveDatabaseJSON()
        {
            Framework.Print("[SiralimDumper] writing database JSON...");
            File.WriteAllText(DatabaseFile, JsonSerializer.Serialize(DatabaseJSON, new JsonSerializerOptions()
            {
                IndentSize = 2,
                IndentCharacter = ' ',
                WriteIndented = true,
                Converters = {
                    QuickType.Converter.Settings.Converters
                },
            }));
        }
    }
}
