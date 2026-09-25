using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveEdgeSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveEdgeSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_edge_symbol";

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
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!EdgeSymbolCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int edgeSymbolIndex,
                out double x,
                out double y))
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid move_edge_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid move_edge_symbol input.",
                diagnostics);
        }

        EdgeSymbol? edgeSymbol =
            EdgeSymbolCommandSupport.ResolveEdgeSymbol(
                sheet,
                edgeSymbolIndex,
                diagnostics);

        if (edgeSymbol == null)
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Unable to resolve edge symbol.",
                diagnostics);
        }

        object edgeSymbolBefore =
            EdgeSymbolReadSupport.ReadEdgeSymbolSnapshot(
                drawingDocument,
                sheet,
                edgeSymbol,
                edgeSymbolIndex);

        string? referenceKeyBefore =
            EdgeSymbolCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                edgeSymbol,
                diagnostics,
                "EdgeSymbol.Move.ReferenceKey.Before");

        const string mutationApi =
            "EdgeSymbol.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            edgeSymbol.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Position", message = exception.Message, exceptionType = exception.GetType().FullName, edgeSymbolIndex });
            return EdgeSymbolCommandSupport.CreateError(
                "Failed to move edge symbol.",
                diagnostics);
        }

        object edgeSymbolAfter =
            EdgeSymbolReadSupport.ReadEdgeSymbolSnapshot(
                drawingDocument,
                sheet,
                edgeSymbol,
                edgeSymbolIndex);

        string? referenceKeyAfter =
            EdgeSymbolCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                edgeSymbol,
                diagnostics,
                "EdgeSymbol.Move.ReferenceKey.After");

        bool verificationPassed =
            true;

        if (!EdgeSymbolCommandSupport.TryReadPosition(
                edgeSymbol,
                diagnostics,
                "EdgeSymbol.Position.ReadAfterMove",
                out double? actualX,
                out double? actualY) ||
            !EdgeSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "EdgeSymbol.Move.Verification.Position",
                message =
                    "Inventor did not report the requested edge symbol position after the move operation.",
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

            verificationPassed =
                false;
        }

        if (referenceKeyBefore != null &&
            referenceKeyAfter != null &&
            !string.Equals(
                referenceKeyBefore,
                referenceKeyAfter,
                StringComparison.Ordinal))
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Move.Verification.ReferenceKey", message = "EdgeSymbol referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Edge symbol move was not factually verified.",
                diagnostics);
        }

        return EdgeSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_edge_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                edgeSymbolIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                edgeSymbolBefore,
                edgeSymbolAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
