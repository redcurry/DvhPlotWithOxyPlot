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
            var plotModel = new PlotModel
            {
                PlotAreaBackground = OxyColor.FromAColor(230, OxyColors.Black)
            };
            AddAxes(plotModel);
            SetupLegend(plotModel);
            return plotModel;
        }

        private static void AddAxes(PlotModel plotModel)
        {
            plotModel.Axes.Add(new LinearAxis
            {
                Title = "Dose [Gy]",
                TitleFontSize = 14,
                TitleFontWeight = FontWeights.Bold,
                AxisTitleDistance = 15,
                MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.White),
                MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Title = "Volume [cc]",
                TitleFontSize = 14,
                TitleFontWeight = FontWeights.Bold,
                AxisTitleDistance = 15,
                MajorGridlineColor = OxyColor.FromAColor(64, OxyColors.White),
                MinorGridlineColor = OxyColor.FromAColor(32, OxyColors.White),
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Left
            });
        }

        private void SetupLegend(PlotModel plotModel)
        {
            plotModel.LegendBorder = OxyColors.Black;
            plotModel.LegendBackground = OxyColor.FromAColor(32, OxyColors.Black);
            plotModel.LegendPosition = LegendPosition.BottomCenter;
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
            plotModel.LegendPlacement = LegendPlacement.Outside;
        }

        private DVHData CalculateDvh(Structure structure)
        {
            return _plan.GetDVHCumulativeData(structure,
                DoseValuePresentation.Absolute,
                VolumePresentation.AbsoluteCm3, 0.01);
        }

        private Series CreateDvhSeries(string structureId, DVHData dvh)
        {
            var series = new LineSeries
            {
                Title = structureId,
                Tag = structureId,
                Color = GetStructureColor(structureId),
                StrokeThickness = GetLineThickness(structureId),
                LineStyle = GetLineStyle(structureId)
            };
            var points = dvh.CurveData.Select(CreateDataPoint);
            series.Points.AddRange(points);
            return series;
        }

        private OxyColor GetStructureColor(string structureId)
        {
            var structures = _plan.StructureSet.Structures;
            var structure = structures.First(x => x.Id == structureId);
            var color = structure.Color;
            return OxyColor.FromRgb(color.R, color.G, color.B);
        }

        private double GetLineThickness(string structureId)
        {
            if (structureId.ToUpper().Contains("PTV"))
                return 5;
            return 2;
        }

        private LineStyle GetLineStyle(string structureId)
        {
            if (structureId.ToUpper().Contains("_R"))
                return LineStyle.Dash;
            return LineStyle.Solid;
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