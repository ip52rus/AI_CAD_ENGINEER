using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveHoleTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveHoleTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_hole_table";

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
            return HoleTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!HoleTableCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int holeTableIndex,
                out double x,
                out double y))
        {
            return HoleTableCommandSupport.CreateError(
                "Invalid move_hole_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return HoleTableCommandSupport.CreateError(
                "Invalid move_hole_table input.",
                diagnostics);
        }

        HoleTable? holeTable =
            HoleTableCommandSupport.ResolveHoleTable(
                sheet,
                holeTableIndex,
                diagnostics);

        if (holeTable == null)
        {
            return HoleTableCommandSupport.CreateError(
                "Unable to resolve hole table.",
                diagnostics);
        }

        object holeTableBefore =
            TableReadSupport.ReadHoleTable(
                drawingDocument,
                sheet,
                holeTable,
                holeTableIndex);

        const string mutationApi =
            "HoleTable.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            holeTable.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "HoleTable.Position", message = exception.Message, exceptionType = exception.GetType().FullName, holeTableIndex });
            return HoleTableCommandSupport.CreateError(
                "Failed to move hole table.",
                diagnostics);
        }

        object holeTableAfter =
            TableReadSupport.ReadHoleTable(
                drawingDocument,
                sheet,
                holeTable,
                holeTableIndex);

        if (!HoleTableCommandSupport.TryReadPosition(
                holeTable,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !HoleTableCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "HoleTable.Move.Verification",
                message =
                    "Inventor did not report the requested hole table position after the move operation.",
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

            return HoleTableCommandSupport.CreateError(
                "Hole table move was not applied by Inventor.",
                diagnostics);
        }

        return HoleTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_hole_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                holeTableIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                holeTableBefore,
                holeTableAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
