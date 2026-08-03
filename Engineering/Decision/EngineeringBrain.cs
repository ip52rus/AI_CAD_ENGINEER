using AI_CAD_ENGINEER.Engineering.Models;

namespace AI_CAD_ENGINEER.Engineering.Decision;

public class EngineeringBrain
{
    public DrawingPlan CreateDrawingPlan(
        PartAnalysis analysis,
        IReadOnlyCollection<ViewCandidate> viewCandidates)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(viewCandidates);

        ViewCandidate? bestCandidate =
            viewCandidates
                .OrderByDescending(candidate => candidate.Score)
                .FirstOrDefault();

        StandardViewOrientation mainView =
            bestCandidate?.Orientation ??
            StandardViewOrientation.Front;

        DrawingPlan plan = new()
        {
            MainView = mainView,

            // Временное значение.
            // Позже масштаб будет рассчитываться отдельно
            // по формату листа и габаритам набора видов.
            Scale = 0.25,

            NeedCenterlines =
                analysis.HoleCount > 0,

            NeedDimensions = true,

            NeedSection = false
        };

        plan.AdditionalViews.Add(
            GetVerticalView(mainView));

        plan.AdditionalViews.Add(
            GetSideView(mainView));

        return plan;
    }

    private static StandardViewOrientation GetVerticalView(
        StandardViewOrientation mainView)
    {
        return mainView switch
        {
            StandardViewOrientation.Front =>
                StandardViewOrientation.Top,

            StandardViewOrientation.Back =>
                StandardViewOrientation.Top,

            StandardViewOrientation.Left =>
                StandardViewOrientation.Top,

            StandardViewOrientation.Right =>
                StandardViewOrientation.Top,

            StandardViewOrientation.Top =>
                StandardViewOrientation.Front,

            StandardViewOrientation.Bottom =>
                StandardViewOrientation.Front,

            _ =>
                StandardViewOrientation.Top
        };
    }

    private static StandardViewOrientation GetSideView(
        StandardViewOrientation mainView)
    {
        return mainView switch
        {
            StandardViewOrientation.Front =>
                StandardViewOrientation.Left,

            StandardViewOrientation.Back =>
                StandardViewOrientation.Right,

            StandardViewOrientation.Top =>
                StandardViewOrientation.Left,

            StandardViewOrientation.Bottom =>
                StandardViewOrientation.Left,

            StandardViewOrientation.Left =>
                StandardViewOrientation.Front,

            StandardViewOrientation.Right =>
                StandardViewOrientation.Front,

            _ =>
                StandardViewOrientation.Left
        };
    }
}