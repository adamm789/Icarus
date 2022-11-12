using Icarus.Mods.Interfaces;
using Icarus.Util.Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Mods.DataContainers
{
    public class ModPack
    {
        private ModPackJson _modPackJson = new();

        public ModPack()
        {
            SetDefaultModPack();
        }

        public ModPack(ModPack other)
        {
            CopyFrom(other);
        }

        public ModPack(ModPackJson modPackJson)
        {
            _modPackJson = modPackJson;
        }

        public void MoveMod(int sourceIndex, int targetIndex)
        {
            /*
            var mod = SimpleModsList.ElementAt(sourceIndex);
            SimpleModsList.RemoveAt(sourceIndex);
            if (targetIndex > SimpleModsList.Count)
            {
                targetIndex = SimpleModsList.Count - 1;
            }
            SimpleModsList.Insert(targetIndex, mod);
            */
            SimpleModsList.Move(sourceIndex, targetIndex);
        }

        public void MovePage(int sourceIndex, int targetIndex)
        {
            /*
            var page = ModPackPages.ElementAt(sourceIndex);
            ModPackPages.RemoveAt(sourceIndex);
            if (targetIndex > ModPackPages.Count)
            {
                targetIndex = ModPackPages.Count - 1;
            }
            ModPackPages.Insert(targetIndex, page);
            */
            ModPackPages.Move(sourceIndex, targetIndex);
        }

        /// <summary>
        /// Copies the metadata
        /// Does not copy SimpleModsList nor ModPackPages
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(ModPack other)
        {
            _modPackJson = other._modPackJson;
        }

        private void SetDefaultModPack()
        {
            TTMPVersion = "1.3";
            Name = "My Mod";
            Author = "User";
            Version = "1.0.0";
            Description = "";
            Url = "";
        }

        // Unncessary?
        public string TTMPVersion
        {
            get { return _modPackJson.TTMPVersion; }
            set { _modPackJson.TTMPVersion = value; }
        }

        public string Name
        {
            get { return _modPackJson.Name; }
            set { _modPackJson.Name = value; }
        }

        public string Author
        {
            get { return _modPackJson.Author; }
            set { _modPackJson.Author = value; }
        }

        private Regex VersionRegex = new(@"^[0-9].[0-9].[0-9]$");

        public string Version
        {
            get { return _modPackJson.Version; }
            set
            {
                if (VersionRegex.IsMatch(value))
                {
                    _modPackJson.Version = value;
                }
            }
        }

        public string Description
        {
            get { return _modPackJson.Description; }
            set { _modPackJson.Description = value; }
        }

        public string Url
        {
            get { return _modPackJson.Url; }
            set { _modPackJson.Url = value; }
        }

        public string MinimumFrameworkVersion = "1.0.0.0";

        public List<IMod> SimpleModsList = new();
        public List<ModPackPage> ModPackPages = new();
    }
}
