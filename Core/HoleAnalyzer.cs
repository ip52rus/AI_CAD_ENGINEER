using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class HoleAnalyzer
{
    public List<HoleInfo> Analyze(
        PartDocument partDocument)
    {
        List<HoleInfo> holes = new();

        PartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        HoleFeatures holeFeatures =
            componentDefinition.Features.HoleFeatures;

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("АНАЛИЗ ОТВЕРСТИЙ");
        Console.WriteLine("========================================");
        Console.WriteLine();

        if (holeFeatures.Count == 0)
        {
            Console.WriteLine("Отверстий не найдено.");
            Console.WriteLine("========================================");
            Console.WriteLine();

            return holes;
        }

        int totalPhysicalHoles = 0;

        foreach (HoleFeature holeFeature in holeFeatures)
        {
            int centerPointCount =
                holeFeature.HoleCenterPoints.Count;

            Console.WriteLine(
                $"Операция: {holeFeature.Name}");

            Console.WriteLine(
                $"  Фактических отверстий: {centerPointCount}");

            int pointIndex = 1;

            foreach (SketchPoint centerPoint
                     in holeFeature.HoleCenterPoints)
            {
                Point centerPoint3d =
                    centerPoint.Geometry3d;

                const double centimetersToMillimeters = 10.0;

                HoleInfo holeInfo = new()
                {
                    Name =
                        $"{holeFeature.Name}_{pointIndex}",

                    CenterX =
                        centerPoint3d.X *
                        centimetersToMillimeters,

                    CenterY =
                        centerPoint3d.Y *
                        centimetersToMillimeters,

                    CenterZ =
                        centerPoint3d.Z *
                        centimetersToMillimeters
                };

                holes.Add(holeInfo);

                Console.WriteLine(
                    $"  {pointIndex}. Центр: " +
                    $"X={holeInfo.CenterX:F2}; " +
                    $"Y={holeInfo.CenterY:F2}; " +
                    $"Z={holeInfo.CenterZ:F2} мм");

                pointIndex++;
                totalPhysicalHoles++;
            }

            Console.WriteLine();
        }

        Console.WriteLine(
            $"Операций HoleFeature: {holeFeatures.Count}");

        Console.WriteLine(
            $"Фактических отверстий: {totalPhysicalHoles}");

        Console.WriteLine("========================================");
        Console.WriteLine();

        return holes;
    }
}