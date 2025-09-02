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
                Framework.Print($"[SiralimDumper] spells: [{string.Join(", ", Spell.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] spell properties: [{string.Join(", ", SpellProperty.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] spell property items: [{string.Join(", ", ItemSpellProperty.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] material items: [{string.Join(", ", ItemMaterial.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] artifacts: [{string.Join(", ", ItemArtifact.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] personalities: [{string.Join(", ", Personality.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] skins: [{string.Join(", ", Skin.Database.Values).EscapeNonWS()}]");

                //for (int i = 0; i < 100; i++)
                //{
                //    //var cobj = Game.Engine.CallScript("gml_Script_scr_Creature", i, 0, 0);
                //    //var sgobj = Game.Engine.CallScript("gml_Script_inv_CreateSpellGem", 1);
                //    var v = Game.Engine.CallScript("gml_Script_scr_PersonalityString", i);
                //    Framework.Print($"[SiralimDumper] {i}: {v.PrettyPrint().EscapeNonWS()}");
                //}

                Environment.Exit(0);
            }
        }
    }
}
