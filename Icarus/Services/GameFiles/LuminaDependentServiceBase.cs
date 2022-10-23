using Lumina;
using System.ComponentModel;

namespace Icarus.Services.GameFiles
{
    public abstract class LuminaDependentServiceBase<T> : ServiceBase<T>
        where T : ServiceBase<T>
    {
        protected LuminaService _luminaService;
        protected GameData _lumina;
        readonly PropertyChangedEventHandler? eh;

        public LuminaDependentServiceBase(LuminaService luminaService)
        {
            _luminaService = luminaService;
            eh = new PropertyChangedEventHandler(LuminaPropertyChanged);
            _luminaService.PropertyChanged += eh;
        }

        protected void LuminaPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LuminaService.IsLuminaSet) && sender is LuminaService service)
            {
                if (service.IsLuminaSet)
                {
                    _lumina = service.Lumina;
                    OnLuminaSet();
                    //service.PropertyChanged -= eh;
                }
            }
        }

        /// <summary>
        /// Functions to be called after Lumina has been initialized
        /// </summary>
        protected abstract void OnLuminaSet();
    }
}
