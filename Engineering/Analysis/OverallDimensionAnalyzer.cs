using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Analysis;

public class OverallDimensionAnalyzer
{
    public (double MinX,
            double MaxX,
            double MinY,
            double MaxY)
        Analyze(DrawingView drawingView)
    {
        ArgumentNullException.ThrowIfNull(drawingView);

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            throw new InvalidOperationException(
                "DrawingCurves отсутствуют.");
        }

        double minX = double.MaxValue;
        double maxX = double.MinValue;
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment in curve.Segments)
            {
                if (segment.HiddenLine)
                {
                    continue;
                }

                switch (segment.Geometry)
                {
                    case LineSegment2d line:

                        Update(ref minX, ref maxX,
                            line.StartPoint.X);

                        Update(ref minX, ref maxX,
                            line.EndPoint.X);

                        Update(ref minY, ref maxY,
                            line.StartPoint.Y);

                        Update(ref minY, ref maxY,
                            line.EndPoint.Y);

                        break;

                    case Circle2d circle:

                        Update(ref minX, ref maxX,
                            circle.Center.X - circle.Radius);

                        Update(ref minX, ref maxX,
                            circle.Center.X + circle.Radius);

                        Update(ref minY, ref maxY,
                            circle.Center.Y - circle.Radius);

                        Update(ref minY, ref maxY,
                            circle.Center.Y + circle.Radius);

                        break;
                }
            }
        }

        return (
            minX,
            maxX,
            minY,
            maxY);
    }

    private static void Update(
        ref double min,
        ref double max,
        double value)
    {
        if (value < min)
            min = value;

        if (value > max)
            max = value;
    }
}