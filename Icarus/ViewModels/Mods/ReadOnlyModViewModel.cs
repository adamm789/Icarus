using Icarus.Mods;
using Icarus.Mods.Interfaces;
using Icarus.Services.GameFiles;
using Icarus.Services.GameFiles.Interfaces;
using Icarus.Services.Interfaces;
using ItemDatabase.Interfaces;
using Serilog;
using System.Reflection;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods
{
    /// <summary>
    /// ModViewModel provided as a courtesy to be able to include ttmp2 files that are not models, textures, materials, or metadata
    /// Can only be exported to ttmp2
    /// </summary>
    public class ReadOnlyModViewModel : ModViewModel
    {
        public byte[] Data { get; set; }

        public ReadOnlyModViewModel(IMod mod, ILogService logService)
            : base(mod, null, logService)
        {
            FileName = mod.ModFileName;
        }

        public override Task<IGameFile?> GetFileData(IItem? itemArg = null)
        {
            return Task.FromResult<IGameFile?>(null);
        }

        public override Task<IGameFile?> GetFileData(string path, string name = "")
        {
            return Task.FromResult<IGameFile?>(null);
        }

        public override Task<bool> SetDestinationItem(IItem? item = null)
        {
            _logService.Information("Cannot set item on ReadOnlyMod.");
            return Task.FromResult(false);
        }

        protected override bool TrySetDestinationPath(string path, string name = "")
        {
            return false;
        }

        protected override bool HasValidPathExtension(string path)
        {
            return false;
        }
    }
}
