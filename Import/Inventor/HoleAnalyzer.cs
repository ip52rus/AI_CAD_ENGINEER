using AI_CAD_ENGINEER.Engineering.Models;

using InventorFace = global::Inventor.Face;
using InventorHoleFeature = global::Inventor.HoleFeature;
using InventorHoleFeatures = global::Inventor.HoleFeatures;
using InventorPartComponentDefinition =
    global::Inventor.PartComponentDefinition;
using InventorPartDocument = global::Inventor.PartDocument;
using InventorPartFeatureExtentEnum =
    global::Inventor.PartFeatureExtentEnum;
using InventorPlanarSketch = global::Inventor.PlanarSketch;
using InventorPlane = global::Inventor.Plane;
using InventorPoint = global::Inventor.Point;
using InventorSheetMetalComponentDefinition =
    global::Inventor.SheetMetalComponentDefinition;
using InventorSketchPoint = global::Inventor.SketchPoint;
using InventorUnitVector = global::Inventor.UnitVector;
using InventorWorkPlane = global::Inventor.WorkPlane;

namespace AI_CAD_ENGINEER.Import.Inventor;

public class HoleAnalyzer
{
    private const double CentimetersToMillimeters =
        10.0;

    private const double ComparisonToleranceMillimeters =
        0.01;

    private const double AxisTolerance =
        0.000001;

    public HoleAnalysisResult Analyze(
        InventorPartDocument partDocument)
    {
        ArgumentNullException.ThrowIfNull(
            partDocument);

        InventorPartComponentDefinition componentDefinition =
            partDocument.ComponentDefinition;

        InventorHoleFeatures holeFeatures =
            componentDefinition.Features.HoleFeatures;

        double? sheetMetalThickness =
            TryGetSheetMetalThickness(
                partDocument);

        HoleAnalysisResult result =
            new()
            {
                IsSheetMetal =
                    sheetMetalThickness.HasValue,

                SheetMetalThickness =
                    sheetMetalThickness,

                FeatureCount =
                    holeFeatures.Count
            };

        foreach (InventorHoleFeature holeFeature
                 in holeFeatures)
        {
            double diameter =
                TryGetDiameterMillimeters(
                    holeFeature);

            double depth =
                TryGetDepthMillimeters(
                    holeFeature);

            bool hasThroughAllExtent =
                TryIsThroughAllExtent(
                    holeFeature);

            bool passesThroughSheetMetal =
                sheetMetalThickness.HasValue &&
                depth +
                ComparisonToleranceMillimeters >=
                sheetMetalThickness.Value;

            bool isThroughHole =
                hasThroughAllExtent ||
                passesThroughSheetMetal;

            HoleAxisResult axisResult =
                TryGetHoleAxis(
                    holeFeature);

            HoleFeatureAnalysis featureAnalysis =
                new()
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

            result.Features.Add(
                featureAnalysis);

            int pointIndex =
                1;

            foreach (InventorSketchPoint centerPoint
                     in holeFeature.HoleCenterPoints)
            {
                InventorPoint centerPoint3d =
                    centerPoint.Geometry3d;

                HoleInfo holeInfo =
                    new()
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
                            CentimetersToMillimeters,

                        Axis =
                            axisResult.Axis,

                        AxisDirectionX =
                            axisResult.DirectionX,

                        AxisDirectionY =
                            axisResult.DirectionY,

                        AxisDirectionZ =
                            axisResult.DirectionZ
                    };

                result.Holes.Add(
                    holeInfo);

                pointIndex++;
            }
        }

