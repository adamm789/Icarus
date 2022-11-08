using Icarus.Mods.Interfaces;
using Icarus.Util.Import;
using ItemDatabase.Enums;
using System;
using System.Collections.Generic;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.DataContainers;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.Variants.DataContainers;

namespace Icarus.Mods
{
    public class MetadataMod : Mod, IMetadataFile
    {
        public ItemMetadata ItemMetadata { get; set; }

        // No underscore
        public string Slot { get; set; }
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

        //public static 

        // EqdpEntries
        // EqpEntry
        // EstEntries
        // GmpEntry
        // ImcEntries
        // Root


        public MetadataMod(IMetadataFile file, ImportSource source = ImportSource.Vanilla) : base(file, source)
        {
            ItemMetadata = file.ItemMetadata;
            Slot = file.Slot;
            SetModData(file);
            /*
            Path = file.Path;
            Name = file.Name;
            Category = file.Category;
            Slot = file.Slot;
            */
        }


        public MetadataMod(ItemMetadata data, ImportSource source = ImportSource.Vanilla) : base(source)
        {
            ItemMetadata = data;

            ModFileName = data.Root.ToRawItem().Name;
            ModFilePath = data.Root.ToString();

            Path = data.Root.ToString();
            Slot = data.Root.Info.Slot;
        }

        public override void SetModData(IGameFile gameFile)
        {
            if (gameFile is not IMetadataFile metaFile)
            {
                throw new ArgumentException($"ModData was not of MetadataGameFile. It was {gameFile.GetType()}.");
            }

            if (metaFile.ItemMetadata.Root.Info.Slot != Slot)
            {
                // TODO?: Don't change it if it's not the same slot
                return;
            }
            base.SetModData(gameFile);

            ItemMetadata.Root = metaFile.ItemMetadata.Root;
            Path = metaFile.Path;
            //EstEntries = metaFile.ItemMetadata.EstEntries;
        }
    }
}
