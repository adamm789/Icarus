using Icarus.Mods.DataContainers;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System.Text.RegularExpressions;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class ModPackMetaViewModel : ViewModelBase, IModPackMetaViewModel
    {
        readonly IUserPreferencesService _userPreferencesService;
        public ModPack ModPack { get; }
        public bool IsReadOnly { get; }

        #region Constructor(s)

        public ModPackMetaViewModel(ModPack modPack, IUserPreferencesService userPreferencesService, ILogService logService, bool isReadOnly = false) : base(logService)
        {
            ModPack = modPack;
            IsReadOnly = isReadOnly;
            _userPreferencesService = userPreferencesService;

            Author = userPreferencesService.DefaultAuthor;
            Url = userPreferencesService.DefaultWebsite;
        }

        public void CopyFrom(ModPack modPack)
        {
            ModPack.CopyFrom(modPack);
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Author));
            OnPropertyChanged(nameof(Url));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(Description));
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
