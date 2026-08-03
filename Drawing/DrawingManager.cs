using AI_CAD_ENGINEER.Engineering.Analysis;
using AI_CAD_ENGINEER.Engineering.Decision;
using AI_CAD_ENGINEER.Engineering.Models;
using AI_CAD_ENGINEER.Engineering.Research;
using AI_CAD_ENGINEER.Import.Inventor;
using Inventor;

namespace AI_CAD_ENGINEER.Drawing;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;

    private readonly ViewCandidateGenerator
        _viewCandidateGenerator;

    private readonly EngineeringBrain
        _engineeringBrain;

    private readonly ViewAnalyzer
        _viewAnalyzer;

    private readonly ViewScoreCalculator
        _viewScoreCalculator;

    private readonly ViewNecessityAnalyzer
        _viewNecessityAnalyzer;

    private readonly ViewDecisionReport
        _viewDecisionReport;

    private readonly CenterAnnotationGenerator
        _centerAnnotationGenerator;

    private readonly OverallDimensionGenerator
        _overallDimensionGenerator;

    private readonly OverallDimensionRoleResolver
        _overallDimensionRoleResolver;

    private readonly ViewAxisMappingResolver
        _viewAxisMappingResolver;

    private readonly DimensionDecisionCoordinator
        _dimensionDecisionCoordinator;

    private readonly DrawingGeometryResearch
        _drawingGeometryResearch;

    public DrawingManager(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;

        _viewCandidateGenerator =
            new ViewCandidateGenerator(
                inventor);

        _engineeringBrain =
            new EngineeringBrain();

        _viewAnalyzer =
            new ViewAnalyzer();

        _viewScoreCalculator =
            new ViewScoreCalculator();

        _viewNecessityAnalyzer =
            new ViewNecessityAnalyzer();

        _viewDecisionReport =
            new ViewDecisionReport();

        _centerAnnotationGenerator =
            new CenterAnnotationGenerator();

        _overallDimensionGenerator =
            new OverallDimensionGenerator(
                inventor);

        _overallDimensionRoleResolver =
            new OverallDimensionRoleResolver();

        _viewAxisMappingResolver =
            new ViewAxisMappingResolver();

        _dimensionDecisionCoordinator =
            new DimensionDecisionCoordinator();

        _drawingGeometryResearch =
            new DrawingGeometryResearch();
    }

    public bool CreateDrawingWithViews(
        Document modelDocument,
        PartAnalysis partAnalysis)
    {
        ArgumentNullException.ThrowIfNull(
            modelDocument);

        ArgumentNullException.ThrowIfNull(
            partAnalysis);

        try
        {
            ValidateModelDocument(
                modelDocument);

            DrawingDocument drawingDocument =
                (DrawingDocument)_inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    "",
                    true);

            Sheet sheet =
                drawingDocument.ActiveSheet;

            List<ViewCandidate> candidates =
                _viewCandidateGenerator.Generate(
                    modelDocument,
                    sheet);

            DrawingPlan drawingPlan =
                _engineeringBrain.CreateDrawingPlan(
                    partAnalysis,
                    candidates);

            Dictionary<DrawingViewRole, ViewAxisMapping>
                axisMappings =
                    _viewAxisMappingResolver.Resolve(
                        drawingPlan.MainView);

            ViewAxisMapping mainAxisMapping =
                axisMappings[
                    DrawingViewRole.Main];

            ViewAxisMapping verticalAxisMapping =
                axisMappings[
                    DrawingViewRole.VerticalProjection];

            ViewAxisMapping sideAxisMapping =
                axisMappings[
                    DrawingViewRole.SideProjection];

            ViewOrientationTypeEnum mainOrientation =
                ConvertToInventorOrientation(
                    drawingPlan.MainView);

            string mainOrientationName =
                GetOrientationName(
                    drawingPlan.MainView);

            Console.WriteLine();

            Console.WriteLine(
                $"EngineeringBrain выбрал главный вид: " +
                $"{mainOrientationName}");

            Console.WriteLine(
                $"Центровые линии требуются: " +
                $"{FormatBoolean(
                    drawingPlan.NeedCenterlines)}");

            Console.WriteLine(
                $"Размеры требуются: " +
                $"{FormatBoolean(
                    drawingPlan.NeedDimensions)}");

            Console.WriteLine(
                $"Разрез требуется: " +
                $"{FormatBoolean(
                    drawingPlan.NeedSection)}");

            PrintAxisMappings(
                axisMappings);

            const double viewGap =
                2.5;

            Point2d temporaryBasePosition =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        sheet.Width / 2.0,
                        sheet.Height / 2.0);

            DrawingView baseView =
                sheet.DrawingViews.AddBaseView(
                    (Inventor._Document)modelDocument,
                    temporaryBasePosition,
                    1.0,
                    mainOrientation,
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            Point2d temporaryUpperPosition =
                _inventor.TransientGeometry
                    .CreatePoint2d(
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
                _inventor.TransientGeometry
                    .CreatePoint2d(
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

            drawingPlan.Scale =
                selectedScale;

            baseView.Scale =
                selectedScale;

            drawingDocument.Update();

            ArrangeViews(
                baseView,
                upperView,
                sideView,
                sheet,
                viewGap);

            drawingDocument.Update();

            if (drawingPlan.NeedCenterlines)
            {
                _centerAnnotationGenerator.Create(
                    baseView,
                    upperView,
                    sideView);

                drawingDocument.Update();

                Console.WriteLine(
                    "Автоматические центровые элементы созданы.");
            }

            _drawingGeometryResearch.Analyze(
                baseView,
                mainAxisMapping);

            if (drawingPlan.NeedDimensions)
            {
                CreateOverallDimensions(
                    drawingDocument,
                    partAnalysis,
                    baseView,
                    upperView,
                    sideView,
                    mainAxisMapping,
                    verticalAxisMapping,
                    sideAxisMapping);
            }

            PrintDecisionReport(
                mainOrientationName,
                baseView,
                upperView,
                sideView);

            drawingDocument.Activate();

            Console.WriteLine(
                $"Выбран масштаб: " +
                $"{FormatScale(
                    drawingPlan.Scale)}");

            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Ошибка Inventor API: " +
                $"{exception.Message}");

            return false;
        }
    }

    private void CreateOverallDimensions(
        DrawingDocument drawingDocument,
        PartAnalysis partAnalysis,
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView,
        ViewAxisMapping mainAxisMapping,
        ViewAxisMapping verticalAxisMapping,
        ViewAxisMapping sideAxisMapping)
    {
        List<DimensionCandidate> baseCandidates =
            _overallDimensionGenerator
                .GenerateCandidates(
                    baseView,
                    mainAxisMapping);

        List<DimensionCandidate> upperCandidates =
            _overallDimensionGenerator
                .GenerateCandidates(
                    upperView,
                    verticalAxisMapping);

        List<DimensionCandidate> sideCandidates =
            _overallDimensionGenerator
                .GenerateCandidates(
                    sideView,
                    sideAxisMapping);

        Dictionary<string, List<DimensionCandidate>>
            candidatesByView =
                new()
                {
                    [baseView.Name] =
                        baseCandidates,

                    [upperView.Name] =
                        upperCandidates,

                    [sideView.Name] =
                        sideCandidates
                };

        Dictionary<string, List<DimensionCandidate>>
            selectedByView =
                _overallDimensionRoleResolver.Resolve(
                    partAnalysis,
                    candidatesByView);

        DimensionDecisionResult decisionResult =
            _dimensionDecisionCoordinator.Process(
                candidatesByView,
                selectedByView);

        PrintDimensionDecisionSummary(
            decisionResult);

        PrintOverallDimensionSelection(
            decisionResult.RequiredByView);

        Console.WriteLine();
        Console.WriteLine(
            "Создание размеров главного вида:");

        _overallDimensionGenerator.Create(
            baseView,
            decisionResult.RequiredByView[
                baseView.Name]);

        drawingDocument.Update();

        Console.WriteLine();
        Console.WriteLine(
            "Создание размеров вертикальной проекции:");

        _overallDimensionGenerator.Create(
            upperView,
            decisionResult.RequiredByView[
                upperView.Name]);

        drawingDocument.Update();

        Console.WriteLine();
        Console.WriteLine(
            "Создание размеров боковой проекции:");

        _overallDimensionGenerator.Create(
            sideView,
            decisionResult.RequiredByView[
                sideView.Name]);

        drawingDocument.Update();

        Console.WriteLine(
            "Габаритные размеры из результата " +
            "Dimension Decision Engine созданы.");
    }

    private static void PrintDimensionDecisionSummary(
        DimensionDecisionResult result)
    {
        ArgumentNullException.ThrowIfNull(
            result);

        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "ИТОГ DIMENSION DECISION ENGINE");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Всего кандидатов: " +
            $"{result.TotalCandidateCount}");

        Console.WriteLine(
            $"Обязательных: " +
            $"{result.RequiredCount}");

        Console.WriteLine(
            $"Дублирующих: " +
            $"{result.DuplicateCount}");

        Console.WriteLine(
            $"Избыточных: " +
            $"{result.RedundantCount}");

        Console.WriteLine(
            $"Справочных: " +
            $"{result.ReferenceCount}");

        Console.WriteLine(
            $"Необязательных: " +
            $"{result.OptionalCount}");

        Console.WriteLine(
            $"Отчёт: {result.ReportPath}");

        Console.WriteLine(
            "========================================");
    }

    private static void ValidateModelDocument(
        Document modelDocument)
    {
        if (string.IsNullOrWhiteSpace(
                modelDocument.FullFileName))
        {
            throw new InvalidOperationException(
                "Модель не сохранена. " +
                "Сначала сохраните файл детали, " +
                "затем повторите создание чертежа.");
        }
    }

    private static void PrintAxisMappings(
        IReadOnlyDictionary<
            DrawingViewRole,
            ViewAxisMapping> axisMappings)
    {
        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "ФИЗИЧЕСКИЕ ОСИ ЧЕРТЁЖНЫХ ВИДОВ");

        Console.WriteLine(
            "========================================");

        foreach (KeyValuePair<
                     DrawingViewRole,
                     ViewAxisMapping> entry
                 in axisMappings)
        {
            Console.WriteLine(
                $"{entry.Key}: " +
                $"горизонталь={entry.Value.HorizontalAxis}; " +
                $"вертикаль={entry.Value.VerticalAxis}");
        }

        Console.WriteLine(
            "========================================");
    }

    private static void PrintOverallDimensionSelection(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            selectedByView)
    {
        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "РАЗМЕРЫ, ПЕРЕДАННЫЕ ГЕНЕРАТОРУ");

        Console.WriteLine(
            "========================================");

        int selectedCount =
            0;

        foreach (KeyValuePair<string, List<DimensionCandidate>>
                 entry in selectedByView)
        {
            Console.WriteLine(
                $"Вид: {entry.Key}");

            if (entry.Value.Count == 0)
            {
                Console.WriteLine(
                    "  Размеры не выбраны.");

                continue;
            }

            foreach (DimensionCandidate candidate
                     in entry.Value)
            {
                selectedCount++;

                Console.WriteLine(
                    $"  {candidate.OverallRole}: " +
                    $"{candidate.Value:F3} мм; " +
                    $"ось: {candidate.PhysicalAxis}; " +
                    $"тип: {candidate.Type}; " +
                    $"статус: {candidate.Status}");
            }
        }

        Console.WriteLine(
            $"Всего передано генератору: " +
            $"{selectedCount}");

        Console.WriteLine(
            "========================================");
    }

    private static ViewOrientationTypeEnum
        ConvertToInventorOrientation(
            StandardViewOrientation orientation)
    {
        return orientation switch
        {
            StandardViewOrientation.Front =>
                ViewOrientationTypeEnum
                    .kFrontViewOrientation,

            StandardViewOrientation.Back =>
                ViewOrientationTypeEnum
                    .kBackViewOrientation,

            StandardViewOrientation.Top =>
                ViewOrientationTypeEnum
                    .kTopViewOrientation,

            StandardViewOrientation.Bottom =>
                ViewOrientationTypeEnum
                    .kBottomViewOrientation,

            StandardViewOrientation.Left =>
                ViewOrientationTypeEnum
                    .kLeftViewOrientation,

            StandardViewOrientation.Right =>
                ViewOrientationTypeEnum
                    .kRightViewOrientation,

            _ =>
                ViewOrientationTypeEnum
                    .kFrontViewOrientation
        };
    }

    private static string GetOrientationName(
        StandardViewOrientation orientation)
    {
        return orientation switch
        {
            StandardViewOrientation.Front =>
                "Front",

            StandardViewOrientation.Back =>
                "Back",

            StandardViewOrientation.Top =>
                "Top",

            StandardViewOrientation.Bottom =>
                "Bottom",

            StandardViewOrientation.Left =>
                "Left",

            StandardViewOrientation.Right =>
                "Right",

            _ =>
                "Front"
        };
    }

    private void PrintDecisionReport(
        string mainOrientationName,
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView)
    {
        ViewStatistics mainStatistics =
            _viewAnalyzer.Analyze(
                baseView);

        ViewStatistics upperStatistics =
            _viewAnalyzer.Analyze(
                upperView);

        ViewStatistics sideStatistics =
            _viewAnalyzer.Analyze(
                sideView);

        double mainScore =
            _viewScoreCalculator.Calculate(
                mainStatistics);

        double upperScore =
            _viewScoreCalculator.Calculate(
                upperStatistics);

        double sideScore =
            _viewScoreCalculator.Calculate(
                sideStatistics);

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

        Console.WriteLine(
            report);
    }

    private void ArrangeViews(
        DrawingView baseView,
        DrawingView upperView,
        DrawingView sideView,
        Sheet sheet,
        double viewGap)
    {
        const double leftMargin =
            2.0;

        const double rightMargin =
            2.0;

        const double topMargin =
            2.0;

        const double bottomReservedArea =
            6.0;

        double workingLeft =
            leftMargin;

        double workingBottom =
            bottomReservedArea;

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
                (workingWidth -
                 groupWidth) /
                2.0);

        double groupBottom =
            workingBottom +
            Math.Max(
                0,
                (workingHeight -
                 groupHeight) /
                2.0);

        double baseCenterX =
            groupLeft +
            baseView.Width /
            2.0;

        double upperCenterY =
            groupBottom +
            upperView.Height /
            2.0;

        double baseCenterY =
            groupBottom +
            upperView.Height +
            viewGap +
            baseView.Height /
            2.0;

        double sideCenterX =
            groupLeft +
            baseView.Width +
            viewGap +
            sideView.Width /
            2.0;

        upperView.Position =
            _inventor.TransientGeometry
                .CreatePoint2d(
                    baseCenterX,
                    upperCenterY);

        baseView.Position =
            _inventor.TransientGeometry
                .CreatePoint2d(
                    baseCenterX,
                    baseCenterY);

        sideView.Position =
            _inventor.TransientGeometry
                .CreatePoint2d(
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
        const double leftMargin =
            2.0;

        const double rightMargin =
            2.0;

        const double topMargin =
            2.0;

        const double bottomReservedArea =
            6.0;

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
            (availableWidth -
             viewGap) /
            viewsWidthAtScaleOne;

        double maximumScaleByHeight =
            (availableHeight -
             viewGap) /
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

        foreach (double scale
                 in gostScales)
        {
            if (scale <= maximumScale)
            {
                return scale;
            }
        }

        return 0.001;
    }

    private static string FormatScale(
        double scale)
    {
        if (scale >= 1.0)
        {
            return $"{scale:0.###}:1";
        }

        double denominator =
            1.0 /
            scale;

        return $"1:{denominator:0.###}";
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}