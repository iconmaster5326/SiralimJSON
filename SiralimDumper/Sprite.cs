using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A GML sprite.
    /// </summary>
    public class Sprite
    {
        /// <summary>
        /// The unique ID of this sprite.
        /// </summary>
        public int ID;

        /// <summary>
        /// Do not construct these directly.
        /// Use <see cref="Database"/> instead.
        /// </summary>
        public Sprite(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the sprites we've seen so far.
        /// Prefer getting sprites from here over calling the constructor.
        /// </summary>
        public static SpriteDatabase Database = [];

        /// <summary>
        /// The name of this sprite.
        /// </summary>
        public string Name => Game.Engine.CallFunction("sprite_get_name", ID);
        /// <summary>
        /// How many frames does this sprite have?
        /// </summary>
        public int Frames => Game.Engine.CallFunction("sprite_get_number", ID);
        /// <summary>
        /// How fast does this sprite animate?
        /// </summary>
        public double AnimationSpeed => Game.Engine.CallFunction("sprite_get_speed", ID);
        /// <summary>
        /// Is this sprite animated?
        /// </summary>
        public bool Animated => Frames > 1 && AnimationSpeed != 0;
        /// <summary>
        /// The X size of this sprite.
        /// </summary>
        public int Width => Game.Engine.CallFunction("sprite_get_width", ID);
        /// <summary>
        /// The Y size of this sprite.
        /// </summary>
        public int Height => Game.Engine.CallFunction("sprite_get_height", ID);
        /// <summary>
        /// The X offset of the sprite's origin.
        /// </summary>
        public int OriginX => Game.Engine.CallFunction("sprite_get_xoffset", ID);
        /// <summary>
        /// The Y offset of the sprite's origin.
        /// </summary>
        public int OriginY => Game.Engine.CallFunction("sprite_get_yoffset", ID);

        public override string ToString()
        {
            return $@"Sprite(
    ID={ID},
    Name='{Name}',
    Frames={Frames},
    AnimationSpeed={AnimationSpeed},
    Width={Width},
    Height={Height},
    OriginX={OriginX},
    OriginY={OriginY},
)";
        }
    }

    public class SpriteDatabase : Database<int, Sprite>
    {
        public override IEnumerable<int> Keys => Cache.Keys;

        protected override Sprite? FetchNewEntry(int key)
        {
            if (Game.Engine.CallFunction("sprite_exists", key))
            {
                return new Sprite(key);
            }
            else
            {
                return null;
            }
        }
    }
}
