using System.Windows;
using System.Windows.Controls;
using OxyPlot.Wpf;
using VMS.TPS.Common.Model.API;

namespace DvhPlot.Script
{
    public partial class MainView : UserControl
    {
        private readonly MainViewModel _vm;

        public MainView(MainViewModel viewModel)
        {
            _vm = viewModel;

            // Create dummy PlotView to force OxyPlot.Wpf to be loaded
            new PlotView();

            InitializeComponent();
            DataContext = viewModel;
        }

        private void Structure_OnChecked(object checkBoxObject, RoutedEventArgs e)
        {
            _vm.AddDvhCurve(GetStructure(checkBoxObject));
        }

        private void Structure_OnUnchecked(object checkBoxObject, RoutedEventArgs e)
        {
            _vm.RemoveDvhCurve(GetStructure(checkBoxObject));
        }

        private Structure GetStructure(object checkBoxObject)
        {
            var checkbox = (CheckBox)checkBoxObject;
            var structure = (Structure)checkbox.DataContext;
            return structure;
        }
    }
}
