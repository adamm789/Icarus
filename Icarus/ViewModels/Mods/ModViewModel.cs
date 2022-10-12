using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using ItemDatabase.Paths;
using Serilog;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Icarus.ViewModels.Mods
{
    // TODO: Set MaxWidth for textboxes
    public abstract class ModViewModel : NotifyPropertyChanged, IDropTarget
    {
        protected IMod _mod;
        public IItem? SelectedItem { get; protected set; }
        protected readonly IGameFileService _gameFileService;
        protected readonly ILogService _logService;
        public virtual bool IsReadOnly
        {
            get { return this is ReadOnlyModViewModel || this is MetadataModViewModel; }
        }

        public bool CanParsePath => XivPathParser.CanParsePath(DestinationPath);

        public ModViewModel(IMod mod, IGameFileService gameFileService, ILogService logService)
        {
            _mod = mod;
            _gameFileService = gameFileService;
            _logService = logService;

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
                if (_mod.Path == value) return;
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
            OnPropertyChanged(nameof(CanParsePath));
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
                SelectedItem = _gameFileService.GetItem();
                SetModData(data);
                RaiseDestinationPathChanged();
                return true;
            }
            return await TrySetDestinationPath(DestinationPath);

        }

        /// <summary>
        /// Tries to get data from the user-provided path
        /// Must match in-game exactly
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Whether or not the file data was successfully found.</returns>
        protected virtual async Task<bool> TrySetDestinationPath(string path, string name = "")
        {
            //if (path == DestinationPath) return false;
            var modData = await _gameFileService.TryGetFileData(path, GetType(), name);
            if (modData == null)
            {
                _logService.Warning($"Could not set {path} as {GetType().Name}");
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
            if (item is IItemViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var item = dropInfo.Data;
            if (item is IItemViewModel vm)
            {
                SetDestinationItem(vm.Item);
            }
        }
    }
}
