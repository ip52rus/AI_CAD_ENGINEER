using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteSketchedSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteSketchedSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_sketched_symbol";

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
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!SketchedSymbolCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int symbolIndex))
        {
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid delete_sketched_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid delete_sketched_symbol input.",
                diagnostics);
        }

        SketchedSymbol? symbol =
            SketchedSymbolCommandSupport.ResolveSketchedSymbol(
                sheet,
                symbolIndex,
                diagnostics);

        if (symbol == null)
        {
            return SketchedSymbolCommandSupport.CreateError(
                "Unable to resolve sketched symbol.",
                diagnostics);
        }

        object deletedSymbol =
            SketchedSymbolCommandSupport.ReadSketchedSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                symbolIndex);

        try
        {
            symbol.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SketchedSymbol.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return SketchedSymbolCommandSupport.CreateError(
                "Failed to delete sketched symbol.",
                diagnostics);
        }

        int? remainingSymbolCount =
            null;

        try
        {
            remainingSymbolCount =
                sheet.SketchedSymbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SketchedSymbols.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return SketchedSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_sketched_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedSymbolIndex =
                    symbolIndex,
                deletedSymbol,
                remainingSymbolCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
