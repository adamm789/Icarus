using Icarus.Mods.Interfaces;
using System.Collections.Generic;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Mods.DataContainers
{
    public class ModOption
    {
        public ModOption()
        {

        }
        public ModOption(ModOption option)
        {
            Name = option.Name;
            Description = option.Description;
            ImagePath = option.ImagePath;
            GroupName = option.GroupName;
            SelectionType = option.SelectionType;
            IsChecked = option.IsChecked;
        }

        public ModOption(ModOptionJson option)
        {
            Name = option.Name;
            Description = option.Description;
            ImagePath = option.ImagePath;
            GroupName = option.GroupName;
            SelectionType = option.SelectionType;
            IsChecked = option.IsChecked;
        }

        public void AddMod(IMod mod)
        {
            Mods.Add(mod);
        }

        /// <summary>
        /// The name of the option
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The option description
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// The preview image path for the option
        /// </summary>
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// The list of mods in this option
        /// </summary>
        public List<IMod> Mods = new();

        /// <summary>
        /// The name of the group this mod option belongs to
        /// </summary>
        public string GroupName { get; set; } = "";

        /// <summary>
        /// The selection type for this mod option
        /// </summary>
        public string SelectionType { get; set; } = "Single";

        /// <summary>
        /// The status of the radio or checkbox
        /// </summary>
        public bool IsChecked { get; set; } = false;

    }
}
