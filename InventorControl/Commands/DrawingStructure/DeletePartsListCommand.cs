using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeletePartsListCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeletePartsListCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_parts_list";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError ?? "Unable to resolve active DrawingDocument." });
            return PartsListCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!PartsListCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int partsListIndex))
        {
            return PartsListCommandSupport.CreateError(
                "Invalid delete_parts_list input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return PartsListCommandSupport.CreateError(
                "Invalid delete_parts_list input.",
                diagnostics);
        }

        PartsList? partsList =
            PartsListCommandSupport.ResolvePartsList(
                sheet,
                partsListIndex,
                diagnostics);

        if (partsList == null)
        {
            return PartsListCommandSupport.CreateError(
                "Unable to resolve parts list.",
                diagnostics);
        }

        object deletedPartsList =
            TableReadSupport.ReadPartsList(
                drawingDocument,
                sheet,
                partsList,
                partsListIndex);

        try
        {
            partsList.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PartsList.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, partsListIndex });
            return PartsListCommandSupport.CreateError(
                "Failed to delete parts list.",
                diagnostics);
        }

        int? remainingPartsListCount =
            null;

        try
        {
            remainingPartsListCount =
                sheet
                    .PartsLists
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.PartsLists.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return PartsListCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_parts_list",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedPartsListIndex =
                    partsListIndex,
                deletedPartsList,
                remainingPartsListCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
