using ItemDatabase.Enums;
using System.Text.RegularExpressions;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace ItemDatabase.Paths
{
    // TODO: Textures for rings
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

        static List<Regex> gearRegex = new()
        {
            headRegex,
            bodyRegex,
            handRegex,
            legRegex,
            feetRegex,
            earRegex,
            neckRegex,
            wristRegex,
            rightRingRegex,
            leftRingRegex
        };

        static readonly Regex mdlRegex = new(@".mdl$");
        static readonly Regex mtrlRegex = new(@".mtrl$");

        //static readonly Regex fullGearRegexMtrl = new(@"^(chara/equipment)/e[0-9]{4}\/material\/v([0-9]{4})\/mt_(c[0-9]{4})(e[0-9]{4})_([a-z]+)_{0,1}([a-z]*).mtrl$");
        static readonly Regex gearRegexFileNameMtrl = new(@"(\/){0,1}mt_c[0-9]{4}[a,e][0-9]{4}_[a-z]+_{0,1}([a-z]*).mtrl$");
        static readonly Regex fullGearRegexMtrl = new(@"^(chara\/accessory|chara\/equipment)\/[a,e][0-9]{4}\/material\/v([0-9]{4})\/mt_(c[0-9]{4})([a,e][0-9]{4})_([a-z]+)_{0,1}([a-z]*).mtrl$");


        // Arbitary regex for supported paths
        static readonly Regex canParseRegex = new(@"c[0-9]{4}[a,b,e][0-9]{4}");

        public static bool CanParsePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            return canParseRegex.IsMatch(input);
        }

        public static XivDataFile GetXivDataFileFromPath(string path)
        {
            if (path.Contains("common")) return XivDataFile._00_Common;
            if (path.Contains("bgcommon")) return XivDataFile._01_Bgcommon;
            if (path.Contains("bg")) return XivDataFile._02_Bg;
            if (path.Contains("cut")) return XivDataFile._03_Cut;
            if (path.Contains("chara")) return XivDataFile._04_Chara;
            if (path.Contains("shader")) return XivDataFile._05_Shader;
            if (path.Contains("ui")) return XivDataFile._06_Ui;
            if (path.Contains("sound")) return XivDataFile._07_Sound;
            if (path.Contains("vfx")) return XivDataFile._08_Vfx;
            if (path.Contains("ui_script")) return XivDataFile._09_UiScript;
            if (path.Contains("exd")) return XivDataFile._0A_Exd;
            if (path.Contains("game_script")) return XivDataFile._0B_GameScript;
            if (path.Contains("music")) return XivDataFile._0C_Music;

            throw new ArgumentException($"{path} had no valid data file.");
        }

        public static bool IsMaleSkin(XivRace race)
        {
            if (XivRaces.PlayableRaces.Contains(race))
            {
                var x = race.ToString();
                if (race.ToString().Contains("_Male"))
                {
                    return true;
                }
                else if (race.ToString().Contains("_Female"))
                {
                    return false;
                }
            }
            var err = String.Format("Unknown gendered race: {0}", race.ToString());
            throw new ArgumentException(err);
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

        public static string GetCategoryFromPath(string path)
        {
            var equipSlot = GetEquipmentSlot(path);
            if (equipSlot != EquipmentSlot.None)
            {
                return equipSlot.ToString();
            }
            return "";
        }

        public static bool IsMdl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @".mdl$");
        }

        public static bool IsMtrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @".mtrl$");
        }

        public static bool IsTex(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return Regex.IsMatch(path, @".tex$");
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

        // TODO: Bibo textures... Maybe?
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
            var ret = "a";
            //var matches = gearRegexFileNameMtrl.Matches(input);
            if (!String.IsNullOrWhiteSpace(input))
            {
                var matches = Regex.Matches(input, @"_([a-z]+).mtrl$");
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    ret = matches[0].Groups[1].Value;
                }
            }
            return ret;
        }

        public static int GetMtrlSetVariant(string input)
        {
            var ret = -1;
            if (!String.IsNullOrWhiteSpace(input))
            {
                var matches = Regex.Matches(input, @"v([0-9]{4})");
                if (matches.Count == 1 && matches[0].Groups.Count == 2)
                {
                    ret = int.Parse(matches[0].Groups[1].Value);
                }
            }
            return ret;
        }

        public static string PathCorrectMtrl(string input)
        {
            var retVal = input;
            var matches = fullGearRegexMtrl.Matches(input);
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

        public static string ChangeMtrlVariant(string input, string variant = "a")
        {
            var regex = new Regex(@"([a-z]).mtrl$");
            var ret = $"{regex.Replace(input, variant)}.mtrl";
            return ret;
        }

        #endregion
    }
}
