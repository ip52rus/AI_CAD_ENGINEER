using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class LeaderLayoutBuilder
{
    private const double BendDistanceFromView =
        0.8;

    private const double BendOffset =
        1.2;

    private const double DefaultShelfLength =
        3.0;

    private readonly Inventor.Application
        _inventor;

    public LeaderLayoutBuilder(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public LeaderLayout Build(
        LeaderNotePlacement placement,
        DrawingView view)
    {
        ArgumentNullException.ThrowIfNull(
            placement);

        ArgumentNullException.ThrowIfNull(
            view);

        Point2d bendPoint =
            BuildBendPoint(
                placement,
                view);

        return new LeaderLayout
        {
            Side =
                placement.Side,

            Score =
                placement.Score,

            DistanceFromView =
                placement.DistanceFromView,

            IsInsideSheet =
                placement.IsInsideSheet,

            IntersectsTargetView =
                placement.IntersectsTargetView,

            IntersectsDimensions =
                false,

            IntersectsLeaderNotes =
                false,

            IntersectsOtherViews =
                false,

            ShelfLength =
                DefaultShelfLength,

            TextPoint =
                placement.TextPosition,

            BendPoint =
                bendPoint
        };
    }

    private Point2d BuildBendPoint(
        LeaderNotePlacement placement,
        DrawingView view)
    {
        double x =
            placement.TextPosition.X;

        double y =
            placement.TextPosition.Y;

        switch (placement.Side)
        {
            case LeaderNotePlacementSide.Right:
                x =
                    view.Center.X +
                    view.Width / 2.0 +
                    BendDistanceFromView;

                y =
                    placement.TextPosition.Y -
                    BendOffset;

                break;

            case LeaderNotePlacementSide.Left:
                x =
                    view.Center.X -
                    view.Width / 2.0 -
                    BendDistanceFromView;

                y =
                    placement.TextPosition.Y -
                    BendOffset;

                break;

            case LeaderNotePlacementSide.Top:
                x =
                    placement.TextPosition.X -
                    BendOffset;

                y =
                    view.Center.Y +
                    view.Height / 2.0 +
                    BendDistanceFromView;

                break;

            case LeaderNotePlacementSide.Bottom:
                x =
                    placement.TextPosition.X -
                    BendOffset;

                y =
                    view.Center.Y -
                    view.Height / 2.0 -
                    BendDistanceFromView;

                break;

            default:
                throw new InvalidOperationException(
                    "Не определена сторона размещения выноски.");
        }

        return
            _inventor.TransientGeometry
                .CreatePoint2d(
                    x,
                    y);
    }
}