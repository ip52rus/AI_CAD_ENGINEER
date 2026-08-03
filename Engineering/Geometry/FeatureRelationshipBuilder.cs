namespace AI_CAD_ENGINEER.Engineering.Geometry;

public class FeatureRelationshipBuilder
{
    private const double DiameterToleranceMillimeters =
        0.01;

    private const double PositionToleranceMillimeters =
        0.01;

    public void Build(
        FeatureGraph graph)
    {
        ArgumentNullException.ThrowIfNull(
            graph);

        BuildSameSourceFeatureRelationships(
            graph);

        BuildSameDiameterRelationships(
            graph);

        BuildSameAxisRelationships(
            graph);

        graph.RefreshMetadata();
    }

    private static void BuildSameSourceFeatureRelationships(
        FeatureGraph graph)
    {
        List<HoleFeatureNode> holes =
            graph.Nodes
                .OfType<HoleFeatureNode>()
                .Where(
                    hole =>
                        !string.IsNullOrWhiteSpace(
                            hole.SourceFeatureName))
                .ToList();

        IEnumerable<IGrouping<string, HoleFeatureNode>> groups =
            holes.GroupBy(
                hole =>
                    hole.SourceFeatureName,
                StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<string, HoleFeatureNode> group
                 in groups)
        {
            AddRelationshipsForAllPairs(
                graph,
                group.ToList(),
                FeatureRelationshipType.SameSourceFeature,
                (_, _) =>
                    true,
                (_, _) =>
                    $"Созданы одной операцией Inventor " +
                    $"\"{group.Key}\".");
        }
    }

    private static void BuildSameDiameterRelationships(
        FeatureGraph graph)
    {
        List<HoleFeatureNode> holes =
            graph.Nodes
                .OfType<HoleFeatureNode>()
                .ToList();

        AddRelationshipsForAllPairs(
            graph,
            holes,
            FeatureRelationshipType.SameDiameter,
            HaveSameDiameter,
            (firstHole, _) =>
                $"Одинаковый диаметр: " +
                $"Ø{firstHole.DiameterMillimeters:F3} мм.",
            relationship =>
            {
                HoleFeatureNode source =
                    (HoleFeatureNode)graph.Find(
                        relationship.SourceFeatureId)!;

                relationship.Parameters[
                    "DiameterMillimeters"] =
                        source.DiameterMillimeters;
            });
    }

    private static void BuildSameAxisRelationships(
        FeatureGraph graph)
    {
        List<HoleFeatureNode> holes =
            graph.Nodes
                .OfType<HoleFeatureNode>()
                .ToList();

        AddRelationshipsForAllPairs(
            graph,
            holes,
            FeatureRelationshipType.SameAxis,
            AreCollinear,
            (firstHole, _) =>
                $"Оси отверстий совпадают. " +
                $"Физическая ось: {firstHole.Axis}.",
            relationship =>
            {
                HoleFeatureNode source =
                    (HoleFeatureNode)graph.Find(
                        relationship.SourceFeatureId)!;

                relationship.Parameters[
                    "Axis"] =
                        (double)source.Axis;
            });
    }

    private static bool HaveSameDiameter(
        HoleFeatureNode firstHole,
        HoleFeatureNode secondHole)
    {
        return
            Math.Abs(
                firstHole.DiameterMillimeters -
                secondHole.DiameterMillimeters) <=
            DiameterToleranceMillimeters;
    }

    private static bool AreCollinear(
        HoleFeatureNode firstHole,
        HoleFeatureNode secondHole)
    {
        if (firstHole.Axis ==
            Engineering.Models.ModelAxis.Undefined)
        {
            return false;
        }

        if (firstHole.Axis !=
            secondHole.Axis)
        {
            return false;
        }

        return firstHole.Axis switch
        {
            Engineering.Models.ModelAxis.X =>
                AreEqual(
                    firstHole.CenterYMillimeters,
                    secondHole.CenterYMillimeters) &&
                AreEqual(
                    firstHole.CenterZMillimeters,
                    secondHole.CenterZMillimeters),

            Engineering.Models.ModelAxis.Y =>
                AreEqual(
                    firstHole.CenterXMillimeters,
                    secondHole.CenterXMillimeters) &&
                AreEqual(
                    firstHole.CenterZMillimeters,
                    secondHole.CenterZMillimeters),

            Engineering.Models.ModelAxis.Z =>
                AreEqual(
                    firstHole.CenterXMillimeters,
                    secondHole.CenterXMillimeters) &&
                AreEqual(
                    firstHole.CenterYMillimeters,
                    secondHole.CenterYMillimeters),

            _ =>
                false
        };
    }

    private static bool AreEqual(
        double firstValue,
        double secondValue)
    {
        return
            Math.Abs(
                firstValue -
                secondValue) <=
            PositionToleranceMillimeters;
    }

    private static void AddRelationshipsForAllPairs(
        FeatureGraph graph,
        IReadOnlyList<HoleFeatureNode> holes,
        FeatureRelationshipType relationshipType,
        Func<HoleFeatureNode, HoleFeatureNode, bool>
            condition,
        Func<HoleFeatureNode, HoleFeatureNode, string>
            descriptionFactory,
        Action<FeatureRelationship>? configureRelationship = null)
    {
        if (holes.Count < 2)
        {
            return;
        }

        for (int firstIndex = 0;
             firstIndex < holes.Count - 1;
             firstIndex++)
        {
            for (int secondIndex = firstIndex + 1;
                 secondIndex < holes.Count;
                 secondIndex++)
            {
                HoleFeatureNode firstHole =
                    holes[firstIndex];

                HoleFeatureNode secondHole =
                    holes[secondIndex];

                if (!condition(
                        firstHole,
                        secondHole))
                {
                    continue;
                }

                if (RelationshipExists(
                        graph,
                        firstHole.Id,
                        secondHole.Id,
                        relationshipType))
                {
                    continue;
                }

                FeatureRelationship relationship =
                    new()
                    {
                        Type =
                            relationshipType,

                        SourceFeatureId =
                            firstHole.Id,

                        TargetFeatureId =
                            secondHole.Id,

                        Description =
                            descriptionFactory(
                                firstHole,
                                secondHole)
                    };

                configureRelationship?.Invoke(
                    relationship);

                graph.AddRelationship(
                    relationship);
            }
        }
    }

    private static bool RelationshipExists(
        FeatureGraph graph,
        Guid firstFeatureId,
        Guid secondFeatureId,
        FeatureRelationshipType relationshipType)
    {
        return graph.Relationships.Any(
            relationship =>
                relationship.Type ==
                    relationshipType &&
                (
                    relationship.SourceFeatureId ==
                        firstFeatureId &&
                    relationship.TargetFeatureId ==
                        secondFeatureId
                    ||
                    relationship.SourceFeatureId ==
                        secondFeatureId &&
                    relationship.TargetFeatureId ==
                        firstFeatureId
                ));
    }
}