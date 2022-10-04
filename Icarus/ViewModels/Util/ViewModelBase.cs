using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Util
{
    public class ViewModelBase : NotifyPropertyChanged
    {
        protected ILogService _logService;
        protected IUIService _uiService;
        public ViewModelBase(ILogService logService, IUIService uiService)
        {
            _logService = logService;
            _uiService = uiService;
        }
    }
}
