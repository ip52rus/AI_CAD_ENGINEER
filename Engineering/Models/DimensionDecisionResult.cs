namespace AI_CAD_ENGINEER.Engineering.Models;

public class DimensionDecisionResult
{
    public Dictionary<string, List<DimensionCandidate>>
        RequiredByView
    { get; } =
            new();

    public string ReportPath { get; set; } =
        string.Empty;

    public int TotalCandidateCount { get; set; }

    public int RequiredCount { get; set; }

    public int DuplicateCount { get; set; }

    public int RedundantCount { get; set; }

    public int ReferenceCount { get; set; }

    public int OptionalCount { get; set; }
}