using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Serilog;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Textures.Enums;
using xivModdingFramework.Textures.FileTypes;

namespace Icarus.Mods
{
    public class MaterialMod : Mod
    {
        // TODO: Start here (I think): https://github.com/TexTools/FFXIV_TexTools_UI/blob/37290b2897c79dd1e913bb4ff90285f0e620ca9d/FFXIV_TexTools/ViewModels/TextureViewModel.cs#L1588
        // TODO: Figure out how to import whatever file type (dds?) to create a MaterialMod
        // TODO: Put related tex files "under" the parent material?
        // TODO: Handle case of importing .dds file

        string _variant = "a";
        readonly Regex _variantRegex = new Regex(@"[0-9]{4}_[a-z]+_[a-z].mtrl");

        public ShaderInfo ShaderInfo { get; set; }
        public string MultiTexPath { get; set; }
        public string NormalTexPath { get; set; }
        public string SpecularTexPath { get; set; }
        public string DiffuseTexPath { get; set; }
        public string ReflectionTexPath { get; set; }

        // TODO: Look at all of the options when creating a new material
        private XivMtrl XivMtrl;

        public override bool IsComplete()
        {
            return XivMtrl != null;
        }

        public MaterialMod()
        {

        }

        public MaterialMod(IGameFile gameFile, bool isInternal = false) : base(gameFile, isInternal)
        {
            if (gameFile is not MaterialGameFile)
            {
                throw new ArgumentException($"ModData was not of MaterialGameFile. It was {gameFile.GetType()}.");
            }

            var materialGameFile = gameFile as MaterialGameFile;
            Init(materialGameFile.XivMtrl);
        }

        public MaterialMod(XivMtrl xivMtrl)
        {
            Init(xivMtrl);
        }

        /// <summary>
        /// Updates the Mtrl to the current settings
        /// </summary>
        public void UpdateMtrl()
        {
            Log.Verbose("Updating mtrl.");
            // Adapted from: https://github.com/TexTools/FFXIV_TexTools_UI/blob/37290b2897c79dd1e913bb4ff90285f0e620ca9d/FFXIV_TexTools/ViewModels/MaterialEditorViewModel.cs#L255

            var oldNormal = XivMtrl.GetMapInfo(XivTexType.Normal);
            var newNormal = GetMapInfo(XivTexType.Normal, oldNormal.Format, NormalTexPath);

            MapInfo newMulti = null;
            MapInfo newDiffuse = null;
            MapInfo newSpecular = null;
            MapInfo newReflection = null;

            if (ShaderInfo.HasMulti)
            {
                newMulti = GetMapInfo(XivTexType.Multi, MtrlTextureDescriptorFormat.NoColorset, MultiTexPath);
            }
            else
            {
                newSpecular = GetMapInfo(XivTexType.Specular, MtrlTextureDescriptorFormat.NoColorset, SpecularTexPath);
            }

            if (ShaderInfo.HasDiffuse)
            {
                newDiffuse = GetMapInfo(XivTexType.Diffuse, MtrlTextureDescriptorFormat.NoColorset, DiffuseTexPath);
            }
            else if (ShaderInfo.HasReflection)
            {
                newReflection = GetMapInfo(XivTexType.Reflection, MtrlTextureDescriptorFormat.NoColorset, ReflectionTexPath);
            }

            XivMtrl.MTRLPath = Path;
            XivMtrl.SetShaderInfo(ShaderInfo, true);

            XivMtrl.SetMapInfo(XivTexType.Normal, newNormal);
            XivMtrl.SetMapInfo(XivTexType.Specular, newSpecular);
            XivMtrl.SetMapInfo(XivTexType.Multi, newMulti);
            XivMtrl.SetMapInfo(XivTexType.Diffuse, newDiffuse);
            XivMtrl.SetMapInfo(XivTexType.Reflection, newReflection);
        }

        public void SetMtrlPath(string str, bool forced = true)
        {
            XivMtrl.MTRLPath = str;
            Path = str;

            try
            {
                //var normal = texPaths.Get(XivTexType.Normal);
                var normal = XivPathParser.GetTexPath(str, XivTexType.Normal);
                NormalTexPath = normal;

                //var specular = texPaths.Get(XivTexType.Specular);
                var specular = XivPathParser.GetTexPath(str, XivTexType.Specular);
                if (forced || !String.IsNullOrWhiteSpace(specular))
                {
                    SpecularTexPath = specular;
                }

                //var multi = texPaths.Get(XivTexType.Multi);
                var multi = XivPathParser.GetTexPath(str, XivTexType.Multi);
                if (forced || !String.IsNullOrWhiteSpace(multi))
                {
                    MultiTexPath = multi;
                }

                //var diffuse = texPaths.Get(XivTexType.Diffuse);
                var diffuse = XivPathParser.GetTexPath(str, XivTexType.Diffuse);
                if (forced || !String.IsNullOrWhiteSpace(diffuse))
                {
                    DiffuseTexPath = diffuse;
                }

                //var reflection = texPaths.Get(XivTexType.Reflection);
                var reflection = XivPathParser.GetTexPath(str, XivTexType.Reflection);
                if (forced || !String.IsNullOrWhiteSpace(reflection))
                {
                    ReflectionTexPath = reflection;
                }

                UpdateMtrl();
            }
            catch (ArgumentException)
            {
                var err = $"Could not parse {str} as mtrl.";
                Log.Error(err);
            }
        }

