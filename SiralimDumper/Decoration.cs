using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A Siralim Ultimate decoration definition.
    /// Decorations can be placed in your castle.
    /// </summary>
    public class Decoration
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;

        //public int Unknown0; // Always 0.

        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the sprite this decoration uses.
        /// </summary>
        public int SpriteID;
        /// <summary>
        /// Information about this decoration's hitbox.
        /// </summary>
        public DecorationHitbox Hitbox;
        /// <summary>
        /// Is this deocration visible when you exit decoration mode?
        /// Only used for the player spawn point currently.
        /// </summary>
        public bool Visible;

        //public bool Unknown5; // Always true.

        /// <summary>
        /// The maximum number of this decoration you can have in your castle, if there is such a limit.
        /// </summary>
        public int? MaxCount;
        /// <summary>
        /// Can this decoration not appear in standard loot pools?
        /// </summary>
        public bool Reserved; // TODO: as of 2.0 this is now probably meaningless; verify
        /// <summary>
        /// What category this decoration is in. From 0 to 13.
        /// </summary>
        public DecorationCategory Category;
        /// <summary>
        /// The ID of the sound that plays when this decoration is interacted with, if any.
        /// </summary>
        public int? SoundID;

        public Decoration(int id, string name, int spriteID, DecorationHitbox hitbox, bool visible, int? maxCount, bool reserved, DecorationCategory category, int? soundID)
        {
            ID = id;
            Name = name;
            SpriteID = spriteID;
            Hitbox = hitbox;
            Visible = visible;
            MaxCount = maxCount;
            Reserved = reserved;
            Category = category;
            SoundID = soundID;
        }

        /// <summary>
        /// All decorations in the game.
        /// </summary>
        public static DecorationDatabase Database = [];

        internal static Decoration FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new Decoration(
                id: id,
                name: gml[1].GetString(),
                spriteID: gml[2].GetSpriteID(),
                hitbox: (DecorationHitbox)gml[3].GetInt32(),
                visible: gml[4].GetBoolean(),
                maxCount: gml[6].GetInt32() == -1 ? null : gml[6].GetInt32(),
                reserved: !gml[7].GetBoolean(),
                category: (DecorationCategory)gml[8].GetInt32(),
                soundID: (gml[9].IsNumber() && gml[9].GetInt32() == -1) ? null : gml[9].GetSoundID()
            );
        }

        public override string ToString()
        {
            return $@"Decoration(
    ID={ID},
    Name='{Name}',
    Sprite={Sprite.ToString().Replace("\n", "\n  ")},
    Hitbox={Hitbox} ({Hitbox.Width()}x{Hitbox.Height()}),
    Visible={Visible},
    MaxCount={MaxCount},
    Reserved={Reserved},
    Category={Category.Name()},
    SoundID={SoundID},
)";
        }

        /// <summary>
        /// The sprite this decoration uses.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();

        public string SpriteFilename => $@"decor\object\{Name.EscapeForFilename()}.png";

        public static readonly IReadOnlyDictionary<DecorationHitbox, QuickType.Collision> COLLIDE_MAP = new Dictionary<DecorationHitbox, QuickType.Collision>() {
            [DecorationHitbox.FOUR_BY_THREE] = QuickType.Collision.Full,
            [DecorationHitbox.THREE_BY_THREE] = QuickType.Collision.Full,
            [DecorationHitbox.MENAGERIE] = QuickType.Collision.Menagerie,
            [DecorationHitbox.THREE_BY_TWO] = QuickType.Collision.Full,
            [DecorationHitbox.NONE_TOP_PASSABLE] = QuickType.Collision.NoneTall,
            [DecorationHitbox.NONE] = QuickType.Collision.None,
            [DecorationHitbox.ONE_BY_ONE] = QuickType.Collision.Full,
            [DecorationHitbox.ONE_BY_TWO] = QuickType.Collision.Full,
            [DecorationHitbox.ONE_WIDE_TOP_PASSABLE] = QuickType.Collision.FullTall,
            [DecorationHitbox.RELIQUARY] = QuickType.Collision.Reliquary,
            [DecorationHitbox.TWO_BY_ONE] = QuickType.Collision.Full,
            [DecorationHitbox.TWO_BY_TWO] = QuickType.Collision.Full,
            [DecorationHitbox.TWO_WIDE_TOP_PASSABLE] = QuickType.Collision.FullTall,
        };
        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Decoration AsJSON => new()
        {
#nullable disable
            Category = (QuickType.Category)(int)Category,
            Collision = COLLIDE_MAP[Hitbox],
            Creator = null,
            Height = Hitbox.Height(),
            Id = ID,
            MaxCount = MaxCount,
            Name = Name,
            Notes = [],
            Sources = Reserved ? [] : [new() { Type = QuickType.SourceType.Everett }],
            Sprite = $@"images\{SpriteFilename}".Replace("\\", "/"),
            Visible = Visible,
            Width = Hitbox.Width(),
#nullable enable
        };
    }

    public class DecorationDatabase : Database<int, Decoration>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["dec"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override Decoration? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return Decoration.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    /// <summary>
    /// A Siralim Ultimate wall style definition.
    /// Your castle's tileset can be changed to fit these styles.
    /// </summary>
    public class DecorationWalls
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the tileset this decoration uses.
        /// </summary>
        public int TilesetID;

        public DecorationWalls(int id, string name, int tilesetID)
        {
            ID = id;
            Name = name;
            TilesetID = tilesetID;
        }

        /// <summary>
        /// All the wall styles in the game.
        /// </summary>
        public static DecorationWallsDatabase Database = [];

        internal static DecorationWalls FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new DecorationWalls(
                id: id,
                name: gml[0].GetString(),
                tilesetID: gml[1].GetTilesetID()
            );
        }

        public override string ToString()
        {
            return $@"DecorationWalls(
    ID={ID},
    Name='{Name}',
    Tileset={Tileset.ToString().Replace("\n", "\n  ")},
)";
        }

        /// <summary>
        /// The tileset this decoration uses.
        /// </summary>
        public Tileset Tileset => TilesetID.GetGMLTileset();

        public string SpriteFilename => $@"decor\wall\{Name.EscapeForFilename()}.png";
        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.WallStyle AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = [new() { Type = QuickType.SourceType.Everett }], // TODO
            Sprite = $@"images\{SpriteFilename}".Replace("\\", "/"),
