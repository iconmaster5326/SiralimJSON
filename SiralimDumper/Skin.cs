using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate skin definition.
    /// Skins are a special sprite some creatures can wear, once you unlock them.
    /// </summary>
    public class Skin
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

        /// <summary>
        /// Can this skin not appear on wild creatures?
        /// </summary>
        public bool Reserved => Game.Engine.CallScript("gml_Script_inv_SkinReserved", ID);

        /// <summary>
        /// The sprite this skin gives to the creature when in battle.
        /// See also <see cref="BattleSpriteIndex"/> for the correct frame.
        /// </summary>
        public Sprite BattleSprite => "spr_crits_battle".GetGMLSprite();
        /// <summary>
        /// The sprite this skin gives to the creature when in the overworld.
        /// </summary>
        public Sprite OverworldSprite => OverworldSpriteID.GetGMLSprite();
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
}
