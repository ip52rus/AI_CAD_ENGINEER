using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class LeaderNotePlacementEvaluator
{
    private readonly AnnotationCollisionAnalyzer
        _collisionAnalyzer;

    public LeaderNotePlacementEvaluator()
    {
        _collisionAnalyzer =
            new AnnotationCollisionAnalyzer();
    }

    public void Evaluate(
        Sheet sheet,
        DrawingView view,
        LeaderNotePlacement placement)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        ArgumentNullException.ThrowIfNull(
            view);

        ArgumentNullException.ThrowIfNull(
            placement);

        EvaluateTargetViewIntersection(
            view,
            placement);

        EvaluateDistanceFromTargetView(
            view,
            placement);

        EvaluateOtherViews(
            sheet,
            view,
            placement);

        EvaluateDimensions(
            sheet,
            placement);

        EvaluateLeaderNotes(
            sheet,
            placement);
    }

    private static void EvaluateTargetViewIntersection(
        DrawingView view,
        LeaderNotePlacement placement)
    {
        double left =
            view.Center.X -
            view.Width / 2.0;

        double right =
            view.Center.X +
            view.Width / 2.0;

        double bottom =
            view.Center.Y -
            view.Height / 2.0;

        double top =
            view.Center.Y +
            view.Height / 2.0;

        Point2d point =
            placement.TextPosition;

        bool insideView =
            point.X >= left &&
            point.X <= right &&
            point.Y >= bottom &&
            point.Y <= top;

        placement.IntersectsTargetView =
            insideView;

        if (insideView)
        {
            placement.Score -=
                1000.0;
        }
    }

    private static void EvaluateDistanceFromTargetView(
        DrawingView view,
        LeaderNotePlacement placement)
    {
        double left =
            view.Center.X -
            view.Width / 2.0;

        double right =
            view.Center.X +
            view.Width / 2.0;

        double bottom =
            view.Center.Y -
            view.Height / 2.0;

        double top =
            view.Center.Y +
            view.Height / 2.0;

        Point2d point =
            placement.TextPosition;

        double deltaX =
            Math.Max(
                left - point.X,
                Math.Max(
                    0,
                    point.X - right));

        double deltaY =
            Math.Max(
                bottom - point.Y,
                Math.Max(
                    0,
                    point.Y - top));

        placement.DistanceFromView =
            Math.Sqrt(
                deltaX * deltaX +
                deltaY * deltaY);

        placement.Score -=
            placement.DistanceFromView * 2.0;
    }

    private void EvaluateOtherViews(
        Sheet sheet,
        DrawingView targetView,
        LeaderNotePlacement placement)
    {
        bool intersects =
            _collisionAnalyzer
                .IntersectsAnyDrawingView(
                    sheet,
                    placement,
                    targetView);

        if (intersects)
        {
            placement.Score -=
                800.0;
        }
    }

    private void EvaluateDimensions(
        Sheet sheet,
        LeaderNotePlacement placement)
    {
        bool intersects =
            _collisionAnalyzer
                .IntersectsExistingDimensions(
                    sheet,
                    placement);

        if (intersects)
        {
            placement.Score -=
                500.0;
        }
    }

    private void EvaluateLeaderNotes(
        Sheet sheet,
        LeaderNotePlacement placement)
    {
        bool intersects =
            _collisionAnalyzer
                .IntersectsExistingLeaderNotes(
                    sheet,
                    placement);

        if (intersects)
        {
            placement.Score -=
                600.0;
        }
    }
}