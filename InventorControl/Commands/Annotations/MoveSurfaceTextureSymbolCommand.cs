using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveSurfaceTextureSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveSurfaceTextureSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_surface_texture_symbol";

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
                out int symbolIndex,
                out double x,
                out double y))
        {
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Invalid move_surface_texture_symbol input.",
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
                "Invalid move_surface_texture_symbol input.",
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
                    "SurfaceTextureSymbol.Leader.RootNode.Position";
            }
            else
            {
                symbol.Position =
                    position;

                mutationApi =
                    "SurfaceTextureSymbol.Position";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbol.Move", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Failed to move surface texture symbol.",
                diagnostics);
        }

        object symbolAfter =
            SurfaceTextureSymbolCommandSupport.ReadSurfaceTextureSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                symbolIndex);

        if (!SurfaceTextureSymbolCommandSupport.TryReadEffectivePosition(
                symbol,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !SurfaceTextureSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "SurfaceTextureSymbol.Move.Verification",
                message =
                    "Inventor did not report the requested surface texture symbol placement after the move operation.",
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
                        : null,
                symbolBefore,
                symbolAfter
            });

            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Surface texture symbol move was not applied by Inventor.",
                diagnostics);
        }

        return SurfaceTextureSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_surface_texture_symbol",
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
        sheetName =
            string.Empty;
        symbolIndex =
            0;
        x =
            0.0;
        y =
            0.0;

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

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "x", out x, out string xError))
        {
            diagnostics.Add(new { scope = "input.x", message = xError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "y", out y, out string yError))
        {
            diagnostics.Add(new { scope = "input.y", message = yError });
            valid =
                false;
        }

        return valid;
    }
}
