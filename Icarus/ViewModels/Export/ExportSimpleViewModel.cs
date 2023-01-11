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
using System.Windows.Forms;

namespace Icarus.ViewModels.Export
{
    public class ExportSimpleViewModel : ModsListSelectionViewModel
    {
        public bool ShouldDelete { get; set; } = false;

        protected CommonDialog _dialog;

        public ExportSimpleViewModel(IModsListViewModel modsListViewModel, ILogService logService) 
            : base(modsListViewModel, logService)
        {
            var filterFunction = new Func<ModViewModel, bool>(m => m.ShouldExport);
            FilteredMods.SetFilterFunction(filterFunction);
        }

        public void SetDialog(CommonDialog dialog)
        {
            _dialog = dialog;
        }

        protected override void OnModsListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModViewModel.ShouldExport) && sender is ModViewModel mvm)
            {
                FilteredMods.UpdateList();
                UpdateText();
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
            if (_dialog.ShowDialog() == DialogResult.OK)
            {
                ShouldDelete = true;
                CloseAction?.Invoke();
            }
        }

        protected override void CancelCommand()
        {
            ShouldDelete = false;
            CloseAction?.Invoke();
        }

        protected override void UpdateText()
        {
            ConfirmText = $"Export {FilteredMods.AllMods.NumSelected}/{FilteredMods.AllMods.TotalNum} mods";
            UpdateAllText();
        }
    }
}
