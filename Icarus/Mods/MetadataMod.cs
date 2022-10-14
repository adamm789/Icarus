using Icarus.Mods.Interfaces;
using System;
using System.Collections.Generic;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Variants.DataContainers;

namespace Icarus.Mods
{
    public class MetadataMod : Mod, IMetadataFile
    {
        public ItemMetadata ItemMetadata { get; set; }
        public Dictionary<XivRace, EquipmentDeformationParameter> EqdpEntries
        {
            get { return ItemMetadata.EqdpEntries; }
            set { ItemMetadata.EqdpEntries = value; }
        }
        public EquipmentParameter? EqpEntry
        {
            get { return ItemMetadata.EqpEntry; }
            set { ItemMetadata.EqpEntry = value; }
        }
        public Dictionary<XivRace, ExtraSkeletonEntry> EstEntries
        {
            get { return ItemMetadata.EstEntries; }
            set { ItemMetadata.EstEntries = value; }
        }
        public GimmickParameter? GmpEntry
        {
            get { return ItemMetadata.GmpEntry; }
            set { ItemMetadata.GmpEntry = value; }
        }
        public List<XivImc> ImcEntries
        {
            get { return ItemMetadata.ImcEntries; }
            set { ItemMetadata.ImcEntries = value; }
        }

        // EqdpEntries
        // EqpEntry
        // EstEntries
        // GmpEntry
        // ImcEntries
        // Root

        public MetadataMod(ItemMetadata data, bool inInternal = false)
        {
            IsInternal = inInternal;
            ItemMetadata = data;

            ModFileName = data.Root.ToRawItem().Name;
            ModFilePath = data.Root.ToString();
            Path = data.Root.ToString();
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not IMetadataFile metaFile)
            {
                throw new ArgumentException($"ModData was not of MetadataGameFile. It was {gameFile.GetType()}.");
            }
            base.SetModData(gameFile);

            ItemMetadata.Root = metaFile.ItemMetadata.Root;
        }
    }
}
