using AI_CAD_ENGINEER.Engineering.Analysis;
using AI_CAD_ENGINEER.Engineering.Models;
using Inventor;

namespace AI_CAD_ENGINEER.Drawing;

public class OverallDimensionGenerator
{
    private readonly Inventor.Application _inventor;

    private readonly OverallDimensionCandidateGenerator
        _candidateGenerator;

    public OverallDimensionGenerator(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;

        _candidateGenerator =
            new OverallDimensionCandidateGenerator();
    }

    public List<DimensionCandidate> GenerateCandidates(
        DrawingView drawingView,
        ViewAxisMapping axisMapping)
    {
        ArgumentNullException.ThrowIfNull(
            drawingView);

        ArgumentNullException.ThrowIfNull(
            axisMapping);

        return _candidateGenerator.Generate(
            drawingView,
            axisMapping);
    }

    public void Create(
        DrawingView drawingView,
        IEnumerable<DimensionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(
            drawingView);

        ArgumentNullException.ThrowIfNull(
            candidates);

        Sheet sheet =
            drawingView.Parent;

        GeneralDimensions generalDimensions =
            sheet.DrawingDimensions.GeneralDimensions;

        List<DimensionCandidate> selectedCandidates =
            candidates.ToList();

        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "СОЗДАНИЕ ГАБАРИТНЫХ РАЗМЕРОВ");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Вид: {drawingView.Name}");

        Console.WriteLine(
            $"Выбрано размеров: " +
            $"{selectedCandidates.Count}");

        Console.WriteLine(
            $"Размеров на листе до создания: " +
            $"{generalDimensions.Count}");

        DimensionCandidate? horizontalCandidate =
            selectedCandidates.FirstOrDefault(
                candidate =>
                    candidate.Type ==
                    DimensionCandidateType.Horizontal);

        DimensionCandidate? verticalCandidate =
            selectedCandidates.FirstOrDefault(
                candidate =>
                    candidate.Type ==
                    DimensionCandidateType.Vertical);

        if (horizontalCandidate != null)
        {
            CreateHorizontalDimension(
                sheet,
                drawingView,
                horizontalCandidate);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine(
                "Горизонтальный габаритный размер пропущен.");
        }

        if (verticalCandidate != null)
        {
            CreateVerticalDimension(
                sheet,
                drawingView,
                verticalCandidate);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine(
                "Вертикальный габаритный размер пропущен.");
        }

        Console.WriteLine(
            $"Размеров на листе после создания: " +
            $"{generalDimensions.Count}");

