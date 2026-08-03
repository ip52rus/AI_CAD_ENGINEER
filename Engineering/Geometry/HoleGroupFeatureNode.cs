using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class HoleGroupFeatureNode : FeatureNode
{
    public HoleGroupFeatureNode()
    {
        Type =
            FeatureType.HoleGroup;
    }

    public double DiameterMillimeters { get; set; }

    public double DepthMillimeters { get; set; }

    public int HoleCount { get; set; }

    public ModelAxis Axis { get; set; } =
        ModelAxis.Undefined;

    public HoleTerminationType TerminationType { get; set; } =
        HoleTerminationType.Unknown;

    public string SourceFeatureName { get; set; } =
        string.Empty;

    public bool IsThroughHole =>
        TerminationType ==
        HoleTerminationType.ThroughAll;

    public List<Guid> HoleFeatureIds { get; } =
        new();

    public void SynchronizeParameters()
    {
        Parameters[
            nameof(DiameterMillimeters)] =
                DiameterMillimeters;

        Parameters[
            nameof(DepthMillimeters)] =
                DepthMillimeters;

        Parameters[
            nameof(HoleCount)] =
                HoleCount;

        Parameters[
            nameof(Axis)] =
                (double)Axis;

        Parameters[
            nameof(TerminationType)] =
                (double)TerminationType;
    }

    public override string ToString()
    {
        string termination =
            IsThroughHole
                ? "сквозные"
                : $"глубина {DepthMillimeters:F3} мм";

        return
            $"HoleGroup: " +
            $"{HoleCount} × Ø{DiameterMillimeters:F3} мм; " +
            $"{termination}; " +
            $"ось {Axis}";
    }
}