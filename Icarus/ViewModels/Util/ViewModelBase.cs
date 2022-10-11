using Icarus.Services.Interfaces;

namespace Icarus.ViewModels.Util
{
    public class BaseViewMode : NotifyPropertyChanged
    {
        protected ILogService _logService;
        protected IUIService _uiService;
        public BaseViewMode(ILogService logService, IUIService uiService)
        {
            _logService = logService;
            _uiService = uiService;
        }
    }
}
