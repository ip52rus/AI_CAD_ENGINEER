using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class UpdateActiveDocumentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public UpdateActiveDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "update_active_document";

    public string Execute(
        JsonElement root)
    {
        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            return CreateError(
                "В Inventor нет активного документа.");
        }

        activeDocument.Update();

        return CreateSuccess(
            new
            {
                message =
                    "Активный документ обновлён.",

                document =
                    activeDocument.DisplayName
            });
    }

    private static string CreateSuccess(
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

    private static string CreateError(
        string message)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message
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