using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class HoleAnalyzer
{
    private const double CentimetersToMillimeters = 10.0;

    // Допуск нужен из-за возможных погрешностей double.
    private const double ComparisonToleranceMillimeters = 0.01;

    public List<HoleInfo> Analyze(
        PartDocument partDocument)
    {
        List<HoleInfo> holes = new();

        PartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        HoleFeatures holeFeatures =
            componentDefinition.Features.HoleFeatures;

        double? sheetMetalThicknessMillimeters =
            TryGetSheetMetalThickness(partDocument);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("АНАЛИЗ ОТВЕРСТИЙ");
        Console.WriteLine("========================================");
        Console.WriteLine();

        if (sheetMetalThicknessMillimeters.HasValue)
        {
            Console.WriteLine(
                $"Толщина листового металла: " +
                $"{sheetMetalThicknessMillimeters.Value:F2} мм");

            Console.WriteLine();
        }

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

            double diameterMillimeters =
                TryGetDiameterMillimeters(holeFeature);

            double depthMillimeters =
                TryGetDepthMillimeters(holeFeature);

            bool hasThroughAllExtent =
                TryIsThroughAllExtent(holeFeature);

            bool passesThroughSheetMetal =
                sheetMetalThicknessMillimeters.HasValue &&
                depthMillimeters +
                ComparisonToleranceMillimeters >=
                sheetMetalThicknessMillimeters.Value;

            bool isThroughHole =
                hasThroughAllExtent ||
                passesThroughSheetMetal;

            Console.WriteLine(
                $"Операция: {holeFeature.Name}");

            Console.WriteLine(
                $"  Диаметр: {diameterMillimeters:F2} мм");

            Console.WriteLine(
                $"  Заданная глубина: {depthMillimeters:F2} мм");

            Console.WriteLine(
                $"  Тип завершения Through All: " +
                $"{(hasThroughAllExtent ? "да" : "нет")}");

            if (sheetMetalThicknessMillimeters.HasValue)
            {
                Console.WriteLine(
                    $"  Проходит через толщину листа: " +
                    $"{(passesThroughSheetMetal ? "да" : "нет")}");
            }

            Console.WriteLine(
                $"  Итоговый тип: " +
                $"{(isThroughHole ? "сквозное" : "глухое")}");

            Console.WriteLine(
                $"  Фактических отверстий: {centerPointCount}");

            int pointIndex = 1;

            foreach (SketchPoint centerPoint
                     in holeFeature.HoleCenterPoints)
            {
                Point centerPoint3d =
                    centerPoint.Geometry3d;

                HoleInfo holeInfo = new()
                {
                    Name =
                        $"{holeFeature.Name}_{pointIndex}",

                    Diameter =
                        diameterMillimeters,

                    Depth =
                        depthMillimeters,

                    IsThroughHole =
                        isThroughHole,

                    CenterX =
                        centerPoint3d.X *
                        CentimetersToMillimeters,

                    CenterY =
                        centerPoint3d.Y *
                        CentimetersToMillimeters,

                    CenterZ =
                        centerPoint3d.Z *
                        CentimetersToMillimeters
                };

                holes.Add(holeInfo);

                string depthText =
                    holeInfo.IsThroughHole
                        ? "сквозное"
                        : $"глубина {holeInfo.Depth:F2} мм";

                Console.WriteLine(
                    $"  {pointIndex}. " +
                    $"Диаметр: {holeInfo.Diameter:F2} мм; " +
                    $"{depthText}; " +
                    $"центр: " +
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

    private static double? TryGetSheetMetalThickness(
        PartDocument partDocument)
    {
        try
        {
            if (partDocument.ComponentDefinition
                is not SheetMetalComponentDefinition
                sheetMetalDefinition)
            {
                return null;
            }

            return
                sheetMetalDefinition.Thickness.Value *
                CentimetersToMillimeters;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                "Не удалось получить толщину листового металла.");

            Console.WriteLine(
                $"Причина: {exception.Message}");

            return null;
        }
    }

    private static double TryGetDiameterMillimeters(
        HoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.HoleDiameter.Value *
                CentimetersToMillimeters;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"Операция {holeFeature.Name}: " +
                "диаметр получить не удалось.");

            Console.WriteLine(
                $"Причина: {exception.Message}");

            return 0;
        }
    }

    private static double TryGetDepthMillimeters(
        HoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.Depth *
                CentimetersToMillimeters;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"Операция {holeFeature.Name}: " +
                "глубину получить не удалось.");

            Console.WriteLine(
                $"Причина: {exception.Message}");

            return 0;
        }
    }

    private static bool TryIsThroughAllExtent(
        HoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.ExtentType ==
                PartFeatureExtentEnum.kThroughAllExtent;
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                $"Операция {holeFeature.Name}: " +
                "тип завершения получить не удалось.");

            Console.WriteLine(
                $"Причина: {exception.Message}");

            return false;
        }
    }
}