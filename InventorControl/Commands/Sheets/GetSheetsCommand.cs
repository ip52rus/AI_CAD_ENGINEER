using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSheetsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetSheetsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_sheets";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            SheetCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return SheetCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        List<object> sheets =
            new();

        int index =
            1;

        foreach (Sheet sheet
                 in drawingDocument.Sheets)
        {
            bool isActive =
                string.Equals(
                    sheet.Name,
                    drawingDocument.ActiveSheet.Name,
                    StringComparison.OrdinalIgnoreCase);

            sheets.Add(
                new
                {
                    index,

                    name =
                        sheet.Name,

                    size =
                        sheet.Size.ToString(),

                    width =
                        sheet.Width,

                    height =
                        sheet.Height,

                    drawingViewCount =
                        sheet.DrawingViews.Count,

                    isActive
                });

            index++;
        }

        return SheetCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    activeSheet =
                        drawingDocument.ActiveSheet.Name,

                    sheetCount =
                        sheets.Count,

                    sheets
                });
    }
}
