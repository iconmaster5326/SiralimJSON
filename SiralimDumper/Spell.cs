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
    /// How a spell may select targets. 
    /// Types 1, 2, and 3 are considered AoE, and subject to the potency reduction.
    /// </summary>
    public enum SpellTargetingType
    {
        /// <summary>
        /// One target.
        /// </summary>
        ONE,
        /// <summary>
        /// All enemies.
        /// Subject to AoE penalty.
        /// </summary>
        ENEMIES,
        /// <summary>
        /// All allies.
        /// Subject to AoE penalty.
        /// </summary>
        ALLIES,
        /// <summary>
        /// All creatures.
        /// Subject to AoE penalty.
        /// </summary>
        ALL,
        /// <summary>
        /// Self, no targets, special targets, etc.
        /// </summary>
        OTHER
    }

    public class Spell
    {
        private const int N_COMPAT_ARRAY = 30;
        private static readonly IReadOnlyDictionary<int, int> COMPAT_INDEX_TO_PROPERTY_ID = new Dictionary<int, int>
        {
            [0] = 0, // more charges
            [1] = 1, // damage +
            [2] = 2, // healing +
            [3] = 18, // magnetic
            [4] = 6, // generous
            [5] = 23, // WAS defence pen, NOW cascading (SU only)
            [6] = 8, // duration +
            [7] = 3, // cast twice
            [8] = 4, // extra target
            // index 9 is unused
            // index 10 is unused
            [11] = 9, // stat +
            [12] = 10, // chaos
            [13] = 11, // death
            [14] = 12, // life
            [15] = 14, // nature
            [16] = 13, // sorcery
            [17] = 16, // provoke after
            [18] = 15, // defend after
            [19] = 17, // attack after
            [20] = 7, // WAS unused, NOW defence pen
            [21] = 20, // singular
            [22] = 21, // potency defence
            [23] = 22, // potency speed
            [24] = 19, // potency attack
            [25] = 5, // potency hp
            [26] = 24, // on attack (SU only)
            [27] = 25, // on defend (SU only)
            [28] = 26, // on provoke (SU only)
            [29] = 27, // on heal (SU only)
        };

        /// <summary>
        /// The unique ID of this spell.
        /// </summary>
        public int ID;
        /// <summary>
        /// The class this spell belongs to.
        /// </summary>
        public string Class;
        /// <summary>
        /// The English description of this spell.
        /// </summary>
        public string Description;
        /// <summary>
        /// The default number of maximum charges this spell has.
        /// </summary>
        public int MaxCharges;
        /// <summary>
        /// The English name of this spell.
        /// </summary>
        public string Name;
        /// <summary>
        /// A list of tags.
        /// Tags are in ALL UPPERCASE, and indicate categories of spell (damaging, healing, buffing, etc).
        /// </summary>
        public string[] Tags;

        public SpellTargetingType TargetingType;
        /// <summary>
        /// Is this spell one that can resurrect creatures? Usually, but not always, paired with RESURRECT <see cref="Tags"/>.
        /// 
        /// Note that as of 2.0.37, Good Fortune is listed as a resurrection effect even though it doesn't resurrect anything.
        /// This is because it did in fact resurrect things in Siralim 3, and it was never updated.
        /// </summary>
        public bool ResurrectionEffect;
        /// <summary>
        /// The ID of the animated sprite that plays when the spell is used on a target.
        /// </summary>
        public int SpriteID;
        /// <summary>
        /// The ID of the sound effect that plays when the spell is used on a target.
        /// </summary>
        public int SoundID;
        /// <summary>
        /// What spell gem properties can be placed on this spell.
        /// </summary>
        public HashSet<SpellProperty> PropertyCompatibility;

        public Spell(int id, string @class, string description, int maxCharges, string name, string[] tags, SpellTargetingType targetingType, bool resurrectionEffect, int spriteID, int soundID, IEnumerable<SpellProperty> propertyCompatibility)
        {
            ID = id;
            Class = @class;
            Description = description;
            MaxCharges = maxCharges;
            Name = name;
            Tags = tags;
            TargetingType = targetingType;
            ResurrectionEffect = resurrectionEffect;
            SpriteID = spriteID;
            SoundID = soundID;
            PropertyCompatibility = [.. propertyCompatibility];
        }

        /// <summary>
        /// All the spells in the game.
        /// </summary>
        public static SpellDatabase Database = [];

        internal static Spell FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Spell(
                id: id,
                @class: gml[0],
                description: gml[1],
                maxCharges: gml[2],
                name: gml[3],
                tags: gml[4].GetString().Split(","),
                targetingType: (SpellTargetingType)gml[5].GetInt32(),
                resurrectionEffect: gml[6],
                spriteID: gml[7].GetSpriteID(),
                soundID: gml[8].GetSoundID(),
                propertyCompatibility: gml.Skip(9).Take(N_COMPAT_ARRAY)
                    .Index()
                    .Where(kv => kv.Item && COMPAT_INDEX_TO_PROPERTY_ID.ContainsKey(kv.Index))
                    .Select(kv => SpellProperty.Database[COMPAT_INDEX_TO_PROPERTY_ID[kv.Index]])
            );
        }

        public override string ToString()
        {
            return $@"Spell(
    ID={ID},
    Class={Class},
    Description='{Description}',
    MaxCharges={MaxCharges},
    Name='{Name}',
    Tags=[{string.Join(", ", Tags)}],
    TargetingType={TargetingType},
    ResurrectionEffect={ResurrectionEffect},
    SpriteID={SpriteID},
    SoundID={SoundID},
    PropertyCompatibility=({PropertyCompatibility.Count} items) ['{string.Join("', '", PropertyCompatibility.Select(p => p.ShortDescription))}'],
    BannedFromRNG={BannedFromRNG},
    ManuallyCastable={ManuallyCastable},
    Booze={Booze},
    Arsenal={Arsenal},
    Ultimate={Ultimate},
    RandomlyDroppable={RandomlyDroppable},
)";
        }

        /// <summary>
        /// Is this spell banned from appearing on enemies in random fights?
        /// </summary>
        public bool BannedFromRNG => Game.Engine.CallScript("gml_Script_inv_SpellBanned", ID);
        /// <summary>
        /// Can this spell be cast manually?
        /// </summary>
        public bool ManuallyCastable => Game.Engine.CallScript("gml_Script_inv_SpellCanBeCastManually", ID);
        /// <summary>
        /// Is this a booze spell?
        /// </summary>
        public bool Booze => Game.Engine.CallScript("gml_Script_inv_SpellIsBooze", ID);
        /// <summary>
        /// Is this an arsenal spell?
        /// </summary>
        public bool Arsenal => Game.Engine.CallScript("gml_Script_inv_SpellIsEquipment", ID);
        /// <summary>
        /// Is this an ultimate spell?
        /// </summary>
        public bool Ultimate => Game.Engine.CallScript("gml_Script_inv_SpellIsUltimate", ID);
        /// <summary>
        /// Is this spell available from the normal random loot pool?
        /// </summary>
        public bool RandomlyDroppable => Game.Engine.CallScript("gml_Script_inv_SpellReserved", ID);
    }

    public class SpellDatabase : Database<int, Spell>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["spell"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Spell? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Spell.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
