using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate nether boss definition.
    /// You fight these every five floors, and see some in the main story!
    /// </summary>
    public class NetherBoss : ISiralimEntity
    {
        public const int N_BOSSES = 35;

        /// <summary>
        /// The unique ID for this nether boss.
        /// </summary>
        public int ID;

        public NetherBoss(int id)
        {
            ID = id;
        }

        /// <summary>
        /// Every nether boss in the game.
        /// </summary>
        public static NetherBossDatabase Database = [];

        public override string ToString()
        {
            return $@"NetherBoss(
    ID={ID},
    Name='{Name}',
)";
        }

        private string? _Name;
        /// <summary>
        /// The English name of this nether boss.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_NetherBossName", ID));

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.NetherBoss()
        {
#nullable disable
            Id = ID,
            Name = Name,
            Notes = [],
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;
    }

    public class NetherBossDatabase : Database<int, NetherBoss>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, NetherBoss.N_BOSSES);

        protected override NetherBoss? FetchNewEntry(int key) => new NetherBoss(key);
    }

    public class NetherBossesInfo : SiralimEntityInfo<int, NetherBoss>
    {
        public override Database<int, NetherBoss> Database => NetherBoss.Database;

        public override string Path => @"netherboss";

        public override string FieldName => "netherBosses";
    }
}
