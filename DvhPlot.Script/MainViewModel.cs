using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace DvhPlot.Script
{
    public class MainViewModel
    {
        private readonly PlanSetup _plan;

        public MainViewModel(PlanSetup plan)
        {
            _plan = plan;
        }

        public IEnumerable<Structure> Structures => _plan.StructureSet?.Structures;
    }
}