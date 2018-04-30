using System;
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
            var xAxis = new LinearAxis
                {Title = "X", Position = AxisPosition.Bottom,};
            plotModel.Axes.Add(xAxis);

            var yAxis = new LinearAxis
                {Title = "Y", Position = AxisPosition.Left};
            plotModel.Axes.Add(yAxis);

            var zAxis = new LinearColorAxis
            {
                Title = "Dose [Gy]",
                Position = AxisPosition.Top,
                Palette = OxyPalettes.Rainbow(256),
                Maximum = 33.1
            };
            plotModel.Axes.Add(zAxis);
        }

        private void AddSeries(PlotModel plotModel)
        {
            plotModel.Series.Add(CreateHeatMap());
        }

        private Series CreateHeatMap()
        {
            return new HeatMapSeries
            {
                X0 = 0, X1 = _plan.Dose.XSize - 1,
                Y0 = 0, Y1 = _plan.Dose.YSize - 1,
                Data = GetDoseData()
            };
        }

        private double[,] GetDoseData()
        {
            _plan.DoseValuePresentation = DoseValuePresentation.Absolute;
            var dose = _plan.Dose;
            var data = new int[dose.XSize, dose.YSize];
            dose.GetVoxels((int)PlaneIndex, data);
            return ConvertToDoseMatrix(data);
        }

        private double[,] ConvertToDoseMatrix(int[,] ints)
        {
            var dose = _plan.Dose;
            var doseMatrix = new double[dose.XSize, dose.YSize];
            for (int i = 0; i < dose.XSize; i++)
                for (int j = 0; j < dose.YSize; j++)
                    doseMatrix[i, j] = dose.VoxelToDoseValue(ints[i, j]).Dose;
            return doseMatrix;
        }

        private double _planeIndex;
        public double PlaneIndex
        {
            get { return _planeIndex; }
            set
            {
                _planeIndex = value;
                PlotModel.Series[0] = CreateHeatMap();
                PlotModel.InvalidatePlot(false);
            }
        }

        public int MaximumPlaneIndex
        {
            get { return _plan.Dose.ZSize - 1; }
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