using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class DimensionClassificationEngine
{
    private const double ValueToleranceMillimeters =
        0.01;

    public void Classify(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView,
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            selectedByView)
    {
        ArgumentNullException.ThrowIfNull(
            candidatesByView);

        ArgumentNullException.ThrowIfNull(
            selectedByView);

        ResetCandidates(
            candidatesByView);

        MarkSelectedDimensions(
            selectedByView);

        MarkDuplicateDimensions(
            candidatesByView,
            selectedByView);
    }

    private static void ResetCandidates(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView)
    {
        foreach (List<DimensionCandidate> candidates
                 in candidatesByView.Values)
        {
            foreach (DimensionCandidate candidate
                     in candidates)
            {
                candidate.Status =
                    DimensionStatus.Optional;

                candidate.IsRequired =
                    false;

                candidate.DecisionReason =
                    "Кандидат пока не выбран для нанесения.";
            }
        }
    }

    private static void MarkSelectedDimensions(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            selectedByView)
    {
        foreach (List<DimensionCandidate> candidates
                 in selectedByView.Values)
        {
            foreach (DimensionCandidate candidate
                     in candidates)
            {
                candidate.Status =
                    DimensionStatus.Required;

                candidate.IsRequired =
                    true;

                candidate.DecisionReason =
                    $"Обязательный габаритный размер " +
                    $"роли {candidate.OverallRole}, " +
                    $"определяющий физическую ось " +
                    $"{candidate.PhysicalAxis}.";
            }
        }
    }

    private static void MarkDuplicateDimensions(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView,
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            selectedByView)
    {
        List<DimensionCandidate> selectedDimensions =
            selectedByView
                .Values
                .SelectMany(
                    candidates => candidates)
                .Where(
                    candidate =>
                        candidate.Status ==
                        DimensionStatus.Required)
                .ToList();

        foreach (List<DimensionCandidate> candidates
                 in candidatesByView.Values)
        {
            foreach (DimensionCandidate candidate
                     in candidates)
            {
                if (candidate.Status ==
                    DimensionStatus.Required)
                {
                    continue;
                }

                if (!candidate.IsOverallDimension)
                {
                    continue;
                }

                if (candidate.PhysicalAxis ==
                    ModelAxis.Undefined)
                {
                    candidate.Status =
                        DimensionStatus.Optional;

                    candidate.DecisionReason =
                        "Физическая ось размера не определена.";

                    continue;
                }

                DimensionCandidate? selectedForSameAxis =
                    selectedDimensions.FirstOrDefault(
                        selected =>
                            selected.PhysicalAxis ==
                                candidate.PhysicalAxis &&
                            Math.Abs(
                                selected.Value -
                                candidate.Value) <=
                            ValueToleranceMillimeters);

                if (selectedForSameAxis == null)
                {
                    candidate.Status =
                        DimensionStatus.Optional;

                    candidate.DecisionReason =
                        "Размер не совпадает с выбранным " +
                        "габаритом той же физической оси.";

                    continue;
                }

                candidate.OverallRole =
                    selectedForSameAxis.OverallRole;

                candidate.Status =
                    DimensionStatus.Duplicate;

                candidate.IsRequired =
                    false;

                candidate.DecisionReason =
                    $"Дублирует размер роли " +
                    $"{selectedForSameAxis.OverallRole} " +
                    $"по физической оси " +
                    $"{candidate.PhysicalAxis}, " +
                    $"уже выбранный на виде " +
                    $"{selectedForSameAxis.SourceViewName}.";
            }
        }
    }
}