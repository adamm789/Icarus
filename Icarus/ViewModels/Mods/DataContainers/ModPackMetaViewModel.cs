using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackMetaViewModel : NotifyPropertyChanged, IModPackMetaViewModel
    {
        readonly IUserPreferencesService _userPreferencesService;
        public ModPack ModPack { get; }

        #region Constructor(s)

        public ModPackMetaViewModel(ModPack modPack, IUserPreferencesService userPreferencesService)
        {
            ModPack = modPack;
            _userPreferencesService = userPreferencesService;

            Author = userPreferencesService.DefaultAuthor;
            Url = userPreferencesService.DefaultWebsite;
        }

        #endregion

        #region Bindings
        public string Name
        {
            get { return ModPack.Name; }
            set { ModPack.Name = value; OnPropertyChanged(); }
        }

        public string Author
        {
            get { return ModPack.Author; }
            set { ModPack.Author = value; OnPropertyChanged(); }
        }

        // Probably not @"^[0-9]*.[0-9]*.[0-9]*$" ?
        private Regex VersionRegex = new(@"^[0-9].[0-9].[0-9]$");

        public string Version
        {
            get { return ModPack.Version; }
            set
            {
                if (VersionRegex.IsMatch(value))
                {
                    ModPack.Version = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get { return ModPack.Description; }
            set { ModPack.Description = value; OnPropertyChanged(); }
        }

        public string Url
        {
            get { return ModPack.Url; }
            set { ModPack.Url = value; OnPropertyChanged(); }
        }

        #endregion
    }
}
