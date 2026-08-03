namespace AI_CAD_ENGINEER.Engineering.Models;

public enum DimensionCandidateType
{
    Horizontal,
    Vertical,
    Diameter,
    Radius,
    Angular
}

public enum OverallDimensionRole
{
    Undefined,
    Length,
    Width,
    Height
}

public enum ModelAxis
{
    Undefined,
    X,
    Y,
    Z
}

public class DimensionCandidate
{
    public string Name { get; set; } = string.Empty;

    public DimensionCandidateType Type { get; set; }

    public OverallDimensionRole OverallRole { get; set; } =
        OverallDimensionRole.Undefined;

    public ModelAxis PhysicalAxis { get; set; } =
        ModelAxis.Undefined;

    public DimensionStatus Status { get; set; } =
        DimensionStatus.Undefined;

    public double Value { get; set; }

    public double StartX { get; set; }

    public double StartY { get; set; }

    public double EndX { get; set; }

    public double EndY { get; set; }

    public bool IsOverallDimension { get; set; }

    public bool IsRequired { get; set; }

    public string SourceViewName { get; set; } =
        string.Empty;

    public string DecisionReason { get; set; } =
        string.Empty;
}