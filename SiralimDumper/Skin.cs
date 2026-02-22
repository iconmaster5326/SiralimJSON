using YYTKInterop;
using static SiralimDumper.SiralimDumper;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate skin definition.
    /// Skins are a special sprite some creatures can wear, once you unlock them.
    /// </summary>
    public class Skin : ISiralimEntity
    {
        /// <summary>
        /// The unique ID of this skin.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this skin.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the creature this skin applies to, if any.
        /// Also see <see cref="RaceName"/>.
        /// </summary>
        public int? CreatureID;
        /// <summary>
        /// The ID of the race this skin applies to, if any.
        /// Also see <see cref="CreatureID"/>.
        /// </summary>
        public string? RaceName;
        /// <summary>
        /// The frame of the battle sprite this skin uses.
        /// </summary>
        public int BattleSpriteIndex;
        /// <summary>
        /// The ID of the sprite this skin gives to the creature when in the overworld.
        /// </summary>
        public int OverworldSpriteID;

        public Skin(int id, string name, int? creatureID, string? raceName, int battleSpriteID, int overworldSpriteID)
        {
            ID = id;
            Name = name;
            CreatureID = creatureID;
            RaceName = raceName;
            BattleSpriteIndex = battleSpriteID;
            OverworldSpriteID = overworldSpriteID;
        }



        /// <summary>
        /// All the creatures in the game.
        /// </summary>
        public static SkinDatabase Database = [];

        internal static Skin FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Skin(
                id: id,
                name: gml[0].ToString(),
                creatureID: gml[1].IsNumber() ? gml[1].ToInt32() : null,
                raceName: gml[1].IsNumber() ? null : gml[1].ToString(),
                battleSpriteID: gml[2].GetSpriteID(),
                overworldSpriteID: gml[3].GetSpriteID()
            );
        }

        public override string ToString()
        {
            return $@"Skin(
    ID={ID},
    Name={Name},
    Creature={(CreatureID == null ? "null" : Creature.Database[CreatureID.Value].Name)},
    Race={RaceName},
    BattleSpriteIndex={BattleSpriteIndex},
    OverworldSprite={OverworldSprite.ToString().Replace("\n", "\n  ")},
    Reserved={Reserved},
)";
        }

        public void MapImages(Dictionary<string, List<SiralimDumper.ImageInfo>> mappings)
        {
            mappings.GetAndAppend(BattleSprite.Name, new ImageInfo(BattleSpriteIndex, BattleSpriteFilename));
            mappings.GetAndAppend(OverworldSprite.Name, values: ImagesForOWSprite(OverworldSprite, OverworldSpriteFilenamePrefix));
        }

        private bool? _Reserved;
        /// <summary>
        /// Can this skin not appear on wild creatures?
        /// </summary>
        public bool Reserved => _Reserved ?? (_Reserved = Game.Engine.CallScript("gml_Script_inv_SkinReserved", ID)).Value;

        private static Sprite? _BattleSprite;
        /// <summary>
        /// The sprite this skin gives to the creature when in battle.
        /// See also <see cref="BattleSpriteIndex"/> for the correct frame.
        /// </summary>
        public Sprite BattleSprite => _BattleSprite ?? (_BattleSprite = "spr_crits_battle".GetGMLSprite());

        /// <summary>
        /// The sprite this skin gives to the creature when in the overworld.
        /// </summary>
        public Sprite OverworldSprite => OverworldSpriteID.GetGMLSprite();

        public string BattleSpriteFilename => @$"{SiralimEntityInfo.SKINS.Path}\{Name.EscapeForFilename()}\battle.png";
        public string OverworldSpriteFilenamePrefix => $@"{SiralimEntityInfo.SKINS.Path}\{Name.EscapeForFilename()}\overworld";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        object ISiralimEntity.AsJSON => new QuickType.Skin()
        {
#nullable disable
            BattleSprite = $@"images\{BattleSpriteFilename}".Replace("\\", "/"),
            Creator = null,
            Creature = CreatureID,
            Id = ID,
            Name = Name,
            Notes = [],
            OverworldSprite = SiralimDumper.OverworldSpriteJSON(OverworldSprite, OverworldSpriteFilenamePrefix),
            Race = RaceName,
            Reserved = Reserved,
            Sources = Reserved ? [] : [new() { Type = QuickType.SourceType.Random }],
#nullable enable
        };
        object ISiralimEntity.Key => ID;
        string ISiralimEntity.Name => Name;

        public QuickType.ShopItemType ShopItemType => QuickType.ShopItemType.Skin;
    }

    public class SkinDatabase : Database<int, Skin>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["skin"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Skin? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Skin.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    public class SkinsInfo : SiralimEntityInfo<int, Skin>
    {
        public override Database<int, Skin> Database => Skin.Database;

        public override string Path => @"skin";

        public override string FieldName => "skins";
    }
}