        Console.WriteLine(
            "========================================");
    }

    private void CreateHorizontalDimension(
        Sheet sheet,
        DrawingView drawingView,
        DimensionCandidate candidate)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Горизонтальный габаритный размер:");

        Console.WriteLine(
            $"  Физическая ось: " +
            $"{candidate.PhysicalAxis}");

        Console.WriteLine(
            $"  Роль: " +
            $"{candidate.OverallRole}");

        Console.WriteLine(
            $"  Ожидаемое значение: " +
            $"{candidate.Value:F3} мм");

        DrawingCurve? leftCurve =
            FindVerticalCurveAtX(
                drawingView,
                candidate.StartX);

        DrawingCurve? rightCurve =
            FindVerticalCurveAtX(
                drawingView,
                candidate.EndX);

        Console.WriteLine(
            $"  Левая крайняя кривая: " +
            $"{FormatFound(leftCurve)}");

        Console.WriteLine(
            $"  Правая крайняя кривая: " +
            $"{FormatFound(rightCurve)}");

        if (leftCurve == null ||
            rightCurve == null)
        {
            throw new InvalidOperationException(
                "Не удалось найти крайние вертикальные " +
                "кривые для горизонтального размера.");
        }

        GeometryIntent leftIntent =
            sheet.CreateGeometryIntent(
                leftCurve);

        GeometryIntent rightIntent =
            sheet.CreateGeometryIntent(
                rightCurve);

        Console.WriteLine(
            "  Оба GeometryIntent созданы.");

        double textX =
            (candidate.StartX +
             candidate.EndX) /
            2.0;

        double textY =
            candidate.StartY -
            1.0;

        Point2d textPosition =
            _inventor.TransientGeometry
                .CreatePoint2d(
                    textX,
                    textY);

        Console.WriteLine(
            $"  Положение текста: " +
            $"X={textX:F3}; Y={textY:F3}");

        LinearGeneralDimension createdDimension =
            sheet.DrawingDimensions
                .GeneralDimensions
                .AddLinear(
                    textPosition,
                    leftIntent,
                    rightIntent,
                    DimensionTypeEnum
                        .kHorizontalDimensionType);

        ConfigureMetricDimension(
            createdDimension);

        Console.WriteLine(
            "  Горизонтальный размер создан.");

        Console.WriteLine(
            "  Единицы размера: миллиметры.");
    }

    private void CreateVerticalDimension(
        Sheet sheet,
        DrawingView drawingView,
        DimensionCandidate candidate)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Вертикальный габаритный размер:");

        Console.WriteLine(
            $"  Физическая ось: " +
            $"{candidate.PhysicalAxis}");

        Console.WriteLine(
            $"  Роль: " +
            $"{candidate.OverallRole}");

        Console.WriteLine(
            $"  Ожидаемое значение: " +
            $"{candidate.Value:F3} мм");

        DrawingCurve? bottomCurve =
            FindHorizontalCurveAtY(
                drawingView,
                candidate.StartY);

        DrawingCurve? topCurve =
            FindHorizontalCurveAtY(
                drawingView,
                candidate.EndY);

        Console.WriteLine(
            $"  Нижняя крайняя кривая: " +
            $"{FormatFound(bottomCurve)}");

        Console.WriteLine(
            $"  Верхняя крайняя кривая: " +
            $"{FormatFound(topCurve)}");

        if (bottomCurve == null ||
            topCurve == null)
        {
            throw new InvalidOperationException(
                "Не удалось найти крайние горизонтальные " +
                "кривые для вертикального размера.");
        }

        GeometryIntent bottomIntent =
            sheet.CreateGeometryIntent(
                bottomCurve);

        GeometryIntent topIntent =
            sheet.CreateGeometryIntent(
                topCurve);

        Console.WriteLine(
            "  Оба GeometryIntent созданы.");

        double textX =
            candidate.StartX -
            1.0;

        double textY =
            (candidate.StartY +
             candidate.EndY) /
            2.0;

        Point2d textPosition =
            _inventor.TransientGeometry
                .CreatePoint2d(
                    textX,
                    textY);

        Console.WriteLine(
            $"  Положение текста: " +
            $"X={textX:F3}; Y={textY:F3}");

        LinearGeneralDimension createdDimension =
            sheet.DrawingDimensions
                .GeneralDimensions
                .AddLinear(
                    textPosition,
                    bottomIntent,
                    topIntent,
                    DimensionTypeEnum
                        .kVerticalDimensionType);

        ConfigureMetricDimension(
            createdDimension);

        Console.WriteLine(
            "  Вертикальный размер создан.");

        Console.WriteLine(
            "  Единицы размера: миллиметры.");
    }

    private static void ConfigureMetricDimension(
        LinearGeneralDimension dimension)
    {
        ArgumentNullException.ThrowIfNull(
            dimension);

        DimensionStyle dimensionStyle =
            dimension.Style;

        dimensionStyle.LinearUnits =
            UnitsTypeEnum.kMillimeterLengthUnits;

        dimension.Layer.Visible =
            true;
    }

    private static DrawingCurve?
        FindVerticalCurveAtX(
            DrawingView drawingView,
            double targetX)
    {
        const double tolerance = 0.0001;

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            return null;
        }

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
                    is not LineSegment2d line)
                {
                    continue;
                }

                bool isVertical =
                    Math.Abs(
                        line.StartPoint.X -
                        line.EndPoint.X) <=
                    tolerance;

                bool isAtTarget =
                    Math.Abs(
                        line.StartPoint.X -
                        targetX) <=
                    tolerance &&
                    Math.Abs(
                        line.EndPoint.X -
                        targetX) <=
                    tolerance;

                if (isVertical &&
                    isAtTarget)
                {
                    return curve;
                }
            }
        }

        return null;
    }

    private static DrawingCurve?
        FindHorizontalCurveAtY(
            DrawingView drawingView,
            double targetY)
    {
        const double tolerance = 0.0001;

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            return null;
        }

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
                    is not LineSegment2d line)
                {
                    continue;
                }

                bool isHorizontal =
                    Math.Abs(
                        line.StartPoint.Y -
                        line.EndPoint.Y) <=
                    tolerance;

                bool isAtTarget =
                    Math.Abs(
                        line.StartPoint.Y -
                        targetY) <=
                    tolerance &&
                    Math.Abs(
                        line.EndPoint.Y -
                        targetY) <=
                    tolerance;

                if (isHorizontal &&
                    isAtTarget)
                {
                    return curve;
                }
            }
        }

        return null;
    }

    private static string FormatFound(
        object? value)
    {
        return value == null
            ? "не найдена"
            : "найдена";
    }
}