﻿using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.ModsList;
using Icarus.ViewModels.Util;
using Icarus.Views.Mods;
using ItemDatabase;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace Icarus.ViewModels.Mods.DataContainers
{
    public class FilteredModsListViewModel : ViewModelBase
    {
        public ObservableCollection<ModViewModel> SimpleModsList { get; }

        Timer _timer = new();
        int numCalled = 0;


        public FilteredModsListViewModel(IModsListViewModel modsListViewModel, ILogService logService) : base(logService)
        {
            SimpleModsList = modsListViewModel.SimpleModsList;
            _logService = logService;

            _timer.Tick += Timer_Tick;
            _timer.Interval = 300;

            AllMods = new(this, "All", logService);
            ModelMods = new(this, "Models", logService);
            MaterialMods = new(this, "Materials", logService);
            TextureMods = new(this, "Textures", logService);
            MetadataMods = new(this, "Metadata", logService);
            ReadOnlyMods = new(this, "ReadOnly", logService);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SearchTerm));
            _timer.Stop();
        }

        // TODO: Track some sort of "incompleteness"
        private void OnExportStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ModViewModel mod && e.PropertyName == nameof(ModViewModel.CanExport))
            {
                //IncompleteMods.Refresh();
            }
        }

        public bool SearchFilterFunction(object o)
        {
            if (o is ModViewModel mvm)
            {
                return mvm.HasMatch(SearchTerm);
            }
            else
            {
                return false;
            }
        }

        string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;

                // Delay calling Search() until user stops/slows typing
                _timer.Stop();
                _timer.Start();
            }
        }

        public void SetFilterFunction(Func<ModViewModel, bool> foo)
        {
            AllMods.FilterFunction = foo;
            ModelMods.FilterFunction = foo;
            MaterialMods.FilterFunction = foo;
            TextureMods.FilterFunction = foo;
            MetadataMods.FilterFunction = foo;
            ReadOnlyMods.FilterFunction = foo;
        }

        public void UpdateList()
        {
            AllMods.UpdateList();
            ModelMods.UpdateList();
            MaterialMods.UpdateList();
            TextureMods.UpdateList();
            MetadataMods.UpdateList();
            ReadOnlyMods.UpdateList();
        }

        public FilteredTypeModsListViewModel<ModViewModel> AllMods { get; }
        public FilteredTypeModsListViewModel<ModelModViewModel> ModelMods { get; }
        public FilteredTypeModsListViewModel<MaterialModViewModel> MaterialMods { get; }
        public FilteredTypeModsListViewModel<TextureModViewModel> TextureMods { get; }
        public FilteredTypeModsListViewModel<MetadataModViewModel> MetadataMods { get; }
        public FilteredTypeModsListViewModel<ReadOnlyModViewModel> ReadOnlyMods { get; }
    }
}
