using AI_CAD_ENGINEER.Engineering.Geometry;
using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlan
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DimensionPlanStatus Status { get; set; } =
        DimensionPlanStatus.Draft;

    public DateTime CreatedAt { get; } =
        DateTime.Now;

    public string SourceDocumentName { get; set; } =
        string.Empty;

    public string SourceDocumentPath { get; set; } =
        string.Empty;

    public List<OverallDimensionPlanItem>
        OverallDimensions
    { get; } =
            new();

    public List<HoleDimensionPlanItem>
        HoleDimensions
    { get; } =
            new();

    public List<HoleGroupDimensionPlanItem>
        HoleGroups
    { get; } =
            new();

    public List<ReferenceDimensionPlanItem>
        ReferenceDimensions
    { get; } =
            new();

    public List<SectionDimensionPlanItem>
        Sections
    { get; } =
            new();

    public List<string>
        Notes
    { get; } =
            new();

    public List<string>
        ValidationErrors
    { get; } =
            new();

    public int TotalItems =>
        OverallDimensions.Count +
        HoleDimensions.Count +
        HoleGroups.Count +
        ReferenceDimensions.Count +
        Sections.Count;

    public bool IsEmpty =>
        TotalItems == 0;

    public bool IsValid =>
        ValidationErrors.Count == 0;

    public void AddValidationError(
        string error)
    {
        if (string.IsNullOrWhiteSpace(
                error))
        {
            return;
        }

        ValidationErrors.Add(
            error);

        Status =
            DimensionPlanStatus.Failed;
    }

    public void MarkValidated()
    {
        if (!IsValid)
        {
            Status =
                DimensionPlanStatus.Failed;

            return;
        }

        Status =
            DimensionPlanStatus.Validated;
    }

    public void MarkReadyForDrawing()
    {
        if (!IsValid)
        {
            Status =
                DimensionPlanStatus.Failed;

            return;
        }

        Status =
            DimensionPlanStatus.ReadyForDrawing;
    }

    public override string ToString()
    {
        return
            $"Status={Status}; " +
            $"Overall={OverallDimensions.Count}; " +
            $"Hole={HoleDimensions.Count}; " +
            $"Groups={HoleGroups.Count}; " +
            $"Reference={ReferenceDimensions.Count}; " +
            $"Sections={Sections.Count}; " +
            $"Errors={ValidationErrors.Count}";
    }
}

public class OverallDimensionPlanItem
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DimensionCandidate Candidate { get; set; } =
        null!;

    public string TargetViewName { get; set; } =
        string.Empty;

    public string DecisionReason { get; set; } =
        string.Empty;
}

public class HoleDimensionPlanItem
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public HoleFeatureNode Hole { get; set; } =
        null!;

    public string TargetViewName { get; set; } =
        string.Empty;

    public bool UseCenterMark { get; set; }

    public bool UseCenterLine { get; set; }

    public bool CreateDiameterDimension { get; set; }

    public bool CreateDepthNote { get; set; }

    public string DecisionReason { get; set; } =
        string.Empty;
}

public class HoleGroupDimensionPlanItem
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public HoleGroupFeatureNode Group { get; set; } =
        null!;

    public string TargetViewName { get; set; } =
        string.Empty;

    public bool UseCountPrefix { get; set; } =
        true;

    public bool UseSingleLeader { get; set; } =
        true;

    public bool CreateDepthNote { get; set; }

    public string DecisionReason { get; set; } =
        string.Empty;
}

public class ReferenceDimensionPlanItem
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public string Description { get; set; } =
        string.Empty;

    public string TargetViewName { get; set; } =
        string.Empty;

    public string DecisionReason { get; set; } =
        string.Empty;
}

public class SectionDimensionPlanItem
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public string SectionName { get; set; } =
        string.Empty;

    public string TargetViewName { get; set; } =
        string.Empty;

    public string DecisionReason { get; set; } =
        string.Empty;
}