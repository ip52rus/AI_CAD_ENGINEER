using AI_CAD_ENGINEER.Engineering.Geometry;
using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanner
{
    private readonly DimensionPlanValidator
        _validator;

    private readonly DimensionPlanViewResolver
        _viewResolver;

    public DimensionPlanner()
    {
        _validator =
            new DimensionPlanValidator();

        _viewResolver =
            new DimensionPlanViewResolver();
    }

    public DimensionPlan BuildPlan(
        string documentName,
        string documentPath,
        FeatureGraph featureGraph,
        IReadOnlyCollection<DimensionCandidate>
            overallDimensions,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            documentPath);

        ArgumentNullException.ThrowIfNull(
            featureGraph);

        ArgumentNullException.ThrowIfNull(
            overallDimensions);

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
            overallDimensions,
            plan);

        AddHoleDimensions(
            featureGraph,
            viewMappings,
            plan);

        AddHoleGroups(
            featureGraph,
            viewMappings,
            plan);

        _validator.Validate(
            plan);

        if (plan.IsValid)
        {
            plan.MarkReadyForDrawing();
        }

        return plan;
    }

    private static void AddOverallDimensions(
        IEnumerable<DimensionCandidate> candidates,
        DimensionPlan plan)
    {
        foreach (DimensionCandidate candidate
                 in candidates)
        {
            if (candidate.Status !=
                DimensionStatus.Required)
            {
                continue;
            }

            plan.OverallDimensions.Add(
                new OverallDimensionPlanItem
                {
                    Candidate =
                        candidate,

                    TargetViewName =
                        candidate.SourceViewName,

                    DecisionReason =
                        string.IsNullOrWhiteSpace(
                            candidate.DecisionReason)
                            ? "Обязательный габаритный размер."
                            : candidate.DecisionReason
                });
        }
    }

    private void AddHoleDimensions(
        FeatureGraph graph,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings,
        DimensionPlan plan)
    {
        HashSet<Guid> groupedHoleIds =
            graph.Nodes
                .OfType<HoleGroupFeatureNode>()
                .SelectMany(
                    group =>
                        group.HoleFeatureIds)
                .ToHashSet();

        foreach (HoleFeatureNode hole
                 in graph.Nodes
                     .OfType<HoleFeatureNode>())
        {
            if (groupedHoleIds.Contains(
                    hole.Id))
            {
                continue;
            }

            string targetViewName =
                _viewResolver.ResolveForHole(
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

    private void AddHoleGroups(
        FeatureGraph graph,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings,
        DimensionPlan plan)
    {
        foreach (HoleGroupFeatureNode group
                 in graph.Nodes
                     .OfType<HoleGroupFeatureNode>())
        {
            string targetViewName =
                _viewResolver.ResolveForHoleGroup(
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

    private static string BuildHoleDecisionReason(
        HoleFeatureNode hole,
        string targetViewName)
    {
        if (string.IsNullOrWhiteSpace(
                targetViewName))
        {
            return
                $"Одиночное отверстие \"{hole.Name}\" " +
                $"обнаружено в Engineering Feature Graph, " +
                $"но подходящий вид по оси {hole.Axis} " +
                "не найден.";
        }

        return
            $"Одиночное отверстие \"{hole.Name}\" " +
            $"назначено на вид \"{targetViewName}\", " +
            $"где ось отверстия {hole.Axis} " +
            "перпендикулярна плоскости проекции.";
    }

    private static string BuildHoleGroupDecisionReason(
        HoleGroupFeatureNode group,
        string targetViewName)
    {
        if (string.IsNullOrWhiteSpace(
                targetViewName))
        {
            return
                $"Группа \"{group.Name}\" обнаружена " +
                $"в Engineering Feature Graph, " +
                $"но подходящий вид по оси {group.Axis} " +
                "не найден.";
        }

        return
            $"Группа \"{group.Name}\" назначена " +
            $"на вид \"{targetViewName}\", " +
            $"где ось отверстий {group.Axis} " +
            "перпендикулярна плоскости проекции.";
    }
}