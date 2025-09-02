using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    public class Personality
    {
        public const int N_PERSONALITIES = 20;
        /// <summary>
        /// The unique ID of this personality.
        /// </summary>
        public int ID;
        public Personality(int id)
        {
            ID = id;
        }
        /// <summary>
        /// The database of all personalities.
        /// </summary>
        public static PersonalityDatabase Database = [];
        public override string ToString()
        {
            return $@"Personality(
    ID={ID},
    Name='{Name}',
    Increases={Increases},
    Decreases={Decreases},
)";
        }
        /// <summary>
        /// The English name of this personality.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_PersonalityString", ID);
        /// <summary>
        /// The stat that is increased by this personality.
        /// </summary>
        public Stat Increases => EnumUtil.StatFromString(Regex.Match(Game.Engine.CallScript("gml_Script_scr_PersonalityStatIncrease", ID), "\\[.*\\]\\[.*\\] (.*)\\[.*\\] \\[.*\\]").Groups[1].Value);
        /// <summary>
        /// The stat that is decreased by this personality.
        /// </summary>
        public Stat Decreases => EnumUtil.StatFromString(Regex.Match(Game.Engine.CallScript("gml_Script_scr_PersonalityStatDecrease", ID), "\\[.*\\]\\[.*\\] (.*)\\[.*\\] \\[.*\\]").Groups[1].Value);
    }
    public class PersonalityDatabase : Database<int, Personality>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, Personality.N_PERSONALITIES);

        protected override Personality? FetchNewEntry(int key) => new Personality(key);
    }
}
