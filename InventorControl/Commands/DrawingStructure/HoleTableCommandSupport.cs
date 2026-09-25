using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class HoleTableCommandSupport
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
        out string viewName,
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

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "viewName", out viewName, out string viewNameError))
        {
            diagnostics.Add(new { scope = "input.viewName", message = viewNameError });
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
        out int holeTableIndex,
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

        if (!TryGetHoleTableIndex(
                root,
                diagnostics,
                out holeTableIndex))
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
        out int holeTableIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetHoleTableIndex(
                root,
                diagnostics,
                out holeTableIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static HoleTable? ResolveHoleTable(
        Sheet sheet,
        int holeTableIndex,
        List<object> diagnostics)
    {
        try
        {
            HoleTables holeTables =
                sheet.HoleTables;

            int count =
                holeTables.Count;

            if (holeTableIndex < 1 ||
                holeTableIndex > count)
            {
                diagnostics.Add(new { scope = "input.holeTableIndex", message = $"holeTableIndex {holeTableIndex} is outside Sheet.HoleTables range 1..{count}.", holeTableIndex, count });
                return null;
            }

            return holeTables[holeTableIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.HoleTables.Item", message = exception.Message, exceptionType = exception.GetType().FullName, holeTableIndex });
            return null;
        }
    }

    public static bool TryReadPosition(
        HoleTable holeTable,
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
                holeTable.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "HoleTable.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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

    public static DrawingView? FindDrawingView(
        Sheet sheet,
        string viewName)
    {
        foreach (DrawingView drawingView in sheet.DrawingViews)
        {
            if (string.Equals(
                    drawingView.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return drawingView;
            }
        }

        return null;
    }

    public static int? ReadHoleTableCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .HoleTables
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool TryGetHoleTableIndex(
        JsonElement root,
        List<object> diagnostics,
        out int holeTableIndex)
    {
        holeTableIndex =
            0;

        if (!root.TryGetProperty(
                "holeTableIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out holeTableIndex))
        {
            diagnostics.Add(new { scope = "input.holeTableIndex", message = "holeTableIndex must be a positive 1-based integer." });
            return false;
        }

        if (holeTableIndex < 1)
        {
            diagnostics.Add(new { scope = "input.holeTableIndex", message = "holeTableIndex must be a positive 1-based integer.", holeTableIndex });
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
