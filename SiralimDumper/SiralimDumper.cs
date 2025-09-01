using AurieSharpInterop;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YYTKInterop;

namespace SiralimDumper
{
    public static class SiralimDumper
    {
        public static AurieStatus InitializeMod(AurieManagedModule Module)
        {
            Framework.Print("[SiralimDumper] Hello, SiralimDumper!");

            Game.Events.OnFrame += OnFrame;

            return AurieStatus.Success;
        }

        public static void UnloadMod(AurieManagedModule Module)
        {
            Framework.Print("[SiralimDumper] Goodbye, SiralimDumper!");
        }

        public static void OnFrame(int FrameNumber, double DeltaTime)
        {
            //Framework.Print($"[SiralimDumper] found global object: {Game.Engine.GetGlobalObject().PrettyPrint().EscapeNonWS()}");
            //Environment.Exit(0);

            if (Game.Engine.GetGlobalObject().Members.ContainsKey("creature"))
            {
                Framework.Print($"[SiralimDumper] found creature array!");

                //Framework.Print($"[SiralimDumper] global object at creature init: {Game.Engine.GetGlobalObject().PrettyPrint().EscapeNonWS()}");

                Framework.Print($"[SiralimDumper] creatures: [{string.Join(", ", Creature.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] traits: [{string.Join(", ", Trait.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] races: [{string.Join(", ", Race.Database.Values).EscapeNonWS()}]");

                //for (int i = 1; i < 2000; i++)
                //{
                //    var cobj = Game.Engine.CallScript("gml_Script_scr_Creature", i, 0, 0);
                //    var v = Game.Engine.CallScript("gml_Script_scr_CritIcon", cobj, true);
                //    Framework.Print($"[SiralimDumper] {v.PrettyPrint().EscapeNonWS()}");
                //}

                Environment.Exit(0);
            }
        }
    }
}
