namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureGraph
{
    public FeatureGraphMetadata Metadata { get; } =
        new();

    public List<FeatureNode>
        Nodes
    { get; } =
            new();

    public List<FeatureRelationship>
        Relationships
    { get; } =
            new();

    public void AddNode(
        FeatureNode node)
    {
        ArgumentNullException.ThrowIfNull(
            node);

        bool alreadyExists =
            Nodes.Any(
                existingNode =>
                    existingNode.Id ==
                    node.Id);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                $"FeatureNode с идентификатором " +
                $"{node.Id} уже существует.");
        }

        Nodes.Add(
            node);

        Metadata.UpdateFrom(
            this);
    }

    public void AddRelationship(
        FeatureRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(
            relationship);

        ValidateRelationship(
            relationship);

        bool alreadyExists =
            Relationships.Any(
                existingRelationship =>
                    existingRelationship.Id ==
                    relationship.Id);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                $"FeatureRelationship с идентификатором " +
                $"{relationship.Id} уже существует.");
        }

        Relationships.Add(
            relationship);

        Metadata.UpdateFrom(
            this);
    }

    public IEnumerable<FeatureNode>
        GetFeatures(
            FeatureType type)
    {
        return Nodes.Where(
            node =>
                node.Type ==
                type);
    }

    public FeatureNode? Find(
        Guid id)
    {
        return Nodes.FirstOrDefault(
            node =>
                node.Id ==
                id);
    }

    public IEnumerable<FeatureRelationship>
        GetRelationships(
            Guid featureId)
    {
        return Relationships.Where(
            relationship =>
                relationship.SourceFeatureId ==
                    featureId ||
                relationship.TargetFeatureId ==
                    featureId);
    }

    public IEnumerable<FeatureNode>
        GetRelatedFeatures(
            Guid featureId,
            FeatureRelationshipType? relationshipType = null)
    {
        IEnumerable<FeatureRelationship> relationships =
            GetRelationships(
                featureId);

        if (relationshipType.HasValue)
        {
            relationships =
                relationships.Where(
                    relationship =>
                        relationship.Type ==
                        relationshipType.Value);
        }

        foreach (FeatureRelationship relationship
                 in relationships)
        {
            Guid relatedFeatureId =
                relationship.SourceFeatureId ==
                    featureId
                    ? relationship.TargetFeatureId
                    : relationship.SourceFeatureId;

            FeatureNode? relatedFeature =
                Find(
                    relatedFeatureId);

            if (relatedFeature != null)
            {
                yield return relatedFeature;
            }
        }
    }

    public bool RemoveNode(
        Guid id)
    {
        FeatureNode? node =
            Find(
                id);

        if (node == null)
        {
            return false;
        }

        Relationships.RemoveAll(
            relationship =>
                relationship.SourceFeatureId ==
                    id ||
                relationship.TargetFeatureId ==
                    id);

        Nodes.Remove(
            node);

        Metadata.UpdateFrom(
            this);

        return true;
    }

    public bool RemoveRelationship(
        Guid id)
    {
        FeatureRelationship? relationship =
            Relationships.FirstOrDefault(
                item =>
                    item.Id ==
                    id);

        if (relationship == null)
        {
            return false;
        }

        Relationships.Remove(
            relationship);

        Metadata.UpdateFrom(
            this);

        return true;
    }

    public void RefreshMetadata()
    {
        Metadata.UpdateFrom(
            this);
    }

    public override string ToString()
    {
        return
            $"Nodes: {Nodes.Count}; " +
            $"Relationships: {Relationships.Count}";
    }

    private void ValidateRelationship(
        FeatureRelationship relationship)
    {
        if (relationship.SourceFeatureId ==
            Guid.Empty)
        {
            throw new InvalidOperationException(
                "SourceFeatureId не может быть пустым.");
        }

        if (relationship.TargetFeatureId ==
            Guid.Empty)
        {
            throw new InvalidOperationException(
                "TargetFeatureId не может быть пустым.");
        }

        if (relationship.SourceFeatureId ==
            relationship.TargetFeatureId)
        {
            throw new InvalidOperationException(
                "Связь не может указывать на один " +
                "и тот же FeatureNode.");
        }

        if (Find(
                relationship.SourceFeatureId) == null)
        {
            throw new InvalidOperationException(
                "Исходный FeatureNode отсутствует в графе.");
        }

        if (Find(
                relationship.TargetFeatureId) == null)
        {
            throw new InvalidOperationException(
                "Целевой FeatureNode отсутствует в графе.");
        }
    }
}