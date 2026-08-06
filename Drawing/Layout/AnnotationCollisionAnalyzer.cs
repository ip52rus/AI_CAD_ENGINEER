using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Layout;

public class AnnotationCollisionAnalyzer
{
    public bool IntersectsAnyDrawingView(
        Sheet sheet,
        LeaderNotePlacement placement,
        DrawingView targetView)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        ArgumentNullException.ThrowIfNull(
            placement);

        ArgumentNullException.ThrowIfNull(
            targetView);

        foreach (DrawingView drawingView
                 in sheet.DrawingViews)
        {
            if (drawingView == targetView)
            {
                continue;
            }

            if (ContainsPoint(
                    drawingView,
                    placement.TextPosition))
            {
                return true;
            }
        }

        return false;
    }

    public bool IntersectsExistingDimensions(
        Sheet sheet,
        LeaderNotePlacement placement)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        ArgumentNullException.ThrowIfNull(
            placement);

        GeneralDimensions dimensions =
            sheet.DrawingDimensions
                .GeneralDimensions;

        foreach (GeneralDimension dimension
                 in dimensions)
        {
            try
            {
                Point2d textPoint =
                    dimension.Text.Origin;

                if (ArePointsClose(
                        placement.TextPosition,
                        textPoint,
                        1.5))
                {
                    return true;
                }
            }
            catch
            {
                // Некоторые типы размеров могут не предоставлять
                // доступ к положению текста одинаковым способом.
            }
        }

        return false;
    }

    public bool IntersectsExistingLeaderNotes(
        Sheet sheet,
        LeaderNotePlacement placement)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        ArgumentNullException.ThrowIfNull(
            placement);

        LeaderNotes leaderNotes =
            sheet.DrawingNotes
                .LeaderNotes;

        foreach (LeaderNote leaderNote
                 in leaderNotes)
        {
            try
            {
                Point2d textPoint =
                    leaderNote.Position;

                if (ArePointsClose(
                        placement.TextPosition,
                        textPoint,
                        2.0))
                {
                    return true;
                }
            }
            catch
            {
                // Если конкретная аннотация не предоставляет
                // положение, просто пропускаем её.
            }
        }

        return false;
    }

    private static bool ContainsPoint(
        DrawingView drawingView,
        Point2d point)
    {
        double left =
            drawingView.Center.X -
            drawingView.Width / 2.0;

        double right =
            drawingView.Center.X +
            drawingView.Width / 2.0;

        double bottom =
            drawingView.Center.Y -
            drawingView.Height / 2.0;

        double top =
            drawingView.Center.Y +
            drawingView.Height / 2.0;

        return
            point.X >= left &&
            point.X <= right &&
            point.Y >= bottom &&
            point.Y <= top;
    }

    private static bool ArePointsClose(
        Point2d first,
        Point2d second,
        double tolerance)
    {
        double deltaX =
            first.X -
            second.X;

        double deltaY =
            first.Y -
            second.Y;

        double distance =
            Math.Sqrt(
                deltaX * deltaX +
                deltaY * deltaY);

        return distance <=
               tolerance;
    }
}