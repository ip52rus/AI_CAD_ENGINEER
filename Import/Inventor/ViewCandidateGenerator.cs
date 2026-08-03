using AI_CAD_ENGINEER.Engineering.Analysis;
using AI_CAD_ENGINEER.Engineering.Decision;
using AI_CAD_ENGINEER.Engineering.Models;

using InventorApplication = global::Inventor.Application;
using InventorDocument = global::Inventor.Document;
using InventorDocumentInternal = global::Inventor._Document;
using DrawingView = global::Inventor.DrawingView;
using DrawingViewStyleEnum = global::Inventor.DrawingViewStyleEnum;
using Point2d = global::Inventor.Point2d;
using Sheet = global::Inventor.Sheet;
using ViewOrientationTypeEnum =
    global::Inventor.ViewOrientationTypeEnum;

namespace AI_CAD_ENGINEER.Import.Inventor;

public class ViewCandidateGenerator
{
    private readonly InventorApplication _inventor;
    private readonly ViewAnalyzer _viewAnalyzer;
    private readonly ViewScoreCalculator _scoreCalculator;

    public ViewCandidateGenerator(
        InventorApplication inventor)
    {
        _inventor = inventor;
        _viewAnalyzer = new ViewAnalyzer();
        _scoreCalculator = new ViewScoreCalculator();
    }

    public List<ViewCandidate> Generate(
        InventorDocument modelDocument,
        Sheet sheet)
    {
        List<ViewCandidate> candidates = new();

        ViewOrientationTypeEnum[] inventorOrientations =
        {
            ViewOrientationTypeEnum.kFrontViewOrientation,
            ViewOrientationTypeEnum.kBackViewOrientation,
            ViewOrientationTypeEnum.kTopViewOrientation,
            ViewOrientationTypeEnum.kBottomViewOrientation,
            ViewOrientationTypeEnum.kLeftViewOrientation,
            ViewOrientationTypeEnum.kRightViewOrientation
        };

        StandardViewOrientation[] standardOrientations =
        {
            StandardViewOrientation.Front,
            StandardViewOrientation.Back,
            StandardViewOrientation.Top,
            StandardViewOrientation.Bottom,
            StandardViewOrientation.Left,
            StandardViewOrientation.Right
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

        Console.WriteLine();
        Console.WriteLine("Оценка стандартных проекций:");
        Console.WriteLine("--------------------------------");

        for (int index = 0;
             index < inventorOrientations.Length;
             index++)
        {
            Point2d temporaryPosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    sheet.Width / 2,
                    sheet.Height / 2);

            DrawingView temporaryView =
                sheet.DrawingViews.AddBaseView(
                    (InventorDocumentInternal)modelDocument,
                    temporaryPosition,
                    1.0,
                    inventorOrientations[index],
                    DrawingViewStyleEnum
                        .kHiddenLineRemovedDrawingViewStyle);

            sheet.Parent.Update();

            ViewStatistics statistics =
                _viewAnalyzer.Analyze(temporaryView);

            double score =
                _scoreCalculator.Calculate(statistics);

            ViewCandidate candidate = new()
            {
                Orientation =
                    standardOrientations[index],

                OrientationName =
                    orientationNames[index],

                Statistics =
                    statistics,

                Score =
                    score
            };

            candidates.Add(candidate);

            PrintCandidate(candidate);

            temporaryView.Delete();
        }

        Console.WriteLine("--------------------------------");

        return candidates;
    }

    private static void PrintCandidate(
        ViewCandidate candidate)
    {
        ViewStatistics statistics =
            candidate.Statistics;

        Console.WriteLine(
            $"{candidate.OrientationName}: " +
            $"{candidate.Score:F2}");

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
}