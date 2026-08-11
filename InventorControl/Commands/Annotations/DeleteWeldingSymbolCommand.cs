using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteWeldingSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteWeldingSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_welding_symbol";

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
            return WeldingSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!TryResolveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int symbolIndex))
        {
            return WeldingSymbolCommandSupport.CreateError(
                "Invalid delete_welding_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return WeldingSymbolCommandSupport.CreateError(
                "Invalid delete_welding_symbol input.",
                diagnostics);
        }

        DrawingWeldingSymbol? symbol =
            WeldingSymbolCommandSupport.ResolveWeldingSymbol(
                sheet,
                symbolIndex,
                diagnostics);

        if (symbol == null)
        {
            return WeldingSymbolCommandSupport.CreateError(
                "Unable to resolve welding symbol.",
                diagnostics);
        }

        object deletedSymbol =
            WeldingSymbolCommandSupport.ReadWeldingSymbolFacts(
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
            diagnostics.Add(new { scope = "DrawingWeldingSymbol.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return WeldingSymbolCommandSupport.CreateError(
                "Failed to delete welding symbol.",
                diagnostics);
        }

        int? remainingSymbolCount =
            null;

        try
        {
            remainingSymbolCount =
                sheet.WeldingSymbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbols.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return WeldingSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_welding_symbol",
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

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int symbolIndex)
    {
        bool valid =
            true;

        if (!WeldingSymbolCommandSupport.TryGetSheetName(
                root,
                diagnostics,
                out sheetName))
        {
            valid =
                false;
        }

        if (!WeldingSymbolCommandSupport.TryGetSymbolIndex(
                root,
                diagnostics,
                out symbolIndex))
        {
            valid =
                false;
        }

        return valid;
    }
}
