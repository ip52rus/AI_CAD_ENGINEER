using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveDrawingDimensionCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public MoveDrawingDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_drawing_dimension";

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

        if (!TryGetRequiredDouble(
                root,
                "x",
                out double x,
                out string xError))
        {
            return CreateError(
                xError);
        }

        if (!TryGetRequiredDouble(
                root,
                "y",
                out double y,
                out string yError))
        {
            return CreateError(
                yError);
        }

        Sheet sheet =
            drawingDocument.ActiveSheet;

        if (!IsPointInsideSheet(
                sheet,
                x,
                y))
        {
            return CreateError(
                "Указанная позиция находится вне листа.");
        }

        GeneralDimensions dimensions =
            sheet.DrawingDimensions
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

        Point2d previousPosition =
            dimension.Text.Origin;

        try
        {
            Point2d newPosition =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            dimension.Text.Origin =
                newPosition;

            drawingDocument.Update();

            return CreateSuccess(
                new
                {
                    message =
                        $"Размер с индексом {index} перемещён.",

                    index,

                    displayedText =
                        dimension.Text.Text,

                    previousPosition =
                        new
                        {
                            x =
                                previousPosition.X,

                            y =
                                previousPosition.Y
                        },

                    newPosition =
                        new
                        {
                            x =
                                dimension.Text.Origin.X,

                            y =
                                dimension.Text.Origin.Y
                        }
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                $"Не удалось переместить размер " +
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

    private static bool TryGetRequiredDouble(
        JsonElement root,
        string propertyName,
        out double value,
        out string error)
    {
        value =
            0.0;

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

        if (!element.TryGetDouble(
                out value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно содержать число.";

            return false;
        }

        return true;
    }

    private static bool IsPointInsideSheet(
        Sheet sheet,
        double x,
        double y)
    {
        return
            x >= 0.0 &&
            y >= 0.0 &&
            x <= sheet.Width &&
            y <= sheet.Height;
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