using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    public interface ISiralimEntity : IShopItem
    {
        /// <summary>
        /// Convert this entity to an object that is JSON serialzable.
        /// </summary>
        public object AsJSON { get; }

        /// <summary>
        /// The unique key of this entity.
        /// Usually its ID, but may be its name.
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// The name of this entity,
        /// or else a short identifiable string representing it.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Generate any needed sprite mappings for an entity.
        /// </summary>
        /// <param name="mappings">A map of sprite names to sprite information for you to mutate.</param>
        public abstract void MapImages(Dictionary<string, List<ImageInfo>> mappings);
    }

    public abstract class SiralimEntityInfo
    {
        /// <summary>
        /// The path fragment for this entity.
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// The name of the field in `combined.json` and `individual.json`,
        /// and the file name in `aggregate.json`.
        /// </summary>
        public abstract string FieldName { get; }

        /// <summary>
        /// Get all keys in the database.
        /// </summary>
        public abstract IEnumerable<object> Keys { get; }

        /// <summary>
        /// Get a entity for a particular key.
        /// </summary>
        public abstract ISiralimEntity GetEntity(object key);

        public T[] AllAsJSON<T>() => Keys.Select(k => GetEntity(k).AsJSON).Cast<T>().ToArray();
        public string IndividualFilePath(ISiralimEntity e) => $@"{Path}\{e.Name.EscapeForFilename()}.json";

        public static readonly AccessoriesInfo ACCESSORIES = new();
        public static readonly ArtifactsInfo ARTIFACTS = new();
        public static readonly BackgroundsInfo BGS = new();
        public static readonly ConditionsInfo CONDITIONS = new();
        public static readonly CostumesInfo COSTUMES = new();
        public static readonly CreaturesInfo CREATURES = new();
        public static readonly DecorationsInfo DECOR = new();
        public static readonly FalseGodsInfo FALSE_GODS = new();
        public static readonly FloorsInfo FLOORS = new();
        public static readonly GodsInfo GODS = new();
        public static readonly MaterialsInfo MATERIALS = new();
        public static readonly MusicInfo MUSIC = new();
        public static readonly NetherBossesInfo NETHER_BOSSES = new();
        public static readonly PerksInfo PERKS = new();
        public static readonly PersonalitiesInfo PERSONALITIES = new();
        public static readonly ProjectItemsInfo PROJECT_ITEMS = new();
        public static readonly ProjectsInfo PROJECTS = new();
        public static readonly RacesInfo RACES = new();
        public static readonly RealmPropsInfo REALM_PROPS = new();
        public static readonly RealmsInfo REALMS = new();
        public static readonly RunesInfo RUNES = new();
        public static readonly SkinsInfo SKINS = new();
        public static readonly SpecialzationsInfo SPECS = new();
        public static readonly SpellPropItemsInfo SPELLPROP_ITEMS = new();
        public static readonly SpellPropsInfo SPELL_PROPS = new();
        public static readonly SpellsInfo SPELLS = new();
        public static readonly TraitsInfo TRAITS = new();
        public static readonly WallsInfo WALLS = new();
        public static readonly WeatherInfo WEATHER = new();

        public static readonly SiralimEntityInfo[] ALL = [
            ACCESSORIES,
            ARTIFACTS,
            BGS,
            CONDITIONS,
            COSTUMES,
            CREATURES,
            DECOR,
            FALSE_GODS,
            FLOORS,
            GODS,
            MATERIALS,
            MUSIC,
            NETHER_BOSSES,
            PERKS,
            PERSONALITIES,
            PROJECT_ITEMS,
            PROJECTS,
            RACES,
            REALM_PROPS,
            REALMS,
            RUNES,
            SKINS,
            SPECS,
            SPELL_PROPS,
            SPELLPROP_ITEMS,
            SPELLS,
            TRAITS,
            WALLS,
            WEATHER,
        ];
    }

    public abstract class SiralimEntityInfo<K, V> : SiralimEntityInfo where K : notnull where V : ISiralimEntity
    {
        public abstract Database<K, V> Database { get; }
        public override IEnumerable<object> Keys => Database.Keys.Cast<object>();

        public override ISiralimEntity GetEntity(object key) => Database[(K)key];
    }
}
