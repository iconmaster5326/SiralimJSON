using System.Text.RegularExpressions;
using YYTKInterop;
using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    public class God : ISiralimEntity
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

        public void MapImages(Dictionary<string, List<SiralimDumper.ImageInfo>> mappings)
        {
            mappings.GetAndAppend(Icon.Name, new ImageInfo(0, IconFilename));
            if (EmblemIcon != null)
            {
                mappings.GetAndAppend(EmblemIcon.Name, new ImageInfo(0, EmblemIconFilename));
            }

            var relic = Relic;
            mappings.GetAndAppend(relic.BigIcon.Name, new ImageInfo(0, relic.BigIconFilename));
            mappings.GetAndAppend(relic.SmallIcon.Name, new ImageInfo(0, relic.SmallIconFilename));
        }

        private string? _Name;
        /// <summary>
        /// The English name of this god.
        /// </summary>
        public string Name => _Name ?? (_Name = Game.Engine.CallScript("gml_Script_scr_GodName", ID));

        private int? _AvatarID;
        /// <summary>
        /// The ID of this god's <see cref="Creature"/> avatar.
        /// </summary>
        public int AvatarID => _AvatarID ?? (_AvatarID = Creature.Database.Values.First(c => c.Name.Equals(Name)).ID).Value;

        public int? _TraitID;
        /// <summary>
        /// The ID of the <see cref="Trait"/> this god possesses in its Gate of the Gods fight.
        /// </summary>
        public int TraitID => _TraitID ?? (_TraitID = Game.Engine.CallScript("gml_Script_scr_GodBossTrait", ID)).Value;

        private bool _SetRealmID;
        private int? _RealmID;
        /// <summary>
        /// The ID of the <see cref="Realm"/> of this god, if any.
        /// </summary>
        public int? RealmID
        {
            get
            {
                if (!_SetRealmID)
                {
                    _SetRealmID = true;
                    int biome = Game.Engine.CallScript("gml_Script_scr_GodBiome", ID);
                    _RealmID = biome <= 0 ? null : biome;
                }
                return _RealmID;
            }
        }

        private string? _FullName;
        private string FullName => _FullName ?? (_FullName = Game.Engine.CallScript("gml_Script_scr_GodBossName", ID));

        private string? _Title;
        /// <summary>
        /// The English title of this god.
        /// </summary>
        public string Title => _Title ?? (_Title = Regex.Match(FullName, "^\\[[^\\]]*\\] [^,]*, (.*)$").Groups[1].Value);

        private int? _IconID;
        /// <summary>
        /// The ID of the icon of this god.
        /// </summary>
        public int IconID => _IconID ?? (_IconID = Regex.Match(FullName, "^\\[([^\\]]*)\\]").Groups[1].Value.GetGMLAssetID()).Value;

        /// <summary>
        /// The icon of this god.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();

        public bool _SetEmblemIconID;
        public int? _EmblemIconID;
        /// <summary>
        /// The ID of the sprite for this god's emblem.
        /// </summary>
        public int? EmblemIconID
        {
            get
            {
                if (!_SetEmblemIconID)
                {
                    _SetEmblemIconID = true;
                    GameVariable v = Game.Engine.CallScript("gml_Script_inv_GetEmblemIconByGod", ID);
                    if (v.Type.Equals("undefined") || v.IsNumber() && v < 0)
                    {
                        _EmblemIconID = null;
                    } else
                    {
                        _EmblemIconID = v.GetSpriteID();
                    }
                }
                return _EmblemIconID;
            }
        }

        /// <summary>
        /// The ID of the sprite for this god's emblem.
        /// </summary>
        public Sprite? EmblemIcon => EmblemIconID?.GetGMLSprite();

        public int? _UltimateSpellID;
        /// <summary>
        /// The ID of the <see cref="Spell"/> this god's Avatar can cast.
        /// </summary>
        public int UltimateSpellID => _UltimateSpellID ?? (_UltimateSpellID = Game.Engine.CallScript("gml_Script_scr_GetUltimateSpellByRelic", ID)).Value;

        /// <summary>
        /// The <see cref="Spell"/> this god's Avatar can cast.
        /// </summary>
        public Spell UltimateSpell => Spell.Database[UltimateSpellID];

        /// <summary>
        /// The <see cref="Relic"/> associated with this god.
        /// </summary>
        public Relic Relic => Relic.Database[ID];

        public string IconFilename => $@"{SiralimEntityInfo.GODS.Path}\{Name.EscapeForFilename()}.png";
        public string EmblemIconFilename => $@"item\emblem\{Name.EscapeForFilename()}.png";
        public string NameForTranslationKeys => ID == 28 ? "ROBO" : Name.EscapeForFilename().ToUpper();

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
       object ISiralimEntity.AsJSON => new QuickType.God()
        {
#nullable disable
            Avatar = AvatarID,
            Creator = null,
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            Name = Name,
            Notes = [],
            Realm = RealmID,
            RelicBigIcon = $@"images\{Relic.BigIconFilename}".Replace("\\", "/"),
            RelicSmallIcon = $@"images\{Relic.SmallIconFilename}".Replace("\\", "/"),
            RelicBonuses = Relic.Bonuses,
            RelicName = Relic.Name,
            RelicStat = Enum.Parse<QuickType.Stat>(Enum.GetName(Relic.Stat), true),
            RelicTitle = Relic.Title,
            Title = Title,
            Trait = TraitID,
            UltimateSpell = UltimateSpellID,
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;

        public QuickType.ShopItemType ShopItemType => throw new NotImplementedException();
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

    public class GodsInfo : SiralimEntityInfo<int, God>
    {
        public override Database<int, God> Database => God.Database;

        public override string Path => @"god";

        public override string FieldName => "gods";
    }
}
