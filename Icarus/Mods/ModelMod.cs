using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Services.Interfaces;
using Icarus.ViewModels.Mods;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Helpers;

namespace Icarus.Mods
{
    public class ModelMod : Mod, IModelGameFile
    {
        // TODO: Distinguish origin?
        // i.e. TexTools Simple, TexTools Advanced, Vanilla, imported fbx
        public TTModel ImportedModel { get; set; }
        public ModelModifierOptions Options { get; set; }
        public XivRace TargetRace { get; set; } = XivRace.Hyur_Midlander_Male;
        public List<TTMeshGroup> MeshGroups => ImportedModel.MeshGroups;
        public TTModel? TTModel { get; set; }
        public XivMdl? XivMdl { get; set; }

        public ModelMod(ModelGameFile modelGameFile, bool isInternal = false) : base(modelGameFile, isInternal)
        {
            ImportedModel = modelGameFile.TTModel;
            TTModel = modelGameFile.TTModel;
            XivMdl = modelGameFile.XivMdl;
            TargetRace = modelGameFile.TargetRace;

            Options = new ModelModifierOptions()
            {
                CopyAttributes = false,
                CopyMaterials = false
            };
        }

        public ModelMod(string filePath, TTModel imported, ModelGameFile? modelGameFile = null)
        {
            Init(filePath, imported, modelGameFile);
        }

        private void Init(string filePath, TTModel imported, ModelGameFile? modelGameFile = null)
        {
            ModFilePath = filePath;
            ImportedModel = imported;

            TargetRace = XivPathParser.GetRaceFromString(filePath);

            Options = new ModelModifierOptions()
            {
                CopyAttributes = false,
                CopyMaterials = false
            };

            if (modelGameFile != null)
            {
                SetModData(modelGameFile);
            }
        }

        public override bool IsComplete()
        {
            return TTModel != null && XivMdl != null && ImportedModel != null;
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not ModelGameFile)
            {
                throw new ArgumentException($"ModData was not of ModelGameFile. It was {gameFile.GetType()}.");
            }
            base.SetModData(gameFile);

            var modelGameFile = gameFile as ModelGameFile;

            ModFileName = modelGameFile.Name;
            ModFilePath = modelGameFile.Path;
            TargetRace = modelGameFile.TargetRace;

            TTModel = modelGameFile.TTModel;
            XivMdl = modelGameFile.XivMdl;
        }
    }
}
