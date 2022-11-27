using ItemDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Services.Interfaces
{
    public interface IItemListService : INotifyPropertyChanged, IServiceProvider
    {
        bool IsLoaded { get; }
        int GetNumMaterialSets(IItem item);
        List<IItem>? GetMaterialSet(IItem item);
        List<IItem> Search(string str, bool exactMatch = false);
        List<IItem> Search(string str, string variantCode, bool exactMatch = false);
        bool TrySearch(string path);
        Dictionary<string, SortedDictionary<string, IItem>> GetAllItems();
        IItem? SelectedItem { get; set; }
        IItem? TryGetItem(string path, string itemName = "");
        string TryGetName(string path, string itemName = "");
    }
}
