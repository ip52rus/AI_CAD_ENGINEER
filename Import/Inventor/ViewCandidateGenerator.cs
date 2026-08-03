using System.Runtime.InteropServices;
using AI_CAD_ENGINEER.Engineering.Analysis;
using AI_CAD_ENGINEER.Engineering.Decision;
using AI_CAD_ENGINEER.Engineering.Models;

using InventorApplication = global::Inventor.Application;
using InventorDocument = global::Inventor.Document;
using InventorDocumentInternal = global::Inventor._Document;
using DrawingDocument = global::Inventor.DrawingDocument;
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
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;

        _viewAnalyzer =
            new ViewAnalyzer();

        _scoreCalculator =
            new ViewScoreCalculator();
    }

    public List<ViewCandidate> Generate(
        InventorDocument modelDocument,
        Sheet sheet)
    {
        ArgumentNullException.ThrowIfNull(
            modelDocument);

        ArgumentNullException.ThrowIfNull(
            sheet);

        DrawingDocument drawingDocument =
            (DrawingDocument)sheet.Parent;

        List<ViewCandidate> candidates =
            new();

        ViewOrientationDefinition[] orientations =
        [
            new(
                ViewOrientationTypeEnum
                    .kFrontViewOrientation,
                StandardViewOrientation.Front,
                "Front"),

            new(
                ViewOrientationTypeEnum
                    .kBackViewOrientation,
                StandardViewOrientation.Back,
                "Back"),

            new(
                ViewOrientationTypeEnum
                    .kTopViewOrientation,
                StandardViewOrientation.Top,
                "Top"),

            new(
                ViewOrientationTypeEnum
                    .kBottomViewOrientation,
                StandardViewOrientation.Bottom,
                "Bottom"),

            new(
                ViewOrientationTypeEnum
                    .kLeftViewOrientation,
                StandardViewOrientation.Left,
                "Left"),

            new(
                ViewOrientationTypeEnum
                    .kRightViewOrientation,
                StandardViewOrientation.Right,
                "Right")
        ];

        Console.WriteLine();
        Console.WriteLine(
            "Оценка стандартных проекций:");

        Console.WriteLine(
            "--------------------------------");

        foreach (ViewOrientationDefinition orientation
                 in orientations)
        {
            DrawingView? temporaryView =
                null;

            try
            {
                Console.WriteLine(
                    $"Анализ проекции {orientation.Name}:");

                Console.WriteLine(
                    "  Создание временного вида...");

                Point2d temporaryPosition =
                    _inventor.TransientGeometry
                        .CreatePoint2d(
                            sheet.Width / 2.0,
                            sheet.Height / 2.0);

                temporaryView =
                    sheet.DrawingViews.AddBaseView(
                        (InventorDocumentInternal)modelDocument,
                        temporaryPosition,
                        1.0,
                        orientation.InventorOrientation,
                        DrawingViewStyleEnum
                            .kHiddenLineRemovedDrawingViewStyle);

                Console.WriteLine(
                    "  Временный вид создан.");

                Console.WriteLine(
                    "  Обновление чертежа...");

                drawingDocument.Update();

                Console.WriteLine(
                    "  Чертёж обновлён.");

                Console.WriteLine(
                    "  Анализ геометрии...");

                ViewStatistics statistics =
                    _viewAnalyzer.Analyze(
                        temporaryView);

                Console.WriteLine(
                    "  Геометрия проанализирована.");

                double score =
                    _scoreCalculator.Calculate(
                        statistics);

                ViewCandidate candidate =
                    new()
                    {
                        Orientation =
                            orientation.StandardOrientation,

                        OrientationName =
                            orientation.Name,

                        Statistics =
                            statistics,

                        Score =
                            score
                    };

                candidates.Add(
                    candidate);

                PrintCandidate(
                    candidate);
            }
            catch (COMException exception)
            {
                Console.WriteLine(
                    $"  Ошибка COM для проекции " +
                    $"{orientation.Name}:");

                Console.WriteLine(
                    $"  {exception.Message}");

                Console.WriteLine(
                    $"  HRESULT: " +
                    $"0x{exception.HResult:X8}");

                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"  Ошибка анализа проекции " +
                    $"{orientation.Name}:");

                Console.WriteLine(
                    $"  {exception.Message}");

                Console.WriteLine();
            }
            finally
            {
                if (temporaryView != null)
                {
                    try
                    {
                        temporaryView.Delete();

                        drawingDocument.Update();

                        Console.WriteLine(
                            "  Временный вид удалён.");

                        Console.WriteLine();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(
                            "  Не удалось удалить " +
                            "временный вид:");

                        Console.WriteLine(
                            $"  {exception.Message}");

                        Console.WriteLine();
                    }
                }
            }
        }

        Console.WriteLine(
            "--------------------------------");

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "Не удалось создать ни одной стандартной " +
                "проекции. Подробности указаны выше в консоли.");
        }

        Console.WriteLine(
            $"Успешно проанализировано проекций: " +
            $"{candidates.Count}");

        Console.WriteLine();

        return candidates;
    }

    private static void PrintCandidate(
        ViewCandidate candidate)
    {
        ViewStatistics statistics =
            candidate.Statistics;

        Console.WriteLine(
            $"  {candidate.OrientationName}: " +
            $"{candidate.Score:F2}");

        Console.WriteLine(
            $"    Кривые: " +
            $"{statistics.CurveCount}");

        Console.WriteLine(
            $"    Линии: " +
            $"{statistics.LineCount}");

        Console.WriteLine(
            $"    Окружности: " +
            $"{statistics.CircleCount}");

        Console.WriteLine(
            $"    Дуги: " +
            $"{statistics.ArcCount}");

        Console.WriteLine(
            $"    Эллиптические дуги: " +
            $"{statistics.EllipticalArcCount}");

        Console.WriteLine(
            $"    Площадь: " +
            $"{statistics.Area:F2}");

        Console.WriteLine();
    }

    private sealed class ViewOrientationDefinition
    {
        public ViewOrientationDefinition(
            ViewOrientationTypeEnum inventorOrientation,
            StandardViewOrientation standardOrientation,
            string name)
        {
            InventorOrientation =
                inventorOrientation;

            StandardOrientation =
                standardOrientation;

            Name =
                name;
        }

        public ViewOrientationTypeEnum
            InventorOrientation
        { get; }

        public StandardViewOrientation
            StandardOrientation
        { get; }

        public string Name { get; }
    }
}