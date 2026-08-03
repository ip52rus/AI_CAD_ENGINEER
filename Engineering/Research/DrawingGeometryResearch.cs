using System.Text;
using AI_CAD_ENGINEER.Engineering.Analysis;
using AI_CAD_ENGINEER.Engineering.Models;
using AI_CAD_ENGINEER.Infrastructure.Reporting;
using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Research;

public class DrawingGeometryResearch
{
    private readonly IReporter _fileReporter;

    private readonly OverallDimensionAnalyzer
        _overallDimensionAnalyzer;

    private readonly OverallDimensionCandidateGenerator
        _overallDimensionCandidateGenerator;

    public DrawingGeometryResearch()
    {
        _fileReporter =
            new FileReporter();

        _overallDimensionAnalyzer =
            new OverallDimensionAnalyzer();

        _overallDimensionCandidateGenerator =
            new OverallDimensionCandidateGenerator();
    }

    public void Analyze(
        DrawingView drawingView,
        ViewAxisMapping axisMapping)
    {
        ArgumentNullException.ThrowIfNull(
            drawingView);

        ArgumentNullException.ThrowIfNull(
            axisMapping);

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            Console.WriteLine(
                "DrawingCurves отсутствуют.");

            return;
        }

        var overall =
            _overallDimensionAnalyzer.Analyze(
                drawingView);

        List<DimensionCandidate> dimensionCandidates =
            _overallDimensionCandidateGenerator.Generate(
                drawingView,
                axisMapping);

        double overallWidth =
            overall.MaxX -
            overall.MinX;

        double overallHeight =
            overall.MaxY -
            overall.MinY;

        StringBuilder report =
            new();

        report.AppendLine(
            "========================================");

        report.AppendLine(
            "DRAWING GEOMETRY RESEARCH");

        report.AppendLine(
            "========================================");

        report.AppendLine();

        report.AppendLine(
            $"Вид: {drawingView.Name}");

        report.AppendLine(
            $"Роль вида: {axisMapping.ViewRole}");

        report.AppendLine(
            $"Горизонтальная физическая ось: " +
            $"{axisMapping.HorizontalAxis}");

        report.AppendLine(
            $"Вертикальная физическая ось: " +
            $"{axisMapping.VerticalAxis}");

        report.AppendLine(
            $"Масштаб: {drawingView.Scale:F6}");

        report.AppendLine(
            $"Количество DrawingCurve: {curves.Count}");

        report.AppendLine();

        report.AppendLine(
            "ГАБАРИТЫ ВИДА");

        report.AppendLine(
            "----------------------------------------");

        report.AppendLine(
            $"Min X: {overall.MinX:F3}");

        report.AppendLine(
            $"Max X: {overall.MaxX:F3}");

        report.AppendLine(
            $"Min Y: {overall.MinY:F3}");

        report.AppendLine(
            $"Max Y: {overall.MaxY:F3}");

        report.AppendLine(
            $"Ширина по геометрии: {overallWidth:F3}");

        report.AppendLine(
            $"Высота по геометрии: {overallHeight:F3}");

        report.AppendLine(
            $"DrawingView.Width: {drawingView.Width:F3}");

        report.AppendLine(
            $"DrawingView.Height: {drawingView.Height:F3}");

        report.AppendLine(
            $"Разница по ширине: " +
            $"{Math.Abs(
                drawingView.Width -
                overallWidth):F6}");

        report.AppendLine(
            $"Разница по высоте: " +
            $"{Math.Abs(
                drawingView.Height -
                overallHeight):F6}");

        report.AppendLine();

        report.AppendLine(
            "КАНДИДАТЫ ГАБАРИТНЫХ РАЗМЕРОВ");

        report.AppendLine(
            "----------------------------------------");

        foreach (DimensionCandidate candidate
                 in dimensionCandidates)
        {
            report.AppendLine(
                $"Имя: {candidate.Name}");

            report.AppendLine(
                $"Тип: {candidate.Type}");

            report.AppendLine(
                $"Физическая ось: " +
                $"{candidate.PhysicalAxis}");

            report.AppendLine(
                $"Роль: {candidate.OverallRole}");

            report.AppendLine(
                $"Статус: {candidate.Status}");

            report.AppendLine(
                $"Значение модели: " +
                $"{candidate.Value:F3} мм");

            report.AppendLine(
                $"Начало: " +
                $"X={candidate.StartX:F3}; " +
                $"Y={candidate.StartY:F3}");

            report.AppendLine(
                $"Конец: " +
                $"X={candidate.EndX:F3}; " +
                $"Y={candidate.EndY:F3}");

            report.AppendLine(
                $"Габаритный: " +
                $"{FormatBoolean(
                    candidate.IsOverallDimension)}");

            report.AppendLine(
                $"Обязательный: " +
                $"{FormatBoolean(
                    candidate.IsRequired)}");

            report.AppendLine(
                $"Вид: {candidate.SourceViewName}");

            report.AppendLine(
                $"Причина: {candidate.DecisionReason}");

            report.AppendLine();
        }

        report.AppendLine(
            "ЛИНЕЙНЫЕ СЕГМЕНТЫ");

        report.AppendLine(
            "----------------------------------------");

        int lineCount = 0;
        int circleCount = 0;

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                if (segment.HiddenLine)
                {
                    continue;
                }

                if (segment.Geometry
                    is not LineSegment2d lineSegment)
                {
                    continue;
                }

                lineCount++;

                double deltaX =
                    lineSegment.EndPoint.X -
                    lineSegment.StartPoint.X;

                double deltaY =
                    lineSegment.EndPoint.Y -
                    lineSegment.StartPoint.Y;

                double length =
                    Math.Sqrt(
                        deltaX * deltaX +
                        deltaY * deltaY);

                string direction =
                    GetLineDirection(
                        deltaX,
                        deltaY);

                report.AppendLine(
                    $"{lineCount}. {direction}");

                report.AppendLine(
                    $"   Начало: " +
                    $"X={lineSegment.StartPoint.X:F3}; " +
                    $"Y={lineSegment.StartPoint.Y:F3}");

                report.AppendLine(
                    $"   Конец: " +
                    $"X={lineSegment.EndPoint.X:F3}; " +
                    $"Y={lineSegment.EndPoint.Y:F3}");

                report.AppendLine(
                    $"   Длина на листе: {length:F3}");

                report.AppendLine(
                    $"   CurveType: {curve.CurveType}");

                report.AppendLine(
                    $"   ProjectedCurveType: " +
                    $"{curve.ProjectedCurveType}");

                report.AppendLine();
            }
        }

        report.AppendLine();

        report.AppendLine(
            "ОКРУЖНОСТИ");

        report.AppendLine(
            "----------------------------------------");

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                if (segment.HiddenLine)
                {
                    continue;
                }

                if (segment.Geometry
                    is not Circle2d circle)
                {
                    continue;
                }

                circleCount++;

                report.AppendLine(
                    $"{circleCount}. Окружность");

                report.AppendLine(
                    $"   Центр: " +
                    $"X={circle.Center.X:F3}; " +
                    $"Y={circle.Center.Y:F3}");

                report.AppendLine(
                    $"   Радиус на листе: " +
                    $"{circle.Radius:F3}");

                report.AppendLine(
                    $"   Диаметр на листе: " +
                    $"{circle.Radius * 2.0:F3}");

                report.AppendLine(
                    $"   CurveType: {curve.CurveType}");

                report.AppendLine(
                    $"   ProjectedCurveType: " +
                    $"{curve.ProjectedCurveType}");

                report.AppendLine();
            }
        }

        report.AppendLine();

        report.AppendLine(
            $"Видимых линейных сегментов: {lineCount}");

        report.AppendLine(
            $"Видимых окружностей: {circleCount}");

        report.AppendLine(
            $"Кандидатов размеров: " +
            $"{dimensionCandidates.Count}");

        report.AppendLine(
            "========================================");

        string reportPath =
            _fileReporter.Write(
                ReportCategory.Research,
                nameof(DrawingGeometryResearch),
                report.ToString());

        Console.WriteLine();

        Console.WriteLine(
            "Исследование геометрии завершено.");

        Console.WriteLine(
            $"Роль вида: {axisMapping.ViewRole}");

        Console.WriteLine(
            $"Оси вида: " +
            $"{axisMapping.HorizontalAxis} × " +
            $"{axisMapping.VerticalAxis}");

        Console.WriteLine(
            $"DrawingCurve: {curves.Count}");

        Console.WriteLine(
            $"Видимых линий: {lineCount}");

        Console.WriteLine(
            $"Видимых окружностей: {circleCount}");

        Console.WriteLine(
            $"Габариты по геометрии: " +
            $"{overallWidth:F3} x " +
            $"{overallHeight:F3}");

        Console.WriteLine(
            $"Габариты DrawingView: " +
            $"{drawingView.Width:F3} x " +
            $"{drawingView.Height:F3}");

        Console.WriteLine(
            $"Кандидатов размеров: " +
            $"{dimensionCandidates.Count}");

        foreach (DimensionCandidate candidate
                 in dimensionCandidates)
        {
            Console.WriteLine(
                $"{candidate.Name}: " +
                $"{candidate.Value:F3} мм; " +
                $"ось: {candidate.PhysicalAxis}");
        }

        Console.WriteLine(
            "Полный отчёт сохранён:");

        Console.WriteLine(
            reportPath);
    }

    private static string GetLineDirection(
        double deltaX,
        double deltaY)
    {
        const double tolerance = 0.0001;

        if (Math.Abs(deltaY) <= tolerance)
        {
            return "Горизонтальная линия";
        }

        if (Math.Abs(deltaX) <= tolerance)
        {
            return "Вертикальная линия";
        }

        return "Наклонная линия";
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}