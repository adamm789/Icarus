using Icarus.Services.Interfaces;

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
