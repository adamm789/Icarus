namespace Icarus.ViewModels.Util
{
    public abstract class ChildRemovableViewModel : NotifyPropertyChanged
    {
        protected bool _shouldRemove = false;
        public bool ShouldRemove
        {
            get { return _shouldRemove; }
            protected set { _shouldRemove = value; OnPropertyChanged(); }
        }

        protected DelegateCommand _removeCommand;
        public DelegateCommand RemoveCommand
        {
            get { return _removeCommand ??= new DelegateCommand(_ => { ShouldRemove = true; }); }
        }
    }
}
