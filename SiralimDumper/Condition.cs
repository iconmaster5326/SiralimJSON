using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate condition definition.
    /// These are buffs, debuffs, and minions.
    /// </summary>
    public class Condition
    {
        public const int HIGHEST_COND_ID = 99;

        /// <summary>
        /// The unique ID of this realm.
        /// </summary>
        public int ID;

        public Condition(int id)
        {
            ID = id;
        }

        public static ConditionDatabase Database = [];

        public override string ToString()
        {
            return $@"Condition(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    Kind={Kind},
    Icon={Icon.ToString().Replace("\n", "\n  ")},
    ResistantIcon={ResistantIcon.ToString().Replace("\n", "\n  ")},
    Reserved={Reserved},
)";
        }

        /// <summary>
        /// The English name of this condition.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_RawConditionName", ID);
        /// <summary>
        /// The English description of this condition.
        /// </summary>
        public string Description => Game.Engine.CallScript("gml_Script_scr_ConditionDesc", ID);
        /// <summary>
        /// Is this a buff, debuff, or minion?
        /// </summary>
        public ConditionKind Kind
        {
            get
            {
                bool buff = Game.Engine.CallScript("gml_Script_scr_ConditionIsBuff", ID);
                bool debuff = Game.Engine.CallScript("gml_Script_scr_ConditionIsDebuff", ID);
                bool minion = Game.Engine.CallScript("gml_Script_scr_ConditionIsMinion", ID);

                if ((buff ? 1 : 0) + (debuff ? 1 : 0) + (minion ? 1 : 0) >= 2)
                {
                    throw new Exception($"Condition {ID} has multiple types! Buff: {buff} Debuff: {debuff} Minion: {minion}");
                }

                if (buff)
                {
                    return ConditionKind.BUFF;
                }
                else if (debuff)
                {
                    return ConditionKind.DEBUFF;
                }
                else if (minion)
                {
                    return ConditionKind.MINION;
                }
                else
                {
                    throw new Exception($"Condition {ID} not of any of the types!");
                }
            }
        }
        /// <summary>
        /// The ID of the icon for this condition.
        /// </summary>
        public int IconID => Game.Engine.CallScript("gml_Script_scr_GetConditionIcon", ID).GetSpriteID();
        /// <summary>
        /// The icon for this condition.
        /// </summary>
        public Sprite Icon => IconID.GetGMLSprite();
        /// <summary>
        /// The ID of the icon for this condition if the creature resists this condition.
        /// Same as <see cref="IconID"/> otherwise.
        /// </summary>
        public int ResistantIconID => Game.Engine.CallScript("gml_Script_scr_GetResistantConditionIcon", ID).GetSpriteID();
        /// <summary>
        /// The icon for this condition if the creature resists this condition.
        /// Same as <see cref="Icon"/> otherwise.
        /// </summary>
        public Sprite ResistantIcon => ResistantIconID.GetGMLSprite();

        private static HashSet<int>? _RandomConds;
        private static void CallGetRandom(string type)
        {
            if (_RandomConds == null) throw new Exception("Unreachable!");
            using (var tci = new TempCreatureInstance(Creature.Database[1]))
            {
                tci.Instance.GetRefInstance()["conditions"] = Game.Engine.CallFunction("array_create", 1, 0);
                //Framework.Print(type);
                GameVariable v = Game.Engine.CallScript($"gml_Script_bc_GetRandom{type}", tci.Instance, tci.Instance);
                //Framework.Print(v.PrettyPrint().EscapeNonWS());
                _RandomConds.Add(v.GetRefInstance()["cid"]);
            }
        }
        private static void InitRandomConds()
        {
            if (_RandomConds == null)
            {
                _RandomConds = [];
                for (int i = 0; i < 200; i++)
                {
                    CallGetRandom("Buff");
                    CallGetRandom("Debuff");
                    CallGetRandom("Minion");
                }
            }
        }
        /// <summary>
        /// Is this condition forbidden from being inflicted at random?
        /// </summary>
        public bool Reserved {
            get {
                InitRandomConds();
                if (_RandomConds == null) throw new Exception("Unreachable!");
                return !_RandomConds.Contains(ID);
            }
        }
    }

    public class ConditionDatabase : Database<int, Condition>
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
                    v = Game.Engine.CallScript("gml_Script_scr_RawConditionName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (i < Condition.HIGHEST_COND_ID);
            }
        }

        protected override Condition? FetchNewEntry(int key) => new Condition(key);
    }
}
