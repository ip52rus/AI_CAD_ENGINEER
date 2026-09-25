using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteSurfaceTextureSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteSurfaceTextureSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_surface_texture_symbol";

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
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!TryResolveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int symbolIndex))
        {
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Invalid delete_surface_texture_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Invalid delete_surface_texture_symbol input.",
                diagnostics);
        }

        SurfaceTextureSymbol? symbol =
            SurfaceTextureSymbolCommandSupport.ResolveSurfaceTextureSymbol(
                sheet,
                symbolIndex,
                diagnostics);

        if (symbol == null)
        {
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Unable to resolve surface texture symbol.",
                diagnostics);
        }

        object symbolBefore =
            SurfaceTextureSymbolCommandSupport.ReadSurfaceTextureSymbolFacts(
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
            diagnostics.Add(new { scope = "SurfaceTextureSymbol.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Failed to delete surface texture symbol.",
                diagnostics);
        }

        int? remainingSymbolCount =
            null;

        try
        {
            remainingSymbolCount =
                sheet
                    .SurfaceTextureSymbols
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbols.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return SurfaceTextureSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_surface_texture_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedSymbolIndex =
                    symbolIndex,
                deletedSymbol =
                    symbolBefore,
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
        sheetName =
            string.Empty;
        symbolIndex =
            0;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredInt32(root, "symbolIndex", out symbolIndex, out string symbolIndexError))
        {
            diagnostics.Add(new { scope = "input.symbolIndex", message = symbolIndexError });
            valid =
                false;
        }
        else if (symbolIndex < 1)
        {
            diagnostics.Add(new { scope = "input.symbolIndex", message = "symbolIndex must be a positive 1-based integer." });
            valid =
                false;
        }

        return valid;
    }
}
