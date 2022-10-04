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
    // Entry points?
    // Vanilla mtrl, .dds
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
                SpecularTexPath = XivPathParser.GetTexPath(Path, XivTexType.Specular);
            }

            if (ShaderInfo.HasMulti)
            {
                MultiTexPath = xivMtrl.GetMapInfo(XivTexType.Multi, false).Path;
            }
            else
            {
                MultiTexPath = XivPathParser.GetTexPath(Path, XivTexType.Multi);
            }

            if (ShaderInfo.HasDiffuse)
            {
                DiffuseTexPath = xivMtrl.GetMapInfo(XivTexType.Diffuse, false).Path;
            }
            else
            {
                DiffuseTexPath = XivPathParser.GetTexPath(Path, XivTexType.Diffuse);
            }

            if (ShaderInfo.HasReflection)
            {
                ReflectionTexPath = xivMtrl.GetMapInfo(XivTexType.Reflection, false).Path;
            }
            else
            {
                ReflectionTexPath = XivPathParser.GetTexPath(Path, XivTexType.Reflection);
            }
        }

        

        /// <summary>
        /// Updates the Mtrl to the current settings
        /// </summary>
        public void UpdateMtrl()
        {
            Log.Information($"Updating mtrl: {Name}");
            // Adapted from: https://github.com/TexTools/FFXIV_TexTools_UI/blob/37290b2897c79dd1e913bb4ff90285f0e620ca9d/FFXIV_TexTools/ViewModels/MaterialEditorViewModel.cs#L255

            XivMtrl.MTRLPath = Path;
            var oldNormal = XivMtrl.GetMapInfo(XivTexType.Normal);
            var newNormal = GetMapInfo(XivTexType.Normal, oldNormal.Format, NormalTexPath);

            MapInfo? newMulti = null;
            MapInfo? newDiffuse = null;
            MapInfo? newSpecular = null;
            MapInfo? newReflection = null;

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

            Log.Debug("Calling SetShaderInfo. InvalidOperationExceptions will probably occur. Can be ignored.");

            // TODO: Maybe have to re-assign these?
            var colorSetCount = XivMtrl.ColorSetCount;
            var colorSetData = XivMtrl.ColorSetData;
            var colorSetDyeData = XivMtrl.ColorSetDyeData;

            XivMtrl.SetShaderInfo(ShaderInfo, true);
            
            XivMtrl.SetMapInfo(XivTexType.Normal, newNormal);
            XivMtrl.SetMapInfo(XivTexType.Specular, newSpecular);
            XivMtrl.SetMapInfo(XivTexType.Multi, newMulti);
            XivMtrl.SetMapInfo(XivTexType.Diffuse, newDiffuse);
            XivMtrl.SetMapInfo(XivTexType.Reflection, newReflection);
        }

        public void SetMtrlPath(string str, bool forced = true)
        {
            try
            {
                var normal = XivPathParser.GetTexPath(str, XivTexType.Normal);
                var specular = XivPathParser.GetTexPath(str, XivTexType.Specular);
                var multi = XivPathParser.GetTexPath(str, XivTexType.Multi);
                var diffuse = XivPathParser.GetTexPath(str, XivTexType.Diffuse);
                var reflection = XivPathParser.GetTexPath(str, XivTexType.Reflection);

                NormalTexPath = normal;

                if (forced || !String.IsNullOrWhiteSpace(specular))
                {
                    SpecularTexPath = specular;
                }

                if (forced || !String.IsNullOrWhiteSpace(multi))
                {
                    MultiTexPath = multi;
                }

                if (forced || !String.IsNullOrWhiteSpace(diffuse))
                {
                    DiffuseTexPath = diffuse;
                }

                if (forced || !String.IsNullOrWhiteSpace(reflection))
                {
                    ReflectionTexPath = reflection;
                }
                Path = str;
            }
            catch (ArgumentException)
            {
                var err = $"Could not parse {str} as mtrl.";
                Log.Error(err);
            }
        }

        public XivMtrl GetMtrl()
        {
            UpdateMtrl();
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

        // TODO: Ability to copy colorset data or leave it alone
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
    }
}
