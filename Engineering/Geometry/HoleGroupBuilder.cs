namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class HoleGroupBuilder
{
    private const double ValueToleranceMillimeters =
        0.01;

    public void Build(
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            graph);

        List<HoleFeatureNode> holes =
            graph.Nodes
                .OfType<HoleFeatureNode>()
                .ToList();

        IEnumerable<IGrouping<HoleGroupKey, HoleFeatureNode>>
            groups =
                holes.GroupBy(
                    CreateGroupKey);

        foreach (IGrouping<HoleGroupKey, HoleFeatureNode> group
                 in groups)
        {
            List<HoleFeatureNode> groupedHoles =
                group.ToList();

            if (groupedHoles.Count < 2)
            {
                continue;
            }

            HoleGroupFeatureNode groupNode =
                CreateGroupNode(
                    group.Key,
                    groupedHoles);

            graph.AddNode(
                groupNode);

            ConnectHolesToGroup(
                graph,
                groupedHoles,
                groupNode);
        }

        graph.RefreshMetadata();
    }

    private static HoleGroupKey CreateGroupKey(
        HoleFeatureNode hole)
    {
        return new HoleGroupKey(
            NormalizeValue(
                hole.DiameterMillimeters),

            NormalizeValue(
                hole.DepthMillimeters),

            hole.Axis,

            hole.TerminationType,

            hole.SourceFeatureName);
    }

    private static HoleGroupFeatureNode CreateGroupNode(
        HoleGroupKey key,
        IReadOnlyCollection<HoleFeatureNode> holes)
    {
        HoleGroupFeatureNode groupNode =
            new()
            {
                Name =
                    BuildGroupName(
                        key,
                        holes.Count),

                DiameterMillimeters =
                    key.DiameterMillimeters,

                DepthMillimeters =
                    key.DepthMillimeters,

                HoleCount =
                    holes.Count,

                Axis =
                    key.Axis,

                TerminationType =
                    key.TerminationType,

                SourceFeatureName =
                    key.SourceFeatureName
            };

        foreach (HoleFeatureNode hole
                 in holes)
        {
            groupNode.HoleFeatureIds.Add(
                hole.Id);
        }

        groupNode.SynchronizeParameters();

        return groupNode;
    }

    private static void ConnectHolesToGroup(
        FeatureGraph graph,
        IEnumerable<HoleFeatureNode> holes,
        HoleGroupFeatureNode groupNode)
    {
        foreach (HoleFeatureNode hole
                 in holes)
        {
            FeatureRelationship relationship =
                new()
                {
                    Type =
                        FeatureRelationshipType.PartOfPattern,

                    SourceFeatureId =
                        hole.Id,

                    TargetFeatureId =
                        groupNode.Id,

                    Description =
                        $"Отверстие \"{hole.Name}\" входит " +
                        $"в инженерную группу " +
                        $"\"{groupNode.Name}\"."
                };

            relationship.Parameters[
                "GroupHoleCount"] =
                    groupNode.HoleCount;

            relationship.Parameters[
                "DiameterMillimeters"] =
                    groupNode.DiameterMillimeters;

            graph.AddRelationship(
                relationship);
        }
    }

    private static string BuildGroupName(
        HoleGroupKey key,
        int holeCount)
    {
        string sourceName =
            string.IsNullOrWhiteSpace(
                key.SourceFeatureName)
                ? "HoleGroup"
                : key.SourceFeatureName;

        return
            $"{sourceName}_Group_" +
            $"{holeCount}xØ" +
            $"{key.DiameterMillimeters:F3}";
    }

    private static double NormalizeValue(
        double value)
    {
        return
            Math.Round(
                value /
                ValueToleranceMillimeters) *
            ValueToleranceMillimeters;
    }

    private readonly record struct HoleGroupKey(
        double DiameterMillimeters,
        double DepthMillimeters,
        Engineering.Models.ModelAxis Axis,
        HoleTerminationType TerminationType,
        string SourceFeatureName);
}