using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class DimensionRuleEngine
{
    public DimensionPlan CreatePlan(
        IEnumerable<DimensionCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        DimensionPlan plan = new();

        foreach (DimensionCandidate candidate in candidates)
        {
            plan.Candidates.Add(candidate);

            if (candidate.IsRequired)
            {
                plan.SelectedDimensions.Add(candidate);
            }
        }

        return plan;
    }
}