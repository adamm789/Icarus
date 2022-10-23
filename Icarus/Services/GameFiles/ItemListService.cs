using Icarus.Services.Interfaces;
using ItemDatabase;
using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.GameFiles
{
    public class ItemListService : LuminaDependentServiceBase<ItemListService>, IItemListService
    {
        ILogService _logService;
        public ItemListService(ILogService logService, LuminaService luminaService) : base(luminaService)
        {
            _logService = logService;
        }

        bool _isLoaded = false;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; OnPropertyChanged();  }
        }

        private ItemList? Data;

        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        public List<IItem> Search(string str, bool exactMatch = false)
        {
            var ret = new List<IItem>();
            if (Data == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = Data.Search(str, exactMatch);
                _logService.Debug($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public List<IItem> Search(string str, string variantCode, bool exactMatch = false)
        {
            var ret = new List<IItem>();
            if (Data == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = Data.Search(str, variantCode, exactMatch);
                _logService.Information($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public bool TrySearch(string path)
        {
            if (!path.Contains(".mdl") && !path.Contains(".mtrl"))
            {
                return false;
            }

            try
            {
                return _lumina.FileExists(path);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        protected override void OnLuminaSet()
        {
            _logService.Verbose("Creating item list.");
            Data = new(_lumina);
            IsLoaded = true;
        }

        public Dictionary<string, SortedDictionary<string, IItem>> GetAllItems() => Data.GetAllItems();
    }
}
