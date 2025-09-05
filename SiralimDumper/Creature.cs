using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate creature definition.
    /// </summary>
    public class Creature
    {
        /// <summary>
        /// The unique ID of this creature.
        /// </summary>
        public int ID;
        /// <summary>
        /// How much Attack this creature gains per level.
        /// </summary>
        public int AttackGrowth;
        /// <summary>
        /// The frame of the battle sprite this creature uses.
        /// </summary>
        public int BattleSpriteIndex;
        /// <summary>
        /// This creature's class.
        /// </summary>
        public SiralimClass Class;
        /// <summary>
        /// How much Defense this creature gains per level.
        /// </summary>
        public int DefenseGrowth;
        /// <summary>
        /// How much HP this creature gains per level.
        /// </summary>
        public int HPGrowth;
        /// <summary>
        /// How much Intelligence this creature gains per level.
        /// </summary>
        public int IntelligenceGrowth;

        //public int MPGrowth; // index 6 was MP growth in Siralim 3; unused now

        /// <summary>
        /// This creature's English name.
        /// </summary>
        public string Name;

        //public string DefaultNickname; // index 8 is the default nickname, which is always equal to the creature's name (save for NYIs and when there's typos)

        /// <summary>
        /// The ID of the sprite this creature uses on the overworld.
        /// </summary>
        public int OverworldSpriteID;
        /// <summary>
        /// The ID of the trait this creature provides.
        /// See also <see cref="Trait"/>.
        /// </summary>
        public int TraitID;
        /// <summary>
        /// The name of this creature's race.
        /// See also <see cref="Race"/>.
        /// </summary>
        public string RaceName;
        /// <summary>
        /// How much Speed this creature gains per level.
        /// </summary>
        public int SpeedGrowth;

        public Creature(
            int id,
            int attackGrowth,
            int battleSpriteIndex,
            SiralimClass @class,
            int defenceGrowth,
            int hpGrowth,
            int intelligenceGrowth,
            string name,
            int overworldSpriteID,
            int traitID,
            string raceName,
            int speedGrowth
        )
        {
            ID = id;
            AttackGrowth = attackGrowth;
            BattleSpriteIndex = battleSpriteIndex;
            Class = @class;
            DefenseGrowth = defenceGrowth;
            HPGrowth = hpGrowth;
            IntelligenceGrowth = intelligenceGrowth;
            Name = name;
            OverworldSpriteID = overworldSpriteID;
            TraitID = traitID;
            RaceName = raceName;
            SpeedGrowth = speedGrowth;
        }

        /// <summary>
        /// All the creatures in the game.
        /// </summary>
        public static CreatureDatabase Database = [];

        internal static Creature FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Creature(
                id: id,
                attackGrowth: gml[0].GetInt32(),
                battleSpriteIndex: gml[1].GetSpriteID(),
                @class: EnumUtil.ClassFromString(gml[2]),
                defenceGrowth: gml[3].GetInt32(),
                hpGrowth: gml[4].GetInt32(),
                intelligenceGrowth: gml[5].GetInt32(),
                name: gml[7].GetString(),
                overworldSpriteID: gml[9].GetSpriteID(),
                traitID: gml[10].GetInt32(),
                raceName: gml[11].GetString(),
                speedGrowth: gml[12].GetInt32()
            );
        }

        public override string ToString()
        {
            return $@"Creature(
    ID={ID},
    AttackGrowth={AttackGrowth},
    BattleSpriteIndex={BattleSpriteIndex},
    Class={Class},
    DefenceGrowth={DefenseGrowth},
    HPGrowth={HPGrowth},
    IntelligenceGrowth={IntelligenceGrowth},
    Name='{Name}',
    OverworldSprite={OverworldSprite.ToString().Replace("\n", "\n  ")},
    Trait={Trait.Name},
    Race='{RaceName}',
    SpeedGrowth={SpeedGrowth},
    Lore='{Lore}',
    Source='{Source}',
    MenagerieDialog=['{string.Join("', '", MenagerieDialog)}'],
    IsGod={IsGod},
)";
        }

        /// <summary>
        /// This creature's race.
        /// </summary>
        public Race Race => Race.Database[RaceName];
        /// <summary>
        /// The trait this creature provides.
        /// </summary>
        public Trait Trait => Trait.Database[TraitID];

        /// <summary>
        /// This creature's English lore.
        /// </summary>
        public string Lore => Game.Engine.CallScript("gml_Script_scr_CreatureLore", ID);
        /// <summary>
        /// A general description of where to find the creature.
        /// May lie.
        /// </summary>
        public string Source => Game.Engine.CallScript("gml_Script_scr_CreatureSource", ID);
        private string MenagerieDialogForStat(string stat)
        {
            using (var tci = new TempCreatureInstance(this))
            {
                return Game.Engine.CallScript($"gml_Script_scr_PersonalityDialog{stat}", tci.Instance);
            }
        }
        /// <summary>
        /// Possible dialogue when spoken to in the menagerie.
        /// Most races all have the same dialog, but some (Avatars, Godspawn) have dialog for each individual member.
        /// </summary>
        public string[] MenagerieDialog => [MenagerieDialogForStat("Health"), MenagerieDialogForStat("Attack"), MenagerieDialogForStat("Intelligence"), MenagerieDialogForStat("Defense"), MenagerieDialogForStat("Speed")];
        /// <summary>
        /// Is this Avatar a god?
        /// Any other Avatars are False God parts.
        /// </summary>
        public bool IsGod
        {
            get
            {
                using (var tci = new TempCreatureInstance(this))
                {
                    return Game.Engine.CallScript("gml_Script_scr_CreatureIsGod", tci.Instance);
                }
            }
        }
        /// <summary>
        /// The god this Avatar is associated with, if any.
        /// </summary>
        public God? God => God.Database.Values.FirstOrDefault(g => g.Name.Equals(Name));
        /// <summary>
        /// Is this creature obtainable in normal gameplay?
        /// </summary>
        public bool Obtainable
        {
            get
            {
                if (Race.UNOBTAINABLE_RACES.Contains(RaceName)) return false;
                if (RaceName.Equals("Avatar") && !IsGod) return false;
                return true;
            }
        }

        /// <summary>
        /// The sprite this creature uses in battle.
        /// This is the same sprite for all creatures;
        /// the frame of the sprite to be used should be <see cref="ID"/>.
        /// </summary>
        public Sprite BattleSprite => "spr_crits_battle".GetGMLSprite();
        /// <summary>
        /// The sprite this creature uses in the overworld.
        /// </summary>
        public Sprite OverworldSprite => OverworldSpriteID.GetGMLSprite();

        public string BattleSpriteFilename => @$"creature\{Name.EscapeForFilename()}\battle.png";
        public string OverworldSpriteFilenamePrefix => $@"creature\{Name.EscapeForFilename()}\overworld";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Creature AsJSON => new()
        {
            Id = ID,
            Name = Name,
            Race = RaceName,
            Class = Enum.Parse<QuickType.Class>(EnumUtil.Name(Class)),
            Trait = TraitID,
            StatGrowth = new()
            {
                Health = HPGrowth,
                Attack = AttackGrowth,
                Intelligence = IntelligenceGrowth,
                Defense = DefenseGrowth,
                Speed = SpeedGrowth,
            },
            BattleSprite = $@"images\{BattleSpriteFilename}".Replace("\\", "/"),
            OverworldSprite = OverworldSprite.Frames < 8
                ? new()
                {
                    String = $@"{OverworldSpriteFilenamePrefix}.png".Replace("\\", "/")
                }
                : new()
                {
                    OverworldSprite = SiralimDumper.OverworldSpriteJSON(OverworldSprite, OverworldSpriteFilenamePrefix)
                },
            Lore = Lore,
            Sources = Source.Split(", ").Select(s =>
            {
                God? god = God;
                if (god != null)
                {
                    return new()
                    {
                        Type = QuickType.TypeEnum.Gotg,
                        God = god.ID,
                    };
                }

                Realm? realm = Realm.Database.Values.FirstOrDefault(r => r.Name.Equals(s));
                if (realm != null)
                {
                    return new()
                    {
                        Type = QuickType.TypeEnum.Realm,
                        Realm = realm.ID,
                    };
                }

                return new QuickType.Source()
                {
                    Type = QuickType.TypeEnum.Special,
                    Desc = s,
                };
            }).ToArray(),
            MinDepth = 1,
            MenagerieDialog = MenagerieDialog,
            God = God?.ID,
            Reserved = !Obtainable,
            Obtainable = Obtainable,
            Skins = Skin.Database.Values.Where(s => s.CreatureID == ID).Select(s => (long)s.ID).ToArray(),
            Specializations = Specialization.Database.Values.Where(s => s.PrimaryCreatureID == ID || s.SecondaryCreatureID == ID).Select(s => (long)s.ID).ToArray(),
            Notes = [],
            Creator = "Unknown",
        };
    }

    public class CreatureDatabase : Database<int, Creature>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["creature"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Creature? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Creature.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
