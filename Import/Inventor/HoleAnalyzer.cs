using AI_CAD_ENGINEER.Engineering.Models;

using InventorHoleFeature = global::Inventor.HoleFeature;
using InventorHoleFeatures = global::Inventor.HoleFeatures;
using InventorPartComponentDefinition =
    global::Inventor.PartComponentDefinition;
using InventorPartDocument = global::Inventor.PartDocument;
using InventorPartFeatureExtentEnum =
    global::Inventor.PartFeatureExtentEnum;
using InventorPoint = global::Inventor.Point;
using InventorSheetMetalComponentDefinition =
    global::Inventor.SheetMetalComponentDefinition;
using InventorSketchPoint = global::Inventor.SketchPoint;

namespace AI_CAD_ENGINEER.Import.Inventor;

public class HoleAnalyzer
{
    private const double CentimetersToMillimeters = 10.0;
    private const double ComparisonToleranceMillimeters = 0.01;

    public HoleAnalysisResult Analyze(
        InventorPartDocument partDocument)
    {
        ArgumentNullException.ThrowIfNull(partDocument);

        InventorPartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        InventorHoleFeatures holeFeatures =
            componentDefinition.Features.HoleFeatures;

        double? sheetMetalThickness =
            TryGetSheetMetalThickness(partDocument);

        HoleAnalysisResult result = new()
        {
            IsSheetMetal =
                sheetMetalThickness.HasValue,

            SheetMetalThickness =
                sheetMetalThickness,

            FeatureCount =
                holeFeatures.Count
        };

        foreach (InventorHoleFeature holeFeature in holeFeatures)
        {
            double diameter =
                TryGetDiameterMillimeters(holeFeature);

            double depth =
                TryGetDepthMillimeters(holeFeature);

            bool hasThroughAllExtent =
                TryIsThroughAllExtent(holeFeature);

            bool passesThroughSheetMetal =
                sheetMetalThickness.HasValue &&
                depth + ComparisonToleranceMillimeters >=
                sheetMetalThickness.Value;

            bool isThroughHole =
                hasThroughAllExtent ||
                passesThroughSheetMetal;

            HoleFeatureAnalysis featureAnalysis = new()
            {
                Name =
                    holeFeature.Name,

                Diameter =
                    diameter,

                Depth =
                    depth,

                HasThroughAllExtent =
                    hasThroughAllExtent,

                PassesThroughSheetMetal =
                    passesThroughSheetMetal,

                IsThroughHole =
                    isThroughHole,

                PhysicalHoleCount =
                    holeFeature.HoleCenterPoints.Count
            };

            result.Features.Add(featureAnalysis);

            int pointIndex = 1;

            foreach (InventorSketchPoint centerPoint
                     in holeFeature.HoleCenterPoints)
            {
                InventorPoint centerPoint3d =
                    centerPoint.Geometry3d;

                HoleInfo holeInfo = new()
                {
                    Name =
                        $"{holeFeature.Name}_{pointIndex}",

                    Diameter =
                        diameter,

                    Depth =
                        depth,

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

                result.Holes.Add(holeInfo);

                pointIndex++;
            }
        }

        return result;
    }

    private static double? TryGetSheetMetalThickness(
        InventorPartDocument partDocument)
    {
        try
        {
            if (partDocument.ComponentDefinition
                is not InventorSheetMetalComponentDefinition
                sheetMetalDefinition)
            {
                return null;
            }

            return
                sheetMetalDefinition.Thickness.Value *
                CentimetersToMillimeters;
        }
        catch
        {
            return null;
        }
    }

    private static double TryGetDiameterMillimeters(
        InventorHoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.HoleDiameter.Value *
                CentimetersToMillimeters;
        }
        catch
        {
            return 0;
        }
    }

    private static double TryGetDepthMillimeters(
        InventorHoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.Depth *
                CentimetersToMillimeters;
        }
        catch
        {
            return 0;
        }
    }

    private static bool TryIsThroughAllExtent(
        InventorHoleFeature holeFeature)
    {
        try
        {
            return
                holeFeature.ExtentType ==
                InventorPartFeatureExtentEnum.kThroughAllExtent;
        }
        catch
        {
            return false;
        }
    }
}