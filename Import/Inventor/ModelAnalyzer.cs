using AI_CAD_ENGINEER.Engineering.Models;

using InventorBox = global::Inventor.Box;
using InventorDocument = global::Inventor.Document;
using InventorMassProperties = global::Inventor.MassProperties;
using InventorPartComponentDefinition =
    global::Inventor.PartComponentDefinition;
using InventorPartDocument =
    global::Inventor.PartDocument;
using InventorPoint =
    global::Inventor.Point;

namespace AI_CAD_ENGINEER.Import.Inventor;

public class ModelAnalyzer
{
    private readonly HoleAnalyzer _holeAnalyzer;

    public ModelAnalyzer()
    {
        _holeAnalyzer =
            new HoleAnalyzer();
    }

    public PartAnalysis Analyze(
        InventorDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document is not InventorPartDocument partDocument)
        {
            throw new ArgumentException(
                "Документ не является деталью.",
                nameof(document));
        }

        InventorPartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        InventorBox rangeBox =
            componentDefinition.RangeBox;

        InventorMassProperties massProperties =
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

        InventorPoint centerOfMass =
            massProperties.CenterOfMass;

        HoleAnalysisResult holeAnalysis =
            _holeAnalyzer.Analyze(partDocument);

        PartAnalysis analysis = new()
        {
            Name =
                partDocument.DisplayName,

            SizeX =
                sizeX,

            SizeY =
                sizeY,

            SizeZ =
                sizeZ,

            Length =
                sortedDimensions[0],

            Width =
                sortedDimensions[1],

            Height =
                sortedDimensions[2],

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
                centimetersToMillimeters,

            HoleAnalysis =
                holeAnalysis
        };

        return analysis;
    }
}