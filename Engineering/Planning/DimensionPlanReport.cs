using System.Text;
using AI_CAD_ENGINEER.Engineering.Geometry;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanReport
{
    public string Build(
        DimensionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        StringBuilder report =
            new();

        report.AppendLine(
            "========================================");

        report.AppendLine(
            "DIMENSION PLANNING ENGINE");

        report.AppendLine(
            "========================================");

        report.AppendLine();

        report.AppendLine(
            $"Plan Id: {plan.Id}");

        report.AppendLine(
            $"Документ: {plan.SourceDocumentName}");

        report.AppendLine(
            $"Путь: {plan.SourceDocumentPath}");

        report.AppendLine(
            $"Создан: " +
            $"{plan.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        report.AppendLine(
            $"Статус: {plan.Status}");

        report.AppendLine(
            $"Всего элементов плана: " +
            $"{plan.TotalItems}");

        report.AppendLine();

        AppendSummary(
            report,
            plan);

        AppendOverallDimensions(
            report,
            plan);

        AppendHoleDimensions(
            report,
            plan);

        AppendHoleGroups(
            report,
            plan);

        AppendReferenceDimensions(
            report,
            plan);

        AppendSections(
            report,
            plan);

        AppendNotes(
            report,
            plan);

        AppendValidationErrors(
            report,
            plan);

        report.AppendLine(
            "========================================");

        return report.ToString();
    }

    private static void AppendSummary(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "СВОДКА");

        report.AppendLine(
            "----------------------------------------");

        report.AppendLine(
            $"Габаритных размеров: " +
            $"{plan.OverallDimensions.Count}");

        report.AppendLine(
            $"Одиночных отверстий: " +
            $"{plan.HoleDimensions.Count}");

        report.AppendLine(
            $"Групп отверстий: " +
            $"{plan.HoleGroups.Count}");

        report.AppendLine(
            $"Справочных размеров: " +
            $"{plan.ReferenceDimensions.Count}");

        report.AppendLine(
            $"Размеров в разрезах: " +
            $"{plan.Sections.Count}");

        report.AppendLine(
            $"Ошибок валидации: " +
            $"{plan.ValidationErrors.Count}");

        report.AppendLine();
    }

    private static void AppendOverallDimensions(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "ГАБАРИТНЫЕ РАЗМЕРЫ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.OverallDimensions.Count == 0)
        {
            report.AppendLine(
                "Габаритные размеры отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (OverallDimensionPlanItem item
                 in plan.OverallDimensions)
        {
            index++;

            report.AppendLine(
                $"{index}. {item.Candidate.OverallRole}");

            report.AppendLine(
                $"   Id: {item.Id}");

            report.AppendLine(
                $"   Значение: " +
                $"{item.Candidate.Value:F3} мм");

            report.AppendLine(
                $"   Физическая ось: " +
                $"{item.Candidate.PhysicalAxis}");

            report.AppendLine(
                $"   Тип: " +
                $"{item.Candidate.Type}");

            report.AppendLine(
                $"   Целевой вид: " +
                $"{FormatValue(
                    item.TargetViewName)}");

            report.AppendLine(
                $"   Причина: " +
                $"{FormatValue(
                    item.DecisionReason)}");

            report.AppendLine();
        }
    }

    private static void AppendHoleDimensions(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "ОДИНОЧНЫЕ ОТВЕРСТИЯ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.HoleDimensions.Count == 0)
        {
            report.AppendLine(
                "Одиночные отверстия отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (HoleDimensionPlanItem item
                 in plan.HoleDimensions)
        {
            index++;

            HoleFeatureNode hole =
                item.Hole;

            report.AppendLine(
                $"{index}. {hole.Name}");

            report.AppendLine(
                $"   Id элемента плана: {item.Id}");

            report.AppendLine(
                $"   Feature Id: {hole.Id}");

            report.AppendLine(
                $"   Диаметр: " +
                $"{hole.DiameterMillimeters:F3} мм");

            report.AppendLine(
                $"   Глубина: " +
                $"{hole.DepthMillimeters:F3} мм");

            report.AppendLine(
                $"   Тип завершения: " +
                $"{hole.TerminationType}");

            report.AppendLine(
                $"   Сквозное: " +
                $"{FormatBoolean(
                    hole.IsThroughHole)}");

            report.AppendLine(
                $"   Ось: {hole.Axis}");

            report.AppendLine(
                $"   Центр: " +
                $"X={hole.CenterXMillimeters:F3}; " +
                $"Y={hole.CenterYMillimeters:F3}; " +
                $"Z={hole.CenterZMillimeters:F3} мм");

            report.AppendLine(
                $"   Целевой вид: " +
                $"{FormatValue(
                    item.TargetViewName)}");

            report.AppendLine(
                $"   Центровая метка: " +
                $"{FormatBoolean(
                    item.UseCenterMark)}");

            report.AppendLine(
                $"   Центровая линия: " +
                $"{FormatBoolean(
                    item.UseCenterLine)}");

            report.AppendLine(
                $"   Размер диаметра: " +
                $"{FormatBoolean(
                    item.CreateDiameterDimension)}");

            report.AppendLine(
                $"   Обозначение глубины: " +
                $"{FormatBoolean(
                    item.CreateDepthNote)}");

            report.AppendLine(
                $"   Причина: " +
                $"{FormatValue(
                    item.DecisionReason)}");

            report.AppendLine();
        }
    }

    private static void AppendHoleGroups(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "ГРУППЫ ОТВЕРСТИЙ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.HoleGroups.Count == 0)
        {
            report.AppendLine(
                "Группы отверстий отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (HoleGroupDimensionPlanItem item
                 in plan.HoleGroups)
        {
            index++;

            HoleGroupFeatureNode group =
                item.Group;

            report.AppendLine(
                $"{index}. {group.Name}");

            report.AppendLine(
                $"   Id элемента плана: {item.Id}");

            report.AppendLine(
                $"   Feature Id: {group.Id}");

            report.AppendLine(
                $"   Количество отверстий: " +
                $"{group.HoleCount}");

            report.AppendLine(
                $"   Диаметр: " +
                $"{group.DiameterMillimeters:F3} мм");

            report.AppendLine(
                $"   Глубина: " +
                $"{group.DepthMillimeters:F3} мм");

            report.AppendLine(
                $"   Тип завершения: " +
                $"{group.TerminationType}");

            report.AppendLine(
                $"   Сквозные: " +
                $"{FormatBoolean(
                    group.IsThroughHole)}");

            report.AppendLine(
                $"   Ось: {group.Axis}");

            report.AppendLine(
                $"   Исходная операция: " +
                $"{FormatValue(
                    group.SourceFeatureName)}");

            report.AppendLine(
                $"   Участников группы: " +
                $"{group.HoleFeatureIds.Count}");

            report.AppendLine(
                $"   Целевой вид: " +
                $"{FormatValue(
                    item.TargetViewName)}");

            report.AppendLine(
                $"   Префикс количества: " +
                $"{FormatBoolean(
                    item.UseCountPrefix)}");

            report.AppendLine(
                $"   Одна выноска: " +
                $"{FormatBoolean(
                    item.UseSingleLeader)}");

            report.AppendLine(
                $"   Обозначение глубины: " +
                $"{FormatBoolean(
                    item.CreateDepthNote)}");

            report.AppendLine(
                $"   Планируемое обозначение: " +
                $"{BuildHoleGroupDesignation(
                    item)}");

            report.AppendLine(
                $"   Причина: " +
                $"{FormatValue(
                    item.DecisionReason)}");

            report.AppendLine();
        }
    }

    private static void AppendReferenceDimensions(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "СПРАВОЧНЫЕ РАЗМЕРЫ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.ReferenceDimensions.Count == 0)
        {
            report.AppendLine(
                "Справочные размеры отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (ReferenceDimensionPlanItem item
                 in plan.ReferenceDimensions)
        {
            index++;

            report.AppendLine(
                $"{index}. {item.Description}");

            report.AppendLine(
                $"   Id: {item.Id}");

            report.AppendLine(
                $"   Целевой вид: " +
                $"{FormatValue(
                    item.TargetViewName)}");

            report.AppendLine(
                $"   Причина: " +
                $"{FormatValue(
                    item.DecisionReason)}");

            report.AppendLine();
        }
    }

    private static void AppendSections(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "РАЗМЕРЫ В РАЗРЕЗАХ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.Sections.Count == 0)
        {
            report.AppendLine(
                "Размеры в разрезах отсутствуют.");

            report.AppendLine();

            return;
        }

        int index =
            0;

        foreach (SectionDimensionPlanItem item
                 in plan.Sections)
        {
            index++;

            report.AppendLine(
                $"{index}. {item.SectionName}");

            report.AppendLine(
                $"   Id: {item.Id}");

            report.AppendLine(
                $"   Целевой вид: " +
                $"{FormatValue(
                    item.TargetViewName)}");

            report.AppendLine(
                $"   Причина: " +
                $"{FormatValue(
                    item.DecisionReason)}");

            report.AppendLine();
        }
    }

    private static void AppendNotes(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "ПРИМЕЧАНИЯ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.Notes.Count == 0)
        {
            report.AppendLine(
                "Примечания отсутствуют.");

            report.AppendLine();

            return;
        }

        for (int index = 0;
             index < plan.Notes.Count;
             index++)
        {
            report.AppendLine(
                $"{index + 1}. {plan.Notes[index]}");
        }

        report.AppendLine();
    }

    private static void AppendValidationErrors(
        StringBuilder report,
        DimensionPlan plan)
    {
        report.AppendLine(
            "ОШИБКИ ВАЛИДАЦИИ");

        report.AppendLine(
            "----------------------------------------");

        if (plan.ValidationErrors.Count == 0)
        {
            report.AppendLine(
                "Ошибки отсутствуют.");

            report.AppendLine();

            return;
        }

        for (int index = 0;
             index < plan.ValidationErrors.Count;
             index++)
        {
            report.AppendLine(
                $"{index + 1}. " +
                $"{plan.ValidationErrors[index]}");
        }

        report.AppendLine();
    }

    private static string BuildHoleGroupDesignation(
        HoleGroupDimensionPlanItem item)
    {
        string prefix =
            item.UseCountPrefix
                ? $"{item.Group.HoleCount}×"
                : string.Empty;

        string designation =
            $"{prefix}Ø" +
            $"{item.Group.DiameterMillimeters:F3}";

        if (item.CreateDepthNote &&
            !item.Group.IsThroughHole)
        {
            designation +=
                $" ↧{item.Group.DepthMillimeters:F3}";
        }

        return designation;
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }

    private static string FormatValue(
        string value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? "не задано"
            : value;
    }
}