using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Analysis;

public class DrawingGeometryAnalyzer
{
    public void Analyze(DrawingView drawingView)
    {
        ArgumentNullException.ThrowIfNull(drawingView);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("ГЕОМЕТРИЯ ЧЕРТЕЖНОГО ВИДА");
        Console.WriteLine("========================================");

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            Console.WriteLine("Кривые отсутствуют.");
            return;
        }

        Console.WriteLine(
            $"Количество DrawingCurve: {curves.Count}");

        int curveIndex = 1;

        foreach (DrawingCurve curve in curves)
        {
            Console.WriteLine();

            Console.WriteLine(
                $"Кривая {curveIndex}");

            Console.WriteLine(
                $"  Тип кривой: {curve.CurveType}");

            Console.WriteLine(
                $"  Сегментов: {curve.Segments.Count}");

            int segmentIndex = 1;

            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                Console.WriteLine(
                    $"    {segmentIndex}. {segment.GeometryType}");

                segmentIndex++;
            }

            curveIndex++;
        }

        Console.WriteLine(
            "========================================");
    }
}