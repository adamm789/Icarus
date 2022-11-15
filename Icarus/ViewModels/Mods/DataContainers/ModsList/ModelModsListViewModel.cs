using Icarus.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icarus.ViewModels.Mods.DataContainers.ModsList
{
    public class ModelModsListViewModel : FilteredTypeModsListViewModel<ModelModViewModel>
    {
        public ModelModsListViewModel(FilteredModsListViewModel parent, ILogService logService) : base(parent, "Models", logService)
        {

        }
    }
}
