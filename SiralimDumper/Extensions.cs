using AurieSharpInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    public static class Extensions
    {
        public static IReadOnlyList<GameVariable> GetArray(this GameVariable v)
        {
            IReadOnlyList<GameVariable> result;
            if (v.TryGetArrayView(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get array of a variable of type '{v.Type}' but failed!");
            }
        }

        public static bool GetBoolean(this GameVariable v)
        {
            bool result;
            if (v.TryGetBoolean(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get bool of a variable of type '{v.Type}' but failed!");
            }
        }

        public static double GetDouble(this GameVariable v)
        {
            double result;
            if (v.TryGetDouble(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get double of a variable of type '{v.Type}' but failed!");
            }
        }

        public static float GetFloat(this GameVariable v)
        {
            float result;
            if (v.TryGetFloat(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get float of a variable of type '{v.Type}' but failed!");
            }
        }

        public static GameInstance GetGameInstance(this GameVariable v)
        {
            GameInstance result;
            if (v.TryGetGameInstance(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get GameInstance of a variable of type '{v.Type}' but failed!");
            }
        }

        public static GameObject GetGameObject(this GameVariable v)
        {
            GameObject result;
            if (v.TryGetGameObject(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get GameObject of a variable of type '{v.Type}' but failed!");
            }
        }
        public static int GetInt32(this GameVariable v)
        {
            int result;
            if (v.TryGetInt32(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get int of a variable of type '{v.Type}' but failed!");
            }
        }

        public static long GetInt64(this GameVariable v)
        {
            long result;
            if (v.TryGetInt64(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get long of a variable of type '{v.Type}' but failed!");
            }
        }

        public static string GetString(this GameVariable v)
        {
            string result;
            if (v.TryGetString(out result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Tried to get string of a variable of type '{v.Type}' but failed!");
            }
        }

        public static IReadOnlyList<GameVariable>? GetArrayOrNull(this GameVariable v)
        {
            IReadOnlyList<GameVariable> result;
            if (v.TryGetArrayView(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static bool? GetBooleanOrNull(this GameVariable v)
        {
            bool result;
            if (v.TryGetBoolean(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static double? GetDoubleOrNull(this GameVariable v)
        {
            double result;
            if (v.TryGetDouble(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static float? GetFloatOrNull(this GameVariable v)
        {
            float result;
            if (v.TryGetFloat(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static GameInstance? GetGameInstanceOrNull(this GameVariable v)
        {
            GameInstance result;
            if (v.TryGetGameInstance(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static GameObject? GetGameObjectOrNull(this GameVariable v)
        {
            GameObject result;
            if (v.TryGetGameObject(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        public static int? GetInt32OrNull(this GameVariable v)
        {
            int result;
            if (v.TryGetInt32(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static long? GetInt64OrNull(this GameVariable v)
        {
            long result;
            if (v.TryGetInt64(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static string? GetStringOrNull(this GameVariable v)
        {
            string result;
            if (v.TryGetString(out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static bool IsNumber(this GameVariable v)
        {
            return v.Type.Equals("number") || v.Type.Equals("int64");
        }

        public static int GetSpriteID(this GameVariable v)
        {
            if (v.IsNumber())
            {
                return v.GetInt32();
            }

            if (!v.Type.Equals("ref"))
            {
                throw new Exception($"Could not get sprite of a variable of type '{v.Type}'!");
            }

            var refName = v.GetString();
            if (!refName.StartsWith("ref sprite "))
            {
                throw new Exception($"Could not get sprite of a reference of form '{refName}'!");
            }

            return refName.Remove(0, "ref sprite ".Length).GetGMLAssetID();
        }

        public static int GetGMLAssetID(this string s)
        {
            GameVariable assetIndex = Game.Engine.GetGlobalObject()["gml_asset_index"];

            if (!assetIndex.Members.ContainsKey(s))
            {
                throw new Exception($"Could not find ID of asset '{s}'!");
            }

            return assetIndex[s].GetInt32();
        }

        private const int MAX_STR_LEN = 1024;

        public static string Escape(this string input)
        {
            // %s (aka \x25s) are escaped due to a bug in Framework.Print with format injection.
            return Regex.Replace(input, "[^\\x20-\\x24\\x26-\\x7e]", new MatchEvaluator((x) => $"\\x{((byte)x.Groups[0].Value[0]):x2}"));
        }

        public static string EscapeNonWS(this string input)
        {
            // %s (aka \x25s) are escaped due to a bug in Framework.Print with format injection.
            return Regex.Replace(input, "[^\\s\\x20-\\x24\\x26-\\x7e]", new MatchEvaluator((x) => $"\\x{((byte)x.Groups[0].Value[0]):x2}"));
        }

        private static string Truncate(string input)
        {
            if (input.Length <= MAX_STR_LEN)
            {
                return input;
            }
            else
            {
                return $"{input.Substring(0, MAX_STR_LEN - 1)}…";
            }
        }

        public static string PrettyPrint(this GameVariable var, HashSet<int>? seenIDs = null)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<int>();
            }

            string as_str;
            double as_dbl;
            bool as_bool;
            GameObject as_obj;
            GameInstance as_gi;
            IReadOnlyList<GameVariable> as_array;
            int as_int;
            long as_long;

            switch (var.Type.Split(" ")[0])
            {
                case "undefined":
                    return "<undefined>";
                case "method":
                    return $"<method>";
                case "struct":
                    if (var.TryGetGameObject(out as_obj))
                    {
                        return as_obj.PrettyPrint(seenIDs);
                    }
                    else
                    {
                        return $"<{var.Type}: could not fetch>";
                    }
                case "ref":
                    if (var.TryGetString(out as_str))
                    {
                        var refType = as_str.Remove(0, "ref ".Length);
                        var refTypeParsed = refType.Split(" ").ToList();

                        if (refTypeParsed[0].Equals("ds_list"))
                        {
                            as_array = var.GetDSList();
                            StringBuilder sb = new StringBuilder($"<{refType}:\n");
                            int i = 0;
                            foreach (var member in as_array)
                            {
                                sb.Append($"  {i} : {member.Type} = {member.PrettyPrint(seenIDs).Replace("\n", "\n  ")}\n");
                                i++;
                            }
                            sb.Append(">");
                            return sb.ToString();
                        }
                        else if (refTypeParsed[0].Equals("ds_map"))
                        {
                            var as_map = var.GetDSMap();
                            StringBuilder sb = new StringBuilder($"<{refType}:\n");
                            int i = 0;
                            foreach (var member in as_map)
                            {
                                sb.Append($"  {member.Key.PrettyPrint(seenIDs).Replace("\n", "\n  ")} : {member.Key.Type} -> {member.Value.PrettyPrint(seenIDs).Replace("\n", "\n  ")} : {member.Value.Type}\n");
                                i++;
                            }
                            sb.Append(">");
                            return sb.ToString();
                        }
                        else
                        {
                            return $"<ref: '{refType}'>";
                        }
                    }
                    else
                    {
                        return "<ref: could not fetch>";
                    }
                case "string":
                    if (var.TryGetString(out as_str))
                    {
                        return $"\"{Truncate(Escape(as_str))}\"";
                    }
                    else
                    {
                        return "<string: could not fetch>";
                    }
                case "number":
                    if (var.TryGetDouble(out as_dbl))
                    {
                        return Escape(as_dbl.ToString());
                    }
                    else
                    {
                        return "<number: could not fetch>";
                    }
                case "bool":
                    if (var.TryGetBoolean(out as_bool))
                    {
                        return as_bool.ToString();
                    }
                    else
                    {
                        return "<bool: could not fetch>";
                    }
                case "array":
                    if (var.TryGetArrayView(out as_array))
                    {
                        StringBuilder sb = new StringBuilder($"<array:\n");
                        int i = 0;
                        foreach (var member in as_array)
                        {
                            sb.Append($"  {i} : {member.Type} = {member.PrettyPrint(seenIDs).Replace("\n", "\n  ")}\n");
                            i++;
                        }
                        sb.Append(">");
                        return sb.ToString();
                    }
                    else
                    {
                        return "<array: could not fetch>";
                    }
                case "int64":
                    if (var.TryGetInt64(out as_long))
                    {
                        return Escape(as_long.ToString());
                    }
                    else
                    {
                        return "<int64: could not fetch>";
                    }
                default:
                    return $"<{var.Type}: unknown type>";
            }
        }

        public static string PrettyPrint(this GameObject obj, HashSet<int>? seenIDs = null)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<int>();
            }

            string t;
            if (obj.IsInstance())
            {
                t = "instance";
            }
            else
            {
                t = "object";
            }

            if (seenIDs.Contains(obj.GetHashCode()))
            {
                return $"<{t} {obj.Name}: (...)>";
            }
            else
            {
                seenIDs.Add(obj.GetHashCode());
            }

            StringBuilder sb = new StringBuilder($"<{t} {obj.Name}:\n");
            foreach (var member in obj.Members)
            {
                sb.Append($"  {Escape(member.Key)} : {member.Value.Type} = {member.Value.PrettyPrint(seenIDs).Replace("\n", "\n  ")}\n");
            }
            sb.Append(">");
            return sb.ToString();
        }

        public static string PrettyPrint<T>(this GameInstance inst, HashSet<int>? seenIDs = null)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<int>();
            }

            if (seenIDs.Contains(inst.ID))
            {
                return $"<instance {inst.ID} of object {inst.Name}: (...)>";
            }
            else
            {
                seenIDs.Add(inst.ID);
            }

            StringBuilder sb = new StringBuilder($"<instance {inst.ID} of object {inst.Name}:\n");
            foreach (var member in inst.Members)
            {
                sb.Append($"  {Escape(member.Key)} : {member.Value.Type} = {member.Value.PrettyPrint(seenIDs).Replace("\n", "\n  ")}\n");
            }
            sb.Append(">");
            return sb.ToString();
        }

        public static IReadOnlyList<GameVariable> GetDSList(this GameVariable v)
        {
            int n = Game.Engine.CallFunction("ds_list_size", v);
            var result = new List<GameVariable>(n);
            for (int i = 0; i < n; i++)
            {
                result.Add(Game.Engine.CallFunction("ds_list_find_value", v, i));
            }
            return result;
        }

        public static IReadOnlyDictionary<GameVariable, GameVariable> GetDSMap(this GameVariable v)
        {
            var keys = Game.Engine.CallFunction("ds_map_keys_to_array", v).GetArray();
            var result = new Dictionary<GameVariable, GameVariable>(keys.Count);
            foreach (var key in keys)
            {
                result[key] = Game.Engine.CallFunction("ds_map_find_value", v, key);
            }
            return result;
        }
    }
}
