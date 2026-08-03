using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Infrastructure.Reporting;

public class ConsoleReporter
{
    public void WritePartAnalysis(
        PartAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        WriteHoleAnalysis(
            analysis.HoleAnalysis);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("АНАЛИЗ 3D-МОДЕЛИ");
        Console.WriteLine("========================================");
        Console.WriteLine();

        Console.WriteLine(
            $"Деталь: {analysis.Name}");

        Console.WriteLine();

        Console.WriteLine(
            "Размеры по осям Inventor:");

        Console.WriteLine(
            $"  X: {analysis.SizeX:F2} мм");

        Console.WriteLine(
            $"  Y: {analysis.SizeY:F2} мм");

        Console.WriteLine(
            $"  Z: {analysis.SizeZ:F2} мм");

        Console.WriteLine();

        Console.WriteLine(
            "Инженерные габариты:");

        Console.WriteLine(
            $"  Длина: {analysis.Length:F2} мм");

        Console.WriteLine(
            $"  Ширина: {analysis.Width:F2} мм");

        Console.WriteLine(
            $"  Высота: {analysis.Height:F2} мм");

        Console.WriteLine();

        Console.WriteLine(
            $"Объём: {analysis.Volume:F2} mm3");

        Console.WriteLine(
            $"Площадь поверхности: " +
            $"{analysis.SurfaceArea:F2} mm2");

        Console.WriteLine(
            $"Центр масс: " +
            $"X={analysis.CenterOfMassX:F2}; " +
            $"Y={analysis.CenterOfMassY:F2}; " +
            $"Z={analysis.CenterOfMassZ:F2} мм");

        Console.WriteLine();

        Console.WriteLine(
            $"Фактических отверстий: " +
            $"{analysis.HoleCount}");

        Console.WriteLine(
            $"Фаски: {analysis.ChamferCount}");

        Console.WriteLine(
            $"Скругления: {analysis.FilletCount}");

        Console.WriteLine(
            "========================================");
    }

    private static void WriteHoleAnalysis(
        HoleAnalysisResult analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("АНАЛИЗ ОТВЕРСТИЙ");
        Console.WriteLine("========================================");
        Console.WriteLine();

        if (analysis.IsSheetMetal &&
            analysis.SheetMetalThickness.HasValue)
        {
            Console.WriteLine(
                $"Толщина листового металла: " +
                $"{analysis.SheetMetalThickness.Value:F2} мм");

            Console.WriteLine();
        }

        if (analysis.FeatureCount == 0)
        {
            Console.WriteLine(
                "Отверстий не найдено.");

            Console.WriteLine(
                "========================================");

            return;
        }

        foreach (HoleFeatureAnalysis feature
                 in analysis.Features)
        {
            Console.WriteLine(
                $"Операция: {feature.Name}");

            Console.WriteLine(
                $"  Диаметр: " +
                $"{feature.Diameter:F2} мм");

            Console.WriteLine(
                $"  Заданная глубина: " +
                $"{feature.Depth:F2} мм");

            Console.WriteLine(
                $"  Тип завершения Through All: " +
                $"{FormatBoolean(feature.HasThroughAllExtent)}");

            if (analysis.IsSheetMetal)
            {
                Console.WriteLine(
                    $"  Проходит через толщину листа: " +
                    $"{FormatBoolean(feature.PassesThroughSheetMetal)}");
            }

            Console.WriteLine(
                $"  Итоговый тип: " +
                $"{(feature.IsThroughHole
                    ? "сквозное"
                    : "глухое")}");

            Console.WriteLine(
                $"  Фактических отверстий: " +
                $"{feature.PhysicalHoleCount}");

            IReadOnlyList<HoleInfo> featureHoles =
                analysis.Holes
                    .Where(hole =>
                        hole.Name.StartsWith(
                            feature.Name + "_",
                            StringComparison.Ordinal))
                    .ToList();

            int index = 1;

            foreach (HoleInfo hole in featureHoles)
            {
                string depthText =
                    hole.IsThroughHole
                        ? "сквозное"
                        : $"глубина {hole.Depth:F2} мм";

                Console.WriteLine(
                    $"  {index}. " +
                    $"Диаметр: {hole.Diameter:F2} мм; " +
                    $"{depthText}; " +
                    $"центр: " +
                    $"X={hole.CenterX:F2}; " +
                    $"Y={hole.CenterY:F2}; " +
                    $"Z={hole.CenterZ:F2} мм");

                index++;
            }

            Console.WriteLine();
        }

        Console.WriteLine(
            $"Операций HoleFeature: " +
            $"{analysis.FeatureCount}");

        Console.WriteLine(
            $"Фактических отверстий: " +
            $"{analysis.PhysicalHoleCount}");

        Console.WriteLine(
            "========================================");
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}