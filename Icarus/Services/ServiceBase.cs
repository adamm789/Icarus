using Icarus.ViewModels.Util;
using Serilog;
using System;

namespace Icarus.Services
{
    public abstract class ServiceBase<T> : NotifyPropertyChanged, IServiceProvider
        where T : ServiceBase<T>
    {
        private static T _instance;

        public object? GetService(Type serviceType)
        {
            if (_instance == null)
            {
                string err = $"{typeof(T).Name} was not instantited.";
                Log.Fatal(err);

                throw new NullReferenceException(err);
            }
            return _instance;
        }
    }
}
