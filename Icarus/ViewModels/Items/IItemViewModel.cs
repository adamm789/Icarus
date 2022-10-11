using Icarus.ViewModels.Util;
using ItemDatabase.Interfaces;
using System.ComponentModel;

namespace Icarus.ViewModels.Items
{
    public interface IItemViewModel : INotifyPropertyChanged
    {
        public IItem? Item { get; }
    }
}
