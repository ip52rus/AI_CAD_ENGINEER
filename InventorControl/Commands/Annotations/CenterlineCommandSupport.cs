using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class CenterlineCommandSupport
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

    public static bool TryGetDeleteInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int centerlineIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetPositiveIndex(
                root,
                "centerlineIndex",
                "input.centerlineIndex",
                diagnostics,
                out centerlineIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static int? ReadCenterlineCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .Centerlines
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static Centerline? ResolveCenterline(
        Sheet sheet,
        int centerlineIndex,
        List<object> diagnostics)
    {
        try
        {
            Centerlines centerlines =
                sheet.Centerlines;

            int count =
                centerlines.Count;

            if (centerlineIndex < 1 ||
                centerlineIndex > count)
            {
                diagnostics.Add(new { scope = "input.centerlineIndex", message = $"centerlineIndex {centerlineIndex} is outside Sheet.Centerlines range 1..{count}.", centerlineIndex, count });
                return null;
            }

            return centerlines[centerlineIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.Centerlines.Item", message = exception.Message, exceptionType = exception.GetType().FullName, centerlineIndex });
            return null;
        }
    }

    private static bool TryGetPositiveIndex(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out int value)
    {
        value =
            0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            !element.TryGetInt32(
                out value) ||
            value < 1)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a positive 1-based integer." });
            return false;
        }

        return true;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented =
                true
        };
    }
}
