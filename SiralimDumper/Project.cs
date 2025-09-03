using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate project definition.
    /// Projects are things you can embark on to unlock new features or farm for resources.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The unique ID of this project.
        /// </summary>
        public int ID;

        /// <summary>
        /// The English name of this project.
        /// </summary>
        public string Name;

        /// <summary>
        /// The English description of this project.
        /// </summary>
        public string Description;
        /// <summary>
        /// The IDs and the quantities of the <see cref="ProjectItem"/>s you need to complete this project.
        /// </summary>
        public Dictionary<int, int> ProjectItemIDs;
        /// <summary>
        /// 
        /// </summary>
        public int Unknown14;
        /// <summary>
        /// Is this project repeatable?
        /// </summary>
        public bool Repeatable;
        /// <summary>
        /// The index into the icons sprite to use for this project's icon.
        /// </summary>
        public int IconIndex;
        /// <summary>
        /// 
        /// </summary>
        public int Unknown17;

        public Project(int id, string name, string description, IEnumerable<KeyValuePair<int, int>> projectItemIDs, int unknown14, bool repeatable, int iconIndex, int unknown17)
        {
            ID = id;
            Name = name;
            Description = description;
            ProjectItemIDs = new Dictionary<int, int>(projectItemIDs);
            Unknown14 = unknown14;
            Repeatable = repeatable;
            IconIndex = iconIndex;
            Unknown17 = unknown17;
        }

        /// <summary>
        /// All the projects in the game.
        /// </summary>
        public static ProjectDatabase Database = [];

        internal static Project FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            var ids = gml.Skip(2).Take(6).Select(x => x.GetInt32()).Where(x => x >= 0).ToArray();
            var counts = gml.Skip(8).Take(ids.Length).Select(x => x.GetInt32()).ToArray();
            return new Project(
                id: id,
                name: gml[0].GetString(),
                description: gml[1].GetString(),
                projectItemIDs: ids.Zip(counts).Select(kv => new KeyValuePair<int, int>(kv.First, kv.Second)),
                unknown14: gml[14].GetInt32(),
                repeatable: gml[15].GetBoolean(),
                iconIndex: gml[16].GetInt32(),
                unknown17: gml[17].GetInt32()
            );
        }

        public override string ToString()
        {
            return $@"Project(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    ProjectItems=[{string.Join(" ; ", ProjectItems.Select(kv => $"'{kv.Key.Name}' : {kv.Value}"))}],
    Unknown14={Unknown14},
    Repeatable={Repeatable},
    Unknown16={IconIndex},
    Unknown17={Unknown17},
    GraniteRequired={GraniteRequired},
    PartsRequired={PartsRequired},
    DustRequired={DustRequired},
)";
        }

        /// <summary>
        /// The <see cref="ProjectItem"/>s you need to complete this project.
        /// </summary>
        public IReadOnlyDictionary<ProjectItem, int> ProjectItems => ProjectItemIDs.Select(kv => new KeyValuePair<ProjectItem, int>(ProjectItem.Database[kv.Key], kv.Value)).ToDictionary();
        /// <summary>
        /// The amount of granite you need to start this project.
        /// </summary>
        public int GraniteRequired => Game.Engine.CallScript("gml_Script_scr_ProjectRequiredResources", ID);
        /// <summary>
        /// The amount of monster parts you need to finish this project.
        /// </summary>
        public int PartsRequired => Game.Engine.CallScript("gml_Script_scr_ProjectRequiredParts", ID);
        /// <summary>
        /// The amount of arcane dust you need to finish this project.
        /// </summary>
        public int DustRequired => Game.Engine.CallScript("gml_Script_scr_ProjectRequiredDust", ID);
    }

    public class ProjectDatabase : Database<int, Project>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["proj"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Project? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Project.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    /// <summary>
    /// A Siralim Ultimate project item definition.
    /// Project items are NOT <see cref="Item"/>s internally.
    /// They're just a number attached to a project!
    /// </summary>
    public class ProjectItem
    {
        public const int N_ITEMS = 180;

        /// <summary>
        /// The unique ID for this item.
        /// </summary>
        public int ID;

        public ProjectItem(int iD)
        {
            ID = iD;
        }

        public static ProjectItemDatabase Database = [];

        public override string ToString()
        {
            return $@"ProjectItem(
    ID={ID},
    Name='{Name}',
    SpriteID={SpriteID},
)";
        }

        /// <summary>
        /// The English name of this item.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_ProjectItemName", ID);
        /// <summary>
        /// The ID of the sprite of this item.
        /// </summary>
        public int SpriteID => Game.Engine.CallScript("gml_Script_scr_ProjectItemSprite", ID).GetSpriteID();
    }

    public class ProjectItemDatabase : Database<int, ProjectItem>
    {
        public override IEnumerable<int> Keys => Enumerable.Range(0, ProjectItem.N_ITEMS);

        protected override ProjectItem? FetchNewEntry(int key) => new ProjectItem(key);
    }
}
