using System.Windows;
using VMS.TPS.Common.Model.API;

namespace DvhPlot.Script
{
    public class Script
    {
        public void Execute(ScriptContext context, Window window)
        {
            var mainViewModel = new MainViewModel(context.PlanSetup);
            var mainView = new MainView(mainViewModel);
            window.Content = mainView;
        }
    }
}
