using AI_CAD_ENGINEER.Engineering.Geometry;
using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanViewResolver
{
    public string ResolveForHole(
        HoleFeatureNode hole,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        ArgumentNullException.ThrowIfNull(
            hole);

        ArgumentNullException.ThrowIfNull(
            viewMappings);

        return ResolveByFeatureAxis(
            hole.Axis,
            viewMappings);
    }

    public string ResolveForHoleGroup(
        HoleGroupFeatureNode group,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        ArgumentNullException.ThrowIfNull(
            group);

        ArgumentNullException.ThrowIfNull(
            viewMappings);

        return ResolveByFeatureAxis(
            group.Axis,
            viewMappings);
    }

    private static string ResolveByFeatureAxis(
        ModelAxis featureAxis,
        IReadOnlyDictionary<string, ViewAxisMapping>
            viewMappings)
    {
        if (featureAxis ==
            ModelAxis.Undefined)
        {
            return string.Empty;
        }

        foreach (KeyValuePair<string, ViewAxisMapping> entry
                 in viewMappings)
        {
            ModelAxis viewNormalAxis =
                ResolveViewNormalAxis(
                    entry.Value);

            if (viewNormalAxis ==
                featureAxis)
            {
                return entry.Key;
            }
        }

        return string.Empty;
    }

    private static ModelAxis ResolveViewNormalAxis(
        ViewAxisMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(
            mapping);

        bool containsX =
            mapping.HorizontalAxis ==
                ModelAxis.X ||
            mapping.VerticalAxis ==
                ModelAxis.X;

        bool containsY =
            mapping.HorizontalAxis ==
                ModelAxis.Y ||
            mapping.VerticalAxis ==
                ModelAxis.Y;

        bool containsZ =
            mapping.HorizontalAxis ==
                ModelAxis.Z ||
            mapping.VerticalAxis ==
                ModelAxis.Z;

        if (!containsX)
        {
            return ModelAxis.X;
        }

        if (!containsY)
        {
            return ModelAxis.Y;
        }

        if (!containsZ)
        {
            return ModelAxis.Z;
        }

        return ModelAxis.Undefined;
    }
}