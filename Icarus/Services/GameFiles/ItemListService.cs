using Icarus.Services.Interfaces;
using Icarus.ViewModels.Items;
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
        public ItemListViewModel ItemListViewModel {get;}

        ILogService _logService;
        public ItemListService(ILogService logService, LuminaService luminaService) : base(luminaService)
        {
            _logService = logService;
            //ItemListViewModel = new(this, _logService);
        }

        public List<IGear>? GetSharedModels(IGear gear)
        {
            return _itemList.GetSharedModels(gear);
        }

        bool _isLoaded = false;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; OnPropertyChanged();  }
        }

        private ItemList? _itemList;

        IItem? _selectedItem;
        public IItem? SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        public int GetNumMaterialSets(IItem item)
        {
            return _itemList.GetNumMaterialSets(item);
        }

        public List<IItem>? GetMaterialSet(IItem item)
        {
            return _itemList.GetMaterialSet(item);
        }

        public List<IItem> Search(string str, bool exactMatch = false)
        {
            var ret = new List<IItem>();
            if (_itemList == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = _itemList.Search(str, exactMatch);
                _logService.Debug($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public List<IItem> Search(string str, string variantCode, bool exactMatch = false)
        {
            var ret = new List<IItem>();
            if (_itemList == null)
            {
                _logService.Error("Item list has not been built. Could not perform search.");
            }
            else
            {
                ret = _itemList.Search(str, variantCode, exactMatch);
                _logService.Information($"Searched for {str}. Found {ret.Count} matching entries.");
            }
            return ret;
        }

        public bool TrySearch(string path)
        {
            if (!path.Contains(".mdl") && !path.Contains(".mtrl") && !path.EndsWith(".tex"))
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
            _itemList = new(_lumina);
            IsLoaded = true;
        }

        public IItem? TryGetItem(string path, string itemName = "")
        {
            List<IItem> results = new();

            if (!String.IsNullOrWhiteSpace(itemName))
            {
                results = Search(itemName, true);
                if (results.Count == 1)
                {
                    return results[0];
                }
                else
                {

                }
            }
            if (results.Count == 0)
            {
                results = Search(path);
                if (results.Count > 0)
                {
                    return results[0];
                }
            }
            return null;
        }

        public string TryGetName(string path, string itemName = "")
        {
            if (!String.IsNullOrWhiteSpace(itemName))
            {
                return itemName;
            }
            var result = TryGetItem(path, itemName);
            if (result != null)
            {
                return result.Name;
            }

            return "";
        }
        public ITreeNode<(string Header, IItem? Item)> CreateList() => _itemList.CreateList();
    }
}
