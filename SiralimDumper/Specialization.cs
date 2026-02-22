using YYTKInterop;
using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate specialization definition.
    /// These are "classes" the player can be. Has perks.
    /// </summary>
    public class Specialization : ISiralimEntity
    {
        public const int HIGHEST_COND_ID = 99;

        /// <summary>
        /// The unique ID of this specialization.
        /// </summary>
        public int ID;

        public Specialization(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the specializations in the game.
        /// </summary>
        public static SpecializationDatabase Database = [];

        public override string ToString()
        {
            return $@"Specialization(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    Playstyle='{Playstyle}',
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    Sprite={Sprite.ToString().Replace("\n", "\n  ")},
    PrimaryCreatureID={PrimaryCreatureID} ({Creature.Database[PrimaryCreatureID].Name}),
    SecondaryCreatureID={SecondaryCreatureID} ({Creature.Database[SecondaryCreatureID].Name}),
    SpellID={SpellID} ({Spell.Database[SpellID].Name}),
    Perks=['{string.Join("', '", Perks.Select(x => x.Name))}'],
    AscendedPerkID={AscendedPerkID} ({AscendedPerk.Name}),
)";
        }

        private string? _Name;
        private static readonly IReadOnlyDictionary<string, string> MANUAL_NAME_FIXUPS = new Dictionary<string, string> {
            ["L_WITCHDOCTOR"] = "L_WITCH_DOCTOR",
            ["L_HELLKNIGHT"] = "L_HELL_KNIGHT",
            ["L_RUNEKNIGHT"] = "L_RUNE_KNIGHT",
        };
        /// <summary>
        /// The English name of this specialization.
        /// Some name translation keys are messed up. We fix them for you.
        /// </summary>
        public string Name
        {
            get
            {
                if (_Name == null)
                {
                    _Name = Game.Engine.CallScript("gml_Script_scr_SpecializationName", ID);
                    if (MANUAL_NAME_FIXUPS.ContainsKey(_Name))
                    {
                        _Name = MANUAL_NAME_FIXUPS[_Name].SUTranslate();
                    }
                }
                return _Name;
            }
        }

        private string? _Description;
        /// <summary>
        /// The first part of the English description of this specialization.
        /// </summary>
        public string Description => _Description ?? (_Description = Game.Engine.CallScript("gml_Script_scr_SpecializationDesc", ID));

        private string? _Playstyle;
        /// <summary>
        /// The second part of the English description of this specialization.
        /// </summary>
        public string Playstyle => _Playstyle ?? (_Playstyle = Game.Engine.CallScript("gml_Script_scr_SpecializationPlaystyle", ID));

        private int? _IconID;
        /// <summary>
        /// The ID of the icon for this specialization.
        /// </summary>
        public int IconID => _IconID ?? (_IconID = Game.Engine.CallScript("gml_Script_scr_SpecializationIcon", ID).GetSpriteID()).Value;

        /// <summary>
        /// The icon for this specialization.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();

        private int? _SpriteID;
        /// <summary>
        /// The ID of the sprite for the initial costume for this specialization.
        /// </summary>
        public int SpriteID => _SpriteID ?? (_SpriteID = Game.Engine.CallScript("gml_Script_scr_SpecializationCostume", ID).GetSpriteID()).Value;

        /// <summary>
        /// The sprite for the initial costume for this specialization.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();

        private int? _PrimaryCreatureID;
        /// <summary>
        /// The ID of the first <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="SecondaryCreatureID"/>.
        /// </summary>
        public int PrimaryCreatureID => _PrimaryCreatureID ?? (_PrimaryCreatureID = Game.Engine.CallScript("gml_Script_scr_SpecializationStartingCreature", ID)).Value;

        private int? _SecondaryCreatureID;
        /// <summary>
        /// The ID of the second <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="PrimaryCreatureID"/>.
        /// </summary>
        public int SecondaryCreatureID => _SecondaryCreatureID ?? (_SecondaryCreatureID = Game.Engine.CallScript("gml_Script_scr_SpecializationSecondaryCreature", ID)).Value;

        private int? _SpellID;
        /// <summary>
        /// The ID of the <see cref="Spell"> this specialization starts the game with.
        /// </summary>
        public int SpellID => _SpellID ?? (_SpellID = Game.Engine.CallScript("gml_Script_scr_SpecializationSpell", ID)).Value;

        private int[]? _PerkIDs;
        /// <summary>
        /// The IDs of the perks this specialization can access.
        /// </summary>
        public int[] PerkIDs => _PerkIDs ?? (_PerkIDs = Game.Engine.CallScript("gml_Script_scr_PerkGetPerkList", ID).GetArray().Skip(1).Select(x => x.GetInt32()).ToArray());

        /// <summary>
        /// The perks this specialization can access.
        /// </summary>
        public Perk[] Perks => PerkIDs.Select(x => Perk.Database[x]).ToArray();

        private int? _AscendedPerkID;
        /// <summary>
        /// The ID of the perk you get when you ascend this specialization.
        /// </summary>
        public int AscendedPerkID => _AscendedPerkID ?? (_AscendedPerkID = Game.Engine.CallScript("gml_Script_scr_AscensionPerk", ID)).Value;

        /// <summary>
        /// The perk you get when you ascend this specialization.
        /// </summary>
        public Perk AscendedPerk => Perk.Database[AscendedPerkID];

        /// <summary>
        /// The costume you get at a certain tier, from 1-3.
        /// </summary>
        public Costume? CostumeAtTier(int tier) => Costume.Database.Values.FirstOrDefault(c => c.Name.Equals($"{Name} (Tier {tier})"));

        public void MapImages(Dictionary<string, List<SiralimDumper.ImageInfo>> mappings)
        {
            mappings.GetAndAppend(Icon.Name, new ImageInfo(0, IconFilename));
        }

        private Costume[]? _Costumes;
        /// <summary>
        /// The costumes you get at all 3 tiers.
        /// </summary>
        public Costume[] Costumes => _Costumes ?? (_Costumes = Enumerable.Range(1, 3).Select(i => CostumeAtTier(i)).OfType<Costume>().ToArray());

        public string IconFilename => $@"{SiralimEntityInfo.SPECS.Path}\{Name.EscapeForFilename()}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Specialization()
        {
#nullable disable
            Advanced = false, // TODO
            AscendedPerk = AscendedPerkID,
            Costumes = Costumes.Select(c => (long)c.ID).ToArray(),
            Creator = null,
            Creatures = [PrimaryCreatureID, SecondaryCreatureID],
            Decoration = 0, // TODO
            Description = [Description, Playstyle],
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            Name = Name,
            Notes = [],
            Perks = Perks.Select(p => (long)p.ID).ToArray(),
            Project = Project.Database.Values.First(p => p.Name.EndsWith(Name)).ID,
            Skin = 0, // TODO
            Spell = SpellID,
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;

        public QuickType.ShopItemType ShopItemType => throw new NotImplementedException();
    }

    public class SpecializationDatabase : Database<int, Specialization>
    {
        public override IEnumerable<int> Keys
        {
            get
            {
                int i = 0;
                GameVariable v;
                do
                {
                    i++;
                    v = Game.Engine.CallScript("gml_Script_scr_SpecializationIcon", i);
                    if (!v.Type.Equals("undefined"))
                    {
                        yield return i;
                    }
                } while (!v.Type.Equals("undefined"));
            }
        }

        protected override Specialization? FetchNewEntry(int key) => new Specialization(key);
    }

    public class SpecialzationsInfo : SiralimEntityInfo<int, Specialization>
    {
        public override Database<int, Specialization> Database => Specialization.Database;

        public override string Path => @"spec";

        public override string FieldName => "specializations";
    }

    /// <summary>
    /// A Siralim Ultimate perk definition.
    /// </summary>
    public class Perk : ISiralimEntity
    {
        /// <summary>
        /// The unique ID of this perk.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this perk.
        /// </summary>
        public string Name;
        /// <summary>
        /// The English description of this perk.
        /// </summary>
        public string Description;
        /// <summary>
        /// The number of ranks this perk has.
        /// </summary>
        public int MaxRanks;

        //public int Unknown3; // Always -1
        //public int Unknown4; // always 0
        //public int Unknown5; // always 1

        /// <summary>
        /// How many perk points it takes to increase the rank of this perk by 1.
        /// </summary>
        public int PointsPerRank;
        /// <summary>
        /// The ID of the sprite for this perk's icon.
        /// </summary>
        public int IconID;

        public Perk(int id, string name, string description, int maxRanks, int pointsPerRank, int iconID)
        {
            ID = id;
            Name = name;
            Description = description;
            MaxRanks = maxRanks;
            PointsPerRank = pointsPerRank;
            IconID = iconID;
        }

        /// <summary>
        /// All the perks in the game.
        /// </summary>
        public static PerkDatabase Database = [];

        internal static Perk FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Perk(
                id: id,
                name: gml[0].GetString(),
                description: gml[1].GetString(),
                maxRanks: gml[2].GetInt32(),
                pointsPerRank: gml[6].GetInt32(),
                iconID: gml[7].GetSpriteID()
            );
        }
        
        public override string ToString()
        {
            return $@"Perk(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    MaxRanks={MaxRanks},
    PointsPerRank={PointsPerRank},
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    Specialization={Specialization.Name},
    IsAnointable={IsAnointable},
)";
        }

        public void MapImages(Dictionary<string, List<ImageInfo>> mappings)
        {
            mappings.GetAndAppend(Icon.Name, new ImageInfo(0, IconFilename));
        }

        /// <summary>
        /// The <see cref="Specialization"/> this perk belongs to.
        /// </summary>
        public Specialization Specialization => Specialization.Database.Values.First(s => s.AscendedPerkID == ID || s.PerkIDs.Contains(ID));

        private bool? _IsAnointable;
        /// <summary>
        /// Is this perk anointable?
        /// </summary>
        public bool IsAnointable => _IsAnointable ?? (_IsAnointable = Game.Engine.CallScript("gml_Script_scr_AnointmentGetSpecialization", ID).GetInt32() >= 1).Value;

        /// <summary>
        /// The sprite for this perk's icon.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();

        public string IconFilename => $@"{SiralimEntityInfo.PERKS.Path}\{Name.EscapeForFilename()}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Perk()
        {
#nullable disable
            Anointable = IsAnointable,
            Description = Description,
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Id = ID,
            MaxRanks = MaxRanks,
            Name = Name,
            Notes = [],
            PointsPerRank = PointsPerRank,
            Specialization = Specialization.ID,
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;

        public QuickType.ShopItemType ShopItemType => throw new NotImplementedException();
    }

    public class PerkDatabase : Database<int, Perk>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["rdb"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Perk? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Perk.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    public class PerksInfo : SiralimEntityInfo<int, Perk>
    {
        public override Database<int, Perk> Database => Perk.Database;

        public override string Path => @"perk";

        public override string FieldName => "perks";
    }
}
