using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class DimensionLayoutCandidateGenerator
{
    private readonly Inventor.Application
        _inventor;

    public DimensionLayoutCandidateGenerator(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public List<DimensionLayoutCandidate> Generate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        double offsetFromView)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            sourceItemId);

        ArgumentNullException.ThrowIfNull(
            targetView);

        ArgumentNullException.ThrowIfNull(
            sheet);

        if (offsetFromView <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offsetFromView),
                "Отступ от вида должен быть больше нуля.");
        }

        List<DimensionLayoutCandidate> candidates =
        [
            CreateTopCandidate(
                sourceItemId,
                targetView,
                sheet,
                offsetFromView),

            CreateBottomCandidate(
                sourceItemId,
                targetView,
                sheet,
                offsetFromView),

            CreateLeftCandidate(
                sourceItemId,
                targetView,
                sheet,
                offsetFromView),

            CreateRightCandidate(
                sourceItemId,
                targetView,
                sheet,
                offsetFromView)
        ];

        return candidates;
    }

    private DimensionLayoutCandidate CreateTopCandidate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        double offsetFromView)
    {
        double x =
            targetView.Center.X;

        double y =
            targetView.Center.Y +
            targetView.Height / 2.0 +
            offsetFromView;

        return CreateCandidate(
            sourceItemId,
            targetView,
            sheet,
            DimensionLayoutSide.Top,
            x,
            y,
            offsetFromView);
    }

    private DimensionLayoutCandidate CreateBottomCandidate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        double offsetFromView)
    {
        double x =
            targetView.Center.X;

        double y =
            targetView.Center.Y -
            targetView.Height / 2.0 -
            offsetFromView;

        return CreateCandidate(
            sourceItemId,
            targetView,
            sheet,
            DimensionLayoutSide.Bottom,
            x,
            y,
            offsetFromView);
    }

    private DimensionLayoutCandidate CreateLeftCandidate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        double offsetFromView)
    {
        double x =
            targetView.Center.X -
            targetView.Width / 2.0 -
            offsetFromView;

        double y =
            targetView.Center.Y;

        return CreateCandidate(
            sourceItemId,
            targetView,
            sheet,
            DimensionLayoutSide.Left,
            x,
            y,
            offsetFromView);
    }

    private DimensionLayoutCandidate CreateRightCandidate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        double offsetFromView)
    {
        double x =
            targetView.Center.X +
            targetView.Width / 2.0 +
            offsetFromView;

        double y =
            targetView.Center.Y;

        return CreateCandidate(
            sourceItemId,
            targetView,
            sheet,
            DimensionLayoutSide.Right,
            x,
            y,
            offsetFromView);
    }

    private DimensionLayoutCandidate CreateCandidate(
        string sourceItemId,
        DrawingView targetView,
        Sheet sheet,
        DimensionLayoutSide side,
        double x,
        double y,
        double offsetFromView)
    {
        const double sheetMargin =
            1.0;

        bool isInsideSheet =
            x >= sheetMargin &&
            y >= sheetMargin &&
            x <= sheet.Width - sheetMargin &&
            y <= sheet.Height - sheetMargin;

        DimensionLayoutCandidate candidate =
            new()
            {
                SourceItemId =
                    sourceItemId,

                TargetViewName =
                    targetView.Name,

                Side =
                    side,

                TextPosition =
                    _inventor.TransientGeometry
                        .CreatePoint2d(
                            x,
                            y),

                OffsetFromView =
                    offsetFromView,

                Score =
                    0.0,

                IsInsideSheet =
                    isInsideSheet,

                IntersectsTargetView =
                    false,

                IntersectsOtherViews =
                    false,

                IntersectsDimensions =
                    false,

                IntersectsAnnotations =
                    false
            };

        candidate.AddReason(
            $"Создан кандидат размещения " +
            $"на стороне {side}.");

        candidate.AddReason(
            isInsideSheet
                ? "Положение находится внутри листа."
                : "Положение выходит за допустимую область листа.");

        return candidate;
    }
}