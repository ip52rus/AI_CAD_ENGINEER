using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveSketchedSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveSketchedSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_sketched_symbol";

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

        if (!SketchedSymbolCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int symbolIndex,
                out double x,
                out double y))
        {
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid move_sketched_symbol input.",
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
                "Invalid move_sketched_symbol input.",
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

        object symbolBefore =
            SketchedSymbolCommandSupport.ReadSketchedSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                symbolIndex);

        string mutationApi;

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            Leader? leader =
                SketchedSymbolCommandSupport.TryReadLeader(
                    symbol,
                    diagnostics);

            if (leader != null &&
                SketchedSymbolCommandSupport.TryReadHasRootNode(
                    leader,
                    diagnostics))
            {
                leader.RootNode.Position =
                    position;

                mutationApi =
                    "SketchedSymbol.Leader.RootNode.Position";
            }
            else
            {
                symbol.Position =
                    position;

                mutationApi =
                    "SketchedSymbol.Position";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SketchedSymbol.Move", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return SketchedSymbolCommandSupport.CreateError(
                "Failed to move sketched symbol.",
                diagnostics);
        }

        object symbolAfter =
            SketchedSymbolCommandSupport.ReadSketchedSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                symbolIndex);

        if (!SketchedSymbolCommandSupport.TryReadEffectivePosition(
                symbol,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !SketchedSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "SketchedSymbol.Move.Verification",
                message =
                    "Inventor did not report the requested sketched symbol placement after the move operation.",
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

            return SketchedSymbolCommandSupport.CreateError(
                "Sketched symbol move was not applied by Inventor.",
                diagnostics);
        }

        return SketchedSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_sketched_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                symbolIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                symbolBefore,
                symbolAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
