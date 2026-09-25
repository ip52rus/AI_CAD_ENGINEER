using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateSketchedSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateSketchedSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_sketched_symbol";

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

        if (!SketchedSymbolCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out SketchedSymbolCreateInput input))
        {
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid create_sketched_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                input.SheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{input.SheetName}\" was not found." });
            return SketchedSymbolCommandSupport.CreateError(
                "Invalid create_sketched_symbol input.",
                diagnostics);
        }

        SketchedSymbolDefinition? definition =
            SketchedSymbolCommandSupport.ResolveDefinition(
                drawingDocument,
                input.DefinitionName,
                diagnostics);

        if (definition == null)
        {
            return SketchedSymbolCommandSupport.CreateError(
                "Unable to resolve sketched symbol definition.",
                diagnostics);
        }

        object promptStrings =
            SketchedSymbolCommandSupport.CreatePromptStringsArgument(
                input.PromptedValues);

        GeometryIntent? attachmentIntent =
            null;

        if (input.Attachment != null)
        {
            DrawingView? drawingView =
                HoleThreadNoteCommandSupport.FindView(
                    sheet,
                    input.Attachment.ViewName);

            if (drawingView == null)
            {
                diagnostics.Add(new { scope = "input.attachment.viewName", message = $"DrawingView \"{input.Attachment.ViewName}\" was not found on the sheet." });
                return SketchedSymbolCommandSupport.CreateError(
                    "Invalid create_sketched_symbol attachment.",
                    diagnostics);
            }

            DrawingCurve? drawingCurve =
                SketchedSymbolCommandSupport.ResolveDrawingCurve(
                    drawingView,
                    input.Attachment.CurveIndex,
                    diagnostics);

            if (drawingCurve == null)
            {
                return SketchedSymbolCommandSupport.CreateError(
                    "Invalid create_sketched_symbol attachment.",
                    diagnostics);
            }

            try
            {
                attachmentIntent =
                    sheet.CreateGeometryIntent(
                        drawingCurve,
                        input.Attachment.PointIntent);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "Sheet.CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex = input.Attachment.CurveIndex, intent = input.Attachment.IntentText });
                return SketchedSymbolCommandSupport.CreateError(
                    "Failed to create sketched symbol GeometryIntent.",
                    diagnostics);
            }
        }

        SketchedSymbol symbol;
        string creationApi;

        try
        {
            SketchedSymbols symbols =
                sheet.SketchedSymbols;

            if (input.LeaderPoints != null)
            {
                ObjectCollection leaderPoints =
                    _inventor
                        .TransientObjects
                        .CreateObjectCollection();

                foreach ((double x, double y) in input.LeaderPoints)
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

                symbol =
                    symbols.AddWithLeader(
                        definition,
                        leaderPoints,
                        input.Rotation,
                        input.Scale,
                        promptStrings);

                creationApi =
                    "SketchedSymbols.AddWithLeader";
            }
            else
            {
                Point2d position =
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            input.X!.Value,
                            input.Y!.Value);

                symbol =
                    symbols.Add(
                        definition,
                        position,
                        input.Rotation,
                        input.Scale,
                        promptStrings);

                creationApi =
                    "SketchedSymbols.Add";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SketchedSymbols.Create", message = exception.Message, exceptionType = exception.GetType().FullName, creationMode = input.LeaderPoints == null ? "free" : "leader", promptedValueCount = input.PromptedValues.Count });
            return SketchedSymbolCommandSupport.CreateError(
                "Failed to create sketched symbol.",
                diagnostics);
        }

        int createdIndex =
            0;

        try
        {
            createdIndex =
                sheet.SketchedSymbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SketchedSymbols.Count.AfterCreate", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        object symbolSnapshot =
            SketchedSymbolCommandSupport.ReadSketchedSymbolFacts(
                drawingDocument,
                sheet,
                symbol,
                createdIndex);

        return SketchedSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_sketched_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                definitionName =
                    input.DefinitionName,
                creationApi,
                attached =
                    input.Attachment != null,
                requestedPosition =
                    input.LeaderPoints == null
                        ? new
                        {
                            x =
                                input.X,
                            y =
                                input.Y
                        }
                        : null,
                requestedLeaderPoints =
                    input.LeaderPoints == null
                        ? null
                        : SketchedSymbolCommandSupport.FormatPoints(
                            input.LeaderPoints),
                attachmentSelector =
                    SketchedSymbolCommandSupport.FormatAttachmentSelector(
                        input.Attachment),
                rotation =
                    input.Rotation,
                scale =
                    input.Scale,
                requestedPromptedValues =
                    input.PromptedValues,
                createdSymbolIndex =
                    createdIndex,
                symbol =
                    symbolSnapshot,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
