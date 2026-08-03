using AI_CAD_ENGINEER.Infrastructure.Reporting;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraphCoordinator
{
    private readonly FeatureGraphReport
        _featureGraphReport;

    private readonly IReporter
        _fileReporter;

    public FeatureGraphCoordinator()
    {
        _featureGraphReport =
            new FeatureGraphReport();

        _fileReporter =
            new FileReporter();
    }

    public FeatureGraph CreateEmptyGraph(
        string documentName,
        string documentPath)
    {
        FeatureGraphBuilder builder =
            new();

        FeatureGraph graph =
            builder.BuildEmpty(
                documentName,
                documentPath);

        SaveReport(
            graph);

        return graph;
    }

    public string SaveReport(
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            graph);

        string report =
            _featureGraphReport.Build(
                graph);

        string reportPath =
            _fileReporter.Write(
                ReportCategory.Analysis,
                nameof(FeatureGraphReport),
                report);

        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "ENGINEERING FEATURE GRAPH");

        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Узлов: {graph.Nodes.Count}");

        Console.WriteLine(
            $"Связей: {graph.Relationships.Count}");

        Console.WriteLine(
            "Полный отчёт Feature Graph сохранён:");

        Console.WriteLine(
            reportPath);

        Console.WriteLine(
            "========================================");

        return reportPath;
    }
}