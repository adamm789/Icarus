﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xivModdingFramework.Mods.DataContainers;

namespace Icarus.Mods.DataContainers
{
    public class ModGroup
    {
        public ModGroup()
        {

        }
        public ModGroup(string name)
        {
            GroupName = name;
            SelectionType = "Single";
        }
        public ModGroup(ModGroupJson group)
        {
            GroupName = group.GroupName;
            SelectionType = group.SelectionType;
        }

        public void AddOption(ModOption option)
        {
            OptionList.Add(option);
        }

        public string GroupName { get; set; } = "";

        public string SelectionType { get; set; } = "Single";

        public List<ModOption> OptionList { get; set; } = new();
    }
}