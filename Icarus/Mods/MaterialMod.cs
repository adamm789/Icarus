using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Util.Import;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Textures.Enums;
using Half = SharpDX.Half;


namespace Icarus.Mods
{
    public class MaterialMod : Mod, IMaterialGameFile
    {
        // TODO: Put related tex files "under" the parent material?
        public ShaderInfo ShaderInfo { get; set; } = new();
        public List<Half> ColorSetData { get; set; }
        public byte[] ColorSetDyeData { get; set; }
        public string MultiTexPath { get; set; }
        public string NormalTexPath { get; set; }
        public string SpecularTexPath { get; set; }
        public string DiffuseTexPath { get; set; }
        public string ReflectionTexPath { get; set; }

        public bool IsFurniture => ShaderInfo.Shader == MtrlShader.Furniture;
        public bool IsDyeableFurniture => ShaderInfo.Shader == MtrlShader.DyeableFurniture;

        public XivMtrl XivMtrl { get; set; }

        public MaterialMod(List<Half> colorSetData, byte[]? colorSetExtraData, ImportSource source = ImportSource.Raw) : base(source)
        {
            ColorSetData = colorSetData;
            if (colorSetExtraData == null)
            {
                ColorSetDyeData = new byte[32];
                for (var i = 0; i < 32; i ++)
                {
                    ColorSetDyeData[i] = 0;
                }
            }
            else
            {
                ColorSetDyeData = colorSetExtraData;
            }
        }

        public MaterialMod(XivMtrl xivMtrl, ImportSource source = ImportSource.TexToolsModPack) : base(source)
        {
            Init(xivMtrl);
        }

        public MaterialMod(IGameFile gameFile, ImportSource source = ImportSource.Vanilla) : base(gameFile, source)
        {
            if (gameFile is not MaterialGameFile)
            {
                throw new ArgumentException($"ModData was not of MaterialGameFile. It was {gameFile.GetType()}.");
            }

            var materialGameFile = gameFile as MaterialGameFile;
            Init(materialGameFile.XivMtrl);
        }


        private void Init(XivMtrl xivMtrl)
        {
            if (this.XivMtrl == null)
            {
                Log.Verbose($"Initializing mtrl: {xivMtrl.MTRLPath}");
                XivMtrl = xivMtrl;

                // Keep any existing colorset data. replace if it does not exist
                if (ColorSetData == null)
                {
                    Log.Verbose("Keeping old colorset data.");
                    ColorSetData = xivMtrl.ColorSetData;
                }
                else
                {
                    Log.Verbose("Setting colorset data.");
                    XivMtrl.ColorSetData = ColorSetData;
                }

                if (ColorSetDyeData == null)
                {
                    Log.Verbose("Keeping old colorset dye data.");
                    ColorSetDyeData = xivMtrl.ColorSetDyeData;
                }
                else
                {
                    Log.Verbose("Setting colorset dye data.");
                    XivMtrl.ColorSetDyeData = ColorSetDyeData;
                }
                ShaderInfo = xivMtrl.GetShaderInfo();
            }

            UpdatePaths(xivMtrl);
        }

        private void UpdatePaths(XivMtrl xivMtrl)
        {
            Path = xivMtrl.MTRLPath;

            NormalTexPath = XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Normal);
            SpecularTexPath = XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Specular);
            MultiTexPath = XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Multi);
            DiffuseTexPath = XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Diffuse);
            ReflectionTexPath = XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Reflection);
        }

        public override bool IsComplete()
        {
            return XivMtrl != null && !String.IsNullOrWhiteSpace(Path);
        }

        /// <summary>
        /// Updates the Mtrl to the current settings
        /// </summary>
        private void UpdateMtrl()
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

            Log.Debug("Calling SetShaderInfo. InvalidOperationExceptions will probably occur and can probably be ignored.");
            XivMtrl.SetShaderInfo(ShaderInfo, true);
            Log.Debug("Done calling SetShaderInfo.");

            XivMtrl.SetMapInfo(XivTexType.Normal, newNormal);
            XivMtrl.SetMapInfo(XivTexType.Specular, newSpecular);
            XivMtrl.SetMapInfo(XivTexType.Multi, newMulti);
            XivMtrl.SetMapInfo(XivTexType.Diffuse, newDiffuse);
            XivMtrl.SetMapInfo(XivTexType.Reflection, newReflection);

            XivMtrl.ColorSetData = ColorSetData;
            XivMtrl.ColorSetDyeData = ColorSetDyeData;
        }

        public void SetMtrlPath(string str, bool forced = true)
        {
            try
            {
                var normal = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Normal);
                var specular = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Specular);
                var multi = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Multi);
                var diffuse = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Diffuse);
                var reflection = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Reflection);

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
            catch (ArgumentException ex)
            {
                Log.Error(ex, $"Could not parse {str} as mtrl.");
            }
        }

        public XivMtrl GetMtrl()
        {
            UpdateMtrl();
            return XivMtrl;
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

        // TODO: Ability to copy colorset data or leave it alone
        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not MaterialGameFile materialGameFile)
            {
                throw new ArgumentException($"ModData was not of MaterialGameFile. It was {gameFile.GetType()}.");
            }
            base.SetModData(materialGameFile);
            //UpdatePaths(materialGameFile.XivMtrl);
            Init(materialGameFile.XivMtrl);
        }
    }
}
