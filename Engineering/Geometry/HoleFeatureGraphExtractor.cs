using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class HoleFeatureGraphExtractor
{
    public void Extract(
        PartAnalysis partAnalysis,
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            partAnalysis);

        ArgumentNullException.ThrowIfNull(
            graph);

        if (partAnalysis.HoleAnalysis.Holes.Count == 0)
        {
            return;
        }

        Dictionary<string, HoleFeatureAnalysis>
            featureAnalysisByName =
                partAnalysis.HoleAnalysis.Features
                    .Where(
                        feature =>
                            !string.IsNullOrWhiteSpace(
                                feature.Name))
                    .GroupBy(
                        feature => feature.Name)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First());

        foreach (HoleInfo hole
                 in partAnalysis.HoleAnalysis.Holes)
        {
            HoleFeatureAnalysis? sourceFeature =
                FindSourceFeature(
                    hole,
                    featureAnalysisByName);

            HoleFeatureNode node =
                CreateHoleNode(
                    hole,
                    sourceFeature);

            graph.AddNode(
                node);
        }

        graph.RefreshMetadata();
    }

    private static HoleFeatureNode CreateHoleNode(
        HoleInfo hole,
        HoleFeatureAnalysis? sourceFeature)
    {
        HoleFeatureNode node =
            new()
            {
                Name =
                    BuildNodeName(
                        hole),

                DiameterMillimeters =
                    hole.Diameter,

                DepthMillimeters =
                    hole.Depth,

                TerminationType =
                    ResolveTerminationType(
                        hole,
                        sourceFeature),

                InstanceCount =
                    1,

                Axis =
                    hole.Axis,

                CenterXMillimeters =
                    hole.CenterX,

                CenterYMillimeters =
                    hole.CenterY,

                CenterZMillimeters =
                    hole.CenterZ,

                SourceFeatureName =
                    sourceFeature?.Name ??
                    hole.Name
            };

        node.SynchronizeParameters();

        node.Parameters[
            nameof(HoleInfo.IsThreaded)] =
                hole.IsThreaded
                    ? 1.0
                    : 0.0;

        node.Parameters[
            nameof(HoleInfo.IsThroughHole)] =
                hole.IsThroughHole
                    ? 1.0
                    : 0.0;

        node.Parameters[
            nameof(HoleInfo.AxisDirectionX)] =
                hole.AxisDirectionX;

        node.Parameters[
            nameof(HoleInfo.AxisDirectionY)] =
                hole.AxisDirectionY;

        node.Parameters[
            nameof(HoleInfo.AxisDirectionZ)] =
                hole.AxisDirectionZ;

        return node;
    }

    private static HoleFeatureAnalysis? FindSourceFeature(
        HoleInfo hole,
        IReadOnlyDictionary<string, HoleFeatureAnalysis>
            featureAnalysisByName)
    {
        if (string.IsNullOrWhiteSpace(
                hole.Name))
        {
            return null;
        }

        if (featureAnalysisByName.TryGetValue(
                hole.Name,
                out HoleFeatureAnalysis? exactMatch))
        {
            return exactMatch;
        }

        return featureAnalysisByName
            .Values
            .FirstOrDefault(
                feature =>
                    hole.Name.StartsWith(
                        feature.Name,
                        StringComparison.OrdinalIgnoreCase) ||
                    feature.Name.StartsWith(
                        hole.Name,
                        StringComparison.OrdinalIgnoreCase));
    }

    private static HoleTerminationType ResolveTerminationType(
        HoleInfo hole,
        HoleFeatureAnalysis? sourceFeature)
    {
        if (hole.IsThroughHole)
        {
            return HoleTerminationType.ThroughAll;
        }

        if (sourceFeature?.HasThroughAllExtent == true)
        {
            return HoleTerminationType.ThroughAll;
        }

        if (sourceFeature?.PassesThroughSheetMetal == true)
        {
            return HoleTerminationType.ThroughAll;
        }

        if (hole.Depth > 0)
        {
            return HoleTerminationType.Distance;
        }

        return HoleTerminationType.Unknown;
    }

    private static string BuildNodeName(
        HoleInfo hole)
    {
        if (!string.IsNullOrWhiteSpace(
                hole.Name))
        {
            return hole.Name;
        }

        return
            $"Hole_Ø{hole.Diameter:F3}_" +
            $"X{hole.CenterX:F3}_" +
            $"Y{hole.CenterY:F3}_" +
            $"Z{hole.CenterZ:F3}";
    }
}