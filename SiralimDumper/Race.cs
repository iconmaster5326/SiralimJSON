using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate race definition.
    /// </summary>
    public class Race
    {
        internal static readonly string[] UNOBTAINABLE_RACES = ["False Godspawn", "Relic"];
        internal static readonly string[] MASTERLESS_RACES = ["Avatar", "Purrghast", "Tanukrook", "Mogwai", "Exotic", "Animatus", "Herbling", "False Godspawn", "Guardian", "Relic"];

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
    Class='{Class}',
    Master={master},
    BossDialogue=['{string.Join("', '", BossDialogue)}'],
    IconID={IconID},
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

        /// <summary>
        /// The ID of the card set this race has, or null if this race does not have cards.
        /// </summary>
        public int? CardSetID => Game.Engine.GetGlobalObject()["card_races"].GetArray()
            .Index()
            .Where(kv => Name.Equals(kv.Item.GetString()))
            .Select(kv => kv.Index)
            .Cast<int?>()
        .FirstOrDefault();

        /// <summary>
        /// The number of cards in this race's card set.
        /// 0 if this race does not have cards.
        /// </summary>
        public int NumCards
        {
            get
            {
                int? id = CardSetID;

                if (id == null)
                {
                    return 0;
                }
                else
                {
                    return Game.Engine.GetGlobalObject()["card_count"].GetArray()[id.Value];
                }
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

        /// <summary>
        /// The English descriptions of all card bonuses this race has.
        /// </summary>
        public string[] CardBonusDescriptions => HasCards ? [CardBonusDescription(1), CardBonusDescription(2), CardBonusDescription(3)] : [];

        /// <summary>
        /// The general class of this race.
        /// Not all members of this race may belong to this class!
        /// </summary>
        public string Class => Game.Engine.CallScript("gml_Script_scr_GetClassOfRace", Name);

        /// <summary>
        /// Does this race have a Rodian Master?
        /// </summary>
        public bool HasMaster => !MASTERLESS_RACES.Contains(Name);
        /// <summary>
        /// The ID of the <see cref="Trait"/> that this race's Rodian Master gives you.
        /// </summary>
        public int MasterTraitID => Game.Engine.CallScript("gml_Script_scr_MasterTrait", Name);
        /// <summary>
        /// The English name of this race's Rodian Master.
        /// </summary>
        public string MasterName => Game.Engine.CallScript("gml_Script_scr_MasterName", Name);
        /// <summary>
        /// What this race's Rodian Master says to you before you fight them.
        /// </summary>
        public string MasterDialogue => Game.Engine.CallScript("gml_Script_scr_MasterDialog", Name);
        /// <summary>
        /// The ID of the costume of this race's Rodian Master.
        /// </summary>
        public int MasterCostumeID => Game.Engine.CallScript("gml_Script_scr_MasterCostumeID", Name);
        private const int MAX_RANDOM_TRIES = 100;
        /// <summary>
        /// The possible lines of dialogue that may appear when you face creatures of this race in mini-boss rooms.
        /// This has a probabalistic chance to fail to return all possible results.
        /// Increase <see cref="MAX_RANDOM_TRIES"/> if this happens to you.
        /// </summary>
        public List<string> BossDialogue
        {
            get
            {
                HashSet<string> result = new HashSet<string>();
                for (int i = 0; i < MAX_RANDOM_TRIES; i++)
                {
                    result.Add(Game.Engine.CallScript("gml_Script_scr_BossGetDialog", Name));
                }
                return result.Order().ToList();
            }
        }
        /// <summary>
        /// The sprite ID of the icon associated with this race.
        /// </summary>
        public int? IconID
        {
            get
            {
                Creature? creature = Creatures.FirstOrDefault<Creature?>();
                if (creature == null)
                {
                    return null;
                }
                using (var tci = new TempCreatureInstance(creature))
                {
                    string iconString = Game.Engine.CallScript("gml_Script_scr_CritIcon", tci.Instance, true);
                    var match = Regex.Match(iconString, "^\\[(.*)\\]");
                    if (!match.Success || match.Groups[1].Value.Length == 0)
                    {
                        return null;
                    }
                    return match.Groups[1].Value.GetGMLAssetID();
                }
            }
        }
    }

    public class RaceDatabase : Database<string, Race>
    {
        public override IEnumerable<string> Keys => Game.Engine.GetGlobalObject()["races_all"].GetArray().Where(v => !v.IsNumber()).Select(v => v.GetString());

        protected override Race? FetchNewEntry(string key) => new Race(key);
    }
}
