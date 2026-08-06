using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class LeaderNotePlacementEngine
{
    private readonly Inventor.Application
        _inventor;

    private readonly LeaderNotePlacementEvaluator
        _evaluator;

    public LeaderNotePlacementEngine(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;

        _evaluator =
            new LeaderNotePlacementEvaluator();
    }

    public LeaderNotePlacement Calculate(
    Sheet sheet,
    DrawingView view)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        ArgumentNullException.ThrowIfNull(
            view);

        List<LeaderNotePlacement> placements =
        [
            CreateRightPlacement(
            sheet,
            view),

        CreateLeftPlacement(
            sheet,
            view),

        CreateTopPlacement(
            sheet,
            view),

        CreateBottomPlacement(
            sheet,
            view)
        ];

        foreach (LeaderNotePlacement placement
                 in placements)
        {
            _evaluator.Evaluate(
                sheet,
                view,
                placement);
        }

        return placements
            .OrderByDescending(
                placement =>
                    placement.Score)
            .First();
    }

    private LeaderNotePlacement CreateRightPlacement(
        Sheet sheet,
        DrawingView view)
    {
        return CreatePlacement(
            sheet,
            view,
            LeaderNotePlacementSide.Right,
            view.Center.X +
            view.Width / 2.0 +
            2.0,
            view.Center.Y);
    }

    private LeaderNotePlacement CreateLeftPlacement(
        Sheet sheet,
        DrawingView view)
    {
        return CreatePlacement(
            sheet,
            view,
            LeaderNotePlacementSide.Left,
            view.Center.X -
            view.Width / 2.0 -
            2.0,
            view.Center.Y);
    }

    private LeaderNotePlacement CreateTopPlacement(
        Sheet sheet,
        DrawingView view)
    {
        return CreatePlacement(
            sheet,
            view,
            LeaderNotePlacementSide.Top,
            view.Center.X,
            view.Center.Y +
            view.Height / 2.0 +
            2.0);
    }

    private LeaderNotePlacement CreateBottomPlacement(
        Sheet sheet,
        DrawingView view)
    {
        return CreatePlacement(
            sheet,
            view,
            LeaderNotePlacementSide.Bottom,
            view.Center.X,
            view.Center.Y -
            view.Height / 2.0 -
            2.0);
    }

    private LeaderNotePlacement CreatePlacement(
        Sheet sheet,
        DrawingView view,
        LeaderNotePlacementSide side,
        double x,
        double y)
    {
        const double sheetMargin =
            1.0;

        bool isInsideSheet =
            x >= sheetMargin &&
            y >= sheetMargin &&
            x <= sheet.Width - sheetMargin &&
            y <= sheet.Height - sheetMargin;

        double initialScore =
            isInsideSheet
                ? 100.0
                : -1000.0;

        return new LeaderNotePlacement
        {
            Side =
                side,

            TextPosition =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        x,
                        y),

            Score =
                initialScore,

            DistanceFromView =
                2.0,

            IsInsideSheet =
                isInsideSheet,

            IntersectsTargetView =
                false
        };
    }
}