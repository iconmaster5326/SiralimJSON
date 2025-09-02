using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// A handler to automatically wrap around temporarily created <see cref="GameInstance"/>s.
    /// Automatically handles the lifecycle, creating and destroying the instance.
    /// </summary>
    internal abstract class TempInstance : IDisposable
    {
        public GameVariable Instance;
        public TempInstance(GameVariable instance)
        {
            Instance = instance;
        }
        void IDisposable.Dispose()
        {
            Game.Engine.CallFunction("instance_destroy", Instance);
        }
    }

    /// <summary>
    /// A temporary instance of a particular Siralim Ultimate creature.
    /// </summary>
    internal class TempCreatureInstance : TempInstance
    {
        public TempCreatureInstance(Creature creature) : base(Game.Engine.CallScript("gml_Script_scr_Creature", creature.ID, 0, 0)) { }
    }

    /// <summary>
    /// A temporary instance of a particular Siralim Ultimate spell gem property material.
    /// </summary>
    internal class TempItemSpellPropertyInstance : TempInstance
    {
        public TempItemSpellPropertyInstance(ItemSpellProperty item) : base(Game.Engine.CallScript("gml_Script_inv_CreateDust", item.ID)) { }
    }

    /// <summary>
    /// A temporary instance of a particular Siralim Ultimate stat/trick/trait material.
    /// </summary>
    internal class TempItemMaterialInstance : TempInstance
    {
        public TempItemMaterialInstance(ItemMaterial item) : base(Game.Engine.CallScript("gml_Script_inv_CreateMaterial", item.ID)) { }
    }

    /// <summary>
    /// A temporary instance of a particular Siralim Ultimate artifact.
    /// </summary>
    internal class TempItemArtifact : TempInstance
    {
        public TempItemArtifact(ItemArtifact item) : base(Game.Engine.CallScript("gml_Script_inv_CreateArtifact", item.ID)) { }
    }
}
