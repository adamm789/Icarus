using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Util
{
    /*
    public abstract class RemovableViewModel : NotifyPropertyChanged
    {
        #region Parent functions
        protected Dictionary<RemovableViewModel, PropertyChangedEventHandler> _eventHandlerDict = new();

        protected void AddChild(RemovableViewModel child)
        {
            var eh = new PropertyChangedEventHandler(OnRemoveChild);
            child.PropertyChanged += eh;

            _eventHandlerDict.Add(child, eh);
        }

        /// <summary>
        /// Allows the implementing parent view model to call additional functions on removal removal
        /// </summary>
        /// <param name="child"></param>
        protected virtual void RemoveChild(RemovableViewModel child)
        {

        }

        private void OnRemoveChild(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(RemovableViewModel.ShouldRemove))
            {
                var child = sender as RemovableViewModel;
                var eh = _eventHandlerDict[child];
                child.PropertyChanged -= eh;

                _eventHandlerDict.Remove(child);

                RemoveChild(child);
            }
        }

        #endregion

        #region Child functions
        protected bool _shouldRemove = false;
        public bool ShouldRemove
        {
            get { return _shouldRemove; }
            protected set { _shouldRemove = value; OnPropertyChanged(); }
        }

        protected DelegateCommand _removeCommand;
        public DelegateCommand RemoveCommand
        {
            get { return _removeCommand ??= new DelegateCommand(_ => ShouldRemove = true); }
        }
        #endregion
    }
    */
}
