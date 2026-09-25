using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class CenterMarkCommandSupport
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
        out int centerMarkIndex)
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
                "centerMarkIndex",
                "input.centerMarkIndex",
                diagnostics,
                out centerMarkIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static int? ReadCenterMarkCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .Centermarks
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static Centermark? ResolveCenterMark(
        Sheet sheet,
        int centerMarkIndex,
        List<object> diagnostics)
    {
        try
        {
            Centermarks centerMarks =
                sheet.Centermarks;

            int count =
                centerMarks.Count;

            if (centerMarkIndex < 1 ||
                centerMarkIndex > count)
            {
                diagnostics.Add(new { scope = "input.centerMarkIndex", message = $"centerMarkIndex {centerMarkIndex} is outside Sheet.Centermarks range 1..{count}.", centerMarkIndex, count });
                return null;
            }

            return centerMarks[centerMarkIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.Centermarks.Item", message = exception.Message, exceptionType = exception.GetType().FullName, centerMarkIndex });
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
