namespace AI_CAD_ENGINEER.Engineering.Models;

public class DimensionPlan
{
    public List<DimensionCandidate> Candidates { get; } = new();

    public List<DimensionCandidate> SelectedDimensions { get; } = new();

    public bool NeedOverallDimensions { get; set; } = true;

    public bool NeedHoleDimensions { get; set; } = true;

    public bool NeedDiameterDimensions { get; set; } = true;

    public bool NeedRadiusDimensions { get; set; } = true;

    public bool NeedAngularDimensions { get; set; } = true;

    public bool NeedThreadDimensions { get; set; } = true;

    public bool NeedChamferDimensions { get; set; } = true;

    public bool NeedFilletDimensions { get; set; } = true;
}