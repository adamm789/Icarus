using Icarus.Mods.GameFiles;
using Icarus.Mods.Interfaces;
using Icarus.Util.Extensions;
using Icarus.Util.Import;
using ItemDatabase.Paths;
using System;
using System.Collections.Generic;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.Helpers;

namespace Icarus.Mods
{
    public class ModelMod : Mod, IModelGameFile
    {
        public TTModel ImportedModel { get; set; }
        public ModelModifierOptions Options { get; set; }
        public XivRace TargetRace { get; set; } = XivRace.Hyur_Midlander_Male;
        public List<TTMeshGroup> MeshGroups => ImportedModel.MeshGroups;
        public TTModel? TTModel { get; set; }
        public XivMdl? XivMdl { get; set; }

        public bool ShouldExportRawMaterials { get; set; } = true;

        // TODO: Mesh Groups cannot reference more than 64 bones
        public ModelMod(IModelGameFile modelGameFile, ImportSource source = ImportSource.Vanilla) : base(modelGameFile, source)
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

        public ModelMod(string filePath, TTModel imported, ImportSource source, IModelGameFile? modelGameFile = null) : base(source)
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
            if (gameFile is not IModelGameFile modelGameFile)
            {
                throw new ArgumentException($"ModData was not of ModelGameFile. It was {gameFile.GetType()}.");
            }
            base.SetModData(gameFile);

            TargetRace = modelGameFile.TargetRace;
            TTModel = modelGameFile.TTModel;
            XivMdl = modelGameFile.XivMdl;
        }
    }
}
