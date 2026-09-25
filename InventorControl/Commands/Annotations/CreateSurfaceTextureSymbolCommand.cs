using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateSurfaceTextureSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateSurfaceTextureSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_surface_texture_symbol";

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
                out List<(double X, double Y)> leaderPointInputs,
                out SurfaceTextureSymbolAttachmentInput? attachment,
                out SurfaceTextureSymbolContentInput content))
        {
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Invalid create_surface_texture_symbol input.",
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
                "Invalid create_surface_texture_symbol input.",
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
                return SurfaceTextureSymbolCommandSupport.CreateError(
                    "Invalid create_surface_texture_symbol attachment.",
                    diagnostics);
            }

            DrawingCurve? drawingCurve =
                SurfaceTextureSymbolCommandSupport.ResolveDrawingCurve(
                    drawingView,
                    attachment.CurveIndex,
                    diagnostics);

            if (drawingCurve == null)
            {
                return SurfaceTextureSymbolCommandSupport.CreateError(
                    "Invalid create_surface_texture_symbol attachment.",
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
                return SurfaceTextureSymbolCommandSupport.CreateError(
                    "Failed to create surface texture GeometryIntent.",
                    diagnostics);
            }
        }

        SurfaceTextureSymbols symbols;

        try
        {
            symbols =
                sheet.SurfaceTextureSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SurfaceTextureSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Unable to access SurfaceTextureSymbols collection.",
                diagnostics);
        }

        SurfaceTextureSymbol symbol;

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

            symbol =
                symbols.Add(
                    leaderPoints,
                    content.SurfaceTextureType,
                    content.ForceTail,
                    content.Majority,
                    content.AllAroundSymbol,
                    content.MaximumRoughness,
                    content.MinimumRoughness,
                    content.ProductionMethod,
                    content.AdditionalProductionMethod,
                    content.SamplingLength,
                    content.AdditionalSamplingLength,
                    content.LayDirection,
                    content.MachiningAllowance,
                    content.AdditionalRoughness,
                    content.SurfaceWaviness,
                    Type.Missing,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbols.Add", message = exception.Message, exceptionType = exception.GetType().FullName, attached = attachment != null });
            return SurfaceTextureSymbolCommandSupport.CreateError(
                "Failed to create surface texture symbol.",
                diagnostics);
        }

        int? createdSymbolIndex =
            null;

        try
        {
            createdSymbolIndex =
                symbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbols.Count.AfterAdd", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return SurfaceTextureSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_surface_texture_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                attached =
                    attachment != null,
                requestedLeaderPoints =
                    SurfaceTextureSymbolCommandSupport.FormatLeaderPoints(
                        leaderPointInputs),
                attachmentSelector =
                    attachment == null
                        ? null
                        : new
                        {
                            viewName =
                                attachment.ViewName,
                            curveIndex =
                                attachment.CurveIndex,
                            intent =
                                attachment.IntentText,
                            intentRaw =
                                attachment.PointIntent.ToString()
                        },
                requestedNativeContent =
                    SurfaceTextureSymbolCommandSupport.FormatNativeContent(
                        content),
                createdSymbolIndex,
                symbol =
                    SurfaceTextureSymbolCommandSupport.ReadSurfaceTextureSymbolFacts(
                        drawingDocument,
                        sheet,
                        symbol,
                        createdSymbolIndex ?? 0),
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
        out SurfaceTextureSymbolAttachmentInput? attachment,
        out SurfaceTextureSymbolContentInput content)
    {
        sheetName =
            string.Empty;
        leaderPoints =
            new List<(double X, double Y)>();
        attachment =
            null;
        content =
            new SurfaceTextureSymbolContentInput(
                string.Empty,
                default,
                false,
                false,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                default,
                string.Empty,
                string.Empty,
                string.Empty);

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryGetLeaderPoints(root, diagnostics, out leaderPoints))
        {
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryResolveAttachmentInput(root, diagnostics, out attachment))
        {
            valid =
                false;
        }

        if (!TryResolveContent(
                root,
                diagnostics,
                out content))
        {
            valid =
                false;
        }

        return valid;
    }

    private static bool TryResolveContent(
        JsonElement root,
        List<object> diagnostics,
        out SurfaceTextureSymbolContentInput content)
    {
        content =
            new SurfaceTextureSymbolContentInput(
                string.Empty,
                default,
                false,
                false,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                default,
                string.Empty,
                string.Empty,
                string.Empty);

        bool valid =
            true;

        if (!SurfaceTextureSymbolCommandSupport.TryGetSurfaceTextureType(root, diagnostics, out string surfaceTextureTypeText, out SurfaceTextureTypeEnum surfaceTextureType))
        {
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryGetRequiredBoolean(root, "forceTail", diagnostics, out bool forceTail))
        {
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryGetRequiredBoolean(root, "majority", diagnostics, out bool majority))
        {
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryGetRequiredBoolean(root, "allAroundSymbol", diagnostics, out bool allAroundSymbol))
        {
            valid =
                false;
        }

        string maximumRoughness =
            string.Empty;
        string minimumRoughness =
            string.Empty;
        string productionMethod =
            string.Empty;
        string additionalProductionMethod =
            string.Empty;
        string samplingLength =
            string.Empty;
        string additionalSamplingLength =
            string.Empty;
        string machiningAllowance =
            string.Empty;
        string additionalRoughness =
            string.Empty;
        string surfaceWaviness =
            string.Empty;

        if (!SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "maximumRoughness", diagnostics, out maximumRoughness) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "minimumRoughness", diagnostics, out minimumRoughness) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "productionMethod", diagnostics, out productionMethod) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "additionalProductionMethod", diagnostics, out additionalProductionMethod) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "samplingLength", diagnostics, out samplingLength) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "additionalSamplingLength", diagnostics, out additionalSamplingLength) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "machiningAllowance", diagnostics, out machiningAllowance) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "additionalRoughness", diagnostics, out additionalRoughness) ||
            !SurfaceTextureSymbolCommandSupport.TryGetRequiredStringAllowEmpty(root, "surfaceWaviness", diagnostics, out surfaceWaviness))
        {
            valid =
                false;
        }

        if (!SurfaceTextureSymbolCommandSupport.TryGetLayDirection(root, diagnostics, out string layDirectionText, out LayDirectionTypeEnum layDirection))
        {
            valid =
                false;
        }

        if (!valid)
        {
            return false;
        }

        content =
            new SurfaceTextureSymbolContentInput(
                surfaceTextureTypeText,
                surfaceTextureType,
                forceTail,
                majority,
                allAroundSymbol,
                maximumRoughness,
                minimumRoughness,
                productionMethod,
                additionalProductionMethod,
                samplingLength,
                additionalSamplingLength,
                layDirectionText,
                layDirection,
                machiningAllowance,
                additionalRoughness,
                surfaceWaviness);

        return true;
    }
}
