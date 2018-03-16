using System.Collections.Generic;
using System.Windows;
using DvhPlot.Script;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext context, Window window)
        {
            Run(context.CurrentUser,
                context.Patient,
                context.Image,
                context.StructureSet,
                context.PlanSetup,
                context.PlansInScope,
                context.PlanSumsInScope,
                window);
        }

        public void Run(
            User user,
            Patient patient,
            Image image,
            StructureSet structureSet,
            PlanSetup planSetup,
            IEnumerable<PlanSetup> planSetupsInScope,
            IEnumerable<PlanSum> planSumsInScope,
            Window window)
        {
            var mainViewModel = new MainViewModel(planSetup);
            var mainView = new MainView(mainViewModel);
            window.Content = mainView;
        }
    }
}
