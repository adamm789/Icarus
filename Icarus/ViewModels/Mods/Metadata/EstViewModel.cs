﻿using Icarus.Mods;
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
        public EstViewModel(MetadataMod mod, EqdpViewModel eqdpViewModel)
        {
            var dict = mod.EstEntries;
            foreach (var kvp in dict)
            {
                var eqdpEntyViewModel = eqdpViewModel.GetEntry(kvp.Key);
                var vm = new EstEntryViewModel(kvp.Key, mod, eqdpEntyViewModel);
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

        public ObservableCollection<EstEntryViewModel> MaleEstEntries { get; } = new();
        public ObservableCollection<EstEntryViewModel> FemaleEstEntries { get; } = new();
    }
}
