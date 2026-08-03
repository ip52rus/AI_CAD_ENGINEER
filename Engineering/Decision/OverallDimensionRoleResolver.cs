using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class OverallDimensionRoleResolver
{
    public Dictionary<string, List<DimensionCandidate>> Resolve(
        PartAnalysis partAnalysis,
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView)
    {
        ArgumentNullException.ThrowIfNull(
            partAnalysis);

        ArgumentNullException.ThrowIfNull(
            candidatesByView);

        Dictionary<string, List<DimensionCandidate>> result =
            candidatesByView.ToDictionary(
                entry => entry.Key,
                _ => new List<DimensionCandidate>());

        SelectRoleCandidate(
            candidatesByView,
            result,
            OverallDimensionRole.Length,
            ModelAxis.Z,
            partAnalysis.Length);

        SelectRoleCandidate(
            candidatesByView,
            result,
            OverallDimensionRole.Width,
            ModelAxis.X,
            partAnalysis.Width);

        SelectRoleCandidate(
            candidatesByView,
            result,
            OverallDimensionRole.Height,
            ModelAxis.Y,
            partAnalysis.Height);

        return result;
    }

    private static void SelectRoleCandidate(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView,
        Dictionary<string, List<DimensionCandidate>>
            selectedByView,
        OverallDimensionRole role,
        ModelAxis physicalAxis,
        double expectedValue)
    {
        CandidateLocation? bestLocation =
            FindBestCandidate(
                candidatesByView,
                selectedByView,
                physicalAxis,
                expectedValue);

        if (bestLocation == null)
        {
            Console.WriteLine(
                $"Предупреждение: не найден кандидат " +
                $"для роли {role}, ось {physicalAxis}, " +
                $"значение {expectedValue:F3} мм.");

            return;
        }

        DimensionCandidate selectedCandidate =
            bestLocation.Candidate;

        selectedCandidate.OverallRole =
            role;

        selectedCandidate.Status =
            DimensionStatus.Required;

        selectedCandidate.IsRequired =
            true;

        selectedCandidate.DecisionReason =
            $"Размер выбран для роли {role} " +
            $"по физической оси {physicalAxis}.";

        selectedByView[bestLocation.ViewName].Add(
            selectedCandidate);
    }

    private static CandidateLocation? FindBestCandidate(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView,
        Dictionary<string, List<DimensionCandidate>>
            selectedByView,
        ModelAxis requiredAxis,
        double expectedValue)
    {
        const double valueToleranceMillimeters =
            0.01;

        CandidateLocation? bestLocation =
            null;

        int bestViewDimensionCount =
            int.MaxValue;

        int bestOrientationPriority =
            int.MaxValue;

        foreach (KeyValuePair<string, List<DimensionCandidate>>
                 entry in candidatesByView)
        {
            int selectedCountForView =
                selectedByView[entry.Key].Count;

            foreach (DimensionCandidate candidate
                     in entry.Value)
            {
                if (!candidate.IsOverallDimension)
                {
                    continue;
                }

                if (candidate.PhysicalAxis !=
                    requiredAxis)
                {
                    continue;
                }

                if (candidate.OverallRole !=
                    OverallDimensionRole.Undefined)
                {
                    continue;
                }

                bool valueMatches =
                    Math.Abs(
                        candidate.Value -
                        expectedValue) <=
                    valueToleranceMillimeters;

                if (!valueMatches)
                {
                    continue;
                }

                int orientationPriority =
                    GetOrientationPriority(
                        candidate.Type);

                bool betterDistribution =
                    selectedCountForView <
                    bestViewDimensionCount;

                bool sameDistributionButBetterOrientation =
                    selectedCountForView ==
                    bestViewDimensionCount &&
                    orientationPriority <
                    bestOrientationPriority;

                if (!betterDistribution &&
                    !sameDistributionButBetterOrientation)
                {
                    continue;
                }

                bestViewDimensionCount =
                    selectedCountForView;

                bestOrientationPriority =
                    orientationPriority;

                bestLocation =
                    new CandidateLocation(
                        entry.Key,
                        candidate);
            }
        }

        return bestLocation;
    }

    private static int GetOrientationPriority(
        DimensionCandidateType type)
    {
        return type switch
        {
            DimensionCandidateType.Vertical =>
                0,

            DimensionCandidateType.Horizontal =>
                1,

            _ =>
                2
        };
    }

    private sealed class CandidateLocation
    {
        public CandidateLocation(
            string viewName,
            DimensionCandidate candidate)
        {
            ViewName =
                viewName;

            Candidate =
                candidate;
        }

        public string ViewName { get; }

        public DimensionCandidate Candidate { get; }
    }
}