using Icarus.Mods;
using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Textures.Enums;

namespace Icarus.ViewModels.Import
{
    public abstract class ImportVanillaFileViewModel : ViewModelBase
    {
        protected readonly IModsListViewModel _modPackViewModel;

        public ImportVanillaFileViewModel(IModsListViewModel modPackViewmodel, ILogService logService)
            : base(logService)
        {
            _modPackViewModel = modPackViewmodel;
        }

        string? _selectedItemName;
        public string? SelectedItemName
        {
            get { return _selectedItemName; }
            set { _selectedItemName = value; OnPropertyChanged(); }
        }

        protected string? _completePath;

        bool _canImport = false;
        public bool CanImport
        {
            get { return _canImport; }
            set { _canImport = value; OnPropertyChanged(); RaiseCanExecuteChanged(); }
        }

        DelegateCommand _importVanillaFileCommand;
        public DelegateCommand ImportVanillaFileCommand
        {
            get { return _importVanillaFileCommand ??= new DelegateCommand(_ => DoImport(), _ => CanImport); }
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            ImportVanillaFileCommand.RaiseCanExecuteChanged();
        }

        protected abstract void DoImport();

        public virtual Task SetCompletePath(string? path)
        {
            SelectedItemName = "";
            return Task.CompletedTask;
        }

        public virtual Task SetItem(IItem? item)
        {
            if (item != null)
            {
                _completePath = null;
            }
            return Task.CompletedTask;
        }
    }
}
