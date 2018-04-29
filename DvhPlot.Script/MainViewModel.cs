using System.Collections.Generic;
using System.IO;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Series = OxyPlot.Series.Series;

namespace DvhPlot.Script
{
    public class MainViewModel
    {
        private readonly PlanSetup _plan;

        public MainViewModel(PlanSetup plan)
        {
            _plan = plan;

            Structures = GetPlanStructures();
            PlotModel = CreatePlotModel();
        }

        public IEnumerable<Structure> Structures { get; private set; }

        public PlotModel PlotModel { get; private set; }

        public void AddDvhCurve(Structure structure)
        {
            var dvh = CalculateDvh(structure);
            PlotModel.Series.Add(CreateDvhSeries(structure.Id, dvh));
            UpdatePlot();
        }

        public void RemoveDvhCurve(Structure structure)
        {
            var series = FindSeries(structure.Id);
            PlotModel.Series.Remove(series);
            UpdatePlot();
        }

        public void ExportPlotAsPdf(string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                PdfExporter.Export(PlotModel, stream, 600, 400);
            }
        }

        private IEnumerable<Structure> GetPlanStructures()
        {
            return _plan.StructureSet != null
                ? _plan.StructureSet.Structures
                : null;
        }

        private PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel();
            AddAxes(plotModel);
            AddSeries(plotModel);
            return plotModel;
        }

        private void AddAxes(PlotModel plotModel)
        {
            var xAxis = new CategoryAxis
                {Title = "Beam", Position = AxisPosition.Bottom};
            var beams = _plan.Beams.Where(b => !b.IsSetupField).Take(5);
            var beamIds = new[] {"A-0", "A-72", "A-144", "A-216", "A-288"};
            xAxis.Labels.AddRange(beamIds);
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
                {Title = "Meterset [MU]", Position = AxisPosition.Left};
            plotModel.Axes.Add(yAxis);
        }

        private void AddSeries(PlotModel plotModel)
        {
            var series = new ColumnSeries();
            var beams = _plan.Beams.Where(b => !b.IsSetupField).Take(5);
            var items = beams.Select(b => new ColumnItem(b.Meterset.Value));
            series.Items.AddRange(items);
            plotModel.Series.Add(series);
        }

        private DVHData CalculateDvh(Structure structure)
        {
            return _plan.GetDVHCumulativeData(structure,
                DoseValuePresentation.Absolute,
                VolumePresentation.AbsoluteCm3, 0.1);
        }

        private Series CreateDvhSeries(string structureId, DVHData dvh)
        {
            var series = new ScatterSeries {Tag = structureId};
            var points = dvh.CurveData.Select(CreateDataPoint);
            series.Points.AddRange(points);
            return series;
        }

        private ScatterPoint CreateDataPoint(DVHPoint p)
        {
            return new ScatterPoint(p.DoseValue.Dose, p.Volume);
        }

        private Series FindSeries(string structureId)
        {
            return PlotModel.Series.FirstOrDefault(x =>
                (string)x.Tag == structureId);
        }

        private void UpdatePlot()
        {
            PlotModel.InvalidatePlot(true);
        }
    }
}