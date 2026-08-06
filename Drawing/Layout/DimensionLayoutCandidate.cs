using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class DimensionLayoutCandidate
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public string SourceItemId { get; set; } =
        string.Empty;

    public string TargetViewName { get; set; } =
        string.Empty;

    public DimensionLayoutSide Side { get; set; } =
        DimensionLayoutSide.Undefined;

    public Point2d TextPosition { get; set; } =
        null!;

    public double OffsetFromView { get; set; }

    public double Score { get; set; }

    public bool IsInsideSheet { get; set; }

    public bool IntersectsTargetView { get; set; }

    public bool IntersectsOtherViews { get; set; }

    public bool IntersectsDimensions { get; set; }

    public bool IntersectsAnnotations { get; set; }

    public bool IsPreferredByFreeSpace { get; set; }

    public List<string> Reasons { get; } =
        new();

    public bool IsValid =>
        TextPosition != null &&
        Side != DimensionLayoutSide.Undefined &&
        IsInsideSheet &&
        !IntersectsTargetView &&
        !IntersectsOtherViews &&
        !IntersectsDimensions &&
        !IntersectsAnnotations;

    public void AddReason(
        string reason)
    {
        if (string.IsNullOrWhiteSpace(
                reason))
        {
            return;
        }

        Reasons.Add(
            reason);
    }

    public override string ToString()
    {
        return
            $"Side={Side}; " +
            $"Score={Score:F3}; " +
            $"Position=({TextPosition.X:F3}, " +
            $"{TextPosition.Y:F3}); " +
            $"Offset={OffsetFromView:F3}; " +
            $"Valid={IsValid}";
    }
}

public enum DimensionLayoutSide
{
    Undefined = 0,

    Top,

    Bottom,

    Left,

    Right
}