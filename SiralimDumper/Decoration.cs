using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    public class Decoration
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;

        //public int Unknown0; // Always 0.

        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the sprite this decoration uses.
        /// </summary>
        public int SpriteID;
        /// <summary>
        /// Information about this decoration's hitbox.
        /// </summary>
        public DecorationHitbox Hitbox;
        /// <summary>
        /// Is this deocration visible when you exit decoration mode?
        /// Only used for the player spawn point currently.
        /// </summary>
        public bool Visible;

        //public bool Unknown5; // Always true.

        /// <summary>
        /// The maximum number of this decoration you can have in your castle, if there is such a limit.
        /// </summary>
        public int? MaxCount;
        /// <summary>
        /// Can this decoration not appear in standard loot pools?
        /// </summary>
        public bool Reserved;
        /// <summary>
        /// What category this decoration is in. From 0 to 13.
        /// </summary>
        public DecorationCategory Category;
        /// <summary>
        /// The ID of the sound that plays when this decoration is interacted with, if any.
        /// </summary>
        public int? SoundID;

        public Decoration(int id, string name, int spriteID, DecorationHitbox hitbox, bool visible, int? maxCount, bool reserved, DecorationCategory category, int? soundID)
        {
            ID = id;
            Name = name;
            SpriteID = spriteID;
            Hitbox = hitbox;
            Visible = visible;
            MaxCount = maxCount;
            Reserved = reserved;
            Category = category;
            SoundID = soundID;
        }

        /// <summary>
        /// All decorations in the game.
        /// </summary>
        public static DecorationDatabase Database = [];

        internal static Decoration FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Decoration(
                id: id,
                name: gml[1].GetString(),
                spriteID: gml[2].GetSpriteID(),
                hitbox: (DecorationHitbox)gml[3].GetInt32(),
                visible: gml[4].GetBoolean(),
                maxCount: gml[6].GetInt32() == -1 ? null : gml[6].GetInt32(),
                reserved: !gml[7].GetBoolean(),
                category: (DecorationCategory)gml[8].GetInt32(),
                soundID: (gml[9].IsNumber() && gml[9].GetInt32() == -1) ? null : gml[9].GetSoundID()
            );
        }

        public override string ToString()
        {
            return $@"Decoration(
    ID={ID},
    Name='{Name}',
    SpriteID={SpriteID},
    Hitbox={Hitbox} ({Hitbox.Width()}x{Hitbox.Height()}),
    Visible={Visible},
    MaxCount={MaxCount},
    Reserved={Reserved},
    Category={Category.Name()},
    SoundID={SoundID},
)";
        }
    }

    public class DecorationDatabase : Database<int, Decoration>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["dec"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Decoration? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Decoration.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }
}
