using AurieSharpInterop;
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
                Framework.Print($"[SiralimDumper] costumes: [{string.Join(", ", Costume.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] decorations: [{string.Join(", ", Decoration.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] walls: [{string.Join(", ", DecorationWalls.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] floors: [{string.Join(", ", DecorationFloors.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] backgrounds: [{string.Join(", ", DecorationBackground.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] weather: [{string.Join(", ", DecorationWeather.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] music: [{string.Join(", ", DecorationMusic.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] gods: [{string.Join(", ", God.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] realms: [{string.Join(", ", Realm.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] conditions: [{string.Join(", ", Condition.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] specializations: [{string.Join(", ", Specialization.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] perks: [{string.Join(", ", Perk.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] realm properties: [{string.Join(", ", RealmProperty.Database.Values).EscapeNonWS()}]");
                Framework.Print($"[SiralimDumper] false gods: [{string.Join(", ", FalseGod.Database.Values).EscapeNonWS()}]");

                // TODO: false god runes
                // TODO: nether bosses
                // TODO: projects
                // TODO: nether stone statistics

                //for (int i = 0; i < 1; i++)
                //{
                //    //var old = Game.Engine.GetGlobalObject().Members.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.PrettyPrint())).ToDictionary();
                //    var v = Game.Engine.CallScript("gml_Script_scr_DatabaseGodShops");
                //    Framework.Print($"[SiralimDumper] {i}: {v.PrettyPrint().EscapeNonWS()}");
                //    //CompareObjectMembers(old, Game.Engine.GetGlobalObject().Members);
                //}

                Environment.Exit(0);
            }
            else
            {
                // simulate the first E press that activates the loading of global databases
                Game.Engine.CallFunction("keyboard_key_press", (int)'E');
                Game.Engine.CallFunction("keyboard_key_release", (int)'E');
            }
        }

        public static void CompareObjectMembers(IReadOnlyDictionary<string, string> d1, IReadOnlyDictionary<string, GameVariable> d2)
        {
            Framework.Print($"[SiralimDumper] BEGIN DIFF");
            var onlyInD1 = d1.Where(kv => !kv.Key.StartsWith("gml_") && !d2.ContainsKey(kv.Key)).ToDictionary();
            var onlyInD2 = d2.Where(kv => !kv.Key.StartsWith("gml_") && !d1.ContainsKey(kv.Key)).ToDictionary();
            var mutual = d1.Keys.Where(k => !k.StartsWith("gml_") && !onlyInD1.ContainsKey(k)).ToHashSet();

            foreach (var k in mutual)
            {
                string d1pp = d1[k];
                string d2pp = d2[k].PrettyPrint();
                if (!d1pp.Equals(d2pp))
                {
                    Framework.Print($"[SiralimDumper] {k}: was: {d1pp.EscapeNonWS().Truncate()} ; is: {d2pp.EscapeNonWS().Truncate()} ;");
                }
            }

            foreach (var k in onlyInD1.Keys)
            {
                Framework.Print($"[SiralimDumper] {k}: removed");
            }

            foreach (var k in onlyInD2.Keys)
            {
                Framework.Print($"[SiralimDumper] {k}: added: {d2[k].PrettyPrint().EscapeNonWS().Truncate()} ;");
            }

            Framework.Print($"[SiralimDumper] END DIFF");
        }
    }
}
