using AI_CAD_ENGINEER.Engineering.Models;
using System.Text;

namespace AI_CAD_ENGINEER.Engineering.Analysis;

public class ViewDecisionReport
{
    public string Build(
        string selectedViewName,
        ViewStatistics mainView,
        ViewStatistics upperView,
        ViewStatistics sideView,
        double mainScore,
        double upperScore,
        double sideScore,
        ViewNecessityResult necessity)
    {
        StringBuilder report = new();

        report.AppendLine();
        report.AppendLine("========================================");
        report.AppendLine("АНАЛИЗ ДЕТАЛИ");
        report.AppendLine("========================================");
        report.AppendLine();

        report.AppendLine($"Главный вид: {selectedViewName}");
        report.AppendLine();

        report.AppendLine("Главный вид");
        AppendView(report, mainView, mainScore);

        report.AppendLine("Вертикальная проекция");
        AppendView(report, upperView, upperScore);

        report.AppendLine("Боковая проекция");
        AppendView(report, sideView, sideScore);

        report.AppendLine("----------------------------------------");
        report.AppendLine(necessity.Message);

        return report.ToString();
    }

    private static void AppendView(
        StringBuilder report,
        ViewStatistics statistics,
        double score)
    {
        report.AppendLine($"  Оценка: {score:F2}");
        report.AppendLine($"  Размер: {statistics.Width:F2} x {statistics.Height:F2}");
        report.AppendLine($"  Линии: {statistics.LineCount}");
        report.AppendLine($"  Окружности: {statistics.CircleCount}");
        report.AppendLine($"  Дуги: {statistics.ArcCount}");
        report.AppendLine($"  Эллиптические дуги: {statistics.EllipticalArcCount}");
        report.AppendLine();
    }
}