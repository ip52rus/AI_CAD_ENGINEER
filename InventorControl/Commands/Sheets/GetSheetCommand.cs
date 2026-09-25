using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSheetCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetSheetCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_sheet";

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

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    sheetNameError);
        }

        Sheet? sheet =
            SheetCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        bool isActive =
            string.Equals(
                drawingDocument.ActiveSheet.Name,
                sheet.Name,
                StringComparison.OrdinalIgnoreCase);

        string? borderDefinitionName =
            null;

        string? titleBlockDefinitionName =
            null;

        try
        {
            if (sheet.Border != null)
            {
                borderDefinitionName =
                    sheet.Border.Definition.Name;
            }
        }
        catch
        {
        }

        try
        {
            if (sheet.TitleBlock != null)
            {
                titleBlockDefinitionName =
                    sheet.TitleBlock.Definition.Name;
            }
        }
        catch
        {
        }

        return SheetCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    name =
                        sheet.Name,

                    size =
                        sheet.Size.ToString(),

                    width =
                        sheet.Width,

                    height =
                        sheet.Height,

                    isActive,

                    drawingViewCount =
                        sheet.DrawingViews.Count,

                    drawingDimensionCount =
                        sheet.DrawingDimensions
                            .GeneralDimensions.Count,

                    border =
                        borderDefinitionName,

                    titleBlock =
                        titleBlockDefinitionName
                });
    }
}
