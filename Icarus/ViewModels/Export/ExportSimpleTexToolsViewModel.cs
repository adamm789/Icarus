using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using Icarus.ViewModels.Mods.DataContainers;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using Icarus.ViewModels.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Icarus.ViewModels.Export
{
    // TODO: Prompt for user to delete any files if it exists, upon pressing "Confirm"
    // TODO: Export text for button
    public class ExportSimpleTexToolsViewModel : ModsListSelectionViewModel
    {
        public bool ShouldDelete { get; set; } = false;

        public ExportSimpleTexToolsViewModel(IModsListViewModel modsListViewModel, ILogService logService) 
            : base(modsListViewModel, logService)
        {
            var headerPredicate = new Func<ModViewModel, bool>(m => m.ShouldExport);
            FilteredMods.SetHeaderPredicate(headerPredicate);
        }

        protected override void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.ShouldExport) && sender is ModViewModel mvm)
            {
                UpdateText();
                FilteredMods.UpdateHeaders();
            }
        }

        protected override void Apply(ModViewModel mvm, bool value)
        {
            mvm.ShouldExport = value;
        }

        protected override void InvertMod(ModViewModel mvm)
        {
            mvm.ShouldExport = !mvm.ShouldExport;
        }

        public override void ConfirmCommand()
        {
            ShouldDelete = true;
            CloseAction?.Invoke();
        }

        protected override void CancelCommand()
        {
            ShouldDelete = false;
            CloseAction?.Invoke();
        }

        protected override void UpdateText()
        {
            var selectedTypeList = _modsListViewModel.SimpleModsList.Where(m => _selectedType.IsInstanceOfType(m));
            var numSelected = selectedTypeList.Where(m => m.ShouldExport).Count();

            ConfirmText = $"Export {_modsListViewModel.SimpleModsList.Where(m => m.ShouldExport).Count()}/{_modsListViewModel.SimpleModsList.Count()} mods";

            base.UpdateText(numSelected, selectedTypeList.Count());
        }
    }
}
