using Icarus.Mods;
using Icarus.ViewModels.Util;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;

namespace Icarus.ViewModels.Mods.Metadata
{
    public class EstViewModel : NotifyPropertyChanged
    {
        //public static AllSkeletonEntries;
        public EstViewModel(MetadataMod mod)
        {
            var dict = mod.EstEntries;
            foreach (var kvp in dict)
            {
                var vm = new EstEntryViewModel(kvp.Key, kvp.Value);
                if (XivPathParser.IsMaleSkin(kvp.Key))
                {
                    MaleEstEntries.Add(vm);
                }
                else
                {
                    FemaleEstEntries.Add(vm);
                }
            }
        }

        public void SetAllSkelId(Dictionary<XivRace, ExtraSkeletonEntry> dict)
        {
            foreach (var entry in MaleEstEntries)
            {
                entry.SkelId = dict[entry.Race].SkelId;
            }
            foreach (var entry in FemaleEstEntries)
            {
                entry.SkelId = dict[entry.Race].SkelId;
            }
        }

        public EstViewModel(Dictionary<XivRace, ExtraSkeletonEntry> dict)
        {
            foreach (var kvp in dict)
            {
                var vm = new EstEntryViewModel(kvp.Key, kvp.Value);
                if (XivPathParser.IsMaleSkin(kvp.Key))
                {
                    MaleEstEntries.Add(vm);
                }
                else
                {
                    FemaleEstEntries.Add(vm);
                }
            }
        }

        public ObservableCollection<EstEntryViewModel> MaleEstEntries { get; } = new();
        public ObservableCollection<EstEntryViewModel> FemaleEstEntries { get; } = new();
    }
}
