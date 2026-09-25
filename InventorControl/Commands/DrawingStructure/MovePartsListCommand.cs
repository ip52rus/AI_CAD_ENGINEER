using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MovePartsListCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MovePartsListCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_parts_list";

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

        if (!PartsListCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int partsListIndex,
                out double x,
                out double y))
        {
            return PartsListCommandSupport.CreateError(
                "Invalid move_parts_list input.",
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
                "Invalid move_parts_list input.",
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

        object partsListBefore =
            TableReadSupport.ReadPartsList(
                drawingDocument,
                sheet,
                partsList,
                partsListIndex);

        const string mutationApi =
            "PartsList.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            partsList.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PartsList.Position", message = exception.Message, exceptionType = exception.GetType().FullName, partsListIndex });
            return PartsListCommandSupport.CreateError(
                "Failed to move parts list.",
                diagnostics);
        }

        object partsListAfter =
            TableReadSupport.ReadPartsList(
                drawingDocument,
                sheet,
                partsList,
                partsListIndex);

        if (!PartsListCommandSupport.TryReadPosition(
                partsList,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !PartsListCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "PartsList.Move.Verification",
                message =
                    "Inventor did not report the requested parts list position after the move operation.",
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

            return PartsListCommandSupport.CreateError(
                "Parts list move was not applied by Inventor.",
                diagnostics);
        }

        return PartsListCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_parts_list",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                partsListIndex,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                mutationApi,
                partsListBefore,
                partsListAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
