namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraphBuilder
{
    private readonly FeatureGraph
        _graph;

    public FeatureGraphBuilder()
    {
        _graph =
            new FeatureGraph();
    }

    public FeatureGraph BuildEmpty(
        string documentName,
        string documentPath)
    {
        _graph.Metadata.SourceDocumentName =
            documentName;

        _graph.Metadata.SourceDocumentPath =
            documentPath;

        _graph.RefreshMetadata();

        return _graph;
    }

    public FeatureNode CreateNode(
        FeatureType type,
        string name)
    {
        FeatureNode node =
            new()
            {
                Type = type,
                Name = name
            };

        _graph.AddNode(
            node);

        return node;
    }

    public FeatureRelationship Connect(
        FeatureNode source,
        FeatureNode target,
        FeatureRelationshipType relationshipType)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            target);

        FeatureRelationship relationship =
            new()
            {
                Type = relationshipType,
                SourceFeatureId = source.Id,
                TargetFeatureId = target.Id
            };

        _graph.AddRelationship(
            relationship);

        return relationship;
    }

    public FeatureGraph Graph =>
        _graph;
}