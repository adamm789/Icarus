using Icarus.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.Services
{
    public class UserPreferencesService : ServiceBase<UserPreferencesService>, IUserPreferencesService
    {
        readonly ILogService _logService;
        readonly Properties.Settings _settings = Properties.Settings.Default;

        public UserPreferencesService(ILogService logService)
        {
            _logService = logService;
        }

        public string GetDefaultSkinMaterialVariant(XivRace race)
        {
            switch (race)
            {
                case XivRace.Lalafell_Male:
                case XivRace.Lalafell_Female:
                    return DefaultLalafellVariant;
                default:
                    if (IsMaleSkin(race))
                    {
                        return DefaultMaleVariant;
                    }
                    else
                    {
                        return DefaultFemaleVariant;
                    }
            }
        }

        // TODO: UI for default author
        public string DefaultAuthor
        {
            get { return _settings.DefaultAuthor; }
            set { _settings.DefaultAuthor = value; OnPropertyChanged(); }
        }

        // TODO: UI for default website
        public string DefaultWebsite
        {
            get { return _settings.DefaultWebsite; }
            set { _settings.DefaultWebsite = value; OnPropertyChanged(); }
        }

        // TODO: On changing, update any items that have that race selected
        public string DefaultMaleVariant
        {
            get { return _settings.DefaultMaleVariant; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _settings.DefaultMaleVariant = value;
                    OnPropertyChanged();
                    _settings.Save();
                }
            }
        }

        public string DefaultFemaleVariant
        {
            get { return _settings.DefaultFemaleVariant; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _settings.DefaultFemaleVariant = value;
                    OnPropertyChanged();
                    _settings.Save();
                }
            }
        }

        public string DefaultLalafellVariant
        {
            get { return _settings.DefaultLalafellVariant; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _settings.DefaultLalafellVariant = value;
                    OnPropertyChanged();
                    _settings.Save();
                }
            }
        }

        private bool IsMaleSkin(XivRace race)
        {
            if (XivRaces.PlayableRaces.Contains(race))
            {
                if (race.ToString().Contains("_Male"))
                {
                    return true;
                }
                else if (race.ToString().Contains("_Female"))
                {
                    return false;
                }
            }
            var err = String.Format("Unknown gendered race: {0}", race.ToString());
            _logService.Error(err);
            throw new ArgumentException(err);
        }
    }
}
