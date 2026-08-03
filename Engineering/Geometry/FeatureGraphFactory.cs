using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraphFactory
{
    private readonly HoleFeatureGraphExtractor
        _holeExtractor;

    private readonly FeatureRelationshipBuilder
        _relationshipBuilder;

    private readonly HoleGroupBuilder
        _holeGroupBuilder;

    public FeatureGraphFactory()
    {
        _holeExtractor =
            new HoleFeatureGraphExtractor();

        _relationshipBuilder =
            new FeatureRelationshipBuilder();

        _holeGroupBuilder =
            new HoleGroupBuilder();
    }

    public FeatureGraph Build(
        PartAnalysis partAnalysis)
    {
        ArgumentNullException.ThrowIfNull(
            partAnalysis);

        FeatureGraphBuilder builder =
            new();

        FeatureGraph graph =
            builder.BuildEmpty(
                partAnalysis.Name,
                string.Empty);

        ExtractFeatures(
            partAnalysis,
            graph);

        BuildRelationships(
            graph);

        BuildEngineeringGroups(
            graph);

        graph.RefreshMetadata();

        return graph;
    }

    private void ExtractFeatures(
        PartAnalysis partAnalysis,
        FeatureGraph graph)
    {
        _holeExtractor.Extract(
            partAnalysis,
            graph);
    }

    private void BuildRelationships(
        FeatureGraph graph)
    {
        _relationshipBuilder.Build(
            graph);
    }

    private void BuildEngineeringGroups(
        FeatureGraph graph)
    {
        _holeGroupBuilder.Build(
            graph);
    }
}