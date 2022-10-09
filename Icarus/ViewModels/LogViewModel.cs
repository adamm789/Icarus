using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels
{
    public class LogViewModel : NotifyPropertyChanged
    {
        ILogService _logService;
        public LogViewModel(ILogService logService)
        {
            _logService = logService;
        }

        ObservableCollection<string> _lines;
        public ObservableCollection<string> Lines
        {
            get { return _lines; }
            set { _lines = value; OnPropertyChanged(); }
        }
    }
}
