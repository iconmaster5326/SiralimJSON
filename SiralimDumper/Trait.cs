using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate trait definition.
    /// </summary>
    public class Trait
    {
        /// <summary>
        /// The unique ID of this trait.
        /// </summary>
        public int ID;

        /// <summary>
        /// The English name of this trait.
        /// </summary>
        public string Name;

        /// <summary>
        /// The English description of this trait.
        /// </summary>
        public string Description;

        public Trait(
            int id,
            string name,
            string description
        )
        {
            ID = id;
            Name = name;
            Description = description;
        }

        private static Dictionary<int, Trait>? _Database;
        /// <summary>
        /// All the traits in the game.
        /// </summary>
        public static Dictionary<int, Trait> Database
        {
            get
            {
                if (_Database == null) { InitDatabase(); }
                return _Database ?? throw new Exception("Database not initialized!");
            }
        }
        private static void InitDatabase()
        {
            _Database = Game.Engine.GetGlobalObject()["passive"].GetArray().Select<GameVariable, KeyValuePair<int, Trait>?>((v, i) =>
            {
                Framework.Print($"[SiralimDumper]   Parsing trait {i}...");
                return v.IsNumber() ? null : new KeyValuePair<int, Trait>(i, FromGML(i, v.GetArray()));
            }).OfType<KeyValuePair<int, Trait>>().ToDictionary();
        }

        public static Trait FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Trait(
                id,
                gml[0].GetString(),
                gml[1].GetString()
            );
        }

        public override string ToString()
        {
            return $@"Trait(
    ID={ID},
    Name='{Name}',
    Description='{Description}',
)";
        }

        /// <summary>
        /// The creature this trait is associated with, if any.
        /// </summary>
        public Creature? Creature => Creature.Database.Values.FirstOrDefault(c => c.TraitID == ID);
        /// <summary>
        /// The Rodian Master this trait is associated with, if any.
        /// </summary>
        public Race? Race => Race.Database.Values.FirstOrDefault(c => c.HasMaster && c.MasterTraitID == ID);
    }
}
