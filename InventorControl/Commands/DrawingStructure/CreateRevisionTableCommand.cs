using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateRevisionTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateRevisionTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_revision_table";

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

        if (!RevisionTableCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out double x,
                out double y))
        {
            return RevisionTableCommandSupport.CreateError(
                "Invalid create_revision_table input.",
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
                "Invalid create_revision_table input.",
                diagnostics);
        }

        int? countBefore =
            RevisionTableCommandSupport.ReadRevisionTableCount(
                sheet,
                diagnostics,
                "Sheet.RevisionTables.Count.BeforeCreate");

        RevisionTable createdRevisionTable;

        try
        {
            Point2d placementPoint =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            createdRevisionTable =
                sheet
                    .RevisionTables
                    .Add(
                        placementPoint);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionTables.Add", message = exception.Message, exceptionType = exception.GetType().FullName });
            return RevisionTableCommandSupport.CreateError(
                "Failed to create revision table.",
                diagnostics);
        }

        if (createdRevisionTable == null)
        {
            diagnostics.Add(new { scope = "RevisionTables.Add", message = "Inventor returned null RevisionTable." });
            return RevisionTableCommandSupport.CreateError(
                "Failed to create revision table.",
                diagnostics);
        }

        int? countAfter =
            RevisionTableCommandSupport.ReadRevisionTableCount(
                sheet,
                diagnostics,
                "Sheet.RevisionTables.Count.AfterCreate");

        int createdRevisionTableIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object revisionTable =
            TableReadSupport.ReadRevisionTableSnapshot(
                drawingDocument,
                sheet,
                createdRevisionTable,
                createdRevisionTableIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "RevisionTable.Create.Verification.Count", message = "Sheet.RevisionTables.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        if (!RevisionTableCommandSupport.TryReadPosition(
                createdRevisionTable,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !RevisionTableCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new { scope = "RevisionTable.Create.Verification.Position", message = "Inventor did not report the requested revision table position after creation.", requestedPosition = new { x, y }, actualPosition = actualX.HasValue && actualY.HasValue ? new { x = actualX.Value, y = actualY.Value } : null });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return RevisionTableCommandSupport.CreateError(
                "Revision table creation was not factually verified.",
                diagnostics);
        }

        return RevisionTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_revision_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                createdRevisionTableIndex,
                countBefore,
                countAfter,
                revisionTable,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
