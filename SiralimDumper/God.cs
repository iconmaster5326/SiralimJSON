using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    public class God
    {
        /// <summary>
        /// The unique ID for this god.
        /// </summary>
        public int ID;

        public God(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the gods in the game.
        /// </summary>
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
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    EmblemIcon={EmblemIcon?.ToString()?.Replace("\n", "\n  ")},
    UltimateSpell={UltimateSpellID} ({UltimateSpell.Name}),
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
        /// <summary>
        /// The icon of this god.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();
        /// <summary>
        /// The ID of the sprite for this god's emblem.
        /// </summary>
        public int? EmblemIconID
        {
            get
            {
                GameVariable v = Game.Engine.CallScript("gml_Script_inv_GetEmblemIconByGod", ID);
                if (v.Type.Equals("undefined") || v.IsNumber() && v < 0)
                {
                    return null;
                }
                return v.GetSpriteID();
            }
        }

        /// <summary>
        /// The ID of the sprite for this god's emblem.
        /// </summary>
        public Sprite? EmblemIcon => EmblemIconID?.GetGMLSprite();
        /// <summary>
        /// The ID of the <see cref="Spell"/> this god's Avatar can cast.
        /// </summary>
        public int UltimateSpellID => Game.Engine.CallScript("gml_Script_scr_GetUltimateSpellByRelic", ID);
        /// <summary>
        /// The <see cref="Spell"/> this god's Avatar can cast.
        /// </summary>
        public Spell UltimateSpell => Spell.Database[UltimateSpellID];
        /// <summary>
        /// The <see cref="Relic"/> associated with this god.
        /// </summary>
        public Relic Relic => Relic.Database[ID];
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
