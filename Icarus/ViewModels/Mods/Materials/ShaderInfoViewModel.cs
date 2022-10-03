using Icarus.Mods;
using Icarus.ViewModels.Mods.Materials;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Materials.DataContainers;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Mods
{
    public class ShaderInfoViewModel : NotifyPropertyChanged
    {
        // TODO: Ability for user to change the shaderinfo

        MaterialMod _materialMod;

        public ShaderInfoViewModel(MaterialMod material)
        {
            ShaderInfo = material.ShaderInfo;
            _materialMod = material;

            NormalTex = new MaterialTexViewModel(_materialMod.NormalTexPath);

            NormalTexPath = material.NormalTexPath;
            SpecularTexPath = material.SpecularTexPath;
            MultiTexPath = material.MultiTexPath;
            DiffuseTexPath = material.DiffuseTexPath;
            ReflectionTexPath = material.ReflectionTexPath;
        }

        public void UpdatePaths(string str)
        {
            _materialMod.SetMtrlPath(str);
            PathProperyChanged();
        }

        public ShaderInfo ShaderInfo { get; }

        public MtrlShader MtrlShader
        {
            get { return ShaderInfo.Shader; }
            set { ShaderInfo.Shader = value; OnPropertyChanged(); }
        }
        public MtrlShaderPreset Preset
        {
            get { return ShaderInfo.Preset; }
            set {
                ShaderInfo.Preset = value;
                OnPropertyChanged();
                PathProperyChanged();
            }
        }
        public bool IsTransparent
        {
            get { return ShaderInfo.TransparencyEnabled; }
            set { ShaderInfo.TransparencyEnabled = value; OnPropertyChanged(); }
        }

        public bool ShowBackfaces
        {
            get { return ShaderInfo.RenderBackfaces; }
            set { ShaderInfo.RenderBackfaces = value; OnPropertyChanged(); }
        }

        MaterialTexViewModel _normalTex;
        public MaterialTexViewModel NormalTex
        {
            get { return _normalTex; }
            set { _normalTex = value; OnPropertyChanged(); }
        }

        // TODO: Figure out how to handle editing texture paths
        /*
        public string NormalTexPath
        {
            get { return _materialMod.NormalTexPath; }
            set { _materialMod.NormalTexPath = value; OnPropertyChanged(); }
        }
        */
        public string NormalTexPath
        {
            get { return _normalTex.Path; }
            set { _normalTex.Path = value; OnPropertyChanged(); }
        }

        public string MultiTexPath
        {
            get { return _materialMod.MultiTexPath; }
            set { _materialMod.MultiTexPath = value; OnPropertyChanged(); }
        }

        public string SpecularTexPath
        {
            get { return _materialMod.SpecularTexPath; }
            set { _materialMod.SpecularTexPath = value; OnPropertyChanged(); }
        }

        public string DiffuseTexPath
        {
            get { return _materialMod.DiffuseTexPath; }
            set { _materialMod.DiffuseTexPath = value; OnPropertyChanged(); }
        }

        public string ReflectionTexPath
        {
            get { return _materialMod.ReflectionTexPath; }
            set { _materialMod.ReflectionTexPath = value; OnPropertyChanged(); }
        }
        private void PathProperyChanged()
        {
            OnPropertyChanged(nameof(HasMulti));
            OnPropertyChanged(nameof(HasDiffuse));
            OnPropertyChanged(nameof(HasSpecular));
            OnPropertyChanged(nameof(HasReflection));

            NormalTexPath = _materialMod.NormalTexPath;

            OnPropertyChanged(nameof(NormalTexPath));
            OnPropertyChanged(nameof(MultiTexPath));
            OnPropertyChanged(nameof(DiffuseTexPath));
            OnPropertyChanged(nameof(SpecularTexPath));
            OnPropertyChanged(nameof(ReflectionTexPath));
        }

        public bool HasMulti => ShaderInfo.HasMulti;
        public bool HasDiffuse => ShaderInfo.HasDiffuse;
        public bool HasSpecular => ShaderInfo.HasSpec;
        public bool HasReflection => ShaderInfo.HasReflection;

        IEnumerable<bool> _transparency;
        public IEnumerable<bool> Transparancy
        {
            get { return _transparency ??= new List<bool> { true, false }; }
        }

        public IEnumerable<MtrlShader> ShaderEnum
        {
            get { return Enum.GetValues(typeof(MtrlShader)).Cast<MtrlShader>(); }
        }

        public IEnumerable<MtrlShaderPreset> ShaderPresetEnum
        {
            get { return Enum.GetValues(typeof(MtrlShaderPreset)).Cast<MtrlShaderPreset>(); }
        }
    }
}
