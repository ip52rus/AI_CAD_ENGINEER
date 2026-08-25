using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DocumentCommandSupport
{
    public static SilentOperationScope
        BeginSilentOperation(
            Inventor.Application inventor,
            bool silent)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        return new SilentOperationScope(
            inventor,
            silent);
    }

    public static List<object>
        SnapshotOpenDocuments(
            Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        List<object> documents =
            new();

        int index =
            1;

        foreach (Document document
                 in inventor.Documents)
        {
            bool isActive =
                ReferenceEquals(
                    document,
                    inventor.ActiveDocument);

            documents.Add(
                new
                {
                    index,

                    name =
                        document.DisplayName,

                    fullFileName =
                        document.FullFileName,

                    documentType =
                        document.DocumentType
                            .ToString(),

                    dirty =
                        document.Dirty,

                    isActive
                });

            index++;
        }

        return documents;
    }

    public static Document? FindDocument(
        Inventor.Application inventor,
        string documentNameOrPath)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        foreach (Document document
                 in inventor.Documents)
        {
            if (string.Equals(
                    document.DisplayName,
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }

            if (!string.IsNullOrWhiteSpace(
                    document.FullFileName) &&
                string.Equals(
                    document.FullFileName,
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }

            if (!string.IsNullOrWhiteSpace(
                    document.FullFileName) &&
                string.Equals(
                    System.IO.Path.GetFileName(
                    document.FullFileName),
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value =
            string.Empty;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле " +
                $"\"{propertyName}\".";

            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue,
        out bool value,
        out string error)
    {
        value =
            defaultValue;

        error =
            string.Empty;

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
            error =
                $"Поле \"{propertyName}\" " +
                "должно содержать true или false.";

            return false;
        }

        value =
            element.GetBoolean();

        return true;
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

    internal sealed class SilentOperationScope :
        IDisposable
    {
        private readonly Inventor.Application
            _inventor;

        private readonly bool
            _previousSilentOperation;

        private bool
            _disposed;

        public SilentOperationScope(
            Inventor.Application inventor,
            bool silent)
        {
            _inventor =
                inventor;

            _previousSilentOperation =
                inventor.SilentOperation;

            RequestedSilentOperation =
                silent;

            inventor.SilentOperation =
                silent;

            AppliedSilentOperation =
                inventor.SilentOperation;
        }

        public bool PreviousSilentOperation =>
            _previousSilentOperation;

        public bool RequestedSilentOperation
        {
            get;
        }

        public bool AppliedSilentOperation
        {
            get;
        }

        public bool CurrentSilentOperation =>
            _inventor.SilentOperation;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _inventor.SilentOperation =
                _previousSilentOperation;

            _disposed =
                true;
        }
    }
}