#nullable enable
        };
    }

    public class DecorationWallsDatabase : Database<int, DecorationWalls>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["wal"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override DecorationWalls? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return DecorationWalls.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    /// <summary>
    /// A Siralim Ultimate floor style definition.
    /// Your castle's tileset can be changed to fit these styles.
    /// </summary>
    public class DecorationFloors
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the tileset this decoration uses.
        /// </summary>
        public int TilesetID;

        public DecorationFloors(int id, string name, int tilesetID)
        {
            ID = id;
            Name = name;
            TilesetID = tilesetID;
        }

        /// <summary>
        /// All the floor styles in the game.
        /// </summary>
        public static DecorationFloorsDatabase Database = [];

        internal static DecorationFloors FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new DecorationFloors(
                id: id,
                name: gml[0].GetString(),
                tilesetID: gml[1].GetTilesetID()
            );
        }

        public override string ToString()
        {
            return $@"DecorationFloors(
    ID={ID},
    Name='{Name}',
    Tileset={Tileset.ToString().Replace("\n", "\n  ")},
)";
        }

        /// <summary>
        /// The tileset this decoration uses.
        /// </summary>
        public Tileset Tileset => TilesetID.GetGMLTileset();

        public string SpriteFilename => $@"decor\floor\{Name.EscapeForFilename()}.png";
        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.FloorStyle AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = [new() { Type = QuickType.SourceType.Everett }], // TODO
            Sprite = $@"images\{SpriteFilename}".Replace("\\", "/"),
#nullable enable
        };
    }

    public class DecorationFloorsDatabase : Database<int, DecorationFloors>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["flo"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override DecorationFloors? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return DecorationFloors.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    /// <summary>
    /// A Siralim Ultimate background definition.
    /// Your castle's background can be changed to fit these styles.
    /// </summary>
    public class DecorationBackground
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the sprite this decoration uses.
        /// </summary>
        public int SpriteID;

        public DecorationBackground(int id, string name, int spriteID)
        {
            ID = id;
            Name = name;
            SpriteID = spriteID;
        }

        /// <summary>
        /// All the backgrounds in the game.
        /// </summary>
        public static DecorationBackgroundDatabase Database = [];

        internal static DecorationBackground FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new DecorationBackground(
                id: id,
                name: gml[0].GetString(),
                spriteID: gml[1].GetSpriteID()
            );
        }

        public override string ToString()
        {
            return $@"DecorationBackground(
    ID={ID},
    Name='{Name}',
    Sprite={Sprite.ToString().Replace("\n", "\n  ")},
)";
        }

        /// <summary>
        /// The sprite this decoration uses.
        /// </summary>
        public Sprite Sprite => SpriteID.GetGMLSprite();

        public string SpriteFilename => $@"decor\bg\{Name.EscapeForFilename()}.png";
        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Background AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = [new() { Type = QuickType.SourceType.Everett }], // TODO
            Sprite = $@"images\{SpriteFilename}".Replace("\\", "/"),
