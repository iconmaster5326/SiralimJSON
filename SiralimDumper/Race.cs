using System.Data;
using System.Text.RegularExpressions;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate race definition.
    /// </summary>
    public class Race : ISiralimEntity
    {
        internal static readonly string[] UNOBTAINABLE_RACES = ["False Godspawn", "Relic"];
        internal static readonly string[] MASTERLESS_RACES = ["Avatar", "Purrghast", "Tanukrook", "Mogwai", "Exotic", "Animatus", "Herbling", "False Godspawn", "Guardian", "Relic"];
        internal static readonly string[] RESERVED_RACES = ["Exotic", "Godspawn", "Avatar", "Animatus", "Herbling", "Guardian"];
        internal static readonly string[] MANALESS_RACES = ["Dumpling", "Godspawn", "Avatar", "Animatus", "Herbling", "Guardian", "Purrghast", "Tanukrook", "Mogwai"];

        /// <summary>
        /// The English name of this race.
        /// </summary>
        public string Name;

        public Race(string name)
        {
            Name = name;
        }

        /// <summary>
        /// All the races in the game.
        /// </summary>
        public static RaceDatabase Database = [];

        public override string ToString()
        {
            string master = "(none)";
            if (HasMaster)
            {
                master = $@"Master(
        Trait={MasterTrait.Name},
        Name='{MasterName}',
        Dialogue='{MasterDialogue}',
        CostumeID={MasterCostumeID},
    )";
            }
            return $@"Race(
    Name={Name},
    Creatures=[{string.Join(", ", Creatures.Select(c => c.Name))}],
    CardSetID={CardSetID},
    NumCards={NumCards},
    CardsRequiredForBonuses=[{string.Join(", ", CardsRequiredForBonuses)}],
    CardBonusDescriptions=['{string.Join("', '", CardBonusDescriptions)}'],
    Class={Class},
    Master={master},
    BossDialogue=['{string.Join("', '", BossDialogue)}'],
    Icon={Icon?.ToString()?.Replace("\n", "\n  ")},
)";
        }

        /// <summary>
        /// Returns all creatures that are part of this race.
        /// </summary>
        public IEnumerable<Creature> Creatures => Creature.Database.Values.Where(c => c.RaceName.Equals(Name));

        /// <summary>
        /// The trait this race's Rodian Master's sigil gives.
        /// </summary>
        public Trait MasterTrait => Trait.Database[MasterTraitID];

        private bool _SetCardSetID;
        private int? _CardSetID;
        /// <summary>
        /// The ID of the card set this race has, or null if this race does not have cards.
        /// </summary>
        public int? CardSetID
        {
            get
            {
                if (!_SetCardSetID)
                {
                    _SetCardSetID = true;
                    _CardSetID = Game.Engine.GetGlobalObject()["card_races"].GetArray()
                        .Index()
                        .Where(kv => Name.Equals(kv.Item.GetString()))
                        .Select(kv => kv.Index)
                        .Cast<int?>()
                    .FirstOrDefault();
                }
                return _CardSetID;
            }
        }

        private int? _NumCards;
        /// <summary>
        /// The number of cards in this race's card set.
        /// 0 if this race does not have cards.
        /// </summary>
        public int NumCards
        {
            get
            {
                if (_NumCards == null)
                {
                    int? id = CardSetID;

                    if (id == null)
                    {
                        _NumCards = 0;
                    }
                    else
                    {
                        _NumCards = Game.Engine.GetGlobalObject()["card_count"].GetArray()[id.Value];
                    }
                }
                return _NumCards.Value;
            }
        }

        /// <summary>
        /// Does this race have a card set?
        /// </summary>
        public bool HasCards => CardSetID != null;

        /// <summary>
        /// How many card bonuses does this card set have?
        /// This is currently 3 in all cases this race has cards.
        /// </summary>
        public int NumCardBonuses => HasCards ? 3 : 0;

        /// <summary>
        /// How many cards does it take to reach a certain card bonus?
        /// </summary>
        /// <param name="i">The card bonus, from 1 to <see cref="NumCardBonuses"/>.</param>
        /// <returns>A number of cards.</returns>
        public int CardsRequiredForBonus(int i)
        {
            var n = NumCards;
            switch (i)
            {
                case 1: return (int)MathF.Floor(n / 3f);
                case 2: return (int)MathF.Floor(n * (2f / 3f));
                case 3: return n;
                default: throw new Exception($"Card bonus out of bounds: {i}");
            }
        }

        /// <summary>
        /// The number of cards required for all card bonuses this race has.
        /// </summary>
        public int[] CardsRequiredForBonuses => HasCards ? [CardsRequiredForBonus(1), CardsRequiredForBonus(2), CardsRequiredForBonus(3)] : [];

        /// <summary>
        /// Gets the English text of a card bonus in this race's card set.
        /// Use <see cref="HasCards"/> to check if this string is valid!
        /// </summary>
        /// <param name="i">The card bonus, from 1 to <see cref="NumCardBonuses"/>.</param>
        /// <returns>The English text of a card bonus, or a translation key if not defined.</returns>
        public string CardBonusDescription(int i)
        {
            return Game.Engine.CallScript("gml_Script_inv_CardSetBonus", Name, i);
        }

        private string[]? _CardBonusDescriptions;
        /// <summary>
        /// The English descriptions of all card bonuses this race has.
        /// </summary>
        public string[] CardBonusDescriptions => _CardBonusDescriptions ?? (_CardBonusDescriptions = HasCards ? [CardBonusDescription(1), CardBonusDescription(2), CardBonusDescription(3)] : []);

        public SiralimClass? _Class;
        /// <summary>
        /// The general class of this race.
        /// Not all members of this race may belong to this class!
        /// </summary>
        public SiralimClass Class => _Class ?? (_Class = EnumUtil.ClassFromString(Game.Engine.CallScript("gml_Script_scr_GetClassOfRace", Name))).Value;

        /// <summary>
        /// Does this race have a Rodian Master?
        /// </summary>
        public bool HasMaster => !MASTERLESS_RACES.Contains(Name);

        private int? _MasterTraitID;
        /// <summary>
        /// The ID of the <see cref="Trait"/> that this race's Rodian Master gives you.
        /// </summary>
        public int MasterTraitID => _MasterTraitID ?? (_MasterTraitID = Game.Engine.CallScript("gml_Script_scr_MasterTrait", Name)).Value;

        private string? _MasterName;
        /// <summary>
        /// The English name of this race's Rodian Master.
        /// </summary>
        public string MasterName => _MasterName ?? (_MasterName = Game.Engine.CallScript("gml_Script_scr_MasterName", Name));

        private string? _MasterDialogue;
        /// <summary>
        /// What this race's Rodian Master says to you before you fight them.
        /// </summary>
        public string MasterDialogue => _MasterDialogue ?? (_MasterDialogue = Game.Engine.CallScript("gml_Script_scr_MasterDialog", Name));

        private int? _MasterCostumeID;
        /// <summary>
        /// The ID of the costume of this race's Rodian Master.
        /// </summary>
        public int MasterCostumeID => _MasterCostumeID ?? (_MasterCostumeID = Game.Engine.CallScript("gml_Script_scr_MasterCostumeID", Name)).Value;

        private const int MAX_RANDOM_TRIES = 100;
        private string[]? _BossDialogue;
        /// <summary>
        /// The possible lines of dialogue that may appear when you face creatures of this race in mini-boss rooms.
        /// This has a probabalistic chance to fail to return all possible results.
        /// Increase <see cref="MAX_RANDOM_TRIES"/> if this happens to you.
        /// </summary>
        public string[] BossDialogue
        {
            get
            {
                if (_BossDialogue == null)
                {
                    HashSet<string> result = new HashSet<string>();
                    for (int i = 0; i < MAX_RANDOM_TRIES; i++)
                    {
                        result.Add(Game.Engine.CallScript("gml_Script_scr_BossGetDialog", Name));
                    }
                    _BossDialogue = result.Order().ToArray();
                }
                return _BossDialogue;
            }
        }

        private bool _SetIconID;
        private int? _IconID;
        /// <summary>
        /// The sprite ID of the icon associated with this race.
        /// </summary>
        public int? IconID
        {
            get
            {
                if (!_SetIconID)
                {
                    _SetIconID = true;
                    Creature? creature = Creatures.FirstOrDefault<Creature?>();
                    if (creature == null)
                    {
                        _IconID = null;
                    }
                    else
                    {
                        using (var tci = new TempCreatureInstance(creature))
                        {
                            string iconString = Game.Engine.CallScript("gml_Script_scr_CritIcon", tci.Instance, true);
                            var match = Regex.Match(iconString, "^\\[(.*)\\]");
                            if (!match.Success || match.Groups[1].Value.Length == 0)
                            {
                                _IconID = null;
                            }
                            else
                            {
                                _IconID = match.Groups[1].Value.GetGMLAssetID();
                            }
                        }
                    }
                }
                return _IconID;
            }
        }

        /// <summary>
        /// The sprite of the icon associated with this race.
        /// </summary>
        public Sprite? Icon => IconID?.GetGMLSprite();

        /// <summary>
        /// Are all creatures of this race obtainable under normal gameplay?
        /// </summary>
        public bool Obtainable => !UNOBTAINABLE_RACES.Contains(Name);

        /// <summary>
        /// Can all creatures of this race not appear in normal enemy packs spawns?
        /// </summary>
        public bool Reserved => RESERVED_RACES.Contains(Name) || !Obtainable;

        /// <summary>
        /// Can all creatures of this race drop mana?
        /// </summary>
        public bool GivesMana => !Obtainable ? false : !MANALESS_RACES.Contains(Name);

        /// <summary>
        /// The file path to the icon image.
        /// </summary>
        public string IconFilename => $@"{SiralimEntityInfo.RACES.Path}\{Name.EscapeForFilename()}.png";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Race()
        {
#nullable disable
            BossDialog = BossDialogue.All(d => d.Length == 0) ? [] : BossDialogue.ToArray(),
            CardBonuses = HasCards ? CardsRequiredForBonuses.Index().Select(kv => new KeyValuePair<string, string>(kv.Item.ToString(), CardBonusDescriptions[kv.Index])).DistinctBy(kv => kv.Key).ToDictionary() : [],
            Cards = NumCards,
            Class = Enum.Parse<QuickType.Class>(EnumUtil.Name(Class)),
            Creator = null,
            Creatures = Creatures.Select(c => (long)c.ID).ToArray(),
            Icon = $@"images\{IconFilename}".Replace("\\", "/"),
            Master = HasMaster ? new()
            {
                Costume = MasterCostumeID,
                Dialog = MasterDialogue,
                Item = ItemMaterialTrait.Database.Values.First(i => i.TraitID == MasterTraitID).ID,
                Name = MasterName,
                Trait = MasterTraitID,
            } : null,
            Name = Name,
            Notes = [],
            Skins = Skin.Database.Values.Where(s => Name.Equals(s.RaceName)).Select(s => (long)s.ID).ToArray(),
#nullable enable
        };
        object ISiralimEntity.Key => Name;
        string ISiralimEntity.Name => Name;
    }

    public class RaceDatabase : Database<string, Race>
    {
        public override IEnumerable<string> Keys => Game.Engine.GetGlobalObject()["races_all"].GetArray().Where(v => !v.IsNumber()).Select(v => v.GetString());

        protected override Race? FetchNewEntry(string key) => new Race(key);
    }

    public class RacesInfo : SiralimEntityInfo<string, Race>
    {
        public override Database<string, Race> Database => Race.Database;

        public override string Path => @"race";

        public override string FieldName => "races";
    }
}
