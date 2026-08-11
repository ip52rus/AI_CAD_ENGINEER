using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveWeldingSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveWeldingSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_welding_symbol";

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
                out int symbolIndex,
                out double x,
                out double y))
        {
            return WeldingSymbolCommandSupport.CreateError(
                "Invalid move_welding_symbol input.",
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
                "Invalid move_welding_symbol input.",
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

        object symbolBefore =
            WeldingSymbolCommandSupport.ReadWeldingSymbolFacts(
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
                symbol.Leader;

            if (leader != null &&
                leader.HasRootNode)
            {
                LeaderNode rootNode =
                    leader.RootNode;

                rootNode.Position =
                    position;

                mutationApi =
                    "DrawingWeldingSymbol.Leader.RootNode.Position";
            }
            else
            {
                symbol.Position =
                    position;

                mutationApi =
                    "DrawingWeldingSymbol.Position";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbol.Move", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return WeldingSymbolCommandSupport.CreateError(
                "Failed to move welding symbol.",
                diagnostics);
        }

        object symbolAfter =
            WeldingSymbolCommandSupport.ReadWeldingSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                symbolIndex);

        if (!WeldingSymbolCommandSupport.TryReadEffectivePosition(
                symbol,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !WeldingSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "DrawingWeldingSymbol.Move.Verification",
                message =
                    "Inventor did not report the requested welding symbol placement after the move operation.",
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

            return WeldingSymbolCommandSupport.CreateError(
                "Welding symbol move was not applied by Inventor.",
                diagnostics);
        }

        return WeldingSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_welding_symbol",
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

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int symbolIndex,
        out double x,
        out double y)
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

        if (!WeldingSymbolCommandSupport.TryGetPosition(
                root,
                diagnostics,
                out x,
                out y))
        {
            valid =
                false;
        }

        return valid;
    }
}
