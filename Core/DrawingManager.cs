using Inventor;
using System.Collections.Generic;

namespace AI_CAD_ENGINEER.Core;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;

    public DrawingManager(Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public bool CreateDrawingWithViews(Document modelDocument)
    {
        try
        {
            DrawingDocument drawingDocument =
                (DrawingDocument)_inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    "",
                    true);

            Sheet sheet = drawingDocument.ActiveSheet;

            ViewOrientationTypeEnum mainOrientation =
                SelectMainViewOrientation(
                    modelDocument,
                    sheet,
                    out string mainOrientationName);

            Console.WriteLine();
            Console.WriteLine(
                $"Выбран главный вид: {mainOrientationName}");

            const double viewGap = 2.5;

            Point2d temporaryBasePosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    sheet.Width / 2,
                    sheet.Height / 2);

            DrawingView baseView =
                sheet.DrawingViews.AddBaseView(
                    (Inventor._Document)modelDocument,
                    temporaryBasePosition,
                    1.0,
                    mainOrientation,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporaryUpperPosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    baseView.Center.X,
                    baseView.Center.Y - baseView.Height - viewGap);

            DrawingView upperView =
                sheet.DrawingViews.AddProjectedView(
                    baseView,
                    temporaryUpperPosition,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporarySidePosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    baseView.Center.X + baseView.Width + viewGap,
                    baseView.Center.Y);

            DrawingView sideView =
                sheet.DrawingViews.AddProjectedView(
                    baseView,
                    temporarySidePosition,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            drawingDocument.Update();

            double selectedScale =
                CalculateGostScale(
                    baseView,
                    upperView,
                    sideView,
                    sheet,
                    viewGap);

            baseView.Scale = selectedScale;

            drawingDocument.Update();

            ArrangeViews(
                baseView,
                upperView,
                sideView,
                sheet,
                viewGap);

            drawingDocument.Update();
            drawingDocument.Activate();

            Console.WriteLine(
                $"Выбран масштаб: {FormatScale(selectedScale)}");

            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"Ошибка Inventor API: {exception.Message}");

            return false;
        }
    }

    private ViewOrientationTypeEnum SelectMainViewOrientation(
        Document modelDocument,
        Sheet sheet,
        out string selectedOrientationName)
    {
        ViewOrientationTypeEnum[] orientations =
        {
            ViewOrientationTypeEnum.kFrontViewOrientation,
            ViewOrientationTypeEnum.kBackViewOrientation,
            ViewOrientationTypeEnum.kTopViewOrientation,
            ViewOrientationTypeEnum.kBottomViewOrientation,
            ViewOrientationTypeEnum.kLeftViewOrientation,
            ViewOrientationTypeEnum.kRightViewOrientation
        };

        string[] orientationNames =
        {
            "Front",
            "Back",
            "Top",
            "Bottom",
            "Left",
            "Right"
        };

        double bestScore = double.MinValue;

        ViewOrientationTypeEnum bestOrientation =
            ViewOrientationTypeEnum.kFrontViewOrientation;

        selectedOrientationName = "Front";

        Console.WriteLine();
        Console.WriteLine("Оценка стандартных проекций:");
        Console.WriteLine("--------------------------------");

        for (int index = 0;
             index < orientations.Length;
             index++)
        {
            Point2d temporaryPosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    sheet.Width / 2,
                    sheet.Height / 2);

            DrawingView temporaryView =
                sheet.DrawingViews.AddBaseView(
                    (Inventor._Document)modelDocument,
                    temporaryPosition,
                    1.0,
                    orientations[index],
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            temporaryView.Parent.Parent.Update();

            ViewStatistics statistics =
                CollectViewStatistics(temporaryView);

            double score =
                CalculateViewScore(statistics);

            Console.WriteLine(
                $"{orientationNames[index]}: {score:F2}");

            Console.WriteLine(
                $"  Кривые: {statistics.CurveCount}");

            Console.WriteLine(
                $"  Линии: {statistics.LineCount}");

            Console.WriteLine(
                $"  Окружности: {statistics.CircleCount}");

            Console.WriteLine(
                $"  Дуги: {statistics.ArcCount}");

            Console.WriteLine(
                $"  Эллиптические дуги: " +
                $"{statistics.EllipticalArcCount}");

            Console.WriteLine(
                $"  Площадь: {statistics.Area:F2}");

            Console.WriteLine();

            if (score > bestScore)
            {
                bestScore = score;
                bestOrientation = orientations[index];
                selectedOrientationName =
                    orientationNames[index];
            }

            temporaryView.Delete();
        }

        Console.WriteLine("--------------------------------");

        return bestOrientation;
    }

    private static ViewStatistics CollectViewStatistics(
        DrawingView drawingView)
    {
        ViewStatistics statistics = new()
        {
            Width = drawingView.Width,
            Height = drawingView.Height,
            Area = drawingView.Width * drawingView.Height
        };

        if (drawingView.Height > 0)
        {
            statistics.AspectRatio =
                drawingView.Width / drawingView.Height;
        }

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            return statistics;
        }

        statistics.CurveCount = curves.Count;

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                statistics.SegmentCount++;

                string geometryType =
                    segment.GeometryType.ToString();

                switch (geometryType)
                {
                    case "kLineSegmentCurve2d":
                        statistics.LineCount++;
                        break;

                    case "kCircleCurve2d":
                        statistics.CircleCount++;
                        break;

                    case "kCircularArcCurve2d":
                        statistics.ArcCount++;
                        break;

                    case "kEllipticalArcCurve2d":
                        statistics.EllipticalArcCount++;
                        break;

                    default:
                        statistics.OtherGeometryCount++;
                        break;
                }
            }
        }

        return statistics;
    }

    private static double CalculateViewScore(
        ViewStatistics statistics)
    {
        double score = 0;

        // Базовая информативность проекции.
        score += statistics.LineCount;
        score += statistics.ArcCount * 2.0;
        score += statistics.EllipticalArcCount * 2.0;

        // Окружности часто показывают отверстия,
        // поэтому получают повышенный вес.
        score += statistics.CircleCount * 8.0;

        // Небольшой бонус за площадь проекции.
        score += statistics.Area * 0.05;

        // Штраф за очень вытянутые проекции.
        if (statistics.AspectRatio > 8.0)
        {
            score *= 0.75;
        }
        else if (statistics.AspectRatio > 12.0)
        {
            score *= 0.50;
        }

        return score;
    }

    private void ArrangeViews(
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView,
        Sheet sheet,
        double viewGap)
    {
        const double leftMargin = 2.0;
        const double rightMargin = 2.0;
        const double topMargin = 2.0;
        const double bottomReservedArea = 6.0;

        double workingLeft = leftMargin;
        double workingBottom = bottomReservedArea;

        double workingWidth =
            sheet.Width -
            leftMargin -
            rightMargin;

        double workingHeight =
            sheet.Height -
            topMargin -
            bottomReservedArea;

        double groupWidth =
            baseView.Width +
            viewGap +
            sideView.Width;

        double groupHeight =
            upperView.Height +
            viewGap +
            baseView.Height;

        double groupLeft =
            workingLeft +
            Math.Max(
                0,
                (workingWidth - groupWidth) / 2);

        double groupBottom =
            workingBottom +
            Math.Max(
                0,
                (workingHeight - groupHeight) / 2);

        double baseCenterX =
            groupLeft +
            baseView.Width / 2;

        double upperCenterY =
            groupBottom +
            upperView.Height / 2;

        double baseCenterY =
            groupBottom +
            upperView.Height +
            viewGap +
            baseView.Height / 2;

        double sideCenterX =
            groupLeft +
            baseView.Width +
            viewGap +
            sideView.Width / 2;

        upperView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                baseCenterX,
                upperCenterY);

        baseView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                baseCenterX,
                baseCenterY);

        sideView.Position =
            _inventor.TransientGeometry.CreatePoint2d(
                sideCenterX,
                baseCenterY);
    }

    private static double CalculateGostScale(
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView,
        Sheet sheet,
        double viewGap)
    {
        const double leftMargin = 2.0;
        const double rightMargin = 2.0;
        const double topMargin = 2.0;
        const double bottomReservedArea = 6.0;

        double availableWidth =
            sheet.Width -
            leftMargin -
            rightMargin;

        double availableHeight =
            sheet.Height -
            topMargin -
            bottomReservedArea;

        double viewsWidthAtScaleOne =
            baseView.Width +
            sideView.Width;

        double viewsHeightAtScaleOne =
            baseView.Height +
            upperView.Height;

        if (viewsWidthAtScaleOne <= 0 ||
            viewsHeightAtScaleOne <= 0)
        {
            return 0.1;
        }

        double maximumScaleByWidth =
            (availableWidth - viewGap) /
            viewsWidthAtScaleOne;

        double maximumScaleByHeight =
            (availableHeight - viewGap) /
            viewsHeightAtScaleOne;

        double maximumScale =
            Math.Min(
                maximumScaleByWidth,
                maximumScaleByHeight);

        double[] gostScales =
        {
            100.0,
            50.0,
            40.0,
            20.0,
            10.0,
            5.0,
            4.0,
            2.5,
            2.0,
            1.0,
            0.5,
            0.4,
            0.25,
            0.2,
            0.1,
            1.0 / 15.0,
            0.05,
            0.04,
            0.025,
            0.02,
            1.0 / 75.0,
            0.01,
            0.005,
            0.0025,
            0.002,
            0.00125,
            0.001
        };

        foreach (double scale in gostScales)
        {
            if (scale <= maximumScale)
            {
                return scale;
            }
        }

        return 0.001;
    }

    private static string FormatScale(double scale)
    {
        if (scale >= 1.0)
        {
            return $"{scale:0.###}:1";
        }

        double denominator = 1.0 / scale;

        return $"1:{denominator:0.###}";
    }

    private sealed class ViewStatistics
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public double Area { get; set; }

        public double AspectRatio { get; set; }

        public int CurveCount { get; set; }

        public int SegmentCount { get; set; }

        public int LineCount { get; set; }

        public int CircleCount { get; set; }

        public int ArcCount { get; set; }

        public int EllipticalArcCount { get; set; }

        public int OtherGeometryCount { get; set; }
    }
}