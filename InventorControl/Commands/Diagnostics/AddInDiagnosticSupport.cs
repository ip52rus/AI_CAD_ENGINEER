using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class AddInDiagnosticSupport
{
    public static string SafeGetString(
        Func<string> getter)
    {
        try
        {
            return getter() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool? SafeGetBoolean(
        Func<bool> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    public static string SafeGetAutomationType(
        ApplicationAddIn addIn,
        out bool automationAvailable,
        out string? automationError)
    {
        automationAvailable =
            false;

        automationError =
            null;

        try
        {
            object? automation =
                addIn.Automation;

            if (automation == null)
            {
                return string.Empty;
            }

            automationAvailable =
                true;

            return automation
                .GetType()
                .FullName
                ?? automation
                    .GetType()
                    .Name;
        }
        catch (Exception exception)
        {
            automationError =
                exception.Message;

            return string.Empty;
        }
    }

    public static bool LooksLikeGostAddIn(
        string displayName,
        string description,
        string classId,
        string location)
    {
        string combined =
            string.Join(
                " ",
                displayName,
                description,
                classId,
                location);

        return
            combined.Contains(
                "gost",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "ескд",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "msd",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "ais",
                StringComparison.OrdinalIgnoreCase);
    }

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
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                details
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions
        CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}
