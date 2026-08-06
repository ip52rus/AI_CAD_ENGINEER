using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class LeaderNotePlacement
{
    public Point2d TextPosition { get; set; } =
        null!;

    public LeaderNotePlacementSide Side { get; set; } =
        LeaderNotePlacementSide.Undefined;

    public double Score { get; set; }

    public double DistanceFromView { get; set; }

    public bool IsInsideSheet { get; set; }

    public bool IntersectsTargetView { get; set; }

    public bool IsValid =>
        TextPosition != null &&
        Side != LeaderNotePlacementSide.Undefined &&
        IsInsideSheet &&
        !IntersectsTargetView;
}

public enum LeaderNotePlacementSide
{
    Undefined = 0,

    Left,

    Right,

    Top,

    Bottom
}