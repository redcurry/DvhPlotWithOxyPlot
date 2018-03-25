using System.Collections.Generic;
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

            Structures = GetPlatStructures();
            PlotModel = CreatePlotModel();
        }

        public IEnumerable<Structure> Structures { get; }

        public PlotModel PlotModel { get; }

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

        private IEnumerable<Structure> GetPlatStructures()
        {
            return _plan.StructureSet?.Structures;
        }

        private PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel();
            plotModel.Axes.Add(new LinearAxis {Title = "Dose [Gy]", Position = AxisPosition.Bottom});
            plotModel.Axes.Add(new LinearAxis {Title = "Volume [cc]", Position = AxisPosition.Left});
            return plotModel;
        }

        private DVHData CalculateDvh(Structure structure)
        {
            return _plan.GetDVHCumulativeData(structure,
                DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.01);
        }

        private Series CreateDvhSeries(string structureId, DVHData dvh)
        {
            var series = new LineSeries {Tag = structureId};
            series.Points.AddRange(dvh.CurveData.Select(x => new DataPoint(x.DoseValue.Dose, x.Volume)));
            return series;
        }

        private Series FindSeries(string structureId)
        {
            return PlotModel.Series.FirstOrDefault(x => (string)x.Tag == structureId);
        }

        private void UpdatePlot()
        {
            PlotModel.InvalidatePlot(true);
        }
    }
}