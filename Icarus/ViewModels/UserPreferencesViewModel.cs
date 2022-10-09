using Icarus.Services.Interfaces;
using Icarus.ViewModels.Util;

namespace Icarus.ViewModels
{
    public class UserPreferencesViewModel : NotifyPropertyChanged
    {
        IUserPreferencesService _userPreferences;
        public UserPreferencesViewModel(IUserPreferencesService userPreferences)
        {
            _userPreferences = userPreferences;
        }

        public string DefaultMaleVariant
        {
            get { return _userPreferences.DefaultMaleVariant; }
            set { _userPreferences.DefaultMaleVariant = value; OnPropertyChanged(); }
        }
        public string DefaultFemaleVariant
        {
            get { return _userPreferences.DefaultFemaleVariant; }
            set { _userPreferences.DefaultFemaleVariant = value; OnPropertyChanged(); }
        }
        public string DefaultLalafellVariant
        {
            get { return _userPreferences.DefaultLalafellVariant; }
            set { _userPreferences.DefaultLalafellVariant = value; OnPropertyChanged(); }
        }
    }
}
