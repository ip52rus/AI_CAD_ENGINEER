using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveCustomTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveCustomTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_custom_table";

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
            return CustomTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!CustomTableCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int customTableIndex,
                out double x,
                out double y))
        {
            return CustomTableCommandSupport.CreateError(
                "Invalid move_custom_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return CustomTableCommandSupport.CreateError(
                "Invalid move_custom_table input.",
                diagnostics);
        }

        CustomTable? customTable =
            CustomTableCommandSupport.ResolveCustomTable(
                sheet,
                customTableIndex,
                diagnostics);

        if (customTable == null)
        {
            return CustomTableCommandSupport.CreateError(
                "Unable to resolve custom table.",
                diagnostics);
        }

        object customTableBefore =
            TableReadSupport.ReadCustomTable(
                drawingDocument,
                customTable,
                customTableIndex);

        const string mutationApi =
            "CustomTable.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            customTable.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Position", message = exception.Message, exceptionType = exception.GetType().FullName, customTableIndex });
            return CustomTableCommandSupport.CreateError(
                "Failed to move custom table.",
                diagnostics);
        }

        object customTableAfter =
            TableReadSupport.ReadCustomTable(
                drawingDocument,
                customTable,
                customTableIndex);

        if (!CustomTableCommandSupport.TryReadPosition(
                customTable,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !CustomTableCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "CustomTable.Move.Verification",
                message =
                    "Inventor did not report the requested custom table position after the move operation.",
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                actualPosition =
                    actualX.HasValue &&
                    actualY.HasValue
                        ? new
                        {
                            x =
                                actualX.Value,
                            y =
                                actualY.Value
                        }
                        : null
            });

            return CustomTableCommandSupport.CreateError(
                "Custom table move was not applied by Inventor.",
                diagnostics);
        }

        return CustomTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_custom_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                customTableIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                customTableBefore,
                customTableAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
