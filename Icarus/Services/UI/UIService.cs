using Icarus.Services.Interfaces;
using System.Windows;
using System.Windows.Forms;

namespace Icarus.Services.UI
{
    public class UIService : ServiceBase<UIService>, IUIService
    {
        readonly IMessageBoxService _messageBoxService;
        readonly IWindowService _windowService;

        public UIService(IMessageBoxService messageBoxService, IWindowService windowService)
        {
            _messageBoxService = messageBoxService;
            _windowService = windowService;
        }

        public void Show(string message) => _messageBoxService.Show(message);

        public DialogResult Show(string message, string title, MessageBoxButtons buttons) => _messageBoxService.Show(message, title, buttons);

        public DialogResult ShowMessage(string message, string title, MessageBoxButtons buttons) => _messageBoxService.ShowMessage(message, title, buttons);

        public void ShowWindow<T>(object dataContext) where T : Window, new() => _windowService.ShowWindow<T>(dataContext);
    }
}
