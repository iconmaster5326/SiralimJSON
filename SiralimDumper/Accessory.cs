using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate accessory definition.
    /// Accessories can go onto your creatures to change how they look.
    /// </summary>
    public class Accessory
    {
        /// <summary>
        /// The unique ID of this accessory.
        /// </summary>
        public int ID;

        /// <summary>
        /// The English name of this accessory.
        /// </summary>
        public string Name;

        /// <summary>
        /// The ID of the sprite for this accessory.
        /// </summary>
        public int SpriteID;

        public Accessory(int id, string name, int spriteID)
        {
            ID = id;
            Name = name;
            SpriteID = spriteID;
        }

        /// <summary>
        /// All the accessories in the game.
        /// </summary>
        public static AccessoryDatabase Database = [];

        internal static Accessory FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Accessory(
                id,
                gml[0].GetString(),
                gml[1].GetSpriteID()
            );
        }

        public override string ToString()
        {
            return $@"Accessory(
    ID={ID},
    Name='{Name}',
    Sprite='{Sprite.ToString().Replace("\n", "\n  ")}',
)";
        }

        /// <summary>
        /// The sprite for this accessory.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();
    }

    public class AccessoryDatabase : Database<int, Accessory>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["acc"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Accessory? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Accessory.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
