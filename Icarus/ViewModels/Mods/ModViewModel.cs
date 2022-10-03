using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

using Mod = Icarus.Mods.Mod;
using System.Windows;

namespace Icarus.ViewModels.Mods
{
    public abstract class ModViewModel : NotifyPropertyChanged
    {
        protected IMod _mod;
        //protected readonly ModFileService _modFileService;
        protected readonly IGameFileService _gameFileService;
        protected readonly ItemListService _itemListService;
        public bool IsReadOnly => this is ReadOnlyModViewModel;

        public ModViewModel(IMod mod, ItemListService itemListService, IGameFileService gameFileDataService)
        {
            _mod = mod;
            _gameFileService = gameFileDataService;
            _itemListService = itemListService;

            var file = new FileInfo(mod.ModFilePath);
            if (file != null)
            {
                FileName = file.Name;
            }
            else
            {
                FileName = mod.ModFilePath;
            }
            //DisplayedHeader = FilePath;
            DisplayedHeader = mod.ModFileName;

            RaiseModPropertyChanged();
        }

        string _displayedHeader = "";
        public string DisplayedHeader
        {
            get { return _displayedHeader; }
            set { _displayedHeader = value; OnPropertyChanged(); }
        }

        public virtual IMod GetMod()
        {
            return _mod;
        }


        string _identifier = "";
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; OnPropertyChanged(); }
        }

        string _fileName = "";
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Path to the user-imported item
        /// </summary>
        public string FilePath
        {
            get { return _mod.ModFilePath; }
        }

        /// <summary>
        /// In-game path to item
        /// </summary>
        public virtual string DestinationPath
        {
            // TODO: Allow user to manally put in destination path if desired?
            // TODO: If allowed, update information upon valid destination path change
            get { return _mod.Path; }
            set { _mod.Path = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Item name
        /// </summary>
        public string DestinationName
        {
            get { return _mod.Name; }
            set { _mod.Name = value; OnPropertyChanged(); }
        }

        /*
        IItem? _destinationItem;
        public IItem? DestinationItem
        {
            get { return _destinationItem; }
            protected set { _destinationItem = value; OnPropertyChanged(); }
        }
        */

        bool _canExport = true;
        public bool CanExport
        {
            get { return _canExport; }
            set { _canExport = value; OnPropertyChanged(); }
        }

        bool _shouldDelete = false;
        public bool ShouldDelete
        {
            get { return _shouldDelete; }
            set { _shouldDelete = value; OnPropertyChanged(); }
        }

        bool _isSelected = true;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(); }
        }

        DelegateCommand _deleteCommand;
        public DelegateCommand DeleteCommand
        {
            get { return _deleteCommand ??= new DelegateCommand(o => ShouldDelete = true); }
        }

        DelegateCommand? _setDestinationCommand;
        public DelegateCommand? SetDestinationCommand
        {
            get { return _setDestinationCommand ??= new DelegateCommand(o => SetDestinationItem()); }
        }

        public virtual void RaiseModPropertyChanged()
        {
            OnPropertyChanged(nameof(DestinationPath));
            OnPropertyChanged(nameof(DestinationName));
            SetCanExport();
        }

        public void SetCanExport()
        {
            CanExport = _mod.IsComplete();
        }

        public abstract Task SetDestinationItem(IItem? item = null);

        public abstract bool TrySetDestinationPath(string item);
    }
}
