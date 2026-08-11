using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateWeldingSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateWeldingSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_welding_symbol";

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
                out List<(double X, double Y)> leaderPointInputs,
                out WeldingSymbolAttachmentInput? attachment,
                out List<WeldingSymbolDefinitionInput> definitionInputs))
        {
            return WeldingSymbolCommandSupport.CreateError(
                "Invalid create_welding_symbol input.",
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
                "Invalid create_welding_symbol input.",
                diagnostics);
        }

        GeometryIntent? attachmentIntent =
            null;

        if (attachment != null)
        {
            DrawingView? drawingView =
                HoleThreadNoteCommandSupport.FindView(
                    sheet,
                    attachment.ViewName);

            if (drawingView == null)
            {
                diagnostics.Add(new { scope = "input.attachment.viewName", message = $"DrawingView \"{attachment.ViewName}\" was not found on the sheet." });
                return WeldingSymbolCommandSupport.CreateError(
                    "Invalid create_welding_symbol attachment.",
                    diagnostics);
            }

            DrawingCurve? drawingCurve =
                WeldingSymbolCommandSupport.ResolveDrawingCurve(
                    drawingView,
                    attachment.CurveIndex,
                    diagnostics);

            if (drawingCurve == null)
            {
                return WeldingSymbolCommandSupport.CreateError(
                    "Invalid create_welding_symbol attachment.",
                    diagnostics);
            }

            try
            {
                attachmentIntent =
                    sheet.CreateGeometryIntent(
                        drawingCurve,
                        attachment.PointIntent);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "Sheet.CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex = attachment.CurveIndex, intent = attachment.IntentText });
                return WeldingSymbolCommandSupport.CreateError(
                    "Failed to create welding symbol GeometryIntent.",
                    diagnostics);
            }
        }

        DrawingWeldingSymbols symbols;

        try
        {
            symbols =
                sheet.WeldingSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.WeldingSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
            return WeldingSymbolCommandSupport.CreateError(
                "Unable to access WeldingSymbols collection.",
                diagnostics);
        }

        DrawingWeldingSymbol symbol;

        try
        {
            ObjectCollection leaderPoints =
                _inventor
                    .TransientObjects
                    .CreateObjectCollection();

            foreach ((double x, double y) in leaderPointInputs)
            {
                leaderPoints.Add(
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            x,
                            y));
            }

            if (attachmentIntent != null)
            {
                leaderPoints.Add(
                    attachmentIntent);
            }

            DrawingWeldingSymbolDefinitions definitions =
                symbols.CreateDefinitions();

            int definitionIndex =
                1;

            foreach (WeldingSymbolDefinitionInput definitionInput in definitionInputs)
            {
                DrawingWeldingSymbolDefinition definition =
                    definitions.Add(
                        definitionIndex);

                WeldingSymbolCommandSupport.ApplyDefinitionInput(
                    definition,
                    definitionInput);

                definitionIndex++;
            }

            diagnostics.Add(
                WeldingSymbolCommandSupport.ReadCreationInputFacts(
                    leaderPoints,
                    definitions,
                    Type.Missing));

            symbol =
                symbols.Add(
                    leaderPoints,
                    definitions,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbols.Add", message = exception.Message, exceptionType = exception.GetType().FullName, attached = attachment != null });
            return WeldingSymbolCommandSupport.CreateError(
                "Failed to create welding symbol.",
                diagnostics);
        }

        int createdIndex =
            0;

        try
        {
            createdIndex =
                sheet.WeldingSymbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbols.Count.AfterCreate", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        object symbolSnapshot =
            WeldingSymbolCommandSupport.ReadWeldingSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                createdIndex);

        return WeldingSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_welding_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                attached =
                    attachment != null,
                requestedLeaderPoints =
                    WeldingSymbolCommandSupport.FormatLeaderPoints(
                        leaderPointInputs),
                attachmentSelector =
                    WeldingSymbolCommandSupport.FormatAttachmentSelector(
                        attachment),
                requestedDefinitions =
                    WeldingSymbolCommandSupport.FormatDefinitionsInput(
                        definitionInputs),
                createdSymbolIndex =
                    createdIndex,
                symbol =
                    symbolSnapshot,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out List<(double X, double Y)> leaderPoints,
        out WeldingSymbolAttachmentInput? attachment,
        out List<WeldingSymbolDefinitionInput> definitions)
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

        if (!WeldingSymbolCommandSupport.TryGetLeaderPoints(
                root,
                diagnostics,
                out leaderPoints))
        {
            valid =
                false;
        }

        if (!WeldingSymbolCommandSupport.TryResolveAttachmentInput(
                root,
                diagnostics,
                out attachment))
        {
            valid =
                false;
        }

        if (!WeldingSymbolCommandSupport.TryGetDefinitionsInput(
                root,
                diagnostics,
                out definitions))
        {
            valid =
                false;
        }

        return valid;
    }
}
