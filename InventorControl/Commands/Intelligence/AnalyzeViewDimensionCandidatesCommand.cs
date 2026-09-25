using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class AnalyzeViewDimensionCandidatesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public AnalyzeViewDimensionCandidatesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "analyze_view_dimension_candidates";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            ViewDimensionCandidateSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!ViewDimensionCandidateSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    sheetNameError);
        }

        if (!ViewDimensionCandidateSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewNameError))
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    viewNameError);
        }

        double tolerance =
            Math.Max(
                0.0001,
                ViewDimensionCandidateSupport
                    .GetOptionalDouble(
                        root,
                        "tolerance",
                        0.01));

        Sheet? sheet =
            ViewDimensionCandidateSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            ViewDimensionCandidateSupport
                .FindView(
                    sheet,
                    viewName);

        if (view == null)
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден.");
        }

        List<DrawingCurve> curves;

        try
        {
            curves =
                ViewDimensionCandidateSupport
                    .GetCurves(
                        view);
        }
        catch (Exception exception)
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    "Не удалось получить геометрию вида.",
                    exception.Message);
        }

        List<ViewDimensionCandidateSupport.CurvePoint> points =
            new();

        List<ViewDimensionCandidateSupport.CircleCandidate> circles =
            new();

        for (int curveIndex = 1;
             curveIndex <= curves.Count;
             curveIndex++)
        {
            DrawingCurve curve =
                curves[curveIndex - 1];

            AddPoint(
                points,
                curveIndex,
                "start",
                curve.StartPoint);

            AddPoint(
                points,
                curveIndex,
                "end",
                curve.EndPoint);

            AddPoint(
                points,
                curveIndex,
                "mid",
                curve.MidPoint);

            Point2d? center =
                ViewDimensionCandidateSupport
                    .TryGetCenter(
                        curve);

            AddPoint(
                points,
                curveIndex,
                "center",
                center);

            string curveType =
                curve.CurveType.ToString();

            if (center != null &&
                (curveType == "kCircleCurve" ||
                 curveType == "kCircularArcCurve"))
            {
                circles.Add(
                    new ViewDimensionCandidateSupport.CircleCandidate(
                        curveIndex,
                        curveType,
                        center.X,
                        center.Y,
                        ViewDimensionCandidateSupport
                            .TryGetRadius(
                                curve)));
            }
        }

        if (points.Count == 0)
        {
            return ViewDimensionCandidateSupport
                .CreateError(
                    "В виде не найдено пригодных точек.");
        }

        double minX =
            points.Min(
                point => point.X);

        double maxX =
            points.Max(
                point => point.X);

        double minY =
            points.Min(
                point => point.Y);

        double maxY =
            points.Max(
                point => point.Y);

        List<ViewDimensionCandidateSupport.CurvePoint> leftPoints =
            points
                .Where(
                    point =>
                        Math.Abs(
                            point.X - minX) <=
                        tolerance)
                .ToList();

        List<ViewDimensionCandidateSupport.CurvePoint> rightPoints =
            points
                .Where(
                    point =>
                        Math.Abs(
                            point.X - maxX) <=
                        tolerance)
                .ToList();

        List<ViewDimensionCandidateSupport.CurvePoint> bottomPoints =
            points
                .Where(
                    point =>
                        Math.Abs(
                            point.Y - minY) <=
                        tolerance)
                .ToList();

        List<ViewDimensionCandidateSupport.CurvePoint> topPoints =
            points
                .Where(
                    point =>
                        Math.Abs(
                            point.Y - maxY) <=
                        tolerance)
                .ToList();

        object? overallWidthCandidate =
            CreateLinearCandidate(
                "overall_width",
                "horizontal",
                leftPoints,
                rightPoints,
                view.Position.X,
                view.Top + 1.5);

        object? overallHeightCandidate =
            CreateLinearCandidate(
                "overall_height",
                "vertical",
                bottomPoints,
                topPoints,
                view.Left + view.Width + 1.5,
                view.Position.Y);

        object? extremeHoleCentersCandidate =
            null;

        List<ViewDimensionCandidateSupport.CircleCandidate> fullCircles =
            circles
                .Where(
                    circle =>
                        circle.CurveType ==
                        "kCircleCurve")
                .ToList();

        if (fullCircles.Count >= 2)
        {
            ViewDimensionCandidateSupport.CircleCandidate leftCircle =
                fullCircles
                    .OrderBy(
                        circle =>
                            circle.CenterX)
                    .First();

            ViewDimensionCandidateSupport.CircleCandidate rightCircle =
                fullCircles
                    .OrderByDescending(
                        circle =>
                            circle.CenterX)
                    .First();

            extremeHoleCentersCandidate =
                new
                {
                    candidateType =
                        "extreme_hole_centers",

                    recommendedCommand =
                        "create_linear_dimension",

                    dimensionType =
                        "horizontal",

                    firstCurveIndex =
                        leftCircle.CurveIndex,

                    firstIntent =
                        "center",

                    secondCurveIndex =
                        rightCircle.CurveIndex,

                    secondIntent =
                        "center",

                    proposedPosition =
                        new
                        {
                            x =
                                (leftCircle.CenterX +
                                 rightCircle.CenterX) /
                                2.0,

                            y =
                                view.Top + 1.5
                        }
                };
        }

        List<object> holeNoteCandidates =
            new();

        List<List<ViewDimensionCandidateSupport.CircleCandidate>>
            concentricGroups =
                GroupConcentricCircles(
                    fullCircles,
                    tolerance);

        foreach (
            List<ViewDimensionCandidateSupport.CircleCandidate> group
            in concentricGroups)
        {
            ViewDimensionCandidateSupport.CircleCandidate primary =
                group
                    .OrderByDescending(
                        circle =>
                            circle.Radius ??
                            0.0)
                    .ThenBy(
                        circle =>
                            circle.CurveIndex)
                    .First();

            List<object> alternates =
                group
                    .Where(
                        circle =>
                            circle.CurveIndex !=
                            primary.CurveIndex)
                    .Select(
                        circle =>
                            (object)new
                            {
                                curveIndex =
                                    circle.CurveIndex,

                                radius =
                                    circle.Radius
                            })
                    .ToList();

            holeNoteCandidates.Add(
                new
                {
                    candidateType =
                        "hole_or_thread_note",

                    recommendedCommand =
                        "create_hole_thread_note",

                    curveIndex =
                        primary.CurveIndex,

                    curveType =
                        primary.CurveType,

                    center =
                        new
                        {
                            x =
                                primary.CenterX,

                            y =
                                primary.CenterY
                        },

                    radius =
                        primary.Radius,

                    concentricCurveCount =
                        group.Count,

                    alternateCurves =
                        alternates,

                    warning =
                        "Кандидат выбран среди полных концентрических " +
                        "окружностей. Окончательный тип подтверждает " +
                        "Inventor через HoleThreadNotes.Add."
                });
        }

        List<object> candidates =
            new();

        if (overallWidthCandidate != null)
        {
            candidates.Add(
                overallWidthCandidate);
        }

        if (overallHeightCandidate != null)
        {
            candidates.Add(
                overallHeightCandidate);
        }

        if (extremeHoleCentersCandidate != null)
        {
            candidates.Add(
                extremeHoleCentersCandidate);
        }

        candidates.AddRange(
            holeNoteCandidates);

        return ViewDimensionCandidateSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    view =
                        view.Name,

                    tolerance,

                    curveCount =
                        curves.Count,

                    pointCount =
                        points.Count,

                    bounds =
                        new
                        {
                            minX,
                            maxX,
                            minY,
                            maxY,
                            width =
                                maxX - minX,

                            height =
                                maxY - minY
                        },

                    circleCount =
                        circles.Count,

                    fullCircleCount =
                        fullCircles.Count,

                    concentricHoleGroupCount =
                        concentricGroups.Count,

                    candidateCount =
                        candidates.Count,

                    candidates
                });
    }

    private static void AddPoint(
        List<ViewDimensionCandidateSupport.CurvePoint> points,
        int curveIndex,
        string intent,
        Point2d? point)
    {
        if (point == null)
        {
            return;
        }

        points.Add(
            new ViewDimensionCandidateSupport.CurvePoint(
                curveIndex,
                intent,
                point.X,
                point.Y));
    }

    private static List<
        List<ViewDimensionCandidateSupport.CircleCandidate>>
        GroupConcentricCircles(
            List<ViewDimensionCandidateSupport.CircleCandidate> circles,
            double tolerance)
    {
        List<
            List<ViewDimensionCandidateSupport.CircleCandidate>> groups =
                new();

        foreach (
            ViewDimensionCandidateSupport.CircleCandidate circle
            in circles)
        {
            List<ViewDimensionCandidateSupport.CircleCandidate>? group =
                groups.FirstOrDefault(
                    existingGroup =>
                    {
                        ViewDimensionCandidateSupport.CircleCandidate
                            representative =
                                existingGroup[0];

                        return
                            Math.Abs(
                                representative.CenterX -
                                circle.CenterX) <=
                            tolerance &&
                            Math.Abs(
                                representative.CenterY -
                                circle.CenterY) <=
                            tolerance;
                    });

            if (group == null)
            {
                groups.Add(
                    new List<
                        ViewDimensionCandidateSupport.CircleCandidate>
                    {
                        circle
                    });
            }
            else
            {
                group.Add(
                    circle);
            }
        }

        return groups;
    }

    private static object? CreateLinearCandidate(
        string candidateType,
        string dimensionType,
        List<ViewDimensionCandidateSupport.CurvePoint> firstPoints,
        List<ViewDimensionCandidateSupport.CurvePoint> secondPoints,
        double proposedX,
        double proposedY)
    {
        if (firstPoints.Count == 0 ||
            secondPoints.Count == 0)
        {
            return null;
        }

        ViewDimensionCandidateSupport.CurvePoint first =
            firstPoints[0];

        ViewDimensionCandidateSupport.CurvePoint second =
            secondPoints[0];

        return new
        {
            candidateType,

            recommendedCommand =
                "create_linear_dimension",

            dimensionType,

            firstCurveIndex =
                first.CurveIndex,

            firstIntent =
                first.Intent,

            secondCurveIndex =
                second.CurveIndex,

            secondIntent =
                second.Intent,

            proposedPosition =
                new
                {
                    x =
                        proposedX,

                    y =
                        proposedY
                }
        };
    }
}
