using AurieSharpInterop;
using SiralimDumper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public static Dictionary<int, SpellProperty> Database
        {
            get
            {
                if (_Database == null)
                {
                    _Database = [];
                }
                for (int i = 0; i < N_SPELL_PROPERTIES; i++)
                {
                    _Database[i] = new SpellProperty(i);
                }
                return _Database;
            }
        }
        private static Dictionary<int, SpellProperty>? _Database;

        public override string ToString()
        {
            return $@"SpellProperty(
    ID={ID},
    ShortDescription='{ShortDescription}',
    IconID={IconID},
    IconIndex={IconIndex},
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
    }
}
