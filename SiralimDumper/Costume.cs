using QuickType;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate costume definition.
    /// A costume is a skin you can apply to NPCs and to yourself, in the overworld only.
    /// </summary>
    public class Costume
    {
        /// <summary>
        /// The unique ID of this costume.
        /// </summary>
        public int ID;

        public Costume(int id)
        {
            ID = id;
        }

        /// <summary>
        /// All the costumes in the game.
        /// </summary>
        public static CostumeDatabase Database = [];

        public override string ToString()
        {
            return $@"Costume(
    ID={ID},
    Name='{Name}',
    Reserved={Reserved},
    Sprite={Sprite.ToString().Replace("\n", "\n  ")},
)";
        }

        /// <summary>
        /// The English name of this costume.
        /// </summary>
        public string Name => Game.Engine.CallScript("gml_Script_scr_WardrobeName", ID);
        /// <summary>
        /// Can this costume not drop through the normal loot pool?
        /// </summary>
        public bool Reserved => Game.Engine.CallScript("gml_Script_scr_WardrobeReserved", ID); // TODO: as of 2.0, this function is no longer being maintained
        /// <summary>
        /// The ID of this costume's sprite.
        /// </summary>
        public int SpriteID => Game.Engine.CallScript("gml_Script_scr_WardrobeSprite", ID).GetSpriteID();
        /// <summary>
        /// This costume's sprite.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();

        public string SpriteFilenamePrefix => $@"costume\{Name.EscapeForFilename()}\overworld";

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Costume AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = Reserved ? [] : [new() { Type = QuickType.SourceType.Everett }],
            Sprite = SiralimDumper.OverworldSpriteJSON(Sprite, SpriteFilenamePrefix),
#nullable enable
        };
    }

    public class CostumeDatabase : Database<int, Costume>
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
                    v = Game.Engine.CallScript("gml_Script_scr_WardrobeName", i);
                    if (v.Length > 0)
                    {
                        yield return i;
                    }
                } while (v.Length > 0);
            }
        }

        protected override Costume? FetchNewEntry(int key) => new Costume(key);
    }
}
