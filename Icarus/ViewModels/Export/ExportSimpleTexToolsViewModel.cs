using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Export
{
    public class ExportSimpleTexToolsViewModel : UIViewModelBase
    {
        IModsListViewModel _modsListViewModel;

        FilteredModsListViewModel _filteredMods;
        public FilteredModsListViewModel FilteredMods { get; set; } 
        public bool? DialogResult { get; set; }

        public bool ShouldDelete { get; private set; } = false;

        string _confirmText = "";
        public string ConfirmText
        {
            get { return _confirmText; }
            set { _confirmText = value; OnPropertyChanged(); }
        }

        int _numSelected;

        public ExportSimpleTexToolsViewModel(IModsListViewModel modsListViewModel, ILogService logService) : base(logService)
        {
            _modsListViewModel = modsListViewModel;
            FilteredMods = new(_modsListViewModel, logService);
            _numSelected = _modsListViewModel.SimpleModsList.Count;
            eh = new(OnPropertyChanged);
            _modsListViewModel.SimpleModsList.CollectionChanged += new(OnCollectionChanged);

            UpdateText();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.ShouldExport) && sender is ModViewModel mvm)
            {
                UpdateText();
            }
        }

        PropertyChangedEventHandler eh;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var mods = e.NewItems.Cast<ModViewModel>();
                foreach (var m in mods)
                {
                    m.PropertyChanged += eh;
                }
                UpdateText();
            }
            if (e.OldItems != null)
            {
                UpdateText();
            }
        }

        private void UpdateText()
        {
            ConfirmText = $"Export {_modsListViewModel.SimpleModsList.Where(m => m.ShouldExport == true).Count()}/{_modsListViewModel.SimpleModsList.Count} mods";
        }

        DelegateCommand _onConfirmCommand;
        public DelegateCommand OnConfirmCommand
        {
            get { return _onConfirmCommand ??= new DelegateCommand(_ => { ShouldDelete = true; CloseAction?.Invoke(); }) ; }
        }

        DelegateCommand _onCancelCommand;
        public DelegateCommand OnCancelCommand
        {
            get { return _onCancelCommand ??= new DelegateCommand(_ => { ShouldDelete = false; CloseAction?.Invoke(); }); }
        }
    }
}
