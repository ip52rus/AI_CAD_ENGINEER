using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class PartsListCommandSupport
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

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int partsListIndex,
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

        if (!TryGetPartsListIndex(
                root,
                diagnostics,
                out partsListIndex))
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
        out int partsListIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetPartsListIndex(
                root,
                diagnostics,
                out partsListIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static PartsList? ResolvePartsList(
        Sheet sheet,
        int partsListIndex,
        List<object> diagnostics)
    {
        try
        {
            PartsLists partsLists =
                sheet.PartsLists;

            int count =
                partsLists.Count;

            if (partsListIndex < 1 ||
                partsListIndex > count)
            {
                diagnostics.Add(new { scope = "input.partsListIndex", message = $"partsListIndex {partsListIndex} is outside Sheet.PartsLists range 1..{count}.", partsListIndex, count });
                return null;
            }

            return partsLists[partsListIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.PartsLists.Item", message = exception.Message, exceptionType = exception.GetType().FullName, partsListIndex });
            return null;
        }
    }

    public static bool TryReadPosition(
        PartsList partsList,
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
                partsList.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PartsList.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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

    private static bool TryGetPartsListIndex(
        JsonElement root,
        List<object> diagnostics,
        out int partsListIndex)
    {
        partsListIndex =
            0;

        if (!root.TryGetProperty(
                "partsListIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out partsListIndex))
        {
            diagnostics.Add(new { scope = "input.partsListIndex", message = "partsListIndex must be a positive 1-based integer." });
            return false;
        }

        if (partsListIndex < 1)
        {
            diagnostics.Add(new { scope = "input.partsListIndex", message = "partsListIndex must be a positive 1-based integer.", partsListIndex });
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
