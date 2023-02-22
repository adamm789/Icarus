using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Util.Import;
using ItemDatabase.Paths;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using xivModdingFramework.Cache;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Materials.FileTypes;
using xivModdingFramework.Textures.Enums;
using Half = SharpDX.Half;


namespace Icarus.Mods
{
    public class MaterialMod : Mod, IMaterialGameFile, IAdditionalPathsMod
    {
        // TODO: Keep track of original paths? And allow a reset button?
        // TODO: Put related tex files "under" the parent material?
        public ShaderInfo ShaderInfo { get; set; } = new();
        public List<Half> ColorSetData { get; set; }
        public byte[] ColorSetDyeData { get; set; }
        public string MultiTexPath { get; set; }
        public string NormalTexPath { get; set; }
        public string SpecularTexPath { get; set; }
        public string DiffuseTexPath { get; set; }
        public string ReflectionTexPath { get; set; }
        public int MaterialSet { get; set; }

        string _variant = "a";
        public string Variant
        {
            get { return _variant; }
            set
            {
                _variant = value;

                var temp = new Dictionary<string, string>();
                foreach (var (path, name) in AllPathsDictionary)
                {
                    var newPath = XivPathParser.ChangeMtrlVariant(path, _variant);
                    temp.Add(newPath, name);
                }
                AllPathsDictionary = temp;
            }
        }

        public bool IsFurniture => ShaderInfo.Shader == MtrlShader.Furniture;
        public bool IsDyeableFurniture => ShaderInfo.Shader == MtrlShader.DyeableFurniture;

        public bool AssignToAllPaths { get; set; } = false;
        public Dictionary<string, string> AllPathsDictionary { get; protected set; } = new();

        public XivMtrl XivMtrl { get; set; }

        public MaterialMod(List<Half> colorSetData, byte[]? colorSetExtraData, ImportSource source = ImportSource.Raw) : base(source)
        {
            ColorSetData = colorSetData;
            if (colorSetExtraData == null)
            {
                ColorSetDyeData = new byte[32];
                for (var i = 0; i < 32; i++)
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

        public void SetAllPaths(List<IMaterialGameFile> files)
        {
            AllPathsDictionary.Clear();
            foreach (var file in files)
            {
                var path = XivPathParser.ChangeMtrlVariant(file.Path, Variant);
                AllPathsDictionary.TryAdd(path, file.Name);
            }
        }


        private void Init(XivMtrl xivMtrl)
        {
            Log.Debug($"Calling Init");
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

            InitializePaths(xivMtrl);
        }

        private void InitializePaths(XivMtrl xivMtrl)
        {
            // TODO: How do I handle custom texture paths?
            Path = xivMtrl.MTRLPath;
            Variant = XivPathParser.GetMtrlVariant(Path);
            foreach (var ttp in xivMtrl.TextureTypePathList)
            {
                switch (ttp.Type)
                {
                    case XivTexType.Diffuse:
                        if (XivPathParser.CanParsePath(DiffuseTexPath) || String.IsNullOrWhiteSpace(DiffuseTexPath)) DiffuseTexPath = ttp.Path;
                        break;
                    case XivTexType.Specular:
                        if (XivPathParser.CanParsePath(SpecularTexPath) || String.IsNullOrWhiteSpace(SpecularTexPath)) SpecularTexPath = ttp.Path;
                        break;
                    case XivTexType.Normal:
                        if (XivPathParser.CanParsePath(NormalTexPath) || String.IsNullOrWhiteSpace(NormalTexPath)) NormalTexPath = ttp.Path;
                        break;
                    case XivTexType.Multi:
                        if (XivPathParser.CanParsePath(MultiTexPath) || String.IsNullOrWhiteSpace(MultiTexPath)) MultiTexPath = ttp.Path;
                        break;
                    case XivTexType.Mask:
                        break;
                    case XivTexType.Reflection:
                        if (XivPathParser.CanParsePath(ReflectionTexPath) || String.IsNullOrWhiteSpace(ReflectionTexPath)) ReflectionTexPath = ttp.Path;
                        break;
                    case XivTexType.Skin:
                        if (XivPathParser.CanParsePath(MultiTexPath) || String.IsNullOrWhiteSpace(MultiTexPath)) MultiTexPath = ttp.Path;
                        break;
                    case XivTexType.ColorSet:
                        break;
                    case XivTexType.Map:
                        break;
                    case XivTexType.Icon:
                        break;
                    case XivTexType.Vfx:
                        break;
                    case XivTexType.UI:
                        break;
                    case XivTexType.Decal:
                        break;
                    case XivTexType.Other:
                        break;
                }
            }

            NormalTexPath ??= XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Normal);
            SpecularTexPath ??= XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Specular);
            MultiTexPath ??= XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Multi);
            DiffuseTexPath ??= XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Diffuse);
            ReflectionTexPath ??= XivPathParser.GetTexPathFromMtrl(Path, XivTexType.Reflection);
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
            Log.Debug($"Updating mtrl: {Name}");
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

        public void SetMtrlPath(string str, bool forced = false)
        {
            try
            {
                // TODO: This does not take into account stupid material variants and their texture paths
                var normal = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Normal);
                var specular = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Specular);
                var multi = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Multi);
                var diffuse = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Diffuse);
                var reflection = XivPathParser.GetTexPathFromMtrl(str, XivTexType.Reflection);

                if ((forced || !String.IsNullOrWhiteSpace(normal)) && XivPathParser.CanParsePath(NormalTexPath))
                {
                    NormalTexPath = normal;
                }

                if ((forced || !String.IsNullOrWhiteSpace(specular)) && XivPathParser.CanParsePath(SpecularTexPath))
                {
                    SpecularTexPath = specular;
                }

                if ((forced || !String.IsNullOrWhiteSpace(multi)) && XivPathParser.CanParsePath(MultiTexPath))
                {
                    MultiTexPath = multi;
                }

                if ((forced || !String.IsNullOrWhiteSpace(diffuse)) && XivPathParser.CanParsePath(DiffuseTexPath))
                {
                    DiffuseTexPath = diffuse;
                }

                if ((forced || !String.IsNullOrWhiteSpace(reflection)) && XivPathParser.CanParsePath(ReflectionTexPath))
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
            if (String.IsNullOrWhiteSpace(path))
            {
                // ??
            }
            var retVal = new MapInfo()
            {
                Usage = type,
                Format = format,
                Path = path
            };
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
        }
    }
}
