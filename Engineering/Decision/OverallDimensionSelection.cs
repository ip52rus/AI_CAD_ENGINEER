using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class OverallDimensionSelection
{
    public Dictionary<string, List<DimensionCandidate>> Select(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView)
    {
        ArgumentNullException.ThrowIfNull(
            candidatesByView);

        Dictionary<string, List<DimensionCandidate>> result =
            new();

        HashSet<double> selectedValues =
            new();

        foreach (KeyValuePair<string, List<DimensionCandidate>> entry
                 in candidatesByView)
        {
            List<DimensionCandidate> selectedForView =
                new();

            foreach (DimensionCandidate candidate
                     in entry.Value)
            {
                double normalizedValue =
                    Math.Round(
                        candidate.Value,
                        3);

                if (selectedValues.Add(
                        normalizedValue))
                {
                    selectedForView.Add(
                        candidate);
                }
            }

            result[entry.Key] =
                selectedForView;
        }

        return result;
    }
}