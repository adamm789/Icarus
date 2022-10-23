using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Formatting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Icarus.ViewModels.Util;

namespace Icarus.UI
{
    public class LogSink : NotifyPropertyChanged, ILogEventSink
    {
        //readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");
        readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp:MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}");

        public ConcurrentQueue<string> Events { get; } = new ConcurrentQueue<string>();

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            var renderSpace = new StringWriter();
            if (logEvent.Level == LogEventLevel.Information)
            {
                
            }
            else if (logEvent.Level == LogEventLevel.Warning)
            {

            }
            else if (logEvent.Level >= LogEventLevel.Error)
            {

            }
            _textFormatter.Format(logEvent, renderSpace);
            Events.Enqueue(renderSpace.ToString());
            OnPropertyChanged(nameof(Events));
        }
    }
}
