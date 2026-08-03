using AI_CAD_ENGINEER.Engineering.Models;
using AI_CAD_ENGINEER.Infrastructure.Reporting;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class DimensionDecisionCoordinator
{
    private readonly DimensionClassificationEngine
        _classificationEngine;

    private readonly DimensionDecisionReport
        _decisionReport;

    private readonly IReporter
        _fileReporter;

    public DimensionDecisionCoordinator()
    {
        _classificationEngine =
            new DimensionClassificationEngine();

        _decisionReport =
            new DimensionDecisionReport();

        _fileReporter =
            new FileReporter();
    }

    public DimensionDecisionResult Process(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView,
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            selectedByView)
    {
        ArgumentNullException.ThrowIfNull(
            candidatesByView);

        ArgumentNullException.ThrowIfNull(
            selectedByView);

        _classificationEngine.Classify(
            candidatesByView,
            selectedByView);

        string report =
            _decisionReport.Build(
                candidatesByView);

        string reportPath =
            _fileReporter.Write(
                ReportCategory.Analysis,
                nameof(DimensionDecisionReport),
                report);

        Console.WriteLine(report);

        Console.WriteLine(
            "Полный отчёт классификации размеров сохранён:");

        Console.WriteLine(reportPath);

        DimensionDecisionResult result =
            new();

        foreach (KeyValuePair<string, List<DimensionCandidate>>
                 entry in selectedByView)
        {
            result.RequiredByView.Add(
                entry.Key,
                entry.Value);
        }

        result.ReportPath =
            reportPath;

        List<DimensionCandidate> allCandidates =
            candidatesByView.Values
                .SelectMany(
                    candidates => candidates)
                .ToList();

        result.TotalCandidateCount =
            allCandidates.Count;

        result.RequiredCount =
            allCandidates.Count(
                candidate =>
                    candidate.Status ==
                    DimensionStatus.Required);

        result.DuplicateCount =
            allCandidates.Count(
                candidate =>
                    candidate.Status ==
                    DimensionStatus.Duplicate);

        result.RedundantCount =
            allCandidates.Count(
                candidate =>
                    candidate.Status ==
                    DimensionStatus.Redundant);

        result.ReferenceCount =
            allCandidates.Count(
                candidate =>
                    candidate.Status ==
                    DimensionStatus.Reference);

        result.OptionalCount =
            allCandidates.Count(
                candidate =>
                    candidate.Status ==
                    DimensionStatus.Optional);

        return result;
    }
}