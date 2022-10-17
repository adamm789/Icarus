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
        List<IItem> Search(string str, bool exactMatch = false);
        List<IItem> Search(string str, string variantCode, bool exactMatch = false);
        public bool TrySearch(string path);
        public Dictionary<string, SortedDictionary<string, IItem>> GetAllItems();
        public IItem? SelectedItem { get; set; }
    }
}
