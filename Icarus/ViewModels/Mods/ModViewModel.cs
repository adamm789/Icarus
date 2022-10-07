using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.ViewModels.Import;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

using Mod = Icarus.Mods.Mod;
using System.Windows;
using Serilog;
using Icarus.Services.GameFiles.Interfaces;

namespace Icarus.ViewModels.Mods
{
    public abstract class ModViewModel : NotifyPropertyChanged, IDropTarget
    {
        protected IMod _mod;
        protected readonly IGameFileService _gameFileService;
        //protected readonly ItemListService _itemListService;
        public bool IsReadOnly => this is ReadOnlyModViewModel;

        public ModViewModel(IMod mod, ItemListService itemListService, IGameFileService gameFileDataService)
        {
            _mod = mod;
            _gameFileService = gameFileDataService;
            //_itemListService = itemListService;

            var file = Path.GetFileName(mod.ModFilePath);
            if (file != null)
            {
                FileName = file;
            }
            else
            {
                FileName = mod.ModFilePath;
            }

            if (mod.IsInternal)
            {
                DisplayedHeader = mod.Path;
            }
            else
            {
                DisplayedHeader = FileName;
            }

            //RaiseModPropertyChanged();
        }

        public virtual IMod GetMod()
        {
            return _mod;
        }

        string _displayedHeader = "";
        public string DisplayedHeader
        {
            get { return _displayedHeader; }
            set { _displayedHeader = value; OnPropertyChanged(); }
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
            get { return _mod.Path; }
            set
            {
                var couldSetDestinationPath = Task.Run(() => TrySetDestinationPath(value, DestinationName)).Result;
                if (!couldSetDestinationPath) return;
                _mod.Path = value;
                RaiseDestinationPathChanged();
            }
        }

        /// <summary>
        /// Item name
        /// </summary>
        public string DestinationName
        {
            get { return _mod.Name; }
            set { _mod.Name = value; OnPropertyChanged(); }
        }

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
            get { return _setDestinationCommand ??= new DelegateCommand(async o => await SetDestinationItem()); }
        }

        public virtual void RaiseDestinationPathChanged()
        {
            OnPropertyChanged(nameof(DestinationPath));
            OnPropertyChanged(nameof(DestinationName));
            SetCanExport();
        }

        public void SetCanExport()
        {
            CanExport = _mod.IsComplete();
        }

        public virtual async Task<bool> SetDestinationItem(IItem? itemArg = null)
        {
            var data = await _gameFileService.GetFileData(itemArg, _mod.GetType());
            if (data != null)
            {
                SetModData(data);
                RaiseDestinationPathChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to get data from the user-provided path
        /// Must match in-game exactly
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Whether or not the file data was successfully found.</returns>
        protected virtual async Task<bool> TrySetDestinationPath(string path, string name = "")
        {
            if (path == DestinationPath) return false;
            var modData = await _gameFileService.TryGetFileData(path, GetType(), name);
            if (modData == null)
            {
                Log.Warning($"Could not set {path} as {GetType().Name}");
                return false;
            }
            else
            {
                SetModData(modData);
            }
            return true;
        }

        public virtual bool SetModData(IGameFile gameFile)
        {
            _mod.SetModData(gameFile);
            return true;
            //RaiseModPropertyChanged();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var item = dropInfo.Data;
            if (item is ItemViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var item = dropInfo.Data;
            if (item is ItemViewModel vm)
            {
                SetDestinationItem(vm.Item);
            }
        }
    }
}
