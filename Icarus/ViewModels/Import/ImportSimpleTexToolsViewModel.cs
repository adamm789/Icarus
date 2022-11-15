using Icarus.Mods.DataContainers;
using Icarus.Services;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Import
{
    public class ImportSimpleTexToolsViewModel : ViewModelBase
    {
        readonly IUserPreferencesService _userPreferencesService;
        readonly IWindowService _windowService;
        public ObservableCollection<ModPack> ModPacks { get; } = new();

        public ImportSimpleTexToolsViewModel(IUserPreferencesService userPreferencesService, IWindowService windowService, ILogService logService) : base(logService)
        {
            _userPreferencesService = userPreferencesService;
            _windowService = windowService;
        }

        public void Show()
        {

        }
    }
}
