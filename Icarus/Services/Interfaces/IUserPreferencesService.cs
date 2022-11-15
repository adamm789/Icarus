using System;
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
        bool AlwaysPromptBeforeSimpleTexToolsImport { get; set; }
        bool AlwaysPromptBeforeSimpleTexToolsExport { get; set; }
        bool AlwaysPromptBeforeSimpleRawExport { get; set; }
    }
}
