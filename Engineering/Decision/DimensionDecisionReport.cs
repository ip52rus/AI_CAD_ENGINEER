using System.Text;
using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class DimensionDecisionReport
{
    public string Build(
        IReadOnlyDictionary<string, List<DimensionCandidate>>
            candidatesByView)
    {
        ArgumentNullException.ThrowIfNull(
            candidatesByView);

        StringBuilder report =
            new();

        report.AppendLine();
        report.AppendLine(
            "========================================");

        report.AppendLine(
            "КЛАССИФИКАЦИЯ ГАБАРИТНЫХ РАЗМЕРОВ");

        report.AppendLine(
            "========================================");

        int totalCount =
            0;

        int requiredCount =
            0;

        int duplicateCount =
            0;

        int redundantCount =
            0;

        int referenceCount =
            0;

        int optionalCount =
            0;

        int recommendedCount =
            0;

        int groupedCount =
            0;

        foreach (KeyValuePair<
                     string,
                     List<DimensionCandidate>> entry
                 in candidatesByView)
        {
            report.AppendLine();
            report.AppendLine(
                $"Вид: {entry.Key}");

            if (entry.Value.Count == 0)
            {
                report.AppendLine(
                    "  Кандидаты отсутствуют.");

                continue;
            }

            foreach (DimensionCandidate candidate
                     in entry.Value)
            {
                totalCount++;

                CountStatus(
                    candidate.Status,
                    ref requiredCount,
                    ref duplicateCount,
                    ref redundantCount,
                    ref referenceCount,
                    ref optionalCount,
                    ref recommendedCount,
                    ref groupedCount);

                report.AppendLine(
                    $"  Размер: {candidate.Value:F3} мм");

                report.AppendLine(
                    $"    Имя: {candidate.Name}");

                report.AppendLine(
                    $"    Тип: {candidate.Type}");

                report.AppendLine(
                    $"    Физическая ось: " +
                    $"{candidate.PhysicalAxis}");

                report.AppendLine(
                    $"    Роль: {candidate.OverallRole}");

                report.AppendLine(
                    $"    Статус: {candidate.Status}");

                report.AppendLine(
                    $"    Обязательный: " +
                    $"{FormatBoolean(
                        candidate.IsRequired)}");

                report.AppendLine(
                    $"    Причина: " +
                    $"{candidate.DecisionReason}");
            }
        }

        report.AppendLine();
        report.AppendLine(
            "----------------------------------------");

        report.AppendLine(
            $"Всего кандидатов: {totalCount}");

        report.AppendLine(
            $"Обязательных: {requiredCount}");

        report.AppendLine(
            $"Рекомендуемых: {recommendedCount}");

        report.AppendLine(
            $"Дублирующих: {duplicateCount}");

        report.AppendLine(
            $"Избыточных: {redundantCount}");

        report.AppendLine(
            $"Справочных: {referenceCount}");

        report.AppendLine(
            $"Групповых: {groupedCount}");

        report.AppendLine(
            $"Необязательных: {optionalCount}");

        report.AppendLine(
            "========================================");

        return report.ToString();
    }

    private static void CountStatus(
        DimensionStatus status,
        ref int requiredCount,
        ref int duplicateCount,
        ref int redundantCount,
        ref int referenceCount,
        ref int optionalCount,
        ref int recommendedCount,
        ref int groupedCount)
    {
        switch (status)
        {
            case DimensionStatus.Required:

                requiredCount++;

                break;

            case DimensionStatus.Recommended:

                recommendedCount++;

                break;

            case DimensionStatus.Reference:

                referenceCount++;

                break;

            case DimensionStatus.Duplicate:

                duplicateCount++;

                break;

            case DimensionStatus.Redundant:

                redundantCount++;

                break;

            case DimensionStatus.Grouped:

                groupedCount++;

                break;

            case DimensionStatus.Optional:
            case DimensionStatus.Undefined:

                optionalCount++;

                break;

            default:

                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Неизвестный статус размера.");
        }
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}