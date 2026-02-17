using System.Text.RegularExpressions;
using YYTKInterop;
using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    public class Personality : ISiralimEntity
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
    TomeIconIndex={TomeIconIndex},
)";
        }

        public void MapImages(Dictionary<string, List<SiralimDumper.ImageInfo>> mappings)
        {
            mappings.GetAndAppend("icons", new ImageInfo(TomeIconIndex, TomeIconFilename));
        }

        private string? _Name;
        /// <summary>
        /// The English name of this personality.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_PersonalityString", ID));

        private Stat? _Increases;
        /// <summary>
        /// The stat that is increased by this personality.
        /// </summary>
        public Stat Increases => _Increases ?? (_Increases = EnumUtil.StatFromString(Regex.Match(Game.Engine.CallScript("gml_Script_scr_PersonalityStatIncrease", ID), "\\[.*\\]\\[.*\\] (.*)\\[.*\\] \\[.*\\]").Groups[1].Value)).Value;

        private Stat? _Decreases;
        /// <summary>
        /// The stat that is decreased by this personality.
        /// </summary>
        public Stat Decreases => _Decreases ?? (_Decreases = EnumUtil.StatFromString(Regex.Match(Game.Engine.CallScript("gml_Script_scr_PersonalityStatDecrease", ID), "\\[.*\\]\\[.*\\] (.*)\\[.*\\] \\[.*\\]").Groups[1].Value)).Value;

        /// <summary>
        /// The index into <tt>icons</tt> for the tome for this personality.
        /// </summary>
        public int TomeIconIndex => 2101 + ID;

        public string TomeIconFilename => $@"item\tome\{Name.EscapeForFilename()}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Personality()
        {
#nullable disable
            TomeIcon = $@"images\{TomeIconFilename}".Replace("\\", "/"),
            Decreases = Enum.Parse<QuickType.Stat>(Enum.GetName(Decreases), true),
            Increases = Enum.Parse<QuickType.Stat>(Enum.GetName(Increases), true),
            Id = ID,
            Name = Name,
            Notes = [],
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;
    }

    public class PersonalityDatabase : Database<int, Personality>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, Personality.N_PERSONALITIES);

        protected override Personality? FetchNewEntry(int key) => new Personality(key);
    }

    public class PersonalitiesInfo : SiralimEntityInfo<int, Personality>
    {
        public override Database<int, Personality> Database => Personality.Database;

        public override string Path => @"personality";

        public override string FieldName => "personalities";
    }
}