        return result;
    }

    private static HoleAxisResult TryGetHoleAxis(
        InventorHoleFeature holeFeature)
    {
        try
        {
            foreach (InventorSketchPoint centerPoint
                     in holeFeature.HoleCenterPoints)
            {
                if (centerPoint.Parent
                    is not InventorPlanarSketch planarSketch)
                {
                    continue;
                }

                InventorUnitVector? normal =
                    TryGetSketchNormal(
                        planarSketch);

                if (normal == null)
                {
                    continue;
                }

                return CreateAxisResult(
                    normal.X,
                    normal.Y,
                    normal.Z);
            }
        }
        catch
        {
            // Используем запасной способ ниже.
        }

        return TryGetHoleAxisFromFeatureFaces(
            holeFeature);
    }

    private static InventorUnitVector? TryGetSketchNormal(
        InventorPlanarSketch planarSketch)
    {
        try
        {
            object planarEntity =
                planarSketch.PlanarEntity;

            if (planarEntity
                is InventorWorkPlane workPlane)
            {
                return workPlane.Plane.Normal;
            }

            if (planarEntity
                    is InventorFace face &&
                face.Geometry
                    is InventorPlane plane)
            {
                return plane.Normal;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static HoleAxisResult
        TryGetHoleAxisFromFeatureFaces(
            InventorHoleFeature holeFeature)
    {
        try
        {
            foreach (InventorFace face
                     in holeFeature.Faces)
            {
                dynamic geometry =
                    face.Geometry;

                string geometryTypeName =
                    geometry
                        .GetType()
                        .Name;

                bool isCylinder =
                    geometryTypeName.Contains(
                        "Cylinder",
                        StringComparison.OrdinalIgnoreCase);

                if (!isCylinder)
                {
                    continue;
                }

                dynamic axisVector =
                    geometry.AxisVector;

                return CreateAxisResult(
                    Convert.ToDouble(
                        axisVector.X),

                    Convert.ToDouble(
                        axisVector.Y),

                    Convert.ToDouble(
                        axisVector.Z));
            }
        }
        catch
        {
            // Если определить ось не удалось,
            // возвращаем Undefined.
        }

        return HoleAxisResult.Undefined;
    }

    private static HoleAxisResult CreateAxisResult(
        double directionX,
        double directionY,
        double directionZ)
    {
        double vectorLength =
            Math.Sqrt(
                directionX * directionX +
                directionY * directionY +
                directionZ * directionZ);

        if (vectorLength <=
            AxisTolerance)
        {
            return HoleAxisResult.Undefined;
        }

        double normalizedX =
            directionX /
            vectorLength;

        double normalizedY =
            directionY /
            vectorLength;

        double normalizedZ =
            directionZ /
            vectorLength;

        ModelAxis axis =
            ResolveDominantAxis(
                normalizedX,
                normalizedY,
                normalizedZ);

        CanonicalizeDirection(
            axis,
            ref normalizedX,
            ref normalizedY,
            ref normalizedZ);

        return new HoleAxisResult(
            axis,
            normalizedX,
            normalizedY,
            normalizedZ);
    }

    private static ModelAxis ResolveDominantAxis(
        double directionX,
        double directionY,
        double directionZ)
    {
        double absoluteX =
            Math.Abs(
                directionX);

        double absoluteY =
            Math.Abs(
                directionY);

        double absoluteZ =
            Math.Abs(
                directionZ);

        if (absoluteX >= absoluteY &&
            absoluteX >= absoluteZ)
        {
            return ModelAxis.X;
        }

        if (absoluteY >= absoluteX &&
            absoluteY >= absoluteZ)
        {
            return ModelAxis.Y;
        }

        return ModelAxis.Z;
    }

    private static void CanonicalizeDirection(
        ModelAxis axis,
        ref double directionX,
        ref double directionY,
        ref double directionZ)
    {
        bool mustReverse =
            axis switch
            {
                ModelAxis.X =>
                    directionX < 0,

                ModelAxis.Y =>
                    directionY < 0,

                ModelAxis.Z =>
                    directionZ < 0,

                _ =>
                    false
            };

        if (!mustReverse)
        {
            return;
        }

        directionX =
            -directionX;

        directionY =
            -directionY;

        directionZ =
            -directionZ;
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
                InventorPartFeatureExtentEnum
                    .kThroughAllExtent;
        }
        catch
        {
            return false;
        }
    }

    private readonly record struct HoleAxisResult(
        ModelAxis Axis,
        double DirectionX,
        double DirectionY,
        double DirectionZ)
    {
        public static HoleAxisResult Undefined =>
            new(
                ModelAxis.Undefined,
                0,
                0,
                0);
    }
}