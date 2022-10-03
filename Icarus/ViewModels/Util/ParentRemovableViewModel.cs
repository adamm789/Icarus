using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Util
{
    public abstract class ParentRemovableViewModel<T> : NotifyPropertyChanged
        where T : ChildRemovableViewModel
    {
        protected Dictionary<T, PropertyChangedEventHandler> _eventHandlerDict = new();

        protected virtual void AddChild(T child)
        {
            OnAddChild(child);
        }

        private void OnAddChild(T child)
        {
            var eh = new PropertyChangedEventHandler(OnRemoveChild);
            child.PropertyChanged += eh;

            _eventHandlerDict.Add(child, eh);
        }

        /// <summary>
        /// Allows the implementing view model to do additional functions for removal
        /// </summary>
        /// <param name="child"></param>
        protected abstract void RemoveChild(T child);

        private void OnRemoveChild(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildRemovableViewModel.ShouldRemove))
            {
                var child = sender as T;
                var eh = _eventHandlerDict[child];
                child.PropertyChanged -= eh;
                _eventHandlerDict.Remove(child);

                RemoveChild(child);
            }
        }
    }
}
