using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;
    private readonly ViewAnalyzer _viewAnalyzer;
    private readonly ViewScoreCalculator _viewScoreCalculator;
    private readonly ViewNecessityAnalyzer _viewNecessityAnalyzer;
    private readonly ViewDecisionReport _viewDecisionReport;

    public DrawingManager(Inventor.Application inventor)
    {
        _inventor = inventor;

        _viewAnalyzer = new ViewAnalyzer();
        _viewScoreCalculator = new ViewScoreCalculator();
        _viewNecessityAnalyzer = new ViewNecessityAnalyzer();
        _viewDecisionReport = new ViewDecisionReport();
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
                    baseView.Center.Y -
                    baseView.Height -
                    viewGap);

            DrawingView upperView =
                sheet.DrawingViews.AddProjectedView(
                    baseView,
                    temporaryUpperPosition,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporarySidePosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    baseView.Center.X +
                    baseView.Width +
                    viewGap,
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

            PrintDecisionReport(
                mainOrientationName,
                baseView,
                upperView,
                sideView);

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
                _viewAnalyzer.Analyze(temporaryView);

            double score =
                _viewScoreCalculator.Calculate(statistics);

            PrintOrientationStatistics(
                orientationNames[index],
                statistics,
                score);

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

    private static void PrintOrientationStatistics(
        string orientationName,
        ViewStatistics statistics,
        double score)
    {
        Console.WriteLine(
            $"{orientationName}: {score:F2}");

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
    }

    private void PrintDecisionReport(
        string mainOrientationName,
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView)
    {
        ViewStatistics mainStatistics =
            _viewAnalyzer.Analyze(baseView);

        ViewStatistics upperStatistics =
            _viewAnalyzer.Analyze(upperView);

        ViewStatistics sideStatistics =
            _viewAnalyzer.Analyze(sideView);

        double mainScore =
            _viewScoreCalculator.Calculate(mainStatistics);

        double upperScore =
            _viewScoreCalculator.Calculate(upperStatistics);

        double sideScore =
            _viewScoreCalculator.Calculate(sideStatistics);

        ViewNecessityResult necessityResult =
            _viewNecessityAnalyzer.Analyze(
                mainStatistics,
                upperStatistics,
                sideStatistics,
                mainScore,
                upperScore,
                sideScore);

        string report =
            _viewDecisionReport.Build(
                mainOrientationName,
                mainStatistics,
                upperStatistics,
                sideStatistics,
                mainScore,
                upperScore,
                sideScore,
                necessityResult);

        Console.WriteLine(report);
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

        double denominator =
            1.0 / scale;

        return $"1:{denominator:0.###}";
    }
}