using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Paths
{
    public static partial class XivPathParser
    {
        // Reminder: When variant is "a", it does not have it in any of the tex paths

        // TODO: Apparently there's "Shared textures" and "Unique textures"
        /// <summary>
        /// Accepts a mtrl path and returns a tex path of the given type
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTexPathFromMtrl(string input, XivTexType type, bool isBibo = false)
        {
            if (String.IsNullOrWhiteSpace(input)) return "";

            input = input.ToLower();
            if (type is XivTexType.Reflection)
            {
                // I guess XivTexType.Reflection is always the same?
                // When I edit a material in TT, it can change it to _o, ala the other TexTypes
                return "chara/common/texture/catchlight_1.tex";
            }
            // TODO: type is multi and... is body? return "chara/common/texture/skin_m.tex" ?
            if (fullGearRegexMtrl.IsMatch(input))
            {
                var fullPathMatches = fullGearRegexMtrl.Matches(input);
                if (fullPathMatches.Count == 1 && fullPathMatches[0].Groups.Count == 7)
                {
                    var dir = fullPathMatches[0].Groups[1].Value;
                    var variantNum = int.Parse(fullPathMatches[0].Groups[2].Value);
                    var raceCode = fullPathMatches[0].Groups[3].Value;
                    var equipCode = fullPathMatches[0].Groups[4].Value;
                    var slotCode = fullPathMatches[0].Groups[5].Value;
                    var mtrlVariant = fullPathMatches[0].Groups[6].Value;

                    var variantNumString = variantNum.ToString().PadLeft(2, '0');
                    string mtrlVariantString = "";
                    if (mtrlVariant != "" && mtrlVariant != "a")
                    {
                        mtrlVariantString = $"_{mtrlVariant}";
                    }

                    var ret = $"{dir}/{equipCode}/texture/v{variantNumString}_{raceCode}{equipCode}_{slotCode}{mtrlVariantString}";
                    switch (type)
                    {
                        case XivTexType.Normal: return ret + "_n.tex";
                        case XivTexType.Multi: return ret + "_m.tex";
                        case XivTexType.Diffuse: return ret + "_d.tex";
                        case XivTexType.Specular: return ret + "_s.tex";
                    }
                }
            }

            if (gearRegexFileNameMtrl.IsMatch(input))
            {
                var fileNameMatches = gearRegexFileNameMtrl.Matches(input);
                var arr = mtrlRegex.Split(input);
                var ret = arr[0];
                switch (type)
                {
                    case XivTexType.Normal: return ret + "_n.tex";
                    case XivTexType.Multi: return ret + "_m.tex";
                    case XivTexType.Diffuse: return ret + "_d.tex";
                    case XivTexType.Specular: return ret + "_s.tex";
                }
            }
            //throw new ArgumentException($"Could not parse mtrl: {input} into {type} texture.");

            if (input.Contains("bibo"))
            {

            }
            return "";
        }

        public static XivTexType GetTexType(string path)
        {
            if (path.EndsWith("m.tex")) return XivTexType.Multi;
            else if (path.EndsWith("n.tex")) return XivTexType.Normal;
            else if (path.EndsWith("s.tex")) return XivTexType.Specular;
            else if (path.EndsWith("d.tex")) return XivTexType.Diffuse;

            throw new ArgumentException($"Could not get TexType from {path}.");
        }

        public static string ChangeTexType(string path, XivTexType type)
        {
            var texTypeRegex = new Regex(@"_([a-z]).tex$");
            if (texTypeRegex.IsMatch(path))
            {
                var matches = texTypeRegex.Matches(path);
                string typeString = matches[0].Groups[1].Value;
                switch (type)
                {
                    case XivTexType.Multi:
                        typeString = "m";
                        break;
                    case XivTexType.Diffuse:
                        typeString = "d";
                        break;
                    case XivTexType.Specular:
                        typeString = "s";
                        break;
                    case XivTexType.Normal:
                        typeString = "n";
                        break;
                    default:
                        break;
                }

                var retVal = path.Substring(0, path.Length - 5);
                retVal += typeString + ".tex";
                return retVal;
            }
            //throw new ArgumentOutOfRangeException($"Could not change tex type of {path}.");
            return path;
        }

        public static string GetTexVariant(string path)
        {
            var texVariantRegex = new Regex(@"_([a-z]+)_[n,m,s,d].tex$");

            if (texVariantRegex.IsMatch(path))
            {
                var matches = texVariantRegex.Matches(path);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    var variant = matches[0].Groups[1].Value;

                    foreach (var r in gearRegex)
                    {
                        if (r.IsMatch($"_{variant}"))
                        {
                            return "a";
                        }
                    }
                    return variant;
                }
            }
            return "a";
        }

        public static string ChangeTexVariant(string path, string variant = "")
        {
            var texVariantRegex = new Regex(@"(_[a-z]+)_[n,m,s,d].tex$");

            if (texVariantRegex.IsMatch(path))
            {
                var matches = texVariantRegex.Matches(path);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    var value = matches[0].Groups[1].Value;
                    var index = matches[0].Groups[1].Index;
                    var length = matches[0].Groups[1].Length;

                    var hasVariant = true;

                    foreach (var r in gearRegex)
                    {
                        if (r.IsMatch(value))
                        {
                            var match = r.Match(value).Value;

                            hasVariant = false;
                            index += match.Length;
                            break;
                        }
                    }
                    string underscoreVariant = "_" + variant;
                    if (variant == "a" || variant == "")
                    {
                        underscoreVariant = "";
                    }

                    var retVal = path;

                    if (hasVariant)
                    {
                        retVal = path.Remove(index, length);
                    }

                    retVal = retVal.Insert(index, underscoreVariant);
                    return retVal;
                }
            }

            return path;
        }
    }
}
