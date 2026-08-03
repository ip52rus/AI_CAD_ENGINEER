using AI_CAD_ENGINEER.Engineering.Models;
using Inventor;

namespace AI_CAD_ENGINEER.Engineering.Analysis;

public class DimensionCandidateGenerator
{
    public List<DimensionCandidate> Generate(
        DrawingView drawingView)
    {
        ArgumentNullException.ThrowIfNull(drawingView);

        List<DimensionCandidate> candidates = new();

        DrawingCurvesEnumerator? curves =
            drawingView.DrawingCurves[null];

        if (curves == null)
        {
            return candidates;
        }

        foreach (DrawingCurve curve in curves)
        {
            foreach (DrawingCurveSegment segment
                     in curve.Segments)
            {
                DimensionCandidate candidate =
                    new()
                    {
                        Name =
                            segment.GeometryType.ToString(),

                        SourceViewName =
                            drawingView.Name
                    };

                candidates.Add(candidate);
            }
        }

        return candidates;
    }
}