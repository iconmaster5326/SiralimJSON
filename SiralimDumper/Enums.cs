using YYTKInterop;

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

    /// <summary>
    /// The possible form factors a <see cref="Decoration"/> can take.
    /// </summary>
    public enum DecorationHitbox
    {
        /// <summary>
        /// Can be any size. Has no hitbox.
        /// </summary>
        NONE = -1,
        /// <summary>
        /// Can be any size. Has no hitbox, but the top tiles render above, and the bottom tile renders below.
        /// </summary>
        NONE_TOP_PASSABLE,
        /// <summary>
        /// 1x1.
        /// </summary>
        ONE_BY_ONE,
        /// <summary>
        /// 1x2.
        /// </summary>
        ONE_BY_TWO,
        /// <summary>
        /// 2x1.
        /// </summary>
        TWO_BY_ONE,
        /// <summary>
        /// 2x2.
        /// </summary>
        TWO_BY_TWO,
        /// <summary>
        /// 1 tile wide. The tiles above that are passable, and renders above.
        /// </summary>
        ONE_WIDE_TOP_PASSABLE,
        /// <summary>
        /// 2 tiles wide. The tiles above that are passable, and renders above.
        /// </summary>
        TWO_WIDE_TOP_PASSABLE,
        /// <summary>
        /// 4x3.
        /// </summary>
        FOUR_BY_THREE,
        /// <summary>
        /// 8x8. Has a custom shaped hitbox.
        /// </summary>
        MENAGERIE,
        /// <summary>
        /// 3x3.
        /// </summary>
        THREE_BY_THREE,
        /// <summary>
        /// 8x3. Has a custom shaped hitbox.
        /// </summary>
        RELIQUARY,
        /// <summary>
        /// 3x2.
        /// </summary>
        THREE_BY_TWO,
    }

    /// <summary>
    /// The category a <see cref="Decoration"/> is found in in the menu.
    /// </summary>
    public enum DecorationCategory
    {
        ALTAR,
        BANNER,
        COLUMN,
        FLOOR_TILE,
        LIGHT,
        MISC,
        PAINTING,
        PLANT,
        PLUSH,
        RUG,
        SCROLL,
        STATUE,
        TABLE_CHAIR,
        UTILITY_NPC,
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

        /// <summary>
        /// The width of decorations with this hitbox, in tiles.
        /// This does NOT give you the total height of the decoration, JUST the hitbox's height!
        /// </summary>
        public static int Width(this DecorationHitbox hitbox)
        {
            switch (hitbox)
            {
                case DecorationHitbox.NONE:
                    return 0;
                case DecorationHitbox.NONE_TOP_PASSABLE:
                    return 0;
                case DecorationHitbox.ONE_BY_ONE:
                    return 1;
                case DecorationHitbox.ONE_BY_TWO:
                    return 1;
                case DecorationHitbox.TWO_BY_ONE:
                    return 2;
                case DecorationHitbox.TWO_BY_TWO:
                    return 2;
                case DecorationHitbox.ONE_WIDE_TOP_PASSABLE:
                    return 1;
                case DecorationHitbox.TWO_WIDE_TOP_PASSABLE:
                    return 2;
                case DecorationHitbox.FOUR_BY_THREE:
                    return 4;
                case DecorationHitbox.MENAGERIE:
                    return 8;
                case DecorationHitbox.THREE_BY_THREE:
                    return 3;
                case DecorationHitbox.RELIQUARY:
                    return 8;
                case DecorationHitbox.THREE_BY_TWO:
                    return 3;
                default:
                    throw new Exception($"Got unknown hitbox type {hitbox}!");
            }
        }

        /// <summary>
        /// The height of decorations with this hitbox, in tiles.
        /// This does NOT give you the total height of the decoration, JUST the hitbox's height!
        /// </summary>
        public static int Height(this DecorationHitbox hitbox)
        {
            switch (hitbox)
            {
                case DecorationHitbox.NONE:
                    return 0;
                case DecorationHitbox.NONE_TOP_PASSABLE:
                    return 0;
                case DecorationHitbox.ONE_BY_ONE:
                    return 1;
                case DecorationHitbox.ONE_BY_TWO:
                    return 2;
                case DecorationHitbox.TWO_BY_ONE:
                    return 1;
                case DecorationHitbox.TWO_BY_TWO:
                    return 2;
                case DecorationHitbox.ONE_WIDE_TOP_PASSABLE:
                    return 1;
                case DecorationHitbox.TWO_WIDE_TOP_PASSABLE:
                    return 1;
                case DecorationHitbox.FOUR_BY_THREE:
                    return 3;
                case DecorationHitbox.MENAGERIE:
                    return 8;
                case DecorationHitbox.THREE_BY_THREE:
                    return 3;
                case DecorationHitbox.RELIQUARY:
                    return 3;
                case DecorationHitbox.THREE_BY_TWO:
                    return 2;
                default:
                    throw new Exception($"Got unknown hitbox type {hitbox}!");
            }
        }

        /// <summary>
        /// The English name of this decoration category.
        /// </summary>
        public static string Name(this DecorationCategory category) => Game.Engine.CallScript("gml_Script_scr_DecorationCatName", (int)category);
    }
}
