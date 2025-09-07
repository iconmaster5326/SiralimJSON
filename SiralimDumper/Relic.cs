using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate relic definition.
    /// A relic is tied to a god, and you may give it to your creatures for benefits.
    /// </summary>
    public class Relic
    {
        /// <summary>
        /// The unique ID for this relic.
        /// </summary>
        public int ID;

        public Relic(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the relics in the game.
        /// </summary>
        public static RelicDatabase Database = [];

        public override string ToString()
        {
            return $@"Relic(
    ID={ID},
    Name='{Name}',
    Sprite={BigIcon.ToString().Replace("\n", "\n  ")},
    Icon={SmallIcon.ToString().Replace("\n", "\n  ")},
    Bonuses=['{string.Join("', '", Bonuses)}'],
    Stat={Stat},
)";
        }

        private string? _Name;
        /// <summary>
        /// The English name of this relic.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_RelicName", ID));

        private int? _SpriteID;
        /// <summary>
        /// The ID of the sprite for this relic.
        /// </summary>
        public int SpriteID => _SpriteID ?? (_SpriteID = Game.Engine.CallScript("gml_Script_scr_RelicBigIcon", ID).GetSpriteID()).Value;

        /// <summary>
        /// The sprite for this relic.
        /// </summary>
        public Sprite BigIcon => SpriteID.GetGMLSprite();

        private int? _IconID;
        /// <summary>
        /// The ID of the icon for this relic.
        /// </summary>
        public int IconID => _IconID ?? (_IconID = Game.Engine.CallScript("gml_Script_scr_RelicMenuIcon", ID).GetSpriteID()).Value;

        /// <summary>
        /// The icon for this relic.
        /// </summary>
        public Sprite SmallIcon => IconID.GetGMLSprite();

        private string[]? _Bonuses;
        /// <summary>
        /// The English text of this relic's bonuses. 10 items long.
        /// </summary>
        public string[] Bonuses => _Bonuses ?? (_Bonuses = Enumerable.Range(1, 10).Select(i => Game.Engine.CallScript("gml_Script_scr_RelicBonusText", ID, i).GetString()).ToArray());

        private Stat? _Stat;
        /// <summary>
        /// The stat this relic increases.
        /// </summary>
        public Stat Stat => _Stat ?? (_Stat = EnumUtil.StatFromString(Game.Engine.CallScript("gml_Script_scr_RelicStat", ID))).Value;

        /// <summary>
        /// The <see cref="God"/> associated with this relic.
        /// </summary>
        public God God => God.Database[ID];

        private string? _Title;
        /// <summary>
        /// The English title given to this relic.
        /// </summary>
        public string Title => _Title ?? (_Title = Regex.Match($"L_RELIC_{God.NameForTranslationKeys}_EXT".SUTranslate(), "^[^,]+, (.+)$").Groups[1].Value);

        public string BigIconFilename => $@"relic\{Name.EscapeForFilename()}\relic.png";
        public string SmallIconFilename => $@"relic\{Name.EscapeForFilename()}\icon.png";
    }

    public class RelicDatabase : Database<int, Relic>
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

        protected override Relic? FetchNewEntry(int key) => new Relic(key);
    }
}
