using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    public class God
    {
        /// <summary>
        /// The unique ID for this god.
        /// </summary>
        public int ID;

        public God(int iD)
        {
            ID = iD;
        }

        public static GodDatabase Database = [];

        public override string ToString()
        {
            return $@"God(
    ID={ID},
    Name='{Name}',
    AvatarID={AvatarID},
    Trait={Trait.Database[TraitID].ToString().Replace("\n", "\n  ")},
    RealmID={RealmID},
    Title='{Title}',
    IconID={IconID},
)";
        }

        /// <summary>
        /// The English name of this god.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_GodName", ID);
        /// <summary>
        /// The ID of this god's <see cref="Creature"/> avatar.
        /// </summary>
        public int AvatarID
        {
            get
            {
                var name = Name;
                return Creature.Database.Values.First(c => c.Name.Equals(name)).ID;
            }
        }
        /// <summary>
        /// The ID of the <see cref="Trait"/> this god possesses in its Gate of the Gods fight.
        /// </summary>
        public int TraitID => Game.Engine.CallScript("gml_Script_scr_GodBossTrait", ID);
        /// <summary>
        /// The ID of the <see cref="Realm"/> of this god, if any.
        /// </summary>
        public int? RealmID
        {
            get
            {
                int biome = Game.Engine.CallScript("gml_Script_scr_GodBiome", ID);
                return biome <= 0 ? null : biome;
            }
        }
        /// <summary>
        /// The English title of this god.
        /// </summary>
        public string Title => Regex.Match(Game.Engine.CallScript("gml_Script_scr_GodBossName", ID), "^\\[[^\\]]*\\] [^,]*, (.*)$").Groups[1].Value;
        /// <summary>
        /// The ID of the icon of this god.
        /// </summary>
        public int IconID => Regex.Match(Game.Engine.CallScript("gml_Script_scr_GodBossName", ID), "^\\[([^\\]]*)\\]").Groups[1].Value.GetGMLAssetID();
    }

    public class GodDatabase : Database<int, God>
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
                    v = Game.Engine.CallScript("gml_Script_scr_GodName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (v.Length > 0);
            }
        }

        protected override God? FetchNewEntry(int key) => new God(key);
    }
}
