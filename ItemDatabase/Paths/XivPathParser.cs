using ItemDatabase.Enums;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Paths
{
    public static partial class XivPathParser
    {
        static readonly Regex skinRegex = new(@"c[0-9]{4}");
        static readonly Regex headRegex = new(@"_met");
        static readonly Regex bodyRegex = new(@"_top");
        static readonly Regex handRegex = new(@"_glv");
        static readonly Regex legRegex = new(@"_dwn");
        static readonly Regex feetRegex = new(@"_sho");
        static readonly Regex earRegex = new(@"_ear");
        static readonly Regex neckRegex = new(@"_nek");
        static readonly Regex wristRegex = new(@"_wrs");
        static readonly Regex rightRingRegex = new(@"_rir");
        static readonly Regex leftRingRegex = new(@"_ril");

        static readonly Regex genderRaceCode = new(@"c[0-9]{4}[a,e][0-9]{4}_[a-z]+");

        static readonly Regex mdlRegex = new(@".mdl$");
        static readonly Regex mtrlRegex = new(@".mtrl$");

        /*
        static Regex headRegex = new(@"e[0-9]{4}_met");
        static Regex bodyRegex = new(@"e[0-9]{4}_top");
        static Regex handRegex = new(@"e[0-9]{4}_glv");
        static Regex legRegex = new(@"e[0-9]{4}_dwn");
        static Regex feetRegex = new(@"e[0-9]{4}_sho");
        static Regex earRegex = new(@"a[0-9]{4}_ear");
        static Regex neckRegex = new(@"a[0-9]_nek");
        static Regex wristRegex = new(@"a[0-9]{4}_wrs");
        static Regex rightRingRegex = new(@"a[0-9]{4}_rir");
        static Regex leftRingRegex = new(@"a[0-9]{4}_ril");
        */

        static readonly Regex fullEquipmentRegexMtrl = new(@"^(chara/equipment)/e[0-9]{4}\/material\/v([0-9]{4})\/mt_(c[0-9]{4})(e[0-9]{4})_([a-z]+)_{0,1}([a-z]*).mtrl$");
        static readonly Regex gearRegexFileNameMtrl = new(@"(\/){0,1}mt_c[0-9]{4}[a,e][0-9]{4}_[a-z]+_{0,1}([a-z]*).mtrl$");

        static readonly Regex canParseRegex = new(@"c[0-9]{4}[a,b,e][0-9]{4}");

        public static bool CanParsePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            return canParseRegex.IsMatch(input);
        }

        public static EquipmentSlot GetEquipmentSlot(string input)
        {
            if (headRegex.IsMatch(input)) return EquipmentSlot.Head;
            if (bodyRegex.IsMatch(input)) return EquipmentSlot.Body;
            if (handRegex.IsMatch(input)) return EquipmentSlot.Hands;
            if (legRegex.IsMatch(input)) return EquipmentSlot.Legs;
            if (feetRegex.IsMatch(input)) return EquipmentSlot.Feet;
            if (earRegex.IsMatch(input)) return EquipmentSlot.Ears;
            if (neckRegex.IsMatch(input)) return EquipmentSlot.Neck;
            if (wristRegex.IsMatch(input)) return EquipmentSlot.Wrists;
            if (rightRingRegex.IsMatch(input)) return EquipmentSlot.RightRing;
            if (leftRingRegex.IsMatch(input)) return EquipmentSlot.LeftRing;
            return EquipmentSlot.None;
        }

        public static string GetCategory(string path)
        {
            var equipSlot = GetEquipmentSlot(path);
            if (equipSlot != EquipmentSlot.None)
            {
                return equipSlot.ToString();
            }
            return "";
        }

        public static bool IsMdl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @".mdl$");
        }

        public static bool IsMtrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @".mtrl$");
        }

        public static XivTexType GetTexType(string path)
        {
            var texTypeRegex = new Regex(@"([a-z]).tex$");
            if (texTypeRegex.IsMatch(path))
            {
                var matches = texTypeRegex.Matches(path);
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    var type = matches[0].Groups[1].Value;
                    switch (type)
                    {
                        case "m": return XivTexType.Multi;
                        case "n": return XivTexType.Normal;
                        case "s": return XivTexType.Specular;
                        case "d": return XivTexType.Diffuse;
                            // TODO: All the other TexTypes?
                    }
                }
            }

            throw new ArgumentException($"Could not get TexType from {path}.");
        }

        public static string GetDatFile(string path)
        {
            // TODO: Error checking for dat file
            var dir = path.ToLower().Split("/").FirstOrDefault();
            var ret = dir switch
            {
                "common" => "000000",
                "bgcommon" => "010000",
                "bg" => "020000",
                "cut" => "030000",
                "chara" => "040000",
                "shader" => "050000",
                "ui" => "060000",
                "sound" => "070000",
                "vfx" => "080000",
                "ui_script" => "090000",
                "exd" => "0a0000",
                "game_script" => "0b0000",
                "music" => "0c0000",

                _ => throw new ArgumentException($"Could not determine Dat folder from: {path}.")
            };

            return ret;
        }

        #region Skin/Race
        public static string ChangeToSkinRace(string input)
        {
            var match = skinRegex.Match(input);
            var race = match.Groups[0].Value;
            var skinRace = XivRaces.GetXivRace(race);

            return ChangeToRace(input, skinRace);
        }

        public static string ChangeToSkinRace(string input, XivRace race)
        {
            var skinRace = race.GetSkinRace();
            return ChangeToRace(input, skinRace);
        }

        public static string ChangeToRace(string input, XivRace race)
        {
            return skinRegex.Replace(input, GetRaceString(race));
        }

        public static bool HasSkin(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return skinRegex.IsMatch(input);
        }

        static readonly Regex skinMtrl = new(@"mt_c[0-9]{4}b");

        public static bool IsSkinMtrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            return skinMtrl.IsMatch(input);
        }

        public static XivRace GetRaceFromString(string input)
        {
            if (skinRegex.IsMatch(input))
            {
                var match = skinRegex.Match(input);
                if (match.Groups.Count == 1)
                {
                    var code = match.Groups[0].Value;
                    var race = XivRaces.GetXivRace(code.Substring(1, 4));
                    return race;
                }
            }
            return XivRace.Hyur_Midlander_Male;

        }

        public static string GetRaceString(XivRace race)
        {
            return "c" + race.GetRaceCode();
        }

        #endregion

        public static bool IsFace(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @"f[0-9]{4}");
        }

        public static bool IsHair(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @"h[0-9]{4}");
        }


        #region Mtrl
        public static string PathCorrectFileNameMtrl(string input)
        {
            var retVal = input;
            var matches = gearRegexFileNameMtrl.Matches(input);
            if (matches.Count == 1 && matches[0].Groups.Count == 3)
            {
                var forwardSlash = matches[0].Groups[1].Value;
                var mtrlVariant = matches[0].Groups[2].Value;

                if (forwardSlash == "")
                {
                    retVal = retVal.Insert(0, "/");
                }
                if (mtrlVariant == "")
                {
                    retVal = retVal.Insert(retVal.Length - 5, "_a");
                }
            }
            return retVal;
        }

        public static string GetMtrlVariant(string input)
        {
            var ret = "";
            var matches = gearRegexFileNameMtrl.Matches(input);
            if (matches.Count == 1 && matches[0].Groups.Count == 3)
            {
                ret = matches[0].Groups[2].Value;
            }
            return ret;
        }

        public static string PathCorrectMtrl(string input)
        {
            var retVal = input;
            var matches = fullEquipmentRegexMtrl.Matches(input);
            if (matches.Count == 1 && matches[0].Groups.Count == 7)
            {
                var mtrlVariant = matches[0].Groups[6].Value;
                if (mtrlVariant == "")
                {
                    retVal = input.Insert(retVal.Length - 5, "_a");
                }
            }
            return retVal;
        }


        public static string GetMtrlFileName(string input, string variant = "a", XivRace race = XivRace.Hyur_Midlander_Male)
        {
            if (genderRaceCode.IsMatch(input))
            {
                var match = genderRaceCode.Match(input);
                var mat = match.Groups[0].Value;
                mat = ChangeToRace(mat, race);

                var suffix = $"_{variant}";
                var mtrl = $"/mt_{mat}{suffix}.mtrl";
                return mtrl;
            }
            return input;
        }

        public static string GetMtrlFileName(string input, XivRace race)
        {
            var mtrlRegex = new Regex(@"c[0-9]{4}[a,e][0-9]{4}_[a-z]+");
            if (mtrlRegex.IsMatch(input))
            {
                var match = mtrlRegex.Match(input);
                var mat = match.Groups[0].Value;
                mat = ChangeToRace(mat, race);
                var mtrl = $"/mt_{mat}.mtrl";
                return mtrl;
            }
            return "";
        }

        #endregion

        #region Tex

        // TODO: Apparently there's "Shared textures" and "Unique textures"
        public static string GetTexPath(string input, XivTexType type)
        {
            if (type is XivTexType.Reflection)
            {
                // I guess XivTexType.Reflection is always the same?
                return "chara/common/texture/catchlight_1.tex";
            }
            if (fullEquipmentRegexMtrl.IsMatch(input))
            {
                var fullPathMatches = fullEquipmentRegexMtrl.Matches(input);
                if (fullPathMatches.Count == 1 && fullPathMatches[0].Groups.Count == 7)
                {
                    var dir = fullPathMatches[0].Groups[1].Value;
                    var variantNum = int.Parse(fullPathMatches[0].Groups[2].Value);
                    var raceCode = fullPathMatches[0].Groups[3].Value;
                    var equipCode = fullPathMatches[0].Groups[4].Value;
                    var slotCode = fullPathMatches[0].Groups[5].Value;
                    var mtrlVariant = fullPathMatches[0].Groups[6].Value;

                    var variantNumString = variantNum.ToString().PadLeft(2, '0');
                    if (mtrlVariant == "")
                    {
                        mtrlVariant = "a";
                    }
                    string mtrlVariantString = $"_{mtrlVariant}";

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
            return "";
        }

        #endregion
    }
}
