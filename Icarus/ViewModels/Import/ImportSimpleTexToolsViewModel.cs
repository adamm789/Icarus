using Icarus.Mods.DataContainers;
using Icarus.Mods.Interfaces;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Icarus.Views.Import;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Import
{
    public class ImportSimpleTexToolsViewModel : ModsListSelectionViewModel
    {
        public ImportViewModel? ImportViewModel { get; set; }
        bool _shouldRemove = false;
        public bool ShouldRemove
        {
            get { return _shouldRemove; }
            set { _shouldRemove = value; OnPropertyChanged(); }
        }

        public ImportSimpleTexToolsViewModel(IModsListViewModel modsListViewModel, ILogService logService)
            : base(modsListViewModel, logService)
        {
        }

        protected override void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.ShouldImport) && sender is ModViewModel mvm)
            {
                UpdateText();
            }
        }
        protected override void Apply(ModViewModel mvm, bool value)
        {
            mvm.ShouldImport = value;
        }

        protected override void InvertMod(ModViewModel mvm)
        {
            mvm.ShouldImport = !mvm.ShouldImport;
        }

        public override void ConfirmCommand()
        {
            ImportViewModel?.Add(_modsListViewModel.SimpleModsList);
            ShouldRemove = true;
        }

        protected override void CancelCommand()
        {
            CloseAction?.Invoke();
        }

        protected override void UpdateText()
        {
            var selectedTypeList = _modsListViewModel.SimpleModsList.Where(m => _selectedType.IsInstanceOfType(m));
            var numSelected = selectedTypeList.Where(m => m.ShouldImport).Count();

            ConfirmText = $"Import {numSelected}/{selectedTypeList.Count()} mods";

            base.UpdateText(numSelected, selectedTypeList.Count());
        }
    }
}
