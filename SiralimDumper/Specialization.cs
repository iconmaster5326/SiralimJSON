using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate specialization definition.
    /// These are "classes" the player can be. Has perks.
    /// </summary>
    public class Specialization
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

        public static SpecializationDatabase Database = [];

        public override string ToString()
        {
            return $@"Specialization(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
    Playstyle='{Playstyle}',
    IconID={IconID},
    SpriteID={SpriteID},
    PrimaryCreatureID={PrimaryCreatureID} ({Creature.Database[PrimaryCreatureID].Name}),
    SecondaryCreatureID={SecondaryCreatureID} ({Creature.Database[SecondaryCreatureID].Name}),
    SpellID={SpellID} ({Spell.Database[SpellID].Name}),
)";
        }

        /// <summary>
        /// The English name of this specialization.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_SpecializationName", ID);
        /// <summary>
        /// The first part of the English description of this specialization.
        /// </summary>
        public string Description => Game.Engine.CallScript("gml_Script_scr_SpecializationDesc", ID);
        /// <summary>
        /// The second part of the English description of this specialization.
        /// </summary>
        public string Playstyle => Game.Engine.CallScript("gml_Script_scr_SpecializationPlaystyle", ID);
        /// <summary>
        /// The ID of the icon for this specialization.
        /// </summary>
        public int IconID => Game.Engine.CallScript("gml_Script_scr_SpecializationIcon", ID).GetSpriteID();
        /// <summary>
        /// The ID of the sprite for the initial costume for this specialization.
        /// </summary>
        public int SpriteID => Game.Engine.CallScript("gml_Script_scr_SpecializationCostume", ID).GetSpriteID();
        /// <summary>
        /// The ID of the first <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="SecondaryCreatureID"/>.
        /// </summary>
        public int PrimaryCreatureID => Game.Engine.CallScript("gml_Script_scr_SpecializationStartingCreature", ID);
        /// <summary>
        /// The ID of the second <see cref="Creature"> this specialization starts the game with.
        /// Also see <see cref="PrimaryCreatureID"/>.
        /// </summary>
        public int SecondaryCreatureID => Game.Engine.CallScript("gml_Script_scr_SpecializationSecondaryCreature", ID);
        /// <summary>
        /// The ID of the <see cref="Spell"> this specialization starts the game with.
        /// </summary>
        public int SpellID => Game.Engine.CallScript("gml_Script_scr_SpecializationSpell", ID);
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
}
