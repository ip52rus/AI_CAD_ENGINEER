using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;

    public DrawingManager(Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public bool CreateEmptyDrawing()
    {
        try
        {
            DrawingDocument drawingDocument =
                (DrawingDocument)_inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    "",
                    true);

            drawingDocument.Activate();

            return true;
        }
        catch
        {
            return false;
        }
    }
}