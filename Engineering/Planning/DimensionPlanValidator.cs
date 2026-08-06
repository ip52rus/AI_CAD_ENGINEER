using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Planning;

public class DimensionPlanValidator
{
    public void Validate(
        DimensionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        plan.ValidationErrors.Clear();

        ValidateDocument(
            plan);

        ValidateOverallDimensions(
            plan);

        ValidateHoleDimensions(
            plan);

        ValidateHoleGroups(
            plan);

        ValidateReferenceDimensions(
            plan);

        ValidateSections(
            plan);

        ValidateDuplicateItems(
            plan);

        if (plan.ValidationErrors.Count == 0)
        {
            plan.MarkValidated();
        }
        else
        {
            plan.Status =
                DimensionPlanStatus.Failed;
        }
    }

    private static void ValidateDocument(
        DimensionPlan plan)
    {
        if (string.IsNullOrWhiteSpace(
                plan.SourceDocumentName))
        {
            plan.AddValidationError(
                "Не указано имя исходного документа.");
        }

        if (string.IsNullOrWhiteSpace(
                plan.SourceDocumentPath))
        {
            plan.AddValidationError(
                "Не указан путь к исходному документу.");
        }
    }

    private static void ValidateOverallDimensions(
        DimensionPlan plan)
    {
        foreach (OverallDimensionPlanItem item
                 in plan.OverallDimensions)
        {
            if (item.Candidate == null)
            {
                plan.AddValidationError(
                    $"OverallDimensionPlanItem {item.Id} " +
                    "не содержит DimensionCandidate.");

                continue;
            }

            if (item.Candidate.Value <= 0)
            {
                plan.AddValidationError(
                    $"Габаритный размер {item.Id} " +
                    "должен иметь значение больше нуля.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.TargetViewName))
            {
                plan.AddValidationError(
                    $"Для габаритного размера {item.Id} " +
                    "не указан целевой вид.");
            }
        }
    }

    private static void ValidateHoleDimensions(
        DimensionPlan plan)
    {
        foreach (HoleDimensionPlanItem item
                 in plan.HoleDimensions)
        {
            if (item.Hole == null)
            {
                plan.AddValidationError(
                    $"HoleDimensionPlanItem {item.Id} " +
                    "не содержит HoleFeatureNode.");

                continue;
            }

            if (item.Hole.DiameterMillimeters <= 0)
            {
                plan.AddValidationError(
                    $"Отверстие {item.Hole.Name} " +
                    "имеет некорректный диаметр.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.TargetViewName))
            {
                plan.AddValidationError(
                    $"Для отверстия {item.Hole.Name} " +
                    "не указан целевой вид.");
            }

            bool hasAnyAnnotation =
                item.UseCenterMark ||
                item.UseCenterLine ||
                item.CreateDiameterDimension ||
                item.CreateDepthNote;

            if (!hasAnyAnnotation)
            {
                plan.AddValidationError(
                    $"Для отверстия {item.Hole.Name} " +
                    "не запланировано ни одного обозначения.");
            }
        }
    }

    private static void ValidateHoleGroups(
        DimensionPlan plan)
    {
        foreach (HoleGroupDimensionPlanItem item
                 in plan.HoleGroups)
        {
            if (item.Group == null)
            {
                plan.AddValidationError(
                    $"HoleGroupDimensionPlanItem {item.Id} " +
                    "не содержит HoleGroupFeatureNode.");

                continue;
            }

            if (item.Group.HoleCount < 2)
            {
                plan.AddValidationError(
                    $"Группа {item.Group.Name} должна " +
                    "содержать не менее двух отверстий.");
            }

            if (item.Group.DiameterMillimeters <= 0)
            {
                plan.AddValidationError(
                    $"Группа {item.Group.Name} " +
                    "имеет некорректный диаметр.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.TargetViewName))
            {
                plan.AddValidationError(
                    $"Для группы {item.Group.Name} " +
                    "не указан целевой вид.");
            }
        }
    }

    private static void ValidateReferenceDimensions(
        DimensionPlan plan)
    {
        foreach (ReferenceDimensionPlanItem item
                 in plan.ReferenceDimensions)
        {
            if (string.IsNullOrWhiteSpace(
                    item.Description))
            {
                plan.AddValidationError(
                    $"Справочный размер {item.Id} " +
                    "не содержит описание.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.TargetViewName))
            {
                plan.AddValidationError(
                    $"Для справочного размера {item.Id} " +
                    "не указан целевой вид.");
            }
        }
    }

    private static void ValidateSections(
        DimensionPlan plan)
    {
        foreach (SectionDimensionPlanItem item
                 in plan.Sections)
        {
            if (string.IsNullOrWhiteSpace(
                    item.SectionName))
            {
                plan.AddValidationError(
                    $"SectionDimensionPlanItem {item.Id} " +
                    "не содержит имя разреза.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.TargetViewName))
            {
                plan.AddValidationError(
                    $"Для разреза {item.Id} " +
                    "не указан целевой вид.");
            }
        }
    }

    private static void ValidateDuplicateItems(
        DimensionPlan plan)
    {
        ValidateDuplicateOverallDimensions(
            plan);

        ValidateDuplicateHoleDimensions(
            plan);

        ValidateDuplicateHoleGroups(
            plan);
    }

    private static void ValidateDuplicateOverallDimensions(
        DimensionPlan plan)
    {
        IEnumerable<
            IGrouping<
                DimensionCandidate,
                OverallDimensionPlanItem>>
            duplicateGroups =
                plan.OverallDimensions
                    .Where(
                        item =>
                            item.Candidate != null)
                    .GroupBy(
                        item =>
                            item.Candidate)
                    .Where(
                        group =>
                            group.Count() > 1);

        foreach (
            IGrouping<
                DimensionCandidate,
                OverallDimensionPlanItem> duplicateGroup
            in duplicateGroups)
        {
            plan.AddValidationError(
                "Один DimensionCandidate добавлен " +
                "в план несколько раз.");
        }
    }

    private static void ValidateDuplicateHoleDimensions(
        DimensionPlan plan)
    {
        IEnumerable<
            IGrouping<
                Guid,
                HoleDimensionPlanItem>>
            duplicateGroups =
                plan.HoleDimensions
                    .Where(
                        item =>
                            item.Hole != null)
                    .GroupBy(
                        item =>
                            item.Hole.Id)
                    .Where(
                        group =>
                            group.Count() > 1);

        foreach (
            IGrouping<
                Guid,
                HoleDimensionPlanItem> duplicateGroup
            in duplicateGroups)
        {
            plan.AddValidationError(
                $"Отверстие {duplicateGroup.Key} " +
                "добавлено в план несколько раз.");
        }
    }

    private static void ValidateDuplicateHoleGroups(
        DimensionPlan plan)
    {
        IEnumerable<
            IGrouping<
                Guid,
                HoleGroupDimensionPlanItem>>
            duplicateGroups =
                plan.HoleGroups
                    .Where(
                        item =>
                            item.Group != null)
                    .GroupBy(
                        item =>
                            item.Group.Id)
                    .Where(
                        group =>
                            group.Count() > 1);

        foreach (
            IGrouping<
                Guid,
                HoleGroupDimensionPlanItem> duplicateGroup
            in duplicateGroups)
        {
            plan.AddValidationError(
                $"Группа отверстий {duplicateGroup.Key} " +
                "добавлена в план несколько раз.");
        }
    }
}