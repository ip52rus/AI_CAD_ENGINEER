using AI_CAD_ENGINEER.Engineering.Models;
using Inventor;

namespace AI_CAD_ENGINEER.Import.Inventor;

public class ModelAnalyzer
{
    private readonly HoleAnalyzer _holeAnalyzer;

    public ModelAnalyzer()
    {
        _holeAnalyzer = new HoleAnalyzer();
    }

    public PartAnalysis Analyze(Document document)
    {
        if (document is not PartDocument partDocument)
        {
            throw new ArgumentException(
                "Документ не является деталью.",
                nameof(document));
        }

        PartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        Box rangeBox =
            componentDefinition.RangeBox;

        MassProperties massProperties =
            componentDefinition.MassProperties;

        const double centimetersToMillimeters = 10.0;
        const double squareCentimetersToSquareMillimeters = 100.0;
        const double cubicCentimetersToCubicMillimeters = 1000.0;

        double sizeX =
            (rangeBox.MaxPoint.X - rangeBox.MinPoint.X) *
            centimetersToMillimeters;

        double sizeY =
            (rangeBox.MaxPoint.Y - rangeBox.MinPoint.Y) *
            centimetersToMillimeters;

        double sizeZ =
            (rangeBox.MaxPoint.Z - rangeBox.MinPoint.Z) *
            centimetersToMillimeters;

        double[] sortedDimensions =
        {
            sizeX,
            sizeY,
            sizeZ
        };

        Array.Sort(sortedDimensions);
        Array.Reverse(sortedDimensions);

        double length = sortedDimensions[0];
        double width = sortedDimensions[1];
        double height = sortedDimensions[2];

        Point centerOfMass =
            massProperties.CenterOfMass;

        PartAnalysis analysis = new()
        {
            Name = partDocument.DisplayName,

            SizeX = sizeX,
            SizeY = sizeY,
            SizeZ = sizeZ,

            Length = length,
            Width = width,
            Height = height,

            Volume =
                massProperties.Volume *
                cubicCentimetersToCubicMillimeters,

            SurfaceArea =
                massProperties.Area *
                squareCentimetersToSquareMillimeters,

            CenterOfMassX =
                centerOfMass.X *
                centimetersToMillimeters,

            CenterOfMassY =
                centerOfMass.Y *
                centimetersToMillimeters,

            CenterOfMassZ =
                centerOfMass.Z *
                centimetersToMillimeters
        };

        List<HoleInfo> holes =
            _holeAnalyzer.Analyze(partDocument);

        analysis.Holes.AddRange(holes);

        return analysis;
    }

    public void PrintReport(PartAnalysis analysis)
    {
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("АНАЛИЗ 3D-МОДЕЛИ");
        Console.WriteLine("========================================");
        Console.WriteLine();

        Console.WriteLine($"Деталь: {analysis.Name}");
        Console.WriteLine();

        Console.WriteLine("Размеры по осям Inventor:");

        Console.WriteLine(
            $"  X: {analysis.SizeX:F2} мм");

        Console.WriteLine(
            $"  Y: {analysis.SizeY:F2} мм");

        Console.WriteLine(
            $"  Z: {analysis.SizeZ:F2} мм");

        Console.WriteLine();

        Console.WriteLine("Инженерные габариты:");

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
            $"Фактических отверстий: {analysis.HoleCount}");

        Console.WriteLine(
            $"Фаски: {analysis.ChamferCount}");

        Console.WriteLine(
            $"Скругления: {analysis.FilletCount}");

        Console.WriteLine("========================================");
    }
}