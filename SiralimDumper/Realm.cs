using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate realm definition.
    /// A realm is a place you go to in the game.
    /// </summary>
    public class Realm
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

        /// <summary>
        /// The English name of this realm.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_BiomeName", ID);
        /// <summary>
        /// The ID of the <see cref="God"/> that this realm belongs to.
        /// </summary>
        public int GodID => Game.Engine.CallScript("gml_Script_scr_BiomeGod", ID);
        /// <summary>
        /// The sprite ID for a chest in this realm.
        /// </summary>
        public int ChestSpriteID
        {
            get
            {
                using (new TempRealm(this))
                {
                    return Game.Engine.GetGlobalObject()["spritechest"].GetSpriteID();
                }
            }
        }
        /// <summary>
        /// The sprite ID for a default breakable in this realm.
        /// </summary>
        public int DefaultBreakableSpriteID
        {
            get
            {
                using (new TempRealm(this))
                {
                    return Game.Engine.GetGlobalObject()["breakable1"].GetSpriteID();
                }
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

        /// <summary>
        /// The English reward text for a certain devotion rank, between 1 and 100.
        /// </summary>
        public string Blessing(int rank) => Regex.Match(Game.Engine.CallScript("gml_Script_scr_BlessingName", God.ID, rank), "^\\[[^\\]]*\\] (.*)$").Groups[1].Value;
        /// <summary>
        /// The icon for this devotion rank, between 1 and 100, if any.
        /// </summary>
        public Sprite BlessingIcon(int rank)
        {
            string icon = Regex.Match(Game.Engine.CallScript("gml_Script_scr_BlessingName", God.ID, rank), "^\\[([^\\]]*)\\] .*$").Groups[1].Value;
            if (icon.Equals("ob_monlith"))
            {
                // fixing a typo from Zack!
                icon = "ot_monolith";
            }
            return icon.GetGMLSprite();
        }

        public string BlessingIconFilename(int rank) => $@"blessing\{BlessingIcon(rank).Name.EscapeForFilename()}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Realm AsJSON => new()
        {
#nullable disable
            Creator = null,
            EmblemIcon = $@"images\{God.EmblemIconFilename}".Replace("\\", "/"),
            God = GodID,
            GodBlessings = Enumerable.Range(1, 100).Select(rank => new QuickType.GodBlessing() {
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

    /// <summary>
    /// A Siralim Ultimate realm property definition.
    /// At higher realm instabilities, these begin to apply.
    /// </summary>
    public class RealmProperty
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

        internal static string FullName(int id)
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
            return Game.Engine.CallScript("gml_Script_scr_RealmPropertyName", id, arg).GetString().Replace("[carnage] Carnage", "{1}").Replace("[stat_g_savage] Savage", "{1}");
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

        public string IconFilename => $@"realmprop\{ID}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.RealmProperty AsJSON => new()
        {
#nullable disable
            Description = Name,
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            Notes = [],
            Reserved = false, // TODO
#nullable enable
        };
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
}
