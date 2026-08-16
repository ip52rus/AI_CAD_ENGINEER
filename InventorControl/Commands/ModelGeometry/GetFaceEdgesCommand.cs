using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetFaceEdgesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetFaceEdgesCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_face_edges";

    public string Execute(JsonElement root)
    {
        if (!ModelGeometryReadSupport
                .TryGetRequiredInt32(
                    root,
                    "bodyIndex",
                    out int bodyIndex,
                    out string bodyError))
        {
            return ModelGeometryReadSupport
                .CreateError(bodyError);
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredInt32(
                    root,
                    "faceIndex",
                    out int faceIndex,
                    out string faceError))
        {
            return ModelGeometryReadSupport
                .CreateError(faceError);
        }

        PartDocument? part =
            ResolvePart(
                root,
                out DrawingDocument? drawing,
                out Sheet? sheet,
                out DrawingView? view,
                out string? error);

        if (part == null)
        {
            return ModelGeometryReadSupport
                .CreateError(error ?? "Не удалось получить модель детали.");
        }

        object? data =
            ModelGeometryReadSupport
                .ReadFaceEdges(
                    part,
                    bodyIndex,
                    faceIndex);

        if (data == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    "Указанное тело или грань не найдены.");
        }

        return ModelGeometryReadSupport
            .CreateSuccess(
                new
                {
                    drawing = drawing?.DisplayName,
                    sheet = sheet?.Name,
                    view = view?.Name,
                    source =
                        drawing == null
                            ? "activePartDocument"
                            : "drawingViewReferencedPartDocument",
                    modelDocument =
                        ModelGeometryReadSupport
                            .ReadDocument(part),
                    data
                });
    }

    private PartDocument? ResolvePart(
        JsonElement root,
        out DrawingDocument? drawing,
        out Sheet? sheet,
        out DrawingView? view,
        out string? error)
    {
        Document? activeDocument =
            _inventor.ActiveDocument;

        drawing = null;
        sheet = null;
        view = null;
        error = null;

        if (activeDocument == null)
        {
            error = "В Inventor нет активного документа.";
            return null;
        }

        if (activeDocument.DocumentType ==
            DocumentTypeEnum.kPartDocumentObject)
        {
            return (PartDocument)activeDocument;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Active document must be a PartDocument or DrawingDocument.";
            return null;
        }

        drawing =
            (DrawingDocument)activeDocument;

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out error) ||
            !ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out error))
        {
            return null;
        }

        sheet =
            ModelGeometryReadSupport
                .FindSheet(drawing, sheetName);

        view =
            sheet == null
                ? null
                : ModelGeometryReadSupport
                    .FindView(sheet, viewName);

        if (sheet == null || view == null)
        {
            error = "Лист или вид не найден.";
            return null;
        }

        return ModelGeometryReadSupport
            .GetReferencedPartDocument(
                view,
                out error);
    }
}
