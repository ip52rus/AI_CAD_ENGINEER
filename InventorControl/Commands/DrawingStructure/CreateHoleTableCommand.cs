using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateHoleTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateHoleTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_hole_table";

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

        if (!HoleTableCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out string viewName,
                out double x,
                out double y))
        {
            return HoleTableCommandSupport.CreateError(
                "Invalid create_hole_table input.",
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
                "Invalid create_hole_table input.",
                diagnostics);
        }

        DrawingView? drawingView =
            HoleTableCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = $"DrawingView \"{viewName}\" was not found on the sheet." });
            return HoleTableCommandSupport.CreateError(
                "Invalid create_hole_table input.",
                diagnostics);
        }

        int? countBefore =
            HoleTableCommandSupport.ReadHoleTableCount(
                sheet,
                diagnostics,
                "Sheet.HoleTables.Count.BeforeCreate");

        HoleTable createdHoleTable;

        try
        {
            Point2d placementPoint =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            createdHoleTable =
                sheet
                    .HoleTables
                    .Add(
                        drawingView,
                        placementPoint,
                        Type.Missing,
                        Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "HoleTables.Add", message = exception.Message, exceptionType = exception.GetType().FullName, viewName });
            return HoleTableCommandSupport.CreateError(
                "Failed to create hole table.",
                diagnostics);
        }

        if (createdHoleTable == null)
        {
            diagnostics.Add(new { scope = "HoleTables.Add", message = "Inventor returned null HoleTable." });
            return HoleTableCommandSupport.CreateError(
                "Failed to create hole table.",
                diagnostics);
        }

        int? countAfter =
            HoleTableCommandSupport.ReadHoleTableCount(
                sheet,
                diagnostics,
                "Sheet.HoleTables.Count.AfterCreate");

        int createdHoleTableIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object holeTable =
            TableReadSupport.ReadHoleTable(
                drawingDocument,
                sheet,
                createdHoleTable,
                createdHoleTableIndex);

        return HoleTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_hole_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                view =
                    new
                    {
                        name =
                            drawingView.Name
                    },
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                createdHoleTableIndex,
                countBefore,
                countAfter,
                holeTable,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
