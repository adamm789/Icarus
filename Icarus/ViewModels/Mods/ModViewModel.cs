using GongSolutions.Wpf.DragDrop;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.Util.Import;
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
using System.Windows.Shapes;

namespace Icarus.ViewModels.Mods
{
    // TODO?: Reset button for certain attributes
    public abstract class ModViewModel : ViewModelBase, IDropTarget
    {
        public IMod Mod { get; }
        public IItem? SelectedItem { get; protected set; }
        readonly IGameFileService _gameFileService;
        public ImportSource ImportSource => Mod.ImportSource;
        public bool IsInternal => Mod.ImportSource == ImportSource.Vanilla;
        public virtual bool IsReadOnly
        {
            get { return this is ReadOnlyModViewModel; }
        }

        public bool CanParsePath => XivPathParser.CanParsePath(DestinationPath);

        public ModViewModel(IMod mod, IGameFileService gameFileService, ILogService logService) : base(logService)
        {
            Mod = mod;
            _gameFileService = gameFileService;
            _logService = logService;

            var file = System.IO.Path.GetFileName(mod.ModFilePath);
            if (file != null)
            {
                FileName = file;
            }
            else
            {
                FileName = mod.ModFilePath;
            }

            /*
            if (mod.IsInternal)
            {
                DisplayedHeader = mod.Path;
            }
            else
            {
                DisplayedHeader = FileName;
            }
            */
            DisplayedHeader = mod.ModFileName;
            //RaiseModPropertyChanged();
        }

        public virtual IMod GetMod()
        {
            return Mod;
        }

        public bool ShouldImport
        {
            get { return Mod.ShouldImport; }
            set { Mod.ShouldImport = value; OnPropertyChanged(); }
        }

        public bool ShouldExport
        {
            get { return Mod.ShouldExport; }
            set { Mod.ShouldExport = value; OnPropertyChanged(); }
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
            get { return Mod.ModFilePath; }
        }

        /// <summary>
        /// In-game path to item
        /// </summary>
        public virtual string DestinationPath
        {
            get { return Mod.Path; }
            set
            {
                if (Mod.Path == value) return;
                if (!HasValidPathExtension(value))
                {
                    return;
                }
                // TODO: If a user selects an item, then inputs a custom path, the 
                var couldSetDestinationPath = TrySetDestinationPath(value, DestinationName);

                if (!couldSetDestinationPath)
                {
                    DestinationName = value;
                }
                Mod.Path = value;
                RaiseDestinationPathChanged();
            }
        }

        /// <summary>
        /// Only checks to see if the extension is appropriate for the type of mod
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected abstract bool HasValidPathExtension(string path);

        public virtual bool HasMatch(string? str)
        {
            if (string.IsNullOrWhiteSpace(str)) return true;

            return
                DestinationPath.Contains(str, System.StringComparison.OrdinalIgnoreCase) ||
                DestinationName.Contains(str, System.StringComparison.OrdinalIgnoreCase) ||
                DisplayedHeader.Contains(str, System.StringComparison.OrdinalIgnoreCase) ||
                FilePath.Contains(str, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Item name
        /// </summary>
        public string DestinationName
        {
            get { return Mod.Name; }
            set { Mod.Name = value; OnPropertyChanged(); }
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

        protected virtual void RaiseDestinationPathChanged()
        {
            OnPropertyChanged(nameof(DestinationPath));
            OnPropertyChanged(nameof(DestinationName));
            OnPropertyChanged(nameof(CanParsePath));
            SetCanExport();
        }

        public void SetCanExport()
        {
            CanExport = Mod.IsComplete();
        }

        public virtual async Task<bool> SetDestinationItem(IItem? itemArg = null)
        {
            var data = await GetFileData(itemArg);
            if (data != null)
            {
                SelectedItem = _gameFileService.GetItem(itemArg);
                SetModData(data);
                RaiseDestinationPathChanged();
                return true;
            }
            return TrySetDestinationPath(DestinationPath);
        }

        public abstract Task<IGameFile?> GetFileData(IItem? itemArg = null);
        public abstract Task<IGameFile?> GetFileData(string path, string name = "");

        /// <summary>
        /// Tries to get data from the user-provided path
        /// Must match in-game path exactly
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Whether or not the file data was successfully found.</returns>
        protected virtual bool TrySetDestinationPath(string path, string name = "")
        {
            if (!HasValidPathExtension(path))
            {
                return false;
            }
            //if (path == DestinationPath) return false;
            //var modData = Task.Run(() => _gameFileService.TryGetFileData(path, GetType(), name)).Result;
            //var modData = Task.Run(() => GetFileData()).Result;
            //if (modData == null)
            //{
            var modData = Task.Run(() => GetFileData(path, name)).Result;
            //}
            
            if (modData != null)
            {
                SetModData(modData);
                return true;
            }
            else
            {
                //DestinationName = "???";
                //DestinationName = path;
                return false;
            }
        }

        public virtual bool SetModData(IGameFile gameFile)
        {
            Mod.SetModData(gameFile);
            return true;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var item = dropInfo.Data;
            if (item is ItemTreeNodeViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        async void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var item = dropInfo.Data;

            _logService.Debug($"Drop {item.GetType()} onto {GetType()}");

            if (item is ItemTreeNodeViewModel vm)
            {
                await SetDestinationItem(vm.Item);
            }
        }
    }
}
