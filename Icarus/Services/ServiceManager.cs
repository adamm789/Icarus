using CommunityToolkit.Mvvm.DependencyInjection;
using Icarus.Services.Files;
using Icarus.Services.GameFiles;
using Icarus.Services.Interfaces;
using Icarus.Services.UI;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.Services
{
    public class ServiceManager
    {
        protected ServiceCollection _services = new();
        public static T? GetService<T>() where T : IServiceProvider => Ioc.Default.GetService<T>();
        public static T GetRequiredService<T>() where T : IServiceProvider => Ioc.Default.GetRequiredService<T>();

        public ServiceManager()
        {
            var settings = new SettingsService();
            _services.AddSingleton(settings);

            var logService = new LogService(settings);
            _services.AddSingleton<ILogService>(logService);

            AddUIServices();
            AddRequiredServices();

            Ioc.Default.ConfigureServices(_services.BuildServiceProvider());

            Ioc.Default.GetRequiredService<IGameFileService>();
            Ioc.Default.GetRequiredService<ConverterService>();
            Ioc.Default.GetRequiredService<ExportService>();
            Ioc.Default.GetRequiredService<ItemListService>();
        }

        protected virtual void AddUIServices()
        {
            _services.AddSingleton<IWindowService, WindowService>();
            _services.AddSingleton<IMessageBoxService, MessageBoxService>();
        }

        protected void AddRequiredServices()
        {
            _services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
            _services.AddSingleton<ItemListService>();
            _services.AddSingleton<ImportService>();
            _services.AddSingleton<ExportService>();

            _services.AddSingleton<ViewModelService>();
            _services.AddSingleton<IGameFileService, GameFileService>();
            _services.AddSingleton<ConverterService>();

            _services.AddSingleton<LuminaService>();
        }
    }
}