#nullable enable
        };
    }

    public class DecorationBackgroundDatabase : Database<int, DecorationBackground>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["bac"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override DecorationBackground? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return DecorationBackground.FromGML(key, gml);
            }
            else
            {
                return null;
            }

        }
    }

    /// <summary>
    /// A Siralim Ultimate weather effect definition.
    /// Your castle's weather effect can be changed to fit these styles.
    /// </summary>
    public class DecorationWeather
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;

        public DecorationWeather(int id, string name)
        {
            ID = id;
            Name = name;
        }

        /// <summary>
        /// All the weather effects in the game.
        /// </summary>
        public static DecorationWeatherDatabase Database = [];

        internal static DecorationWeather FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new DecorationWeather(
                id: id,
                name: gml[0].GetString()
            );
        }

        public override string ToString()
        {
            return $@"DecorationWeather(
    ID={ID},
    Name='{Name}',
)";
        }

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Weather AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = [new() { Type = QuickType.SourceType.Everett }],
#nullable enable
        };
    }

    public class DecorationWeatherDatabase : Database<int, DecorationWeather>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["wea"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override DecorationWeather? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return DecorationWeather.FromGML(key, gml);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// A Siralim Ultimate music track definition.
    /// Your castle's music can be changed to play these tracks.
    /// </summary>
    public class DecorationMusic
    {
        /// <summary>
        /// The unique ID of this decoration.
        /// </summary>
        public int ID;
        /// <summary>
        /// The English name of this decoration.
        /// </summary>
        public string Name;
        /// <summary>
        /// The ID of the sound that plays when the track begins, if any.
        /// </summary>
        public int? MusicOpeningID;
        /// <summary>
        /// The ID of the sound that consists of the main, looping part of the track.
        /// </summary>
        public int MusicLoopID;

        public DecorationMusic(int id, string name, int? musicOpeningID, int musicLoopID)
        {
            ID = id;
            Name = name;
            MusicOpeningID = musicOpeningID;
            MusicLoopID = musicLoopID;
        }

        /// <summary>
        /// All the weather effects in the game.
        /// </summary>
        public static DecorationMusicDatabase Database = [];

        internal static DecorationMusic FromGML(int id, IReadOnlyList<GameVariable> gml)
        {
            return new DecorationMusic(
                id: id,
                name: gml[0].GetString(),
                musicOpeningID: (gml[2].IsNumber() && gml[2].GetInt32() == -1) ? null : gml[1].GetSoundID(),
                musicLoopID: (gml[2].IsNumber() && gml[2].GetInt32() == -1) ? gml[1].GetSoundID() : gml[2].GetSoundID()
            );
        }

        public override string ToString()
        {
            return $@"DecorationMusic(
    ID={ID},
    Name='{Name}',
    MusicOpeningID={MusicOpeningID},
    MusicLoopID={MusicLoopID},
)";
        }

        /// <summary>
        /// Convert this to an exportable entity.
        /// </summary>
        public QuickType.Music AsJSON => new()
        {
#nullable disable
            Creator = null,
            Id = ID,
            Name = Name,
            Notes = [],
            Sources = [new() { Type = QuickType.SourceType.Everett }], // TODO
#nullable enable
        };
    }

    public class DecorationMusicDatabase : Database<int, DecorationMusic>
    {
        private IReadOnlyList<GameVariable> Array => Game.Engine.GetGlobalObject()["mus"].GetArray();

        public override IEnumerable<int> Keys => Array.Index().Where(kv => !kv.Item.IsNumber()).Select(kv => kv.Index);

        protected override DecorationMusic? FetchNewEntry(int key)
        {
            IReadOnlyList<GameVariable> gml;

            if (Array[key].TryGetArrayView(out gml))
            {
                return DecorationMusic.FromGML(key, gml);
            }
            else
            {
                return null;
            }
        }
    }
}
