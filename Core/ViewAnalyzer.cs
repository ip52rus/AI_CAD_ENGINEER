using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class ViewAnalyzer
{
    public ViewStatistics Analyze(DrawingView drawingView)
    {
        ViewStatistics statistics = new()
        {
            Width = drawingView.Width,
            Height = drawingView.Height,
            Area = drawingView.Width * drawingView.Height
        };

        if (drawingView.Height > 0)
        {
            statistics.AspectRatio =
                drawingView.Width / drawingView.Height;
        }

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            return statistics;
        }

        statistics.CurveCount = curves.Count;

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment in curve.Segments)
            {
                statistics.SegmentCount++;

                switch (segment.GeometryType)
                {
                    case Curve2dTypeEnum.kLineSegmentCurve2d:
                        statistics.LineCount++;
                        break;

                    case Curve2dTypeEnum.kCircleCurve2d:
                        statistics.CircleCount++;
                        break;

                    case Curve2dTypeEnum.kCircularArcCurve2d:
                        statistics.ArcCount++;
                        break;

                    case Curve2dTypeEnum.kEllipticalArcCurve2d:
                        statistics.EllipticalArcCount++;
                        break;

                    default:
                        statistics.OtherGeometryCount++;
                        break;
                }
            }
        }

        return statistics;
    }
}