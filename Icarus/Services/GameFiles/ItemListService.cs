using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemDatabase;

using Lumina;
using ItemDatabase.Interfaces;
using ItemDatabase.Enums;
using Icarus.Services.Interfaces;
using System.Collections.ObjectModel;
using xivModdingFramework.General.Enums;
using Icarus.Mods.Interfaces;

namespace Icarus.Services.GameFiles
{
    public class ItemListService : LuminaDependentServiceBase<ItemListService>
    {
        ILogService _logService;
        public ItemListService(ILogService logService, LuminaService luminaService) : base(luminaService)
        {
            _logService = logService;
        }

        protected ItemList? _data;
        public ItemList? Data
        {
            get { return _data; }
            set { _data = value; OnPropertyChanged(); }
        }

        IItem _selectedItem;
        public IItem SelectedItem
        {
            get { return _selectedItem; }
            set {
                _selectedItem = value; 
                OnPropertyChanged();
                AllRaceMdls = new(Data.GetAllRaceMdls(SelectedItem));
            }
        }

        ObservableCollection<XivRace> _allRaceMdls = new();
        public ObservableCollection<XivRace> AllRaceMdls
        {
            get { return _allRaceMdls; }
            set { _allRaceMdls = value; OnPropertyChanged(); }
        }

        public List<XivRace> GetAllRaceMdls(IItem? itemArg = null)
        {
            var item = itemArg;
            if (itemArg == null)
            {
                item = SelectedItem;
            }
            if (item == null)
            {
                return new List<XivRace>();
            }
            return Data.GetAllRaceMdls(item);
        }

        public List<IItem> Search(string str)
        {
            var ret = new List<IItem>();
            if (Data == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = Data.Search(str);
                _logService.Debug($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public List<IItem> Search(string str, string variantCode)
        {
            var ret = new List<IItem>();
            if (Data == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = Data.Search(str, variantCode);
                _logService.Information($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public bool TrySearch(string path)
        {
            return _lumina.FileExists(path);
        }

        protected override void OnLuminaSet()
        {
            base.OnLuminaSet();
            _logService.Verbose("Creating item list.");
            Data = new(_lumina);
        }

        public Dictionary<string, SortedDictionary<string, IItem>> GetAllItems() => Data.GetAllItems();
    }
}
