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

        /// <summary>
        /// The English name of this false god.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_FalseGodName", ID);
        /// <summary>
        /// The ID of the sprite this false god uses in the overworld.
        /// </summary>
        public int OverworldSpriteID => Game.Engine.CallScript("gml_Script_scr_FalseGodOWSprite", ID).GetSpriteID();
        /// <summary>
        /// The sprite this false god uses in the overworld.
        /// </summary>
        public Sprite OverworldSprite => OverworldSpriteID.GetGMLSprite();
        /// <summary>
        /// The English description and lore of this false god.
        /// </summary>
        public string Description => Game.Engine.CallScript("gml_Script_scr_FalseGodLore", ID);
        /// <summary>
        /// The English text the false god speaks before you fight them.
        /// </summary>
        public string Dialog => Game.Engine.CallScript("gml_Script_scr_FalseGodDialog", ID);
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
        public int IconID => ICON_SPRITES[ID].GetGMLAssetID();
        /// <summary>
        /// The icon for this false god.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();
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

        /// <summary>
        /// The English description of this rune.
        /// </summary>
        public string Description => Game.Engine.CallScript("gml_Script_scr_RuneName", ID);
        /// <summary>
        /// The ID of the sprite for this rune.
        /// </summary>
        public int SpriteID => Game.Engine.CallScript("gml_Script_scr_RuneSprite", ID, false).GetSpriteID();
        /// <summary>
        /// The sprite for this rune.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();
        /// <summary>
        /// The ID of the sprite for this rune when not selected.
        /// </summary>
        public int InactiveSpriteID => Game.Engine.CallScript("gml_Script_scr_RuneSprite", ID, true).GetSpriteID();
        /// <summary>
        /// The sprite for this rune when not selected.
        /// </summary>
        public Sprite InactiveSprite => InactiveSpriteID.GetGMLSprite();
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
