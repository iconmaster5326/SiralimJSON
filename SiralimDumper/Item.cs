using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A generic Siralim Ultimate item.
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// The ID of this item.
        /// Item IDs are unique within each category of item, but may overlap between categories.
        /// </summary>
        public int ID;

        protected Item(int id)
        {
            ID = id;
        }

        internal abstract TempInstance TempInstance { get; }

        /// <summary>
        /// The English name of this item.
        /// </summary>
        public string Name
        {
            get
            {
                using (var ti = TempInstance)
                {
                    return Game.Engine.CallScript("gml_Script_inv_ItemGetName", ti.Instance);
                }
            }
        }
        /// <summary>
        /// A English description of this item.
        /// </summary>
        public string Description
        {
            get
            {
                using (var ti = TempInstance)
                {
                    return Game.Engine.CallScript("gml_Script_inv_ItemGetDescription", ti.Instance);
                }
            }
        }
        /// <summary>
        /// The sprite ID of the icon for this item.
        /// This is a large sprite with many frames; see <see cref="IconIndex"/> for the index to use.
        /// </summary>
        public int IconID => "icons".GetGMLAssetID();
        /// <summary>
        /// The frame of the icon sprite to use for this item.
        /// This is a large sprite with many frames; see <see cref="IconID"/> for the sprite to use.
        /// </summary>
        public int IconIndex
        {
            get
            {
                using (var ti = TempInstance)
                {
                    return Game.Engine.CallScript("gml_Script_inv_ItemIconIndex", ti.Instance);
                }
            }
        }
    }

    /// <summary>
    /// A type of material, used to add spell properties to spell gems.
    /// </summary>
    public class ItemSpellProperty : Item
    {
        /// <summary>
        /// How many items of this type there are in the game.
        /// </summary>
        public const int N_DUSTS = 21;

        public ItemSpellProperty(int id) : base(id) { }
        internal override TempInstance TempInstance => new TempItemSpellPropertyInstance(this);

        /// <summary>
        /// All the spell property items in the game.
        /// </summary>
        public static Dictionary<int, ItemSpellProperty> Database
        {
            get
            {
                if (_Database == null)
                {
                    _Database = [];
                }
                for (int i = 0; i < N_DUSTS; i++)
                {
                    _Database[i] = new ItemSpellProperty(i);
                }
                return _Database;
            }
        }
        private static Dictionary<int, ItemSpellProperty>? _Database;

        public override string ToString()
        {
            return $@"ItemSpellProperty(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    IconID={IconID},
    IconIndex={IconIndex},
    PropertiesApplicable=['{string.Join("', '", PropertiesApplicable.Select(sp => sp.ShortDescription))}'],
)";
        }

        /// <summary>
        /// What spell properties can we use this material to apply to spell gems?
        /// </summary>
        public IEnumerable<SpellProperty> PropertiesApplicable => SpellProperty.Database.Values.Where(sp => sp.ItemID == ID);
    }
}
