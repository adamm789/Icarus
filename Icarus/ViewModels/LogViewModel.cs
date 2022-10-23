using Icarus.Services.Interfaces;
using Icarus.UI;
using Icarus.ViewModels.Util;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels
{
    public class LogViewModel : NotifyPropertyChanged
    {
        LogSink _sink;

        public LogViewModel(ILogService logService)
        {
            _sink = logService.Sink;
            _sink.PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            var events = _sink.Events;

            while (!events.IsEmpty)
            {
                events.TryDequeue(out _text2);
                Text += $"{_text2}\n";
            }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var s = sender as LogSink;
            var events = s.Events;
            while (!events.IsEmpty)
            {
                s.Events.TryDequeue(out _text2);
                Text += $"{_text2}\n";

            }
            OnPropertyChanged(nameof(Text));
        }

        ObservableCollection<string> _lines;
        public ObservableCollection<string> Lines
        {
            get { return _lines; }
            set { _lines = value; OnPropertyChanged(); }
        }

        string _text = "";
        string _text2 = "";
        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(); }
        }
    }
}
