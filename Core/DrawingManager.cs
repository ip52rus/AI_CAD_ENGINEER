using Inventor;

namespace AI_CAD_ENGINEER.Core;

public class DrawingManager
{
    private readonly Inventor.Application _inventor;

    public DrawingManager(Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public bool CreateDrawingWithBaseView(Document modelDocument)
    {
        try
        {
            DrawingDocument drawingDocument =
                (DrawingDocument)_inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    "",
                    true);

            Sheet sheet = drawingDocument.ActiveSheet;

            Point2d viewPosition =
                _inventor.TransientGeometry.CreatePoint2d(
                    sheet.Width / 2,
                    sheet.Height / 2);

            sheet.DrawingViews.AddBaseView(
                (Inventor._Document)modelDocument,
                viewPosition,
                0.1,
                ViewOrientationTypeEnum.kFrontViewOrientation,
                DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle);

            drawingDocument.Activate();

            return true;
        }
        catch
        {
            return false;
        }
    }
}