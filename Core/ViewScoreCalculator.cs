namespace AI_CAD_ENGINEER.Core;

public class ViewScoreCalculator
{
    public double Calculate(ViewStatistics statistics)
    {
        double score = 0;

        // Основная геометрия
        score += statistics.LineCount;

        // Дуги более информативны
        score += statistics.ArcCount * 2.0;

        // Эллиптические дуги обычно появляются
        // при проекции отверстий
        score += statistics.EllipticalArcCount * 2.0;

        // Окружности имеют высокий вес
        score += statistics.CircleCount * 8.0;

        // Небольшой бонус за площадь
        score += statistics.Area * 0.05;

        // Очень вытянутые виды менее информативны
        if (statistics.AspectRatio > 12.0)
        {
            score *= 0.50;
        }
        else if (statistics.AspectRatio > 8.0)
        {
            score *= 0.75;
        }

        return score;
    }
}