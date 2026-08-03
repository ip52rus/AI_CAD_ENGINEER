namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureNode
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public FeatureType Type { get; set; } =
        FeatureType.Unknown;

    public string Name { get; set; } =
        string.Empty;

    public Dictionary<string, double>
        Parameters
    { get; } =
            new();

    public List<FeatureNode>
        Children
    { get; } =
            new();

    public FeatureNode? Parent { get; set; }

    public override string ToString()
    {
        return
            $"{Type}: {Name}";
    }
}