using System.Runtime.InteropServices;
using System.Text;
using AI_CAD_ENGINEER.Infrastructure.Reporting;
using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Research;

public class LeaderNoteResearch
{
    private readonly Inventor.Application
        _inventor;

    private readonly IReporter
        _reporter;

    public LeaderNoteResearch(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;

        _reporter =
            new FileReporter();
    }

    public bool Run(
        DrawingDocument drawingDocument,
        string targetViewName)
    {
        ArgumentNullException.ThrowIfNull(
            drawingDocument);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            targetViewName);

        StringBuilder report =
            new();

        AppendHeader(
            report);

        Sheet sheet =
            drawingDocument.ActiveSheet;

        report.AppendLine(
            $"Лист: {sheet.Name}");

        report.AppendLine(
            $"Целевой вид: {targetViewName}");

        report.AppendLine();

        DrawingView? drawingView =
            FindDrawingView(
                sheet,
                targetViewName);

        if (drawingView == null)
        {
            report.AppendLine(
                "Результат: целевой вид не найден.");

            SaveAndPrintReport(
                report);

            return false;
        }

        report.AppendLine(
            $"Вид найден: {drawingView.Name}");

        report.AppendLine(
            $"Размер вида: " +
            $"{drawingView.Width:F3} x " +
            $"{drawingView.Height:F3}");

        report.AppendLine();

        DrawingCurve? anchorCurve =
            FindFirstVisibleCircularCurve(
                drawingView,
                report);

        if (anchorCurve == null)
        {
            report.AppendLine(
                "Результат: видимая окружность " +
                "или дуга не найдена.");

            SaveAndPrintReport(
                report);

            return false;
        }

        GeometryIntent geometryIntent;

        try
        {
            geometryIntent =
                sheet.CreateGeometryIntent(
                    anchorCurve);

            report.AppendLine(
                "GeometryIntent: создан успешно.");
        }
        catch (COMException exception)
        {
            AppendComException(
                report,
                "CreateGeometryIntent",
                exception);

            SaveAndPrintReport(
                report);

            return false;
        }
        catch (Exception exception)
        {
            AppendException(
                report,
                "CreateGeometryIntent",
                exception);

            SaveAndPrintReport(
                report);

            return false;
        }

        try
        {
            ObjectCollection leaderPoints =
                _inventor.TransientObjects
                    .CreateObjectCollection();

            Point2d notePosition =
                CreateNotePosition(
                    sheet,
                    drawingView);

            leaderPoints.Add(
                notePosition);

            leaderPoints.Add(
                geometryIntent);

            report.AppendLine(
                $"Точка текста: " +
                $"X={notePosition.X:F3}; " +
                $"Y={notePosition.Y:F3}");

            report.AppendLine(
                $"Элементов в ObjectCollection: " +
                $"{leaderPoints.Count}");

            LeaderNote leaderNote =
                sheet.DrawingNotes
                    .LeaderNotes
                    .Add(
                        leaderPoints,
                        "LEADER NOTE TEST");

            leaderNote.Layer.Visible =
                true;

            drawingDocument.Update();

            report.AppendLine(
                "LeaderNote: создан успешно.");

            report.AppendLine(
                $"Текст: {leaderNote.Text}");

            report.AppendLine(
                "Результат: SUCCESS.");

            SaveAndPrintReport(
                report);

            return true;
        }
        catch (COMException exception)
        {
            AppendComException(
                report,
                "LeaderNotes.Add",
                exception);

            SaveAndPrintReport(
                report);

            return false;
        }
        catch (Exception exception)
        {
            AppendException(
                report,
                "LeaderNotes.Add",
                exception);

            SaveAndPrintReport(
                report);

            return false;
        }
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
            DrawingView drawingView,
            StringBuilder report)
    {
        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[
                null];

        if (curves == null)
        {
            report.AppendLine(
                "DrawingCurves вернул null.");

            return null;
        }

        report.AppendLine(
            $"DrawingCurve: {curves.Count}");

        int visibleCircleCount =
            0;

        int visibleArcCount =
            0;

        DrawingCurve? firstArcCurve =
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
                    visibleCircleCount++;

                    report.AppendLine(
                        "Найдена видимая окружность.");

                    report.AppendLine(
                        $"Видимых окружностей: " +
                        $"{visibleCircleCount}");

                    report.AppendLine(
                        $"Видимых дуг до окружности: " +
                        $"{visibleArcCount}");

                    return curve;
                }

                if (segment.Geometry
                    is Arc2d)
                {
                    visibleArcCount++;

                    firstArcCurve ??=
                        curve;
                }
            }
        }

        report.AppendLine(
            $"Видимых окружностей: " +
            $"{visibleCircleCount}");

        report.AppendLine(
            $"Видимых дуг: " +
            $"{visibleArcCount}");

        if (firstArcCurve != null)
        {
            report.AppendLine(
                "Полная окружность не найдена. " +
                "Для теста выбрана первая видимая дуга.");
        }

        return firstArcCurve;
    }

    private Point2d CreateNotePosition(
        Sheet sheet,
        DrawingView drawingView)
    {
        const double horizontalOffset =
            2.0;

        const double verticalOffset =
            1.0;

        const double margin =
            1.0;

        double rightX =
            drawingView.Center.X +
            drawingView.Width / 2.0 +
            horizontalOffset;

        double leftX =
            drawingView.Center.X -
            drawingView.Width / 2.0 -
            horizontalOffset;

        double x =
            rightX <=
            sheet.Width - margin
                ? rightX
                : leftX;

        double y =
            drawingView.Center.Y +
            drawingView.Height / 2.0 -
            verticalOffset;

        x =
            Math.Clamp(
                x,
                margin,
                sheet.Width - margin);

        y =
            Math.Clamp(
                y,
                margin,
                sheet.Height - margin);

        return
            _inventor.TransientGeometry
                .CreatePoint2d(
                    x,
                    y);
    }

    private void SaveAndPrintReport(
        StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine(
            "========================================");

        string reportText =
            report.ToString();

        string reportPath =
            _reporter.Write(
                ReportCategory.Research,
                nameof(LeaderNoteResearch),
                reportText);

        Console.WriteLine();
        Console.WriteLine(
            reportText);

        Console.WriteLine(
            "Полный отчёт LeaderNoteResearch сохранён:");

        Console.WriteLine(
            reportPath);
    }

    private static void AppendHeader(
        StringBuilder report)
    {
        report.AppendLine(
            "========================================");

        report.AppendLine(
            "LEADER NOTE RESEARCH");

        report.AppendLine(
            "========================================");

        report.AppendLine();
    }

    private static void AppendComException(
        StringBuilder report,
        string operation,
        COMException exception)
    {
        report.AppendLine();
        report.AppendLine(
            $"Операция: {operation}");

        report.AppendLine(
            "COMException:");

        report.AppendLine(
            exception.Message);

        report.AppendLine(
            $"HRESULT: 0x" +
            $"{exception.HResult:X8}");

        report.AppendLine(
            "Результат: FAILED.");
    }

    private static void AppendException(
        StringBuilder report,
        string operation,
        Exception exception)
    {
        report.AppendLine();
        report.AppendLine(
            $"Операция: {operation}");

        report.AppendLine(
            $"{exception.GetType().Name}:");

        report.AppendLine(
            exception.Message);

        report.AppendLine(
            "Результат: FAILED.");
    }
}
