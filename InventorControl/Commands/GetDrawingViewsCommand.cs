using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingViewsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDrawingViewsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_drawing_views";

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

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            return CreateError(
                "Активный документ не является чертежом.");
        }

        DrawingDocument drawingDocument =
            (DrawingDocument)activeDocument;

        Sheet activeSheet =
            drawingDocument.ActiveSheet;

        List<object> views =
            new();

        int index =
            1;

        foreach (DrawingView drawingView
                 in activeSheet.DrawingViews)
        {
            views.Add(
                new
                {
                    index,

                    name =
                        drawingView.Name,

                    viewType =
                        drawingView.ViewType
                            .ToString(),

                    orientation =
                        drawingView.Camera
                            .ViewOrientationType
                            .ToString(),

                    position =
                        new
                        {
                            x =
                                drawingView.Position.X,

                            y =
                                drawingView.Position.Y
                        },

                    center =
                        new
                        {
                            x =
                                drawingView.Center.X,

                            y =
                                drawingView.Center.Y
                        },

                    width =
                        drawingView.Width,

                    height =
                        drawingView.Height,

                    scale =
                        drawingView.Scale,

                    suppressed =
                        drawingView.Suppressed,

                    aligned =
                        drawingView.Aligned,

                    referencedDocument =
                        TryGetReferencedDocumentName(
                            drawingView)
                });

            index++;
        }

        return CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    activeSheet.Name,

                sheetWidth =
                    activeSheet.Width,

                sheetHeight =
                    activeSheet.Height,

                viewCount =
                    views.Count,

                views
            });
    }

    private static string?
        TryGetReferencedDocumentName(
            DrawingView drawingView)
    {
        try
        {
            return drawingView
                .ReferencedDocumentDescriptor
                .ReferencedDocument
                .DisplayName;
        }
        catch
        {
            return null;
        }
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