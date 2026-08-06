using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class DeleteDrawingViewCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public DeleteDrawingViewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_drawing_view";

    public string Execute(
        JsonElement root)
    {
        if (!root.TryGetProperty(
                "viewName",
                out JsonElement element))
        {
            return Error(
                "Не найден параметр viewName.");
        }

        string viewName =
            element.GetString() ??
            string.Empty;

        Document? document =
            _inventor.ActiveDocument;

        if (document == null)
        {
            return Error(
                "Нет активного документа.");
        }

        if (document.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            return Error(
                "Активный документ не является чертежом.");
        }

        DrawingDocument drawing =
            (DrawingDocument)document;

        Sheet sheet =
            drawing.ActiveSheet;

        foreach (DrawingView view
                 in sheet.DrawingViews)
        {
            if (!string.Equals(
                    view.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                view.Delete();

                drawing.Update();

                return Success(
                    new
                    {
                        message =
                            $"Вид \"{viewName}\" удалён.",

                        remainingViews =
                            sheet.DrawingViews.Count
                    });
            }
            catch (Exception exception)
            {
                return Error(
                    exception.Message);
            }
        }

        return Error(
            $"Вид \"{viewName}\" не найден.");
    }

    private static string Success(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            Options());
    }

    private static string Error(
        string message)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message
            },
            Options());
    }

    private static JsonSerializerOptions
        Options()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}