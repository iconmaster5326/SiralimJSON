using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    /// <summary>
    /// An instance of a particular Siralim Ultimate creature.
    /// Automatically handles the lifecycle, creating and destroying the creature instance.
    /// </summary>
    internal class TempCreatureInstance : IDisposable
    {
        public GameVariable Instance;
        public TempCreatureInstance(Creature creature) {
            Instance = Game.Engine.CallScript("gml_Script_scr_Creature", creature.ID, 0, 0);
        }
        void IDisposable.Dispose()
        {
            Game.Engine.CallFunction("instance_destroy", Instance);
        }
    }
}