        private void Init(XivMtrl xivMtrl)
        {
            ShaderInfo = xivMtrl.GetShaderInfo();
            XivMtrl = xivMtrl;
            Path = xivMtrl.MTRLPath;

            _variant = XivPathParser.GetMtrlVariant(Path);

            NormalTexPath = xivMtrl.GetMapInfo(XivTexType.Normal, false).Path;

            if (ShaderInfo.HasSpec)
            {
                SpecularTexPath = xivMtrl.GetMapInfo(XivTexType.Specular, false).Path;
            }
            else
            {
                //SpecularTexPath = texPaths.Get(XivTexType.Specular);
                SpecularTexPath = XivPathParser.GetTexPath(Path, XivTexType.Specular);
            }

            if (ShaderInfo.HasMulti)
            {
                MultiTexPath = xivMtrl.GetMapInfo(XivTexType.Multi, false).Path;
            }
            else
            {
                //MultiTexPath = texPaths.Get(XivTexType.Multi);
                MultiTexPath = XivPathParser.GetTexPath(Path, XivTexType.Multi);
            }

            if (ShaderInfo.HasDiffuse)
            {
                DiffuseTexPath = xivMtrl.GetMapInfo(XivTexType.Diffuse, false).Path;
            }
            else
            {
                //DiffuseTexPath = texPaths.Get(XivTexType.Diffuse);
                DiffuseTexPath = XivPathParser.GetTexPath(Path, XivTexType.Diffuse);
            }

            if (ShaderInfo.HasReflection)
            {
                ReflectionTexPath = xivMtrl.GetMapInfo(XivTexType.Reflection, false).Path;
            }
            else
            {
                //ReflectionTexPath = texPaths.Get(XivTexType.Reflection);
                ReflectionTexPath = XivPathParser.GetTexPath(Path, XivTexType.Reflection);
            }
            //ParsePath(XivMtrl.MTRLPath);
        }

        public XivMtrl GetMtrl()
        {
            //UpdateMtrl();
            /*
            if (HasChanged())
            {
                // TODO: Auto-update material?
                Log.Warning("Mtrl has not been updated.");
            }
            */
            return XivMtrl;
        }

        private static MapInfo GetMapInfo(XivTexType type, MtrlTextureDescriptorFormat format, string path)
        {
            var retVal = new MapInfo()
            {
                Usage = type,
                Format = format,
                Path = path
            };
            if (String.IsNullOrWhiteSpace(path))
            {
                Log.Error($"Trying to set {path} as {nameof(type)}.");
            }
            return retVal;
        }

        public List<SharpDX.Half> GetColorSetData()
        {
            return XivMtrl.ColorSetData;
        }

        public bool IsParentMaterial(string texPath)
        {
            if (NormalTexPath.Equals(texPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (SpecularTexPath.Equals(texPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (MultiTexPath.Equals(texPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (DiffuseTexPath.Equals(texPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (ReflectionTexPath.Equals(texPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not MaterialGameFile)
            {
                throw new ArgumentException($"ModData was not of MaterialGameFile. It was {gameFile.GetType()}.");
            }
            var materialGameFile = gameFile as MaterialGameFile;
            base.SetModData(gameFile);
            Init(materialGameFile.XivMtrl);
        }

        private void SetVariant(string str)
        {
            if (_variantRegex.IsMatch(str))
            {
                var index = str.LastIndexOf('_') + 1;
                _variant = str.Substring(index, str.Length - 5 - index);
                Log.Debug($"Path: {str}, variant: {_variant}.");
            }
            else
            {
                _variant = "a";
            }
        }

        static Regex gearRegex = new Regex(@"chara/equipment/e[0-9]{4}/material/v[0-9]{4}/mt_(c[0-9]{4})(e[0-9]{4})_([a-z]+)_([a-z]+).mtrl");

        // TODO: Consider non-gear materials

        public static bool IsValidMtrlPath(string str)
        {
            return gearRegex.IsMatch(str);
        }
    }
}
