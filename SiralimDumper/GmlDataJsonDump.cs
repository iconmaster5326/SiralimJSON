using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YYTKInterop;

namespace SiralimDumper
{
    public static class GmlDataJsonDump
    {
        private const int MAX_RECUR = 16;
        public static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            MaxDepth = 1024,
            IndentCharacter = ' ',
            IndentSize = 2,
            WriteIndented = true,
        };
        private static Dictionary<string, object> Object(string type, Dictionary<string, object>? others = null) => (new Dictionary<string, object>()
        {
            ["type"] = type
        }).Concat(others ?? new()).ToDictionary();
        public static object AsJSON(this GameVariable var, HashSet<string>? seenIDs = null, int recursions = 0)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<string>();
            }

            if (recursions > MAX_RECUR)
            {
                return Object(var.Type, new() { ["recursionLimitReached"] = true });
            }

            switch (var.Type.Split(" ")[0])
            {
                case "ptr":
                case "method":
                case "undefined":
                    return Object(var.Type);
                case "struct":
                    return var.GetGameObject().AsJSON(seenIDs, recursions + 1);
                case "ref":
                    var value = var.GetString();
                    var refType = value.Remove(0, "ref ".Length);
                    var refTypeParsed = refType.Split(" ").ToList();
                    switch (refTypeParsed[0])
                    {
                        case "instance":
                            try
                            {
                                return GameInstance.FromInstanceID(int.Parse(refTypeParsed.Last())).AsJSON(seenIDs, recursions + 1, true);
                            }
                            catch (InvalidCastException)
                            {
                                return Object("instance", new() {
                                    ["id"] = int.Parse(refTypeParsed.Last()),
                                    ["object_index"] = (Game.Engine.CallFunction("variable_instance_get", var, "object_index").GetStringOrNull() ?? "<unknown>").Split(" ").Last(),
                                    ["invalid_ref"] = true,
                                    ["members"] = Game.Engine.CallFunction("struct_get_names", var).GetArray().Select(name => new KeyValuePair<string, object>(name, Game.Engine.CallFunction("struct_get", var, name).AsJSON(seenIDs, recursions + 1))).ToDictionary(),
                                });
                            }
                        case "ds_list":
                            {
                                var key = $"list {refTypeParsed.Last()}";
                                if (seenIDs.Contains(key))
                                {
                                    return Object("list", new() { ["seen"] = true });
                                }
                                else
                                {
                                    seenIDs.Add(key);
                                }

                                return Object("list", new() { ["items"] = var.GetDSList().Select(x => x.AsJSON(seenIDs, recursions + 1)).ToList() });
                            }
                        case "ds_map":
                            {
                                var key = $"map {refTypeParsed.Last()}";
                                if (seenIDs.Contains(key))
                                {
                                    return Object("map", new() { ["seen"] = true });
                                }
                                else
                                {
                                    seenIDs.Add(key);
                                }

                                return Object("map", new()
                                {
                                    ["items"] = var.GetDSMap().Select(kv => new Dictionary<string, object>
                                    {
                                        ["key"] = kv.Key.AsJSON(seenIDs, recursions + 1),
                                        ["value"] = kv.Value.AsJSON(seenIDs, recursions + 1),
                                    }).ToList()
                                });
                            }
                        default:
                            return Object("ref", new() { ["referee"] = refType });
                    }
                case "string":
                    return var.GetString();
                case "number":
                    double asDouble = var.GetDouble();
                    if (double.IsPositiveInfinity(asDouble))
                    {
                        return Object("positive infinity");
                    }
                    if (double.IsNegativeInfinity(asDouble))
                    {
                        return Object("negative infinity");
                    }
                    else if (double.IsNaN(asDouble))
                    {
                        return Object("nan");
                    }
                    else
                    {
                        return asDouble;
                    }
                case "bool":
                    return var.GetBoolean();
                case "array":
                    return var.GetArray().Select(x => x.AsJSON(seenIDs, recursions + 1)).ToList();
                case "int32":
                    return Object("int32", new() { ["value"] = var.GetInt32() });
                case "int64":
                    return Object("int64", new() { ["value"] = var.GetInt64() });
                default:
                    return Object(var.Type, new() { ["unknown"] = true });
            }
        }
        public static object AsJSON(this GameInstance gi, HashSet<string>? seenIDs = null, int recursions = 0, bool isRef = false)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<string>();
            }

            if (recursions > MAX_RECUR)
            {
                return Object("instance", new() { ["recursionLimitReached"] = true });
            }
            else if (seenIDs.Contains($"gi {gi.ID}"))
            {
                return Object("instance", new() { ["seen"] = true });
            }
            else
            {
                seenIDs.Add($"gi {gi.ID}");
            }

            var result = Object("instance", new()
            {
                ["id"] = gi.ID,
                ["object_index"] = (Game.Engine.CallFunction("variable_instance_get", gi, "object_index").GetStringOrNull() ?? "<unknown>").Split(" ").Last(),
            });
            if (isRef)
            {
                result.Add("invalid_ref", false);
            }
            result.Add("members", gi.Members.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value.AsJSON(seenIDs, recursions + 1))).ToDictionary());
            return result;
        }
        public static object AsJSON(this GameObject go, HashSet<string>? seenIDs = null, int recursions = 0)
        {
            if (seenIDs == null)
            {
                seenIDs = new HashSet<string>();
            }

            if (go.IsInstance())
            {
                return GameInstance.FromObject(go).AsJSON(seenIDs, recursions);
            }

            if (recursions > MAX_RECUR)
            {
                return Object("object", new() { ["recursionLimitReached"] = true });
            }
            else if (seenIDs.Contains($"go {go.GetHashCode()}"))
            {
                return Object("object", new() { ["seen"] = true });
            }
            else
            {
                seenIDs.Add($"go {go.GetHashCode()}");
            }

            return Object("object", new() {
                ["name"] = go.Name,
                ["members"] = go.Members.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value.AsJSON(seenIDs, recursions + 1))).ToDictionary(),
            });
        }
        public static object AsJSON(this CodeExecutionContext ctx, HashSet<string>? seenIDs = null, int recursions = 0)
        {
            return new Dictionary<string, object>()
            {
                ["name"] = ctx.Name,
                ["frame"] = SiralimDumper.Frame,
                ["self"] = ctx.Self.AsJSON(),
                ["other"] = ctx.Other.AsJSON(),
                ["args"] = ctx.Arguments.Select(arg => arg.AsJSON()).ToList(),
            };
        }
    }
}
