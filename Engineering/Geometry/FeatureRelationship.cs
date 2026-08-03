namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureRelationship
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public FeatureRelationshipType Type { get; set; } =
        FeatureRelationshipType.Unknown;

    public Guid SourceFeatureId { get; set; }

    public Guid TargetFeatureId { get; set; }

    public string Description { get; set; } =
        string.Empty;

    public Dictionary<string, double>
        Parameters
    { get; } =
            new();

    public override string ToString()
    {
        return
            $"{Type}: " +
            $"{SourceFeatureId} -> {TargetFeatureId}";
    }
}