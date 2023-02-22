using Icarus.Services;
using Icarus.Services.Interfaces;

namespace Icarus.ViewModels.Util
{
    public class ViewModelBase : NotifyPropertyChanged
    {
        protected ILogService? _logService;

        public ViewModelBase()
        {

        }

        public ViewModelBase(ILogService? logService)
        {
            _logService = logService;
        }
    }
}
