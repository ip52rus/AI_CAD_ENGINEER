using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateBalloonCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateBalloonCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_balloon";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            SheetCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out string sheetError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetError });
            return CreateError("sheetName is required.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "viewName", out string viewName, out string viewError))
        {
            diagnostics.Add(new { scope = "input.viewName", message = viewError });
            return CreateError("viewName is required.", diagnostics);
        }

        if (!TryGetRequiredCurveIndex(root, out int curveIndex, diagnostics) ||
            !TryGetLeaderPoints(root, diagnostics, out List<(double X, double Y)> leaderPointInputs))
        {
            return CreateError("Invalid balloon input.", diagnostics);
        }

        Sheet? sheet =
            SheetCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = "Drawing view was not found on selected sheet.", viewName });
            return CreateError("Drawing view was not found.", diagnostics);
        }

        DrawingCurve? drawingCurve =
            TryGetDrawingCurve(
                drawingView,
                curveIndex,
                diagnostics);

        if (drawingCurve == null)
        {
            return CreateError("Drawing curve was not found.", diagnostics);
        }

        if (root.TryGetProperty("curveSnapshot", out JsonElement curveSnapshot) &&
            !ValidateCurveSnapshot(
                curveSnapshot,
                drawingCurve,
                diagnostics))
        {
            return CreateError("Curve snapshot does not match selected drawing curve.", diagnostics);
        }

        try
        {
            GeometryIntent geometryIntent =
                sheet.CreateGeometryIntent(
                    drawingCurve);

            ObjectCollection leaderPoints =
                _inventor.TransientObjects.CreateObjectCollection();

            foreach ((double x, double y) in leaderPointInputs)
            {
                leaderPoints.Add(
                    _inventor.TransientGeometry.CreatePoint2d(
                        x,
                        y));
            }

            leaderPoints.Add(
                geometryIntent);

            Balloon balloon =
                sheet.Balloons.Add(
                    leaderPoints,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing,
                    Type.Missing);

            string? referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    balloon,
                    diagnostics);

            return CreateSuccess(new
            {
                capability = "create_balloon",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = drawingView.Name,
                curveIndex,
                position = ReadPoint(SafeRead(() => balloon.Position)),
                attached = SafeReadNullableBoolean(() => balloon.Attached),
                valueSets = ReadBalloonValueSets(balloon, diagnostics),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Balloons.Add",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("Failed to create balloon.", diagnostics);
        }
    }

    private static bool TryGetRequiredCurveIndex(
        JsonElement root,
        out int curveIndex,
        List<object> diagnostics)
    {
        curveIndex =
            0;

        if (!root.TryGetProperty("curveIndex", out JsonElement element) ||
            !element.TryGetInt32(out curveIndex) ||
            curveIndex < 1)
        {
            diagnostics.Add(new { scope = "input.curveIndex", message = "curveIndex must be a positive 1-based integer." });
            return false;
        }

        return true;
    }

    private static bool TryGetLeaderPoints(
        JsonElement root,
        List<object> diagnostics,
        out List<(double X, double Y)> points)
    {
        points =
            new List<(double X, double Y)>();

        if (!root.TryGetProperty("leaderPoints", out JsonElement leaderPoints) ||
            leaderPoints.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must be a non-empty array of point objects." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement point in leaderPoints.EnumerateArray())
        {
            index++;

            if (point.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.leaderPoints[{index}]", message = "Leader point must be an object." });
                return false;
            }

            if (!TryGetFiniteDouble(point, "x", $"input.leaderPoints[{index}].x", out double x, diagnostics) ||
                !TryGetFiniteDouble(point, "y", $"input.leaderPoints[{index}].y", out double y, diagnostics))
            {
                return false;
            }

            points.Add(
                (x, y));
        }

        if (points.Count == 0)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must contain at least one point." });
            return false;
        }

        return true;
    }

    private static DrawingCurve? TryGetDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
        List<object> diagnostics)
    {
        try
        {
            DrawingCurvesEnumerator curves =
                drawingView.DrawingCurves[Type.Missing];

            int count =
                curves.Count;

            if (curveIndex < 1 ||
                curveIndex > count)
            {
                diagnostics.Add(new { scope = "input.curveIndex", message = "curveIndex is outside the drawing view curve collection.", curveIndex, count });
                return null;
            }

            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "DrawingView.DrawingCurves",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return null;
        }
    }

    private static bool ValidateCurveSnapshot(
        JsonElement snapshot,
        DrawingCurve drawingCurve,
        List<object> diagnostics)
    {
        if (snapshot.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = "input.curveSnapshot", message = "curveSnapshot must be an object." });
            return false;
        }

        bool matches =
            true;

        if (snapshot.TryGetProperty("curveType", out JsonElement curveType))
        {
            if (curveType.ValueKind != JsonValueKind.String)
            {
                diagnostics.Add(new { scope = "input.curveSnapshot.curveType", message = "curveType must be a string." });
                matches = false;
            }
            else
            {
                try
                {
                    string actual =
                        drawingCurve.CurveType.ToString();

                    if (!string.Equals(
                            curveType.GetString(),
                            actual,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        diagnostics.Add(new { scope = "input.curveSnapshot.curveType", message = "Curve type does not match.", expected = curveType.GetString(), actual });
                        matches = false;
                    }
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new { scope = "DrawingCurve.CurveType", message = exception.Message, exceptionType = exception.GetType().FullName });
                    matches = false;
                }
            }
        }

        if (snapshot.TryGetProperty("startPoint", out JsonElement startPoint) &&
            !CompareSnapshotPoint(
                startPoint,
                () => drawingCurve.StartPoint,
                "startPoint",
                diagnostics))
        {
            matches =
                false;
        }

        if (snapshot.TryGetProperty("endPoint", out JsonElement endPoint) &&
            !CompareSnapshotPoint(
                endPoint,
                () => drawingCurve.EndPoint,
                "endPoint",
                diagnostics))
        {
            matches =
                false;
        }

        return matches;
    }

    private static bool CompareSnapshotPoint(
        JsonElement snapshotPoint,
        Func<Point2d> readActual,
        string name,
        List<object> diagnostics)
    {
        if (snapshotPoint.ValueKind != JsonValueKind.Object ||
            !TryGetSnapshotNumber(snapshotPoint, "x", out double expectedX) ||
            !TryGetSnapshotNumber(snapshotPoint, "y", out double expectedY))
        {
            diagnostics.Add(new { scope = $"input.curveSnapshot.{name}", message = "Snapshot point must contain finite x and y." });
            return false;
        }

        try
        {
            Point2d actual =
                readActual();

            if (Math.Abs(expectedX - actual.X) > 1e-9 ||
                Math.Abs(expectedY - actual.Y) > 1e-9)
            {
                diagnostics.Add(new
                {
                    scope = $"input.curveSnapshot.{name}",
                    message = "Point does not match.",
                    expected = new { x = expectedX, y = expectedY },
                    actual = new { x = actual.X, y = actual.Y }
                });

                return false;
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"DrawingCurve.{name}", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }

        return true;
    }

    private static bool TryGetFiniteDouble(
        JsonElement root,
        string name,
        string scope,
        out double value,
        List<object> diagnostics)
    {
        value =
            0.0;

        if (!root.TryGetProperty(name, out JsonElement element) ||
            !element.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"Field {name} must be a finite number." });
            return false;
        }

        return true;
    }

    private static bool TryGetSnapshotNumber(
        JsonElement root,
        string name,
        out double value)
    {
        value =
            0.0;

        return root.TryGetProperty(name, out JsonElement element) &&
               element.TryGetDouble(out value) &&
               !double.IsNaN(value) &&
               !double.IsInfinity(value);
    }

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        Balloon balloon,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            balloon.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Balloon.GetReferenceKey",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return null;
        }
    }

    private static object ReadBalloonValueSets(
        Balloon balloon,
        List<object> diagnostics)
    {
        List<object> items =
            new();

        try
        {
            BalloonValueSets valueSets =
                balloon.BalloonValueSets;

            int count =
                valueSets.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                BalloonValueSet valueSet =
                    valueSets[index];

                items.Add(new
                {
                    index,
                    value = SafeRead(() => valueSet.Value),
                    itemNumber = SafeRead(() => valueSet.ItemNumber),
                    overrideValue = SafeRead(() => valueSet.OverrideValue)
                });
            }

            return new
            {
                count,
                items
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Balloon.BalloonValueSets",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return new
            {
                count = 0,
                items
            };
        }
    }

    private static object? ReadPoint(Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x = point.X,
            y = point.Y
        };
    }

    private static T? SafeRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return default;
        }
    }

    private static bool? SafeReadNullableBoolean(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                diagnostics
            },
            CreateJsonOptions());

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
