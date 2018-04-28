using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OxyPlot.Wpf;
using VMS.TPS.Common.Model.API;

namespace DvhPlot.Script
{
    public partial class MainView : UserControl
    {
        // Create dummy PlotView to force OxyPlot.Wpf to be loaded
        private static readonly PlotView PlotView = new PlotView();

        private readonly MainViewModel _vm;

        public MainView(MainViewModel viewModel)
        {
            _vm = viewModel;

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

        private void ExportPlotAsPdf(object sender, RoutedEventArgs e)
        {
            var filePath = GetPdfSavePath();
            if (filePath != null)
                _vm.ExportPlotAsPdf(filePath);
        }

        private string GetPdfSavePath()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export to PDF",
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == true)
                return saveFileDialog.FileName;
            else
                return null;
        }
    }
}
