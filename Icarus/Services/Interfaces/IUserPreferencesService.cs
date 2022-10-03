using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;

namespace Icarus.Services.Interfaces
{
    public interface IUserPreferencesService : IServiceProvider
    {
        string GetDefaultSkinMaterialVariant(XivRace race);
        string DefaultMaleVariant { get; set; }
        string DefaultFemaleVariant { get; set; }
        string DefaultLalafellVariant { get; set; }
        string DefaultAuthor { get; set; }
        string DefaultWebsite { get; set; }
    }
}
