using Icarus.Services.Interfaces;
using Lumina;
using System;
using System.ComponentModel;

namespace Icarus.Services.GameFiles
{
    public class LuminaService : ServiceBase<LuminaService>
    {
        //SettingChangingEventHandler eh;
        PropertyChangedEventHandler eh;
        public GameData? Lumina;
        readonly ISettingsService _settingsService;
        readonly ILogService _logService;

        public LuminaService(ISettingsService settingsService, ILogService logService)
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
            if (e.PropertyName == nameof(ISettingsService.GameDirectoryLumina))
            {
                TrySetLumina();
            }
        }

        public void TrySetLumina()
        {
            try
            {
                _logService.Information("Trying to set Lumina. Please wait.");
                // TODO: Seems this can temporarily freeze the UI
                Lumina = new GameData(_settingsService.GameDirectoryLumina);
                _logService.Information($"Successfully initialized Lumina using {_settingsService.GameDirectoryLumina}");
            }
            catch (ArgumentException ex)
            {
                _logService.Warning(ex, $"Could not initialize Lumina with \"{_settingsService.GameDirectoryLumina}\"");
            }

            if (Lumina != null)
            {
                IsLuminaSet = true;
            }
        }
    }
}
