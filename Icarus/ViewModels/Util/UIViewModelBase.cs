using Icarus.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Util
{
    public class UIViewModelBase : ViewModelBase
    {
        public Action CloseAction;
        public UIViewModelBase(ILogService logService) : base(logService)
        {

        }
    }
}
