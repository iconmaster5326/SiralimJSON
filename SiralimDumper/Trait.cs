using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate trait definition.
    /// </summary>
    public class Trait
    {
        /// <summary>
        /// The unique ID of this trait.
        /// </summary>
        public int ID;

        /// <summary>
        /// The English name of this trait.
        /// </summary>
        public string Name;

        /// <summary>
        /// The English description of this trait.
        /// </summary>
        public string Description;

        public Trait(
            int id,
            string name,
            string description
        )
        {
            ID = id;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// All the traits in the game.
        /// </summary>
        public static TraitDatabase Database = [];

        internal static Trait FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Trait(
                id,
                gml[0].GetString(),
                gml[1].GetString()
            );
        }

        public override string ToString()
        {
            return $@"Trait(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
)";
        }

        /// <summary>
        /// The creature this trait is associated with, if any.
        /// </summary>
        public Creature? Creature => Creature.Database.Values.FirstOrDefault(c => c.TraitID == ID);
        /// <summary>
        /// The Rodian Master this trait is associated with, if any.
        /// </summary>
        public Race? Race => Race.Database.Values.FirstOrDefault(c => c.HasMaster && c.MasterTraitID == ID);
        /// <summary>
        /// The item this trait is associated with, if any.
        /// </summary>
        public ItemMaterialTrait? Item => ItemMaterialTrait.Database.Values.FirstOrDefault(c => c.TraitID == ID);
        /// <summary>
        /// The god this trait is associated with, if any.
        /// </summary>
        public God? God => God.Database.Values.FirstOrDefault(c => c.TraitID == ID);

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Trait AsJSON => new()
        {
#nullable disable
            Creator = null,
            Creature = Creature?.ID,
            Description = Description,
            God = God?.ID,
            Id = ID,
            Item = Item?.ID,
            Name = Name,
            Notes = [],
            Race = Race?.Name,
#nullable enable
        };
    }

    public class TraitDatabase : Database<int, Trait>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["passive"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Trait? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Trait.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
