using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using Lumina;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services.GameFiles
{
    public class LuminaService : ServiceBase<LuminaService>
    {
        //SettingChangingEventHandler eh;
        PropertyChangedEventHandler eh;
        public GameData? Lumina;
        readonly SettingsService _settingsService;
        readonly ILogService _logService;

        public LuminaService(SettingsService settingsService, ILogService logService)
        {
            _settingsService = settingsService;
            _logService = logService;

            eh = new(OnPropertyChanged);
            _settingsService.PropertyChanged += eh;
        }

        bool _isLuminaSet = false;
        public bool IsLuminaSet
        {
            get { return _isLuminaSet; }
            set
            {
                _isLuminaSet = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsService.GameDirectoryLumina))
            {
                TrySetLumina();
            }
        }

        public void TrySetLumina()
        {
            try
            {
                _logService.Verbose($"Trying to set lumina using {_settingsService.GameDirectoryLumina}.");
                // TODO: Seems this can temporarily freeze the UI
                Lumina = new GameData(_settingsService.GameDirectoryLumina);
                _logService.Information("Successfully initialized Lumina.");
            }
            catch (ArgumentException ex)
            {
                _logService.Warning($"Lumina failed to initialize.\n{ex.Message}");
            }

            if (Lumina != null)
            {
                IsLuminaSet = true;
            }
        }
    }
}
