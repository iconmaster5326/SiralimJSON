using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// The ID of the sprite this creature uses in battle.
        /// </summary>
        public int BattleSpriteID;
        /// <summary>
        /// This creature's class.
        /// </summary>
        public string Class;
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
            int battleSpriteID,
            string @class,
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
            BattleSpriteID = battleSpriteID;
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

        private static Dictionary<int, Creature>? _Database;
        /// <summary>
        /// All the creatures in the game.
        /// </summary>
        public static Dictionary<int, Creature> Database
        {
            get
            {
                if (_Database == null) { InitDatabase(); }
                return _Database ?? throw new Exception("Database not initialized!");
            }
        }
        private static void InitDatabase()
        {
            _Database = Game.Engine.GetGlobalObject()["creature"].GetArray().Select<GameVariable, KeyValuePair<int, Creature>?>((v, i) =>
            {
                Framework.Print($"[SiralimDumper]   Parsing creature {i}...");
                return v.IsNumber() ? null : new KeyValuePair<int, Creature>(i, FromGML(i, v.GetArray()));
            }).OfType<KeyValuePair<int, Creature>>().ToDictionary();
        }

        private static Creature FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Creature(
                id: id,
                attackGrowth: gml[0].GetInt32(),
                battleSpriteID: gml[1].GetSpriteID(),
                @class: gml[2].GetString(),
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
    BattleSpriteID={BattleSpriteID},
    Class='{Class}',
    DefenceGrowth={DefenseGrowth},
    HPGrowth={HPGrowth},
    IntelligenceGrowth={IntelligenceGrowth},
    Name='{Name}',
    OverworldSpriteID={OverworldSpriteID},
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
    }
}
