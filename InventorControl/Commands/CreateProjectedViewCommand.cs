using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateProjectedViewCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateProjectedViewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_projected_view";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            GetActiveDrawingDocument(
                out string? documentError);

        if (drawingDocument == null)
        {
            return CreateError(
                documentError ??
                "Не удалось получить активный чертёж.");
        }

        if (!TryGetRequiredString(
                root,
                "parentViewName",
                out string parentViewName,
                out string parentNameError))
        {
            return CreateError(
                parentNameError);
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
                "Позиция нового вида находится вне листа.",
                $"Размер листа: " +
                $"{sheet.Width:F3} × {sheet.Height:F3}. " +
                $"Получено: X={x:F3}; Y={y:F3}.");
        }

        DrawingView? parentView =
            FindDrawingView(
                sheet,
                parentViewName);

        if (parentView == null)
        {
            return CreateError(
                $"Родительский вид " +
                $"\"{parentViewName}\" не найден.");
        }

        DrawingViewStyleEnum viewStyle =
            ParseViewStyle(
                root);

        try
        {
            Point2d position =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            DrawingView projectedView =
                sheet.DrawingViews
                    .AddProjectedView(
                        parentView,
                        position,
                        viewStyle);

            drawingDocument.Update();

            return CreateSuccess(
                new
                {
                    message =
                        $"Проекционный вид " +
                        $"\"{projectedView.Name}\" создан.",

                    name =
                        projectedView.Name,

                    parentView =
                        parentView.Name,

                    viewType =
                        projectedView.ViewType
                            .ToString(),

                    orientation =
                        projectedView.Camera
                            .ViewOrientationType
                            .ToString(),

                    position =
                        new
                        {
                            x =
                                projectedView.Position.X,

                            y =
                                projectedView.Position.Y
                        },

                    width =
                        projectedView.Width,

                    height =
                        projectedView.Height,

                    scale =
                        projectedView.Scale,

                    aligned =
                        projectedView.Aligned,

                    style =
                        viewStyle.ToString()
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                "Не удалось создать проекционный вид.",
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

    private static DrawingViewStyleEnum
        ParseViewStyle(
            JsonElement root)
    {
        if (!root.TryGetProperty(
                "style",
                out JsonElement styleElement) ||
            styleElement.ValueKind !=
            JsonValueKind.String)
        {
            return
                DrawingViewStyleEnum
                    .kHiddenLineRemovedDrawingViewStyle;
        }

        string style =
            styleElement
                .GetString()?
                .Trim()
                .ToLowerInvariant()
            ?? string.Empty;

        return style switch
        {
            "hidden_line" =>
                DrawingViewStyleEnum
                    .kHiddenLineDrawingViewStyle,

            "shaded" =>
                DrawingViewStyleEnum
                    .kShadedDrawingViewStyle,

            "shaded_hidden_line" =>
                DrawingViewStyleEnum
                    .kShadedHiddenLineDrawingViewStyle,

            _ =>
                DrawingViewStyleEnum
                    .kHiddenLineRemovedDrawingViewStyle
        };
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

        if (double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "содержит недопустимое число.";

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