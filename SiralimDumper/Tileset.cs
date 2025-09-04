using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A GML tileset.
    /// </summary>
    public class Tileset
    {
        /// <summary>
        /// The unique ID of this tileset.
        /// </summary>
        public int ID;

        /// <summary>
        /// Do not construct these directly.
        /// Use <see cref="Database"/> instead.
        /// </summary>
        public Tileset(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the tilesets we've seen so far.
        /// Prefer getting tileset from here over calling the constructor.
        /// </summary>
        public static TilesetDatabase Database = [];

        /// <summary>
        /// The name of this tileset.
        /// </summary>
        public string Name => Game.Engine.CallFunction("tileset_get_name", ID);
        /// <summary>
        /// The texture of this tileset.
        /// </summary>
        public GameVariable Texture => Game.Engine.CallFunction("tileset_get_texture", ID);

        public override string ToString()
        {
            return $@"Tileset(
    ID={ID},
    Name='{Name}',
    Texture=({Texture.Type}) {Texture.PrettyPrint().EscapeNonWS().Replace("\n", "\n  ")},
)";
        }
    }

    public class TilesetDatabase : Database<int, Tileset>
    {
        public override IEnumerable<int> Keys => Cache.Keys;

        protected override Tileset? FetchNewEntry(int key)
        {
            return new Tileset(key);
        }
    }
}
