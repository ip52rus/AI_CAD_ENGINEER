using AI_CAD_ENGINEER.Engineering.Models;
using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Analysis;

public class OverallDimensionCandidateGenerator
{
    private const double CentimetersToMillimeters = 10.0;

    private readonly OverallDimensionAnalyzer
        _overallDimensionAnalyzer;

    public OverallDimensionCandidateGenerator()
    {
        _overallDimensionAnalyzer =
            new OverallDimensionAnalyzer();
    }

    public List<DimensionCandidate> Generate(
        DrawingView drawingView,
        ViewAxisMapping axisMapping)
    {
        ArgumentNullException.ThrowIfNull(
            drawingView);

        ArgumentNullException.ThrowIfNull(
            axisMapping);

        if (drawingView.Scale <= 0)
        {
            throw new InvalidOperationException(
                "Масштаб чертёжного вида должен быть больше нуля.");
        }

        if (axisMapping.HorizontalAxis ==
            ModelAxis.Undefined)
        {
            throw new InvalidOperationException(
                "Горизонтальная физическая ось вида не определена.");
        }

        if (axisMapping.VerticalAxis ==
            ModelAxis.Undefined)
        {
            throw new InvalidOperationException(
                "Вертикальная физическая ось вида не определена.");
        }

        var bounds =
            _overallDimensionAnalyzer.Analyze(
                drawingView);

        double widthOnSheet =
            bounds.MaxX -
            bounds.MinX;

        double heightOnSheet =
            bounds.MaxY -
            bounds.MinY;

        double modelWidthMillimeters =
            widthOnSheet /
            drawingView.Scale *
            CentimetersToMillimeters;

        double modelHeightMillimeters =
            heightOnSheet /
            drawingView.Scale *
            CentimetersToMillimeters;

        DimensionCandidate horizontalCandidate =
            new()
            {
                Name =
                    "OverallHorizontal",

                Type =
                    DimensionCandidateType.Horizontal,

                PhysicalAxis =
                    axisMapping.HorizontalAxis,

                Value =
                    modelWidthMillimeters,

                StartX =
                    bounds.MinX,

                StartY =
                    bounds.MinY,

                EndX =
                    bounds.MaxX,

                EndY =
                    bounds.MinY,

                IsOverallDimension =
                    true,

                IsRequired =
                    false,

                SourceViewName =
                    drawingView.Name,

                DecisionReason =
                    "Кандидат горизонтального " +
                    "габаритного размера."
            };

        DimensionCandidate verticalCandidate =
            new()
            {
                Name =
                    "OverallVertical",

                Type =
                    DimensionCandidateType.Vertical,

                PhysicalAxis =
                    axisMapping.VerticalAxis,

                Value =
                    modelHeightMillimeters,

                StartX =
                    bounds.MinX,

                StartY =
                    bounds.MinY,

                EndX =
                    bounds.MinX,

                EndY =
                    bounds.MaxY,

                IsOverallDimension =
                    true,

                IsRequired =
                    false,

                SourceViewName =
                    drawingView.Name,

                DecisionReason =
                    "Кандидат вертикального " +
                    "габаритного размера."
            };

        return
        [
            horizontalCandidate,
            verticalCandidate
        ];
    }
}