namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraphMetadata
{
    public string SourceDocumentName { get; set; } =
        string.Empty;

    public string SourceDocumentPath { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; } =
        DateTime.Now;

    public int NodeCount { get; set; }

    public int RelationshipCount { get; set; }

    public Dictionary<FeatureType, int>
        FeatureCounts
    { get; } =
            new();

    public void UpdateFrom(
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            graph);

        NodeCount =
            graph.Nodes.Count;

        RelationshipCount =
            graph.Relationships.Count;

        FeatureCounts.Clear();

        foreach (IGrouping<FeatureType, FeatureNode> group
                 in graph.Nodes.GroupBy(
                     node => node.Type))
        {
            FeatureCounts[group.Key] =
                group.Count();
        }
    }
}