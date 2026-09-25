using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveDrawingViewCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public MoveDrawingViewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_drawing_view";

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

        if (!TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out string viewNameError))
        {
            return CreateError(
                viewNameError);
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

        DrawingView? drawingView =
            FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            return CreateError(
                $"Вид \"{viewName}\" не найден.");
        }

        double previousX =
            drawingView.Position.X;

        double previousY =
            drawingView.Position.Y;

        bool wasAligned =
            drawingView.Aligned;

        try
        {
            if (drawingView.Aligned)
            {
                drawingView.Aligned =
                    false;
            }

            drawingView.Position =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            drawingDocument.Update();

            return CreateSuccess(
                new
                {
                    message =
                        $"Вид \"{drawingView.Name}\" перемещён.",

                    view =
                        drawingView.Name,

                    previousPosition =
                        new
                        {
                            x =
                                previousX,

                            y =
                                previousY
                        },

                    newPosition =
                        new
                        {
                            x =
                                drawingView.Position.X,

                            y =
                                drawingView.Position.Y
                        },

                    alignmentWasRemoved =
                        wasAligned,

                    aligned =
                        drawingView.Aligned
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                $"Не удалось переместить вид " +
                $"\"{drawingView.Name}\".",
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

    private static DrawingView?
        FindDrawingView(
            Sheet sheet,
            string viewName)
    {
        foreach (DrawingView drawingView
                 in sheet.DrawingViews)
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

    private static bool TryGetRequiredString(
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