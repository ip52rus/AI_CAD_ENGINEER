using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveRevisionTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveRevisionTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_revision_table";

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
            return RevisionTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!RevisionTableCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int revisionTableIndex,
                out double x,
                out double y))
        {
            return RevisionTableCommandSupport.CreateError(
                "Invalid move_revision_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return RevisionTableCommandSupport.CreateError(
                "Invalid move_revision_table input.",
                diagnostics);
        }

        RevisionTable? revisionTable =
            RevisionTableCommandSupport.ResolveRevisionTable(
                sheet,
                revisionTableIndex,
                diagnostics);

        if (revisionTable == null)
        {
            return RevisionTableCommandSupport.CreateError(
                "Unable to resolve revision table.",
                diagnostics);
        }

        object revisionTableBefore =
            TableReadSupport.ReadRevisionTableSnapshot(
                drawingDocument,
                sheet,
                revisionTable,
                revisionTableIndex);

        const string mutationApi =
            "RevisionTable.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            revisionTable.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionTable.Position", message = exception.Message, exceptionType = exception.GetType().FullName, revisionTableIndex });
            return RevisionTableCommandSupport.CreateError(
                "Failed to move revision table.",
                diagnostics);
        }

        object revisionTableAfter =
            TableReadSupport.ReadRevisionTableSnapshot(
                drawingDocument,
                sheet,
                revisionTable,
                revisionTableIndex);

        if (!RevisionTableCommandSupport.TryReadPosition(
                revisionTable,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !RevisionTableCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "RevisionTable.Move.Verification",
                message =
                    "Inventor did not report the requested revision table position after the move operation.",
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

            return RevisionTableCommandSupport.CreateError(
                "Revision table move was not applied by Inventor.",
                diagnostics);
        }

        return RevisionTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_revision_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                revisionTableIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                revisionTableBefore,
                revisionTableAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
