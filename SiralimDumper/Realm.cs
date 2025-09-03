using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate realm definition.
    /// A realm is a place you go to in the game.
    /// </summary>
    public class Realm
    {
        public const int HIGHEST_REALM_ID = 40;

        /// <summary>
        /// The unique ID of this realm.
        /// </summary>
        public int ID;

        public Realm(int id)
        {
            ID = id;
        }

        public static RealmDatabase Database = [];

        public override string ToString()
        {
            return $@"Realm(
    ID={ID},
    Name='{Name}',
    God='{God.Database[GodID].Name}',
    ChestSpriteID={ChestSpriteID},
    DefaultBreakableSpriteID={DefaultBreakableSpriteID},
)";
        }

        /// <summary>
        /// The English name of this realm.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_BiomeName", ID);
        /// <summary>
        /// The ID of the <see cref="God"/> that this realm belongs to.
        /// </summary>
        public int GodID => Game.Engine.CallScript("gml_Script_scr_BiomeGod", ID);
        /// <summary>
        /// The sprite ID for a chest in this realm.
        /// </summary>
        public int ChestSpriteID
        {
            get
            {
                using (new TempRealm(this))
                {
                    return Game.Engine.GetGlobalObject()["spritechest"].GetSpriteID();
                }
            }
        }
        /// <summary>
        /// The sprite ID for a default breakable in this realm.
        /// </summary>
        public int DefaultBreakableSpriteID
        {
            get
            {
                using (new TempRealm(this))
                {
                    return Game.Engine.GetGlobalObject()["breakable1"].GetSpriteID();
                }
            }
        }
    }

    public class RealmDatabase : Database<int, Realm>
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
                    v = Game.Engine.CallScript("gml_Script_scr_BiomeName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (i < Realm.HIGHEST_REALM_ID);
            }
        }

        protected override Realm? FetchNewEntry(int key) => new Realm(key);
    }
}
