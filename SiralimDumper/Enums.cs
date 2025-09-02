using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiralimDumper
{
    /// <summary>
    /// Different kinds of slots that can go into artifacts.
    /// </summary>
    public enum ArtifactSlot
    {
        /// <summary>
        /// A stat slot. Provides a stat bonus to one or two stats.
        /// </summary>
        STAT,
        /// <summary>
        /// A trick slot. Provides a miscellaneous effect.
        /// </summary>
        TRICK,
        /// <summary>
        /// A trait slot. Provides a trait's effect.
        /// </summary>
        TRAIT,
        /// <summary>
        /// A spell slot. Provides a random chance to cast a spell, condition based on the artifact.
        /// </summary>
        SPELL,
        /// <summary>
        /// A nether slot. Nether stones go here.
        /// </summary>
        NETHER,
    }

    /// <summary>
    /// One of the five core Siralim stats.
    /// </summary>
    public enum Stat
    {
        HEALTH,
        ATTACK,
        INTELLIGENCE,
        DEFENSE,
        SPEED
    }

    /// <summary>
    /// One of the five classes of Siralim creatures and spells.
    /// </summary>
    public enum SiralimClass
    {
        LIFE,
        CHAOS,
        SORCERY,
        NATURE,
        DEATH,
    }

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

    public static class EnumUtil
    {
        private static readonly IReadOnlyDictionary<string, Stat> STAT_STRINGS = new Dictionary<string, Stat>()
        {
            ["Health"] = Stat.HEALTH,
            ["Attack"] = Stat.ATTACK,
            ["Intelligence"] = Stat.INTELLIGENCE,
            ["Defense"] = Stat.DEFENSE,
            ["Speed"] = Stat.SPEED,
        };
        /// <summary>
        /// Get a stat from the stat's name.
        /// </summary>
        public static Stat StatFromString(string s)
        {
            return STAT_STRINGS[s];
        }
        private static readonly IReadOnlyDictionary<string, SiralimClass> CLASS_STRINGS = new Dictionary<string, SiralimClass>()
        {
            ["Life"] = SiralimClass.LIFE,
            ["Chaos"] = SiralimClass.CHAOS,
            ["Sorcery"] = SiralimClass.SORCERY,
            ["Nature"] = SiralimClass.NATURE,
            ["Death"] = SiralimClass.DEATH,
        };
        /// <summary>
        /// Get a class from the class's name.
        /// </summary>
        public static SiralimClass ClassFromString(string s)
        {
            return CLASS_STRINGS[s];
        }
    }
}
