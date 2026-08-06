using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class LeaderLayout
{
    public Point2d TextPoint { get; set; } =
        null!;

    public Point2d BendPoint { get; set; } =
        null!;

    public LeaderNotePlacementSide Side { get; set; } =
        LeaderNotePlacementSide.Undefined;

    public double Score { get; set; }

    public double ShelfLength { get; set; }

    public double DistanceFromView { get; set; }

    public bool IsInsideSheet { get; set; }

    public bool IntersectsTargetView { get; set; }

    public bool IntersectsOtherViews { get; set; }

    public bool IntersectsDimensions { get; set; }

    public bool IntersectsLeaderNotes { get; set; }

    public bool IsValid =>
        TextPoint != null &&
        BendPoint != null &&
        Side != LeaderNotePlacementSide.Undefined &&
        IsInsideSheet &&
        !IntersectsTargetView &&
        !IntersectsOtherViews &&
        !IntersectsDimensions &&
        !IntersectsLeaderNotes;

    public override string ToString()
    {
        return
            $"Side={Side}; " +
            $"Score={Score:F3}; " +
            $"Text=({TextPoint.X:F3}, {TextPoint.Y:F3}); " +
            $"Bend=({BendPoint.X:F3}, {BendPoint.Y:F3}); " +
            $"Shelf={ShelfLength:F3}; " +
            $"Valid={IsValid}";
    }
}