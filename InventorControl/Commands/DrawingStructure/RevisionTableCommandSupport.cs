using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class RevisionTableCommandSupport
{
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

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int revisionTableIndex,
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

        if (!TryGetRevisionTableIndex(
                root,
                diagnostics,
                out revisionTableIndex))
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
        out int revisionTableIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetRevisionTableIndex(
                root,
                diagnostics,
                out revisionTableIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static RevisionTable? ResolveRevisionTable(
        Sheet sheet,
        int revisionTableIndex,
        List<object> diagnostics)
    {
        try
        {
            RevisionTables revisionTables =
                sheet.RevisionTables;

            int count =
                revisionTables.Count;

            if (revisionTableIndex < 1 ||
                revisionTableIndex > count)
            {
                diagnostics.Add(new { scope = "input.revisionTableIndex", message = $"revisionTableIndex {revisionTableIndex} is outside Sheet.RevisionTables range 1..{count}.", revisionTableIndex, count });
                return null;
            }

            return revisionTables[revisionTableIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.RevisionTables.Item", message = exception.Message, exceptionType = exception.GetType().FullName, revisionTableIndex });
            return null;
        }
    }

    public static bool TryReadPosition(
        RevisionTable revisionTable,
        List<object> diagnostics,
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
                revisionTable.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionTable.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static int? ReadRevisionTableCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .RevisionTables
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
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

    private static bool TryGetRevisionTableIndex(
        JsonElement root,
        List<object> diagnostics,
        out int revisionTableIndex)
    {
        revisionTableIndex =
            0;

        if (!root.TryGetProperty(
                "revisionTableIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out revisionTableIndex))
        {
            diagnostics.Add(new { scope = "input.revisionTableIndex", message = "revisionTableIndex must be a positive 1-based integer." });
            return false;
        }

        if (revisionTableIndex < 1)
        {
            diagnostics.Add(new { scope = "input.revisionTableIndex", message = "revisionTableIndex must be a positive 1-based integer.", revisionTableIndex });
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
