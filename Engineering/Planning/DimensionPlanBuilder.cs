using AI_CAD_ENGINEER.Engineering.Decision;
using AI_CAD_ENGINEER.Engineering.Geometry;
using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanBuilder
{
    public DimensionPlan Build(
        string documentName,
        string documentPath,
        DimensionDecisionResult decisionResult,
        FeatureGraph featureGraph,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentPath);

        ArgumentNullException.ThrowIfNull(
            decisionResult);

        ArgumentNullException.ThrowIfNull(
            featureGraph);

        ArgumentNullException.ThrowIfNull(
            viewMappings);

        DimensionPlan plan =
            new()
            {
                SourceDocumentName =
                    documentName,

                SourceDocumentPath =
                    documentPath
            };

        AddOverallDimensions(
            decisionResult,
            plan);

        AddHoleGroups(
            featureGraph,
            viewMappings,
            plan);

        AddSingleHoles(
            featureGraph,
            viewMappings,
            plan);

        DimensionPlanValidator validator =
            new();

        validator.Validate(
            plan);

        if (plan.IsValid)
        {
            plan.MarkReadyForDrawing();
        }

        return plan;
    }

    private static void AddOverallDimensions(
        DimensionDecisionResult decisionResult,
        DimensionPlan plan)
    {
        foreach (KeyValuePair<string, List<DimensionCandidate>>
                 entry in decisionResult.RequiredByView)
        {
            foreach (DimensionCandidate candidate
                     in entry.Value)
            {
                plan.OverallDimensions.Add(
                    new OverallDimensionPlanItem
                    {
                        Candidate =
                            candidate,

                        TargetViewName =
                            entry.Key,

                        DecisionReason =
                            string.IsNullOrWhiteSpace(
                                candidate.DecisionReason)
                                ? "Размер выбран Dimension Decision Engine."
                                : candidate.DecisionReason
                    });
            }
        }
    }

    private static void AddHoleGroups(
        FeatureGraph featureGraph,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings,
        DimensionPlan plan)
    {
        DimensionPlanViewResolver viewResolver =
            new();

        foreach (HoleGroupFeatureNode group
                 in featureGraph.Nodes
                     .OfType<HoleGroupFeatureNode>())
        {
            string targetViewName =
                viewResolver.ResolveForHoleGroup(
                    group,
                    viewMappings);

            plan.HoleGroups.Add(
                new HoleGroupDimensionPlanItem
                {
                    Group =
                        group,

                    TargetViewName =
                        targetViewName,

                    UseCountPrefix =
                        true,

                    UseSingleLeader =
                        true,

                    CreateDepthNote =
                        !group.IsThroughHole,

                    DecisionReason =
                        BuildHoleGroupDecisionReason(
                            group,
                            targetViewName)
                });
        }
    }

    private static void AddSingleHoles(
        FeatureGraph featureGraph,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings,
        DimensionPlan plan)
    {
        DimensionPlanViewResolver viewResolver =
            new();

        HashSet<Guid> groupedHoleIds =
            featureGraph.Nodes
                .OfType<HoleGroupFeatureNode>()
                .SelectMany(
                    group =>
                        group.HoleFeatureIds)
                .ToHashSet();

        foreach (HoleFeatureNode hole
                 in featureGraph.Nodes
                     .OfType<HoleFeatureNode>())
        {
            if (groupedHoleIds.Contains(
                    hole.Id))
            {
                continue;
            }

            string targetViewName =
                viewResolver.ResolveForHole(
                    hole,
                    viewMappings);

            plan.HoleDimensions.Add(
                new HoleDimensionPlanItem
                {
                    Hole =
                        hole,

                    TargetViewName =
                        targetViewName,

                    UseCenterMark =
                        true,

                    UseCenterLine =
                        false,

                    CreateDiameterDimension =
                        true,

                    CreateDepthNote =
                        !hole.IsThroughHole,

                    DecisionReason =
                        BuildHoleDecisionReason(
                            hole,
                            targetViewName)
                });
        }
    }

    private static string BuildHoleGroupDecisionReason(
        HoleGroupFeatureNode group,
        string targetViewName)
    {
        if (string.IsNullOrWhiteSpace(
                targetViewName))
        {
            return
                $"Группа \"{group.Name}\" обнаружена, " +
                $"но подходящий вид для оси {group.Axis} не найден.";
        }

        return
            $"Группа \"{group.Name}\" назначена на вид " +
            $"\"{targetViewName}\", где отверстия видны окружностями.";
    }

    private static string BuildHoleDecisionReason(
        HoleFeatureNode hole,
        string targetViewName)
    {
        if (string.IsNullOrWhiteSpace(
                targetViewName))
        {
            return
                $"Отверстие \"{hole.Name}\" обнаружено, " +
                $"но подходящий вид для оси {hole.Axis} не найден.";
        }

        return
            $"Отверстие \"{hole.Name}\" назначено на вид " +
            $"\"{targetViewName}\", где оно видно окружностью.";
    }
}