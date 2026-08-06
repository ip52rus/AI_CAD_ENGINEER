using AI_CAD_ENGINEER.Drawing.Layout;
using AI_CAD_ENGINEER.Engineering.Planning;
using Inventor;

namespace AI_CAD_ENGINEER.Drawing.Executors;

public class HoleGroupDimensionExecutor
{
    private readonly Inventor.Application
        _inventor;

    private readonly LeaderNotePlacementEngine
        _placementEngine;

    private readonly LeaderLayoutBuilder
        _layoutBuilder;

    public HoleGroupDimensionExecutor(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;

        _placementEngine =
            new LeaderNotePlacementEngine(
                inventor);

        _layoutBuilder =
            new LeaderLayoutBuilder(
                inventor);
    }

    public void Execute(
        DrawingDocument drawingDocument,
        DimensionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            drawingDocument);

        ArgumentNullException.ThrowIfNull(
            plan);

        Console.WriteLine();
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            "HOLE GROUP EXECUTOR");

        Console.WriteLine(
            "========================================");

        if (plan.HoleGroups.Count == 0)
        {
            Console.WriteLine(
                "Группы отверстий отсутствуют.");

            Console.WriteLine(
                "========================================");

            return;
        }

        Sheet sheet =
            drawingDocument.ActiveSheet;

        int createdCount =
            0;

        int failedCount =
            0;

        int index =
            1;

        foreach (HoleGroupDimensionPlanItem item
                 in plan.HoleGroups)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"Группа #{index}");

            Console.WriteLine(
                $"Имя: {item.Group.Name}");

            Console.WriteLine(
                $"Количество: {item.Group.HoleCount}");

            Console.WriteLine(
                $"Диаметр: " +
                $"{item.Group.DiameterMillimeters:F3} мм");

            Console.WriteLine(
                $"Глубина: " +
                $"{item.Group.DepthMillimeters:F3} мм");

            Console.WriteLine(
                $"Сквозные: " +
                $"{FormatBoolean(
                    item.Group.IsThroughHole)}");

            Console.WriteLine(
                $"Ось: {item.Group.Axis}");

            Console.WriteLine(
                $"Назначенный вид: " +
                $"{FormatView(
                    item.TargetViewName)}");

            string designation =
                BuildDesignation(
                    item);

            Console.WriteLine(
                $"Обозначение: {designation}");

            Console.WriteLine(
                $"Причина: {item.DecisionReason}");

            bool created =
                TryCreateLeaderNote(
                    sheet,
                    item,
                    designation,
                    out string executionMessage);

            Console.WriteLine();
            Console.WriteLine(
                "Статус:");

            Console.WriteLine(
                executionMessage);

            if (created)
            {
                createdCount++;
            }
            else
            {
                failedCount++;
            }

            index++;
        }

        drawingDocument.Update();

        Console.WriteLine();
        Console.WriteLine(
            $"Создано обозначений: {createdCount}");

        Console.WriteLine(
            $"Не создано: {failedCount}");

        Console.WriteLine(
            "========================================");
    }

    private bool TryCreateLeaderNote(
        Sheet sheet,
        HoleGroupDimensionPlanItem item,
        string designation,
        out string executionMessage)
    {
        if (string.IsNullOrWhiteSpace(
                item.TargetViewName))
        {
            executionMessage =
                "Не создано: целевой вид не назначен.";

            return false;
        }

        DrawingView? targetView =
            FindDrawingView(
                sheet,
                item.TargetViewName);

        if (targetView == null)
        {
            executionMessage =
                $"Не создано: вид " +
                $"\"{item.TargetViewName}\" не найден.";

            return false;
        }

        DrawingCurve? anchorCurve =
            FindFirstVisibleCircularCurve(
                targetView);

        if (anchorCurve == null)
        {
            executionMessage =
                "Не создано: на назначенном виде " +
                "не найдена видимая окружность или дуга.";

            return false;
        }

        LeaderNotePlacement placement =
            _placementEngine.Calculate(
                sheet,
                targetView);

        LeaderLayout layout =
            _layoutBuilder.Build(
                placement,
                targetView);

        PrintLayout(
            layout);

        if (!layout.IsValid)
        {
            executionMessage =
                "Не создано: построенный маршрут " +
                "выноски признан недопустимым.";

            return false;
        }

        try
        {
            GeometryIntent geometryIntent =
                sheet.CreateGeometryIntent(
                    anchorCurve);

            ObjectCollection leaderPoints =
                _inventor.TransientObjects
                    .CreateObjectCollection();

            leaderPoints.Add(
                layout.TextPoint);

            leaderPoints.Add(
                layout.BendPoint);

            leaderPoints.Add(
                geometryIntent);

            LeaderNote leaderNote =
                sheet.DrawingNotes
                    .LeaderNotes
                    .Add(
                        leaderPoints,
                        designation);

            leaderNote.Layer.Visible =
                true;

            executionMessage =
                $"Создана выноска \"{designation}\" " +
                $"на виде \"{targetView.Name}\". " +
                $"Сторона: {layout.Side}. " +
                "Маршрут содержит точку перегиба.";

            return true;
        }
        catch (Exception exception)
        {
            executionMessage =
                "Не удалось создать выноску: " +
                exception.Message;

            return false;
        }
    }

    private static void PrintLayout(
        LeaderLayout layout)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Результат Leader Layout:");

        Console.WriteLine(
            $"  Сторона: {layout.Side}");

        Console.WriteLine(
            $"  Оценка: {layout.Score:F3}");

        Console.WriteLine(
            $"  Точка текста: " +
            $"X={layout.TextPoint.X:F3}; " +
            $"Y={layout.TextPoint.Y:F3}");

        Console.WriteLine(
            $"  Точка перегиба: " +
            $"X={layout.BendPoint.X:F3}; " +
            $"Y={layout.BendPoint.Y:F3}");

        Console.WriteLine(
            $"  Длина полки: " +
            $"{layout.ShelfLength:F3}");

        Console.WriteLine(
            $"  Расстояние от вида: " +
            $"{layout.DistanceFromView:F3}");

        Console.WriteLine(
            $"  Допустимый маршрут: " +
            $"{FormatBoolean(
                layout.IsValid)}");
    }

    private static DrawingView? FindDrawingView(
        Sheet sheet,
        string targetViewName)
    {
        foreach (DrawingView drawingView
                 in sheet.DrawingViews)
        {
            if (string.Equals(
                    drawingView.Name,
                    targetViewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return drawingView;
            }
        }

        return null;
    }

    private static DrawingCurve?
        FindFirstVisibleCircularCurve(
            DrawingView drawingView)
    {
        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[
                null];

        if (curves == null)
        {
            return null;
        }

        DrawingCurve? firstVisibleArc =
            null;

        foreach (DrawingCurve curve
                 in curves)
        {
            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                if (segment.HiddenLine)
                {
                    continue;
                }

                if (segment.Geometry
                    is Circle2d)
                {
                    return curve;
                }

                if (segment.Geometry
                        is Arc2d &&
                    firstVisibleArc == null)
                {
                    firstVisibleArc =
                        curve;
                }
            }
        }

        return firstVisibleArc;
    }

    private static string BuildDesignation(
        HoleGroupDimensionPlanItem item)
    {
        string countPrefix =
            item.UseCountPrefix
                ? $"{item.Group.HoleCount}\u00D7"
                : string.Empty;

        string designation =
            $"{countPrefix}\u00D8" +
            $"{item.Group.DiameterMillimeters:0.###}";

        if (item.CreateDepthNote &&
            !item.Group.IsThroughHole)
        {
            designation +=
                $" \u21A7" +
                $"{item.Group.DepthMillimeters:0.###}";
        }

        return designation;
    }

    private static string FormatView(
        string value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? "не назначен"
            : value;
    }

    private static string FormatBoolean(
        bool value)
    {
        return value
            ? "да"
            : "нет";
    }
}