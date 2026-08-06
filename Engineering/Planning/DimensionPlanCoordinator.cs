using AI_CAD_ENGINEER.Engineering.Geometry;
using AI_CAD_ENGINEER.Engineering.Models;
using AI_CAD_ENGINEER.Infrastructure.Reporting;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanCoordinator
{
    private readonly DimensionPlanner
        _dimensionPlanner;

    private readonly DimensionPlanReport
        _dimensionPlanReport;

    private readonly IReporter
        _fileReporter;

    public DimensionPlanCoordinator()
    {
        _dimensionPlanner =
            new DimensionPlanner();

        _dimensionPlanReport =
            new DimensionPlanReport();

        _fileReporter =
            new FileReporter();
    }

    public DimensionPlan CreatePlan(
        string documentName,
        string documentPath,
        FeatureGraph featureGraph,
        IReadOnlyCollection<DimensionCandidate>
            overallDimensions,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentPath);

        ArgumentNullException.ThrowIfNull(
            featureGraph);

        ArgumentNullException.ThrowIfNull(
            overallDimensions);

        ArgumentNullException.ThrowIfNull(
            viewMappings);

        DimensionPlan plan =
            _dimensionPlanner.BuildPlan(
                documentName,
                documentPath,
                featureGraph,
                overallDimensions,
                viewMappings);

        string reportPath =
            SaveReport(
                plan);

        PrintSummary(
            plan,
            reportPath);

        return plan;
    }

    public string SaveReport(
        DimensionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        string report =
            _dimensionPlanReport.Build(
                plan);

        return _fileReporter.Write(
            ReportCategory.Analysis,
            nameof(DimensionPlanReport),
            report);
    }

    private static void PrintSummary(
        DimensionPlan plan,
        string reportPath)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "DIMENSION PLANNING ENGINE");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Статус: {plan.Status}");

        Console.WriteLine(
            $"Всего элементов плана: " +
            $"{plan.TotalItems}");

        Console.WriteLine(
            $"Габаритных размеров: " +
            $"{plan.OverallDimensions.Count}");

        Console.WriteLine(
            $"Одиночных отверстий: " +
            $"{plan.HoleDimensions.Count}");

        Console.WriteLine(
            $"Групп отверстий: " +
            $"{plan.HoleGroups.Count}");

        Console.WriteLine(
            $"Справочных размеров: " +
            $"{plan.ReferenceDimensions.Count}");

        Console.WriteLine(
            $"Размеров в разрезах: " +
            $"{plan.Sections.Count}");

        Console.WriteLine(
            $"Ошибок валидации: " +
            $"{plan.ValidationErrors.Count}");

        if (plan.HoleDimensions.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Одиночные отверстия:");

            foreach (HoleDimensionPlanItem item
                     in plan.HoleDimensions)
            {
                Console.WriteLine(
                    $"  {item.Hole.Name}: " +
                    $"вид={FormatTargetView(
                        item.TargetViewName)}; " +
                    $"Ø{item.Hole.DiameterMillimeters:F3} мм");
            }
        }

        if (plan.HoleGroups.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Группы отверстий:");

            foreach (HoleGroupDimensionPlanItem item
                     in plan.HoleGroups)
            {
                Console.WriteLine(
                    $"  {item.Group.Name}: " +
                    $"вид={FormatTargetView(
                        item.TargetViewName)}; " +
                    $"{item.Group.HoleCount}×" +
                    $"Ø{item.Group.DiameterMillimeters:F3} мм");
            }
        }

        if (plan.ValidationErrors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine(
                "Ошибки:");

            foreach (string error
                     in plan.ValidationErrors)
            {
                Console.WriteLine(
                    $"  - {error}");
            }
        }

        Console.WriteLine();

        Console.WriteLine(
            "Полный отчёт Dimension Plan сохранён:");

        Console.WriteLine(
            reportPath);

        Console.WriteLine(
            "========================================");
    }

    private static string FormatTargetView(
        string targetViewName)
    {
        return string.IsNullOrWhiteSpace(
            targetViewName)
            ? "не назначен"
            : targetViewName;
    }
}