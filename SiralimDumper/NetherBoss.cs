using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate nether boss definition.
    /// You fight these every five floors, and see some in the main story!
    /// </summary>
    public class NetherBoss
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

        /// <summary>
        /// The English name of this nether boss.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_NetherBossName", ID);

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.NetherBoss AsJSON => new()
        {
#nullable disable
            Id = ID,
            Name = Name,
            Notes = [],
#nullable enable
        };
    }

    public class NetherBossDatabase : Database<int, NetherBoss>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, NetherBoss.N_BOSSES);

        protected override NetherBoss? FetchNewEntry(int key) => new NetherBoss(key);
    }
}
