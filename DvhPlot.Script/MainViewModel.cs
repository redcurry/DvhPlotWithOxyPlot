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
            return plotModel;
        }

        private static void AddAxes(PlotModel plotModel)
        {
            plotModel.Axes.Add(new LinearAxis
            {
                Title = "Dose [Gy]",
                Position = AxisPosition.Bottom
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Title = "Volume [cc]",
                Position = AxisPosition.Left
            });
        }

        private DVHData CalculateDvh(Structure structure)
        {
            return _plan.GetDVHCumulativeData(structure,
                DoseValuePresentation.Absolute,
                VolumePresentation.AbsoluteCm3, 0.01);
        }

        private Series CreateDvhSeries(string structureId, DVHData dvh)
        {
            var series = new LineSeries {Tag = structureId};
            var points = dvh.CurveData.Select(CreateDataPoint);
            series.Points.AddRange(points);
            return series;
        }

        private DataPoint CreateDataPoint(DVHPoint p)
        {
            return new DataPoint(p.DoseValue.Dose, p.Volume);
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