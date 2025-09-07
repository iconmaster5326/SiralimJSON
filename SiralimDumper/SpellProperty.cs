using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    public class SpellProperty
    {
        /// <summary>
        /// How many spell properties there are in the game.
        /// </summary>
        public const int N_SPELL_PROPERTIES = 28;

        /// <summary>
        /// The IDs of all spell properties, in the order the game displays each property.
        /// </summary>
        public static readonly int[] DISPLAY_ORDER = [
            0, 23, 20, 7, 18, 17, 15, 24, 25, 27, 26, 4, 16, 3, 6, 8, 9, 1, 2, 19, 10, 11, 12, 14, 13, 22, 21, 5
        ];

        /// <summary>
        /// The unique ID of this spell property.
        /// </summary>
        public int ID;

        public SpellProperty(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the spell properties in the game.
        /// </summary>
        public static SpellPropertyDatabase Database = [];

        public override string ToString()
        {
            return $@"SpellProperty(
    ID={ID},
    ShortDescription='{ShortDescription}',
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    IconIndex={IconIndex},
    Item='{ItemSpellProperty.Database[ItemID].Name}',
)";
        }

        private string FullShortDesc => Game.Engine.CallScript("gml_Script_inv_SpellGemPropertyString", ID);

        /// <summary>
        /// The English short description of this spell property.
        /// </summary>
        public string ShortDescription => Regex.Replace(FullShortDesc, "^\\[[^\\]]*\\] ", "");
        /// <summary>
        /// The sprite ID of the icon for this spell property.
        /// This is a large sprite with many frames; see <see cref="IconIndex"/> for the index to use.
        /// </summary>
        public int IconID => Regex.Match(FullShortDesc, "^\\[([^,]*)").Groups[1].Value.GetGMLAssetID();
        /// <summary>
        /// The sprite of the icon for this spell property.
        /// This is a large sprite with many frames; see <see cref="IconIndex"/> for the index to use.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();
        /// <summary>
        /// The frame of the icon sprite to use for this spell property.
        /// This is a large sprite with many frames; see <see cref="IconID"/> for the sprite to use.
        /// </summary>
        public int IconIndex
        {
            get
            {
                var match = Regex.Match(FullShortDesc, "^\\[[^,]*,([^\\]]*)");
                if (!match.Success)
                {
                    return 0;
                }
                string value = match.Groups[1].Value;
                if (value.Length == 0)
                {
                    return 0;
                }
                return int.Parse(value);
            }
        }
        /// <summary>
        /// Returns the ID of the <see cref="ItemSpellProperty"/> that applies this property.
        /// </summary>
        public int ItemID => Game.Engine.CallScript("gml_Script_inv_SpellGemGetMaterialByStat", ID);

        /// <summary>
        /// The file path to the icon.
        /// </summary>
        public string IconFilename => $@"spellprop\{ID}.png";
        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.SpellProperty AsJSON => new()
        {
#nullable disable
            ShortDescription = ShortDescription,
            LongDescription = "NYI",
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            Item = ItemID,
            Notes = [],
#nullable enable
        };
    }

    public class SpellPropertyDatabase : Database<int, SpellProperty>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, SpellProperty.N_SPELL_PROPERTIES);

        protected override SpellProperty? FetchNewEntry(int key) => new SpellProperty(key);
    }
}
