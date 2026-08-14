using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class RevisionCloudCommandSupport
{
    public const int MinimumControlPointCount =
        3;

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,
                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
        string error,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,
                error,
                diagnostics
            },
            CreateJsonOptions());
    }

    public static bool TryGetCreateInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out List<(double X, double Y)> controlPoints,
        out bool inverted,
        out string? name,
        out bool nameSupplied)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetControlPoints(
                root,
                diagnostics,
                out controlPoints))
        {
            valid =
                false;
        }

        if (!TryGetOptionalBoolean(
                root,
                "inverted",
                "input.inverted",
                diagnostics,
                false,
                out inverted))
        {
            valid =
                false;
        }

        if (!TryGetOptionalName(
                root,
                diagnostics,
                out name,
                out nameSupplied))
        {
            valid =
                false;
        }

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int revisionCloudIndex,
        out double x,
        out double y)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetRevisionCloudIndex(
                root,
                diagnostics,
                out revisionCloudIndex))
        {
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "x", out x, out string xError))
        {
            diagnostics.Add(new { scope = "input.x", message = xError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "y", out y, out string yError))
        {
            diagnostics.Add(new { scope = "input.y", message = yError });
            valid =
                false;
        }

        return valid;
    }

    public static bool TryGetDeleteInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int revisionCloudIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetRevisionCloudIndex(
                root,
                diagnostics,
                out revisionCloudIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static RevisionCloud? ResolveRevisionCloud(
        Sheet sheet,
        int revisionCloudIndex,
        List<object> diagnostics)
    {
        try
        {
            RevisionClouds clouds =
                sheet.RevisionClouds;

            int count =
                clouds.Count;

            if (revisionCloudIndex < 1 ||
                revisionCloudIndex > count)
            {
                diagnostics.Add(new { scope = "input.revisionCloudIndex", message = $"revisionCloudIndex {revisionCloudIndex} is outside Sheet.RevisionClouds range 1..{count}.", revisionCloudIndex, count });
                return null;
            }

            return clouds[revisionCloudIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.RevisionClouds.Item", message = exception.Message, exceptionType = exception.GetType().FullName, revisionCloudIndex });
            return null;
        }
    }

    public static int? ReadRevisionCloudCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .RevisionClouds
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadPosition(
        RevisionCloud cloud,
        List<object> diagnostics,
        string scope,
        out double? x,
        out double? y)
    {
        x =
            null;
        y =
            null;

        try
        {
            Point2d position =
                cloud.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool CoordinatesMatch(
        double? actualX,
        double? actualY,
        double requestedX,
        double requestedY)
    {
        const double tolerance =
            0.0001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - requestedX) <= tolerance &&
               Math.Abs(actualY.Value - requestedY) <= tolerance;
    }

    public static List<(double X, double Y)> ReadControlPointPositions(
        RevisionCloud cloud,
        List<object> diagnostics,
        string scope)
    {
        List<(double X, double Y)> positions =
            new();

        try
        {
            RevisionCloudControlPoints points =
                cloud
                    .Definition
                    .ControlPoints;

            int count =
                points.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                Point2d position =
                    points[index]
                        .Position;

                positions.Add(
                    (position.X, position.Y));
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return positions;
    }

    public static bool? ReadInverted(
        RevisionCloud cloud,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return cloud
                .Definition
                .Inverted;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static string? ReadReferenceKeyString(
        DrawingDocument drawingDocument,
        RevisionCloud cloud,
        List<object> diagnostics,
        string scope)
    {
        int keyContext =
            0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            cloud.GetReferenceKey(
                ref referenceKey,
                keyContext);

            return manager.KeyToString(
                ref referenceKey);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
        finally
        {
            if (keyContext !=
                0)
            {
                try
                {
                    drawingDocument
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new { scope = $"{scope}.ReleaseKeyContext", message = exception.Message, exceptionType = exception.GetType().FullName });
                }
            }
        }
    }

    public static bool ControlPointPositionsMatchPrefix(
        IReadOnlyList<(double X, double Y)> actual,
        IReadOnlyList<(double X, double Y)> requested)
    {
        if (actual.Count <
            requested.Count)
        {
            return false;
        }

        for (int index = 0;
             index < requested.Count;
             index++)
        {
            if (!CoordinatesMatch(
                    actual[index].X,
                    actual[index].Y,
                    requested[index].X,
                    requested[index].Y))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetControlPoints(
        JsonElement root,
        List<object> diagnostics,
        out List<(double X, double Y)> controlPoints)
    {
        controlPoints =
            new List<(double X, double Y)>();

        if (!root.TryGetProperty(
                "controlPoints",
                out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.controlPoints", message = "controlPoints is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.controlPoints", message = "controlPoints must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement pointElement in element.EnumerateArray())
        {
            if (pointElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.controlPoints[{index}]", message = "Control point must be an object." });
                return false;
            }

            if (!TryGetFiniteDouble(pointElement, "x", $"input.controlPoints[{index}].x", diagnostics, out double x) ||
                !TryGetFiniteDouble(pointElement, "y", $"input.controlPoints[{index}].y", diagnostics, out double y))
            {
                return false;
            }

            controlPoints.Add(
                (x, y));

            index++;
        }

        if (controlPoints.Count <
            MinimumControlPointCount)
        {
            diagnostics.Add(new { scope = "input.controlPoints", message = $"controlPoints must contain at least {MinimumControlPointCount} explicit points.", count = controlPoints.Count, minimum = MinimumControlPointCount });
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        bool defaultValue,
        out bool value)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.True &&
            element.ValueKind !=
            JsonValueKind.False)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a boolean when supplied." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetOptionalName(
        JsonElement root,
        List<object> diagnostics,
        out string? name,
        out bool nameSupplied)
    {
        name =
            null;
        nameSupplied =
            false;

        if (!root.TryGetProperty(
                "name",
                out JsonElement element))
        {
            return true;
        }

        nameSupplied =
            true;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope = "input.name", message = "name must be a non-empty string when supplied." });
            return false;
        }

        name =
            element.GetString();

        if (string.IsNullOrWhiteSpace(
                name))
        {
            diagnostics.Add(new { scope = "input.name", message = "name must be a non-empty string when supplied." });
            return false;
        }

        return true;
    }

    private static bool TryGetRevisionCloudIndex(
        JsonElement root,
        List<object> diagnostics,
        out int revisionCloudIndex)
    {
        revisionCloudIndex =
            0;

        if (!root.TryGetProperty(
                "revisionCloudIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out revisionCloudIndex))
        {
            diagnostics.Add(new { scope = "input.revisionCloudIndex", message = "revisionCloudIndex must be a positive 1-based integer." });
            return false;
        }

        if (revisionCloudIndex < 1)
        {
            diagnostics.Add(new { scope = "input.revisionCloudIndex", message = "revisionCloudIndex must be a positive 1-based integer.", revisionCloudIndex });
            return false;
        }

        return true;
    }

    private static bool TryGetFiniteDouble(
        JsonElement element,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out double value)
    {
        value =
            0;

        if (!element.TryGetProperty(
                propertyName,
                out JsonElement propertyElement) ||
            !propertyElement.TryGetDouble(
                out value) ||
            !double.IsFinite(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
