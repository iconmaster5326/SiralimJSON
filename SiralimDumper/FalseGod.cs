using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate false god definition.
    /// These are bosses you go on bounties to kill, and get anointments from.
    /// </summary>
    public class FalseGod
    {
        /// <summary>
        /// The unique ID for this false god.
        /// </summary>
        public int ID;

        public FalseGod(int iD)
        {
            ID = iD;
        }

        public static FalseGodDatabase Database = [];

        public override string ToString()
        {
            return $@"FalseGod(
    ID={ID},
    Name='{Name}',
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    OverworldSprite={OverworldSprite.ToString().Replace("\n", "\n  ")},
    Description='{Description.Escape()}',
    Dialog='{Dialog.Escape()}'
)";
        }

        private string? _Name;
        /// <summary>
        /// The English name of this false god.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_FalseGodName", ID));

        private int? _OverworldSpriteID;
        /// <summary>
        /// The ID of the sprite this false god uses in the overworld.
        /// </summary>
        public int OverworldSpriteID => _OverworldSpriteID ?? (_OverworldSpriteID = Game.Engine.CallScript("gml_Script_scr_FalseGodOWSprite", ID).GetSpriteID()).Value;

        /// <summary>
        /// The sprite this false god uses in the overworld.
        /// </summary>
        public Sprite OverworldSprite => OverworldSpriteID.GetGMLSprite();

        private string? _Description;
        /// <summary>
        /// The English description and lore of this false god.
        /// </summary>
        public string Description => _Description ?? (_Description = Game.Engine.CallScript("gml_Script_scr_FalseGodLore", ID));

        private string? _Dialog;
        /// <summary>
        /// The English text the false god speaks before you fight them.
        /// </summary>
        public string Dialog => _Dialog ?? (_Dialog = Game.Engine.CallScript("gml_Script_scr_FalseGodDialog", ID));

        private int? _IconID;
        /// <summary>
        /// TODO: Replace this with a script call if possible.
        /// </summary>
        private static readonly IReadOnlyDictionary<int, string> ICON_SPRITES = new Dictionary<int, string>() {
            [0] = "worldboss_althea",
            [1] = "worldboss_nebodar",
            [2] = "worldboss_jotinir",
            [3] = "worldboss_hydranox",
            [4] = "worldboss_loid",
            [5] = "worldboss_impington",
            [6] = "worldboss_ancestor",
            [7] = "worldboss_caliban",
            [8] = "worldboss_wurm",
            [9] = "worldboss_construct",
        };
        /// <summary>
        /// The ID of the icon for this false god.
        /// </summary>
        public int IconID => _IconID ?? (_IconID = ICON_SPRITES[ID].GetGMLAssetID()).Value;

        /// <summary>
        /// The icon for this false god.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();

        /// <summary>
        /// TODO: Replace this with a script call if possible.
        /// </summary>
        private static readonly IReadOnlyDictionary<int, string> PART_SPRITE_NAMES = new Dictionary<int, string>()
        {
            [0] = "althea_@",
            [1] = "nebodar_@",
            [2] = "jotinir_@",
            [3] = "hydranox_@",
            [4] = "loid_@",
            [5] = "impington_@",
            [6] = "ancestor_@",
            [7] = "caliban_0@_ow",
            [8] = "mindwurm_@",
            [9] = "lostconstruct_@",
        };
        /// <summary>
        /// The creature in the fight, from slot 1-6.
        /// </summary>
        public Creature CreatureInSlot(int slot) => Creature.Database.Values.First(c => c.OverworldSprite.Name.Equals(PART_SPRITE_NAMES[ID].Replace("@", slot.ToString())));
        private Creature[]? _Creatures;
        /// <summary>
        /// The creatures in the fight, from top to bottom, left to right.
        /// </summary>
        public Creature[] Creatures => _Creatures ?? (_Creatures = Enumerable.Range(1, 6).Select(CreatureInSlot).ToArray());

        public string IconFilename => $@"falsegod\{Name.EscapeForFilename()}\icon.png";
        public string SpriteFilename0 => $@"falsegod\{Name.EscapeForFilename()}\overworld_0.png";
        public string SpriteFilename1 => $@"falsegod\{Name.EscapeForFilename()}\overworld_1.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.FalseGod AsJSON => new()
        {
#nullable disable
            Creator = null,
            Description = Description,
            Icon = $@"images/{IconFilename}".Replace("\\", "/"),
            Sprite = [$@"images/{SpriteFilename0}".Replace("\\", "/"), $@"images/{SpriteFilename1}".Replace("\\", "/")],
            Id = ID,
            Name = Name,
            Notes = [],
            Parts = Creatures.Select(c => (long)c.ID).ToArray(),
            Anointments = [], // TODO
            BountyCost = [], // TODO
            Spells = [], // TODO
#nullable enable
        };
    }

    public class FalseGodDatabase : Database<int, FalseGod>
    {
        public override IEnumerable<int> Keys
        {
            get
            {
                int i = -1;
                string v = "";
                do
                {
                    i++;
                    v = Game.Engine.CallScript("gml_Script_scr_FalseGodName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (v.Length > 0);
            }
        }

        protected override FalseGod? FetchNewEntry(int key) => new FalseGod(key);
    }

    /// <summary>
    /// A Siralim Ultimate false god rune definition.
    /// Collecting runes before fighting false gods powers them up for bonus loot.
    /// </summary>
    public class FalseGodRune
    {
        /// <summary>
        /// The unique ID for this rune.
        /// </summary>
        public int ID;

        public FalseGodRune(int iD)
        {
            ID = iD;
        }

        public static FalseGodRuneDatabase Database = [];

        public override string ToString()
        {
            return $@"FalseGodRune(
    ID={ID},
    Description='{Description.Escape()}',
    Sprite={Sprite.ToString().Replace("\n", "\n  ")}
)";
        }

        private string? _Description;
        /// <summary>
        /// The English description of this rune.
        /// </summary>
        public string Description => _Description ?? (_Description = Game.Engine.CallScript("gml_Script_scr_RuneName", ID));

        private int? _SpriteID;
        /// <summary>
        /// The ID of the sprite for this rune.
        /// </summary>
        public int SpriteID => _SpriteID ?? (_SpriteID = Game.Engine.CallScript("gml_Script_scr_RuneSprite", ID, false).GetSpriteID()).Value;

        /// <summary>
        /// The sprite for this rune.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();

        private int? _InactiveSpriteID;
        /// <summary>
        /// The ID of the sprite for this rune when not selected.
        /// </summary>
        public int InactiveSpriteID => _InactiveSpriteID ?? (_InactiveSpriteID = Game.Engine.CallScript("gml_Script_scr_RuneSprite", ID, true).GetSpriteID()).Value;

        /// <summary>
        /// The sprite for this rune when not selected.
        /// </summary>
        public Sprite InactiveSprite => InactiveSpriteID.GetGMLSprite();

        public string SpriteFilename => $@"rune\{ID}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Rune AsJSON => new()
        {
#nullable disable
            Bonus = 0, // TODO
            Description = Description,
            Id = ID,
            Notes = [],
            Sprite = $@"images\{SpriteFilename}".Replace("\\", "/"),
            Reserved = ID == 7, // TODO: automate this?
#nullable enable
        };
    }

    public class FalseGodRuneDatabase : Database<int, FalseGodRune>
    {
        public override IEnumerable<int> Keys
        {
            get
            {
                int i = -1;
                string v = "";
                do
                {
                    i++;
                    v = Game.Engine.CallScript("gml_Script_scr_RuneName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (i < 50);
            }
        }

        protected override FalseGodRune? FetchNewEntry(int key) => new FalseGodRune(key);
    }
}
