using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class DeleteDrawingDimensionCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public DeleteDrawingDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_drawing_dimension";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            GetActiveDrawingDocument(
                out string? error);

        if (drawingDocument == null)
        {
            return CreateError(
                error ??
                "Не удалось получить активный чертёж.");
        }

        if (!TryGetRequiredInt(
                root,
                "index",
                out int index,
                out string indexError))
        {
            return CreateError(
                indexError);
        }

        GeneralDimensions dimensions =
            drawingDocument.ActiveSheet
                .DrawingDimensions
                .GeneralDimensions;

        if (index < 1 ||
            index > dimensions.Count)
        {
            return CreateError(
                $"Размер с индексом {index} не найден.",
                $"Доступный диапазон: 1–{dimensions.Count}.");
        }

        GeneralDimension dimension =
            dimensions[index];

        string displayedText;

        try
        {
            displayedText =
                dimension.Text.Text;
        }
        catch
        {
            displayedText =
                string.Empty;
        }

        try
        {
            dimension.Delete();

            drawingDocument.Update();

            return CreateSuccess(
                new
                {
                    message =
                        $"Размер с индексом {index} удалён.",

                    index,

                    displayedText,

                    remainingDimensionCount =
                        drawingDocument.ActiveSheet
                            .DrawingDimensions
                            .GeneralDimensions
                            .Count
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                $"Не удалось удалить размер " +
                $"с индексом {index}.",
                exception.Message);
        }
    }

    private DrawingDocument?
        GetActiveDrawingDocument(
            out string? error)
    {
        error =
            null;

        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Активный документ не является чертежом.";

            return null;
        }

        return
            (DrawingDocument)activeDocument;
    }

    private static bool TryGetRequiredInt(
        JsonElement root,
        string propertyName,
        out int value,
        out string error)
    {
        value =
            0;

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

        if (!element.TryGetInt32(
                out value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно содержать целое число.";

            return false;
        }

        return true;
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