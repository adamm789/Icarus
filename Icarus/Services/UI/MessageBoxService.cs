using Icarus.Services.Interfaces;
using System.Windows.Forms;

using MessageBox = System.Windows.Forms.MessageBox;

namespace Icarus.Services.UI
{
    public class MessageBoxService : ServiceBase<MessageBoxService>, IMessageBoxService
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }

        public DialogResult Show(string message, string title, MessageBoxButtons buttons)
        {
            return MessageBox.Show(message, title, buttons);
        }
        public DialogResult ShowMessage(string message, string title, MessageBoxButtons buttons)
        {
            return MessageBox.Show(message, title, buttons);
        }
    }
}
