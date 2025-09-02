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
        public virtual string Name
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
        public virtual string Description
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
        public virtual int IconID => "icons".GetGMLAssetID();
        /// <summary>
        /// The frame of the icon sprite to use for this item.
        /// This is a large sprite with many frames; see <see cref="IconID"/> for the sprite to use.
        /// </summary>
        public virtual int IconIndex
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
        public static ItemSpellPropertyDatabase Database = [];

        public override string ToString()
        {
            return $@"ItemSpellProperty(
    ID={ID},
    Name='{Name}',
    Description='{Description.Escape()}',
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

    public class ItemSpellPropertyDatabase : Database<int, ItemSpellProperty>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, ItemSpellProperty.N_DUSTS);

        protected override ItemSpellProperty? FetchNewEntry(int key) => new ItemSpellProperty(key);
    }

    public enum MaterialKind
    {
        STAT,
        TRICK,
        TRAIT,
    }

    public enum StatKind
    {
        HP,
        ATTACK,
        INTELLIGENCE,
        DEFENSE,
        SPEED
    }

    public abstract class ItemMaterial : Item
    {
        protected int SpriteID;
        public ItemMaterial(int id, int spriteID) : base(id)
        {
            SpriteID = spriteID;
        }
        /// <summary>
        /// The slot this material can go in.
        /// </summary>
        public abstract MaterialKind MaterialKind { get; }
        internal override TempInstance TempInstance => new TempItemMaterialInstance(this);
        /// <summary>
        /// The database of all materials.
        /// </summary>
        public static ItemMaterialDatabase Database = [];

        override public int IconID => SpriteID;

        override public int IconIndex => 0;
    }

    public class ItemMaterialDatabase : Database<int, ItemMaterial>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["mat"].GetArray();
        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override ItemMaterial? FetchNewEntry(int key)
        {
            var gml = Array[key].GetArray();
            GameVariable kindVar = gml[1];
            switch ((MaterialKind)kindVar.GetInt32())
            {
                case MaterialKind.STAT:
                    return ItemMaterialStat.FromGML(key, gml);
                case MaterialKind.TRICK:
                    return ItemMaterialTrick.FromGML(key, gml);
                case MaterialKind.TRAIT:
                    return ItemMaterialTrait.FromGML(key, gml);
                default:
                    throw new Exception($"Invalid material kind {kindVar.PrettyPrint().EscapeNonWS()}!");
            }
        }
    }

    public class ItemMaterialStat : ItemMaterial
    {
        internal static readonly Dictionary<int, StatKind[]> INT_TO_STATS = new Dictionary<int, StatKind[]>
        {
            [0] = [(StatKind)0],
            [1] = [(StatKind)1],
            [2] = [(StatKind)2],
            [3] = [(StatKind)3],
            [4] = [(StatKind)4],
            [54] = [(StatKind)0, (StatKind)1],
            [55] = [(StatKind)0, (StatKind)2],
            [56] = [(StatKind)0, (StatKind)3],
            [57] = [(StatKind)0, (StatKind)4],
            [58] = [(StatKind)1, (StatKind)2],
            [59] = [(StatKind)1, (StatKind)3],
            [60] = [(StatKind)1, (StatKind)4],
            [61] = [(StatKind)2, (StatKind)3],
            [62] = [(StatKind)2, (StatKind)4],
            [63] = [(StatKind)3, (StatKind)4],
        };
        /// <summary>
        /// What stat(s) this material improves.
        /// </summary>
        public StatKind[] Stats;
        public ItemMaterialStat(int id, int spriteID, StatKind[] stats) : base(id, spriteID)
        {
            Stats = stats;
        }

        internal static ItemMaterialStat FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new ItemMaterialStat(
                id: id,
                spriteID: gml.Count >= 5 ? gml[4].GetSpriteID() : "material_legendary_default".GetGMLAssetID(),
                stats: INT_TO_STATS[gml[2]]
            );
        }

        public override string ToString()
        {
            return $@"ItemMaterialStat(
    ID={ID},
    Name='{Name}',
    Description='{Description.Escape()}',
    IconID={IconID},
    IconIndex={IconIndex},
    MaterialKind={MaterialKind},
    Stats=[{string.Join(", ", Stats)}],
)";
        }

        public override MaterialKind MaterialKind => MaterialKind.STAT;
        /// <summary>
        /// The database of every stat material item.
        /// </summary>
        new public static ItemMaterialStatDatabase Database = [];
    }

    public class ItemMaterialStatDatabase : Database<int, ItemMaterialStat>
    {
        public override IEnumerable<int> Keys => Game.Engine.GetGlobalObject()["mat"].GetArray()
            .Index()
            .Where(kv => !kv.Item.IsNumber() && kv.Item.GetArray()[1].GetInt32() == (int)MaterialKind.STAT)
        .Select(kv => kv.Index);

        protected override ItemMaterialStat? FetchNewEntry(int key)
        {
            return ItemMaterial.Database[key] as ItemMaterialStat;
        }
    }

    public class ItemMaterialTrick : ItemMaterial
    {
        public ItemMaterialTrick(int id, int spriteID) : base(id, spriteID)
        {

        }

        internal static ItemMaterialTrick FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new ItemMaterialTrick(
                id: id,
                spriteID: gml.Count >= 5 ? gml[4].GetSpriteID() : "material_legendary_default".GetGMLAssetID()
            );
        }

        public override string ToString()
        {
            return $@"ItemMaterialTrick(
    ID={ID},
    Name='{Name}',
    Description='{Description.Escape()}',
    IconID={IconID},
    IconIndex={IconIndex},
    MaterialKind={MaterialKind},
)";
        }

        public override MaterialKind MaterialKind => MaterialKind.TRICK;

        /// <summary>
        /// The database of every trick material item.
        /// </summary>
        new public static ItemMaterialTrickDatabase Database = [];
    }

    public class ItemMaterialTrickDatabase : Database<int, ItemMaterialTrick>
    {
        public override IEnumerable<int> Keys => Game.Engine.GetGlobalObject()["mat"].GetArray()
            .Index()
            .Where(kv => !kv.Item.IsNumber() && kv.Item.GetArray()[1].GetInt32() == (int)MaterialKind.TRICK)
        .Select(kv => kv.Index);

        protected override ItemMaterialTrick? FetchNewEntry(int key)
        {
            return ItemMaterial.Database[key] as ItemMaterialTrick;
        }
    }

    public class ItemMaterialTrait : ItemMaterial
    {
        /// <summary>
        /// The ID of the trait this material gives.
        /// </summary>
        public int TraitID;
        public ItemMaterialTrait(int id, int spriteID, int traitID) : base(id, spriteID)
        {
            TraitID = traitID;
        }

        internal static ItemMaterialTrait FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new ItemMaterialTrait(
                id: id,
                spriteID: gml.Count >= 5 ? gml[4].GetSpriteID() : "material_legendary_default".GetGMLAssetID(),
                traitID: gml[3]
            );
        }

        public override string ToString()
        {
            return $@"ItemMaterialTrait(
    ID={ID},
    Name='{Name}',
    Description='{Description.Escape()}',
    IconID={IconID},
    IconIndex={IconIndex},
    MaterialKind={MaterialKind},
    Trait='{(Trait.Database.ContainsKey(TraitID) ? Trait.Database[TraitID].Name : TraitID)}',
)";
        }

        public override MaterialKind MaterialKind => MaterialKind.TRAIT;

        public override string Description => Trait.Database.ContainsKey(TraitID) ? base.Description : "";

        /// <summary>
        /// The database of every trait material item.
        /// </summary>
        new public static ItemMaterialTraitDatabase Database = [];
    }

    public class ItemMaterialTraitDatabase : Database<int, ItemMaterialTrait>
    {
        public override IEnumerable<int> Keys => Game.Engine.GetGlobalObject()["mat"].GetArray()
            .Index()
            .Where(kv => !kv.Item.IsNumber() && kv.Item.GetArray()[1].GetInt32() == (int)MaterialKind.TRAIT)
        .Select(kv => kv.Index);

        protected override ItemMaterialTrait? FetchNewEntry(int key)
        {
            return ItemMaterial.Database[key] as ItemMaterialTrait;
        }
    }

    public class ItemArtifact : Item
    {
        public const int N_ARTIFACTS = 5;

        public ItemArtifact(int id) : base(id)
        {

        }

        internal override TempInstance TempInstance => new TempItemArtifact(this);

        /// <summary>
        /// The database of all artifacts.
        /// </summary>
        public static ItemArtifactDatabase Database = [];

        public override string ToString()
        {
            return $@"ItemArtifact(
    ID={ID},
    Name='{Name}',
    Description='{Description.Escape()}',
    IconID={IconID},
    IconIndex={IconIndex},
    Increases={Increases},
)";
        }

        override public int IconID
        {
            get
            {
                using (var ti = TempInstance)
                {
                    return Game.Engine.CallScript("gml_Script_inv_ArtifactIcon", ti.Instance).GetSpriteID();
                }
            }
        }

        override public int IconIndex => 0;

        /// <summary>
        /// The stat this artifact primarily increases.
        /// </summary>
        public StatKind Increases
        {
            get
            {
                switch (ID)
                {
                    case 0: return StatKind.SPEED;
                    case 1: return StatKind.HP;
                    case 2: return StatKind.DEFENSE;
                    case 3: return StatKind.ATTACK;
                    case 4: return StatKind.INTELLIGENCE;
                    default: throw new Exception($"Unknown artifact ID: {ID}");
                }
            }
        }

        public class ItemArtifactDatabase : Database<int, ItemArtifact>
        {
            public override IEnumerable<int> Keys => Enumerable.Range(0, ItemArtifact.N_ARTIFACTS);

            protected override ItemArtifact? FetchNewEntry(int key) => new ItemArtifact(key);
        }
    }
}
