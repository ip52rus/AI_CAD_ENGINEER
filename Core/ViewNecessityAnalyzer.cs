namespace AI_CAD_ENGINEER.Core;

public class ViewNecessityAnalyzer
{
    public ViewNecessityResult Analyze(
        ViewStatistics mainView,
        ViewStatistics upperView,
        ViewStatistics sideView,
        double mainScore,
        double upperScore,
        double sideScore)
    {
        ViewNecessityResult result = new()
        {
            KeepMainView = true,
            KeepUpperView = true,
            KeepSideView = true
        };

        if (mainScore <= 0)
        {
            result.Message =
                "Недостаточно данных для анализа набора видов.";

            return result;
        }

        double upperRatio =
            upperScore / mainScore;

        double sideRatio =
            sideScore / mainScore;

        result.UpperScoreRatio = upperRatio;
        result.SideScoreRatio = sideRatio;

        // Пока используем консервативный режим:
        // программа не удаляет виды автоматически,
        // а только определяет кандидатов на исключение.

        result.UpperViewIsRemovalCandidate =
            IsRemovalCandidate(
                upperView,
                upperRatio);

        result.SideViewIsRemovalCandidate =
            IsRemovalCandidate(
                sideView,
                sideRatio);

        if (result.UpperViewIsRemovalCandidate &&
            result.SideViewIsRemovalCandidate)
        {
            result.Message =
                "Оба дополнительных вида простые, " +
                "но пока будут сохранены для полноты чертежа.";
        }
        else if (result.UpperViewIsRemovalCandidate)
        {
            result.Message =
                "Вертикальная проекция является кандидатом " +
                "на исключение, но пока будет сохранена.";
        }
        else if (result.SideViewIsRemovalCandidate)
        {
            result.Message =
                "Боковая проекция является кандидатом " +
                "на исключение, но пока будет сохранена.";
        }
        else
        {
            result.Message =
                "Оба дополнительных вида содержат " +
                "существенную информацию.";
        }

        return result;
    }

    private static bool IsRemovalCandidate(
        ViewStatistics statistics,
        double scoreRatio)
    {
        const double maximumScoreRatio = 0.20;
        const int maximumGeometryCount = 12;

        int geometryCount =
            statistics.LineCount +
            statistics.CircleCount +
            statistics.ArcCount +
            statistics.EllipticalArcCount +
            statistics.OtherGeometryCount;

        bool hasVeryLowScore =
            scoreRatio < maximumScoreRatio;

        bool hasVeryLittleGeometry =
            geometryCount <= maximumGeometryCount;

        // Вид считается кандидатом только при одновременном
        // выполнении двух условий. Это уменьшает риск
        // ошибочного удаления важной проекции.
        return hasVeryLowScore &&
               hasVeryLittleGeometry;
    }
}

public class ViewNecessityResult
{
    public bool KeepMainView { get; set; }

    public bool KeepUpperView { get; set; }

    public bool KeepSideView { get; set; }

    public bool UpperViewIsRemovalCandidate { get; set; }

    public bool SideViewIsRemovalCandidate { get; set; }

    public double UpperScoreRatio { get; set; }

    public double SideScoreRatio { get; set; }

    public string Message { get; set; } = string.Empty;
}