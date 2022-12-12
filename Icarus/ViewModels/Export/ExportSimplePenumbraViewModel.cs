using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods.DataContainers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Icarus.ViewModels.Export
{
    public class ExportSimplePenumbraViewModel : ExportSimpleViewModel
    {
        public SaveFileDialog SaveFileDialog { get; set; }
        public FolderBrowserDialog FolderBrowserDialog { get; set; }
        public ExportSimplePenumbraViewModel(IModsListViewModel modsListViewModel, ILogService logService)
            : base(modsListViewModel, logService)
        {

        }

        bool _toPmp = true;
        public bool ToPmp
        {
            get { return _toPmp; }
            set { _toPmp = value; OnPropertyChanged(); }
        }

        bool _toFileStructure = true;
        public bool ToFileStructure
        {
            get { return _toFileStructure; }
            set { _toFileStructure = value; OnPropertyChanged(); }
        }

        public override void ConfirmCommand()
        {
            if (ToPmp)
            {
                _dialog = SaveFileDialog;
            }
            else
            {
                _dialog = FolderBrowserDialog;
            }
            base.ConfirmCommand();
        }
    }
}
