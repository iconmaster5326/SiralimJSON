using System.Text.RegularExpressions;
using YYTKInterop;
using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate realm definition.
    /// A realm is a place you go to in the game.
    /// </summary>
    public class Realm : ISiralimEntity
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

        private string? _Name;
        /// <summary>
        /// The English name of this realm.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_BiomeName", ID));

        private int? _GodID;
        /// <summary>
        /// The ID of the <see cref="God"/> that this realm belongs to.
        /// </summary>
        public int GodID => _GodID ?? (_GodID = Game.Engine.CallScript("gml_Script_scr_BiomeGod", ID)).Value;

        private int? _ChestSpriteID;
        /// <summary>
        /// The sprite ID for a chest in this realm.
        /// </summary>
        public int ChestSpriteID
        {
            get
            {
                if (_ChestSpriteID == null)
                    using (new TempRealm(this))
                    {
                        _ChestSpriteID = Game.Engine.GetGlobalObject()["spritechest"].GetSpriteID();
                    }
                return _ChestSpriteID.Value;
            }
        }

        private int? _DefaultBreakableSpriteID;
        /// <summary>
        /// The sprite ID for a default breakable in this realm.
        /// </summary>
        public int DefaultBreakableSpriteID
        {
            get
            {
                if (_DefaultBreakableSpriteID == null)
                    using (new TempRealm(this))
                    {
                        _DefaultBreakableSpriteID = Game.Engine.GetGlobalObject()["breakable1"].GetSpriteID();
                    }
                return _DefaultBreakableSpriteID.Value;
            }
        }

        /// <summary>
        /// The emblem icon for this realm.
        /// </summary>
        public Sprite EmblemIcon => God.EmblemIcon ?? throw new Exception("God has realm but no emblem icon!");

        /// <summary>
        /// The god this realm is associated with.
        /// </summary>
        public God God => God.Database[GodID];

        private Dictionary<int, string> _Blessings = [];
        private string FullBlessing(int rank) => _Blessings.GetValueOrDefault(rank) ?? (_Blessings[rank] = Game.Engine.CallScript("gml_Script_scr_BlessingName", God.ID, rank));

        /// <summary>
        /// The English reward text for a certain devotion rank, between 1 and 100.
        /// </summary>
        public string Blessing(int rank) => Regex.Match(FullBlessing(rank), "^\\[[^\\]]*\\] (.*)$").Groups[1].Value;

        /// <summary>
        /// The icon for this devotion rank, between 1 and 100, if any.
        /// </summary>
        public Sprite BlessingIcon(int rank)
        {
            string icon = Regex.Match(FullBlessing(rank), "^\\[([^\\]]*)\\] .*$").Groups[1].Value;
            if (icon.Equals("ob_monlith"))
            {
                // fixing a typo from Zack!
                icon = "ot_monolith";
            }
            return icon.GetGMLSprite();
        }

        public string BlessingIconFilename(int rank) => $@"blessing\{BlessingIcon(rank).Name.EscapeForFilename()}.png";

        public void MapImages(Dictionary<string, List<SiralimDumper.ImageInfo>> mappings)
        {
            for (var i = 1; i <= 100; i++)
            {
                string icon = BlessingIcon(i).Name;
                if (!mappings.ContainsKey(icon))
                {
                    mappings.GetAndAppend(icon, new ImageInfo(0, BlessingIconFilename(i)));
                }
            }
        }

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Realm()
        {
#nullable disable
            Creator = null,
            EmblemIcon = $@"images\{God.EmblemIconFilename}".Replace("\\", "/"),
            God = GodID,
            GodBlessings = Enumerable.Range(1, 100).Select(rank => new QuickType.GodBlessing()
            {
                Blessing = Blessing(rank),
                Icon = $@"images\{BlessingIconFilename(rank)}".Replace("\\", "/"),
            }).ToArray(),
            GodShop = [], // TODO
            Id = ID,
            Name = Name,
            Notes = [],
            UnlockedAtDepth = null, // TODO
            Project = Project.Database.Values.FirstOrDefault(p => p.Name.EndsWith(Name))?.ID,
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;
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

    public class RealmsInfo : SiralimEntityInfo<int, Realm>
    {
        public override Database<int, Realm> Database => Realm.Database;

        public override string Path => @"realm";

        public override string FieldName => "realms";
    }

    /// <summary>
    /// A Siralim Ultimate realm property definition.
    /// At higher realm instabilities, these begin to apply.
    /// </summary>
    public class RealmProperty : ISiralimEntity
    {

        /// <summary>
        /// The unique ID of this realm property.
        /// </summary>
        public int ID;

        public RealmProperty(int id)
        {
            ID = id;
        }

        public static RealmPropertyDatabase Database = [];

        public override string ToString()
        {
            return $@"RealmProperty(
    ID={ID},
    Name='{Name}',
    Icon={Icon?.ToString()?.Replace("\n", "\n  ")},
)";
        }

        private static Dictionary<int, string> _FullName = [];
        internal static string FullName(int id)
        {
            if (!_FullName.ContainsKey(id))
            {
                GameVariable arg;
                switch (id)
                {
                    case 16:
                        arg = "Carnage";
                        break;
                    case 25:
                    case 26:
                    case 38:
                        arg = 1;
                        break;
                    default:
                        arg = "{1}";
                        break;
                }
                _FullName[id] = Game.Engine.CallScript("gml_Script_scr_RealmPropertyName", id, arg).GetString().Replace("[carnage] Carnage", "{1}").Replace("[stat_g_savage] Savage", "{1}");
            }
            return _FullName[id];
        }

        public void MapImages(Dictionary<string, List<ImageInfo>> mappings)
        {
            Sprite? icon = Icon;
            if (icon != null)
            {
                mappings.GetAndAppend(icon.Name, new ImageInfo(0, IconFilename));
            }
        }

        /// <summary>
        /// The English name of this realm property.
        /// </summary>
        public string Name => Regex.Match(FullName(ID), "^\\[[^\\]]*\\] (.*)$").Groups[1].Value;

        /// <summary>
        /// The ID of the icon sprite for this realm property.
        /// </summary>
        public int? IconID => Regex.Match(FullName(ID), "^\\[([^\\]]*)\\] .*$").Groups[1].Value.GetGMLAssetIDOrNull();

        /// <summary>
        /// The icon sprite for this realm property.
        /// </summary>
        public Sprite? Icon => IconID?.GetGMLSprite();

        public string IconFilename => $@"{SiralimEntityInfo.REALM_PROPS.Path}\{ID}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.RealmProperty()
        {
#nullable disable
            Description = Name,
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            Notes = [],
            Reserved = false, // TODO
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => ID.ToString();
    }

    public class RealmPropertyDatabase : Database<int, RealmProperty>
    {
        public override IEnumerable<int> Keys
        {
            get
            {
                int i = 0;
                string v = "";
                do
                {
                    i++;
                    v = RealmProperty.FullName(i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (v.Length > 0);
            }
        }

        protected override RealmProperty? FetchNewEntry(int key) => new RealmProperty(key);
    }

    public class RealmPropsInfo : SiralimEntityInfo<int, RealmProperty>
    {
        public override Database<int, RealmProperty> Database => RealmProperty.Database;

        public override string Path => @"realmprop";

        public override string FieldName => "realmProperties";
    }
}
