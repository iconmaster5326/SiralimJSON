using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate specialization definition.
    /// These are "classes" the player can be. Has perks.
    /// </summary>
    public class Specialization
    {
        public const int HIGHEST_COND_ID = 99;

        /// <summary>
        /// The unique ID of this specialization.
        /// </summary>
        public int ID;

        public Specialization(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the specializations in the game.
        /// </summary>
        public static SpecializationDatabase Database = [];

        public override string ToString()
        {
            return $@"Specialization(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    Playstyle='{Playstyle}',
    IconID={IconID},
    SpriteID={SpriteID},
    PrimaryCreatureID={PrimaryCreatureID} ({Creature.Database[PrimaryCreatureID].Name}),
    SecondaryCreatureID={SecondaryCreatureID} ({Creature.Database[SecondaryCreatureID].Name}),
    SpellID={SpellID} ({Spell.Database[SpellID].Name}),
    Perks=['{string.Join("', '", Perks.Select(x => x.Name))}'],
    AscendedPerkID={AscendedPerkID} ({AscendedPerk.Name}),
)";
        }

        private static readonly IReadOnlyDictionary<string, string> MANUAL_NAME_FIXUPS = new Dictionary<string, string> {
            ["L_WITCHDOCTOR"] = "L_WITCH_DOCTOR",
            ["L_HELLKNIGHT"] = "L_HELL_KNIGHT",
            ["L_RUNEKNIGHT"] = "L_RUNE_KNIGHT",
        };
        /// <summary>
        /// The English name of this specialization.
        /// Some name translation keys are messed up. We fix them for you.
        /// </summary>
        public string Name
        {
            get
            {
                string result = Game.Engine.CallScript("gml_Script_scr_SpecializationName", ID);
                if (MANUAL_NAME_FIXUPS.ContainsKey(result))
                {
                    result = MANUAL_NAME_FIXUPS[result].SUTranslate();
                }
                return result;
            }
        }
        /// <summary>
        /// The first part of the English description of this specialization.
        /// </summary>
        public string Description => Game.Engine.CallScript("gml_Script_scr_SpecializationDesc", ID);
        /// <summary>
        /// The second part of the English description of this specialization.
        /// </summary>
        public string Playstyle => Game.Engine.CallScript("gml_Script_scr_SpecializationPlaystyle", ID);
        /// <summary>
        /// The ID of the icon for this specialization.
        /// </summary>
        public int IconID => Game.Engine.CallScript("gml_Script_scr_SpecializationIcon", ID).GetSpriteID();
        /// <summary>
        /// The ID of the sprite for the initial costume for this specialization.
        /// </summary>
        public int SpriteID => Game.Engine.CallScript("gml_Script_scr_SpecializationCostume", ID).GetSpriteID();
        /// <summary>
        /// The ID of the first <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="SecondaryCreatureID"/>.
        /// </summary>
        public int PrimaryCreatureID => Game.Engine.CallScript("gml_Script_scr_SpecializationStartingCreature", ID);
        /// <summary>
        /// The ID of the second <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="PrimaryCreatureID"/>.
        /// </summary>
        public int SecondaryCreatureID => Game.Engine.CallScript("gml_Script_scr_SpecializationSecondaryCreature", ID);
        /// <summary>
        /// The ID of the <see cref="Spell"> this specialization starts the game with.
        /// </summary>
        public int SpellID => Game.Engine.CallScript("gml_Script_scr_SpecializationSpell", ID);
        /// <summary>
        /// The IDs of the perks this specialization can access.
        /// </summary>
        public int[] PerkIDs => Game.Engine.CallScript("gml_Script_scr_PerkGetPerkList", ID).GetArray().Skip(1).Select(x => x.GetInt32()).ToArray();
        /// <summary>
        /// The perks this specialization can access.
        /// </summary>
        public Perk[] Perks => PerkIDs.Select(x => Perk.Database[x]).ToArray();
        /// <summary>
        /// The ID of the perk you get when you ascend this specialization.
        /// </summary>
        public int AscendedPerkID => Game.Engine.CallScript("gml_Script_scr_AscensionPerk", ID);
        /// <summary>
        /// The perk you get when you ascend this specialization.
        /// </summary>
        public Perk AscendedPerk => Perk.Database[AscendedPerkID];
    }

    public class SpecializationDatabase : Database<int, Specialization>
    {
        public override IEnumerable<int> Keys
        {
            get
            {
                int i = 0;
                GameVariable v;
                do
                {
                    i++;
                    v = Game.Engine.CallScript("gml_Script_scr_SpecializationIcon", i);
                    if (!v.Type.Equals("undefined"))
                    {
                        yield return i;
                    }
                } while (!v.Type.Equals("undefined"));
            }
        }

        protected override Specialization? FetchNewEntry(int key) => new Specialization(key);
    }

    /// <summary>
    /// A Siralim Ultimate perk definition.
    /// </summary>
    public class Perk
    {
        /// <summary>
        /// The unique ID of this perk.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this perk.
        /// </summary>
        public string Name;
        /// <summary>
        /// The English description of this perk.
        /// </summary>
        public string Description;
        /// <summary>
        /// The number of ranks this perk has.
        /// </summary>
        public int MaxRanks;

        //public int Unknown3; // Always -1
        //public int Unknown4; // always 0
        //public int Unknown5; // always 1

        /// <summary>
        /// How many perk points it takes to increase the rank of this perk by 1.
        /// </summary>
        public int PointsPerRank;
        /// <summary>
        /// The ID of the sprite for this perk's icon.
        /// </summary>
        public int IconID;

        public Perk(int id, string name, string description, int maxRanks, int pointsPerRank, int iconID)
        {
            ID = id;
            Name = name;
            Description = description;
            MaxRanks = maxRanks;
            PointsPerRank = pointsPerRank;
            IconID = iconID;
        }

        /// <summary>
        /// All the perks in the game.
        /// </summary>
        public static PerkDatabase Database = [];

        internal static Perk FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Perk(
                id: id,
                name: gml[0].GetString(),
                description: gml[1].GetString(),
                maxRanks: gml[2].GetInt32(),
                pointsPerRank: gml[6].GetInt32(),
                iconID: gml[7].GetSpriteID()
            );
        }
        
        public override string ToString()
        {
            return $@"Perk(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    MaxRanks={MaxRanks},
    PointsPerRank={PointsPerRank},
    IconID={IconID},
    Specialization={Specialization.Name},
    IsAnointable={IsAnointable},
)";
        }
        
        /// <summary>
        /// The <see cref="Specialization"/> this perk belongs to.
        /// </summary>
        public Specialization Specialization => Specialization.Database.Values.First(s => s.AscendedPerkID == ID || s.PerkIDs.Contains(ID));
        /// <summary>
        /// Is this perk anointable?
        /// </summary>
        public bool IsAnointable => Game.Engine.CallScript("gml_Script_scr_AnointmentGetSpecialization", ID).GetInt32() >= 1;
    }

    public class PerkDatabase : Database<int, Perk>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["rdb"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Perk? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Perk.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
