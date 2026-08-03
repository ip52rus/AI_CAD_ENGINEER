using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class HoleFeatureNode : FeatureNode
{
    public HoleFeatureNode()
    {
        Type =
            FeatureType.Hole;
    }

    public double DiameterMillimeters { get; set; }

    public double DepthMillimeters { get; set; }

    public HoleTerminationType TerminationType { get; set; } =
        HoleTerminationType.Unknown;

    public bool IsThroughHole =>
        TerminationType ==
        HoleTerminationType.ThroughAll;

    public int InstanceCount { get; set; } =
        1;

    public ModelAxis Axis { get; set; } =
        ModelAxis.Undefined;

    public double CenterXMillimeters { get; set; }

    public double CenterYMillimeters { get; set; }

    public double CenterZMillimeters { get; set; }

    public string SourceFeatureName { get; set; } =
        string.Empty;

    public void SynchronizeParameters()
    {
        Parameters[
            nameof(DiameterMillimeters)] =
                DiameterMillimeters;

        Parameters[
            nameof(DepthMillimeters)] =
                DepthMillimeters;

        Parameters[
            nameof(InstanceCount)] =
                InstanceCount;

        Parameters[
            nameof(CenterXMillimeters)] =
                CenterXMillimeters;

        Parameters[
            nameof(CenterYMillimeters)] =
                CenterYMillimeters;

        Parameters[
            nameof(CenterZMillimeters)] =
                CenterZMillimeters;
    }

    public override string ToString()
    {
        string termination =
            IsThroughHole
                ? "сквозное"
                : $"глубина {DepthMillimeters:F3} мм";

        return
            $"Hole: Ø{DiameterMillimeters:F3} мм; " +
            $"{termination}; " +
            $"ось {Axis}; " +
            $"экземпляров {InstanceCount}";
    }
}