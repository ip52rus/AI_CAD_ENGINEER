using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateTransitionSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateTransitionSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_transition_symbol";

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
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!TransitionSymbolCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out List<(double X, double Y)> leaderPointInputs,
                out double x,
                out double y,
                out TransitionSymbolIndicationTypeEnum symbolIndicationType,
                out string symbolIndicationTypeText,
                out bool? combinedMaximumAndLeastMaterial,
                out bool combinedMaximumAndLeastMaterialSupplied))
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid create_transition_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid create_transition_symbol input.",
                diagnostics);
        }

        TransitionSymbols transitionSymbols;

        try
        {
            transitionSymbols =
                sheet.TransitionSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.TransitionSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
            return TransitionSymbolCommandSupport.CreateError(
                "Unable to access TransitionSymbols collection.",
                diagnostics);
        }

        int? countBefore =
            TransitionSymbolCommandSupport.ReadTransitionSymbolCount(
                sheet,
                diagnostics,
                "Sheet.TransitionSymbols.Count.BeforeCreate");

        TransitionSymbolDefinition definition;

        try
        {
            object combinedArgument =
                combinedMaximumAndLeastMaterialSupplied
                    ? combinedMaximumAndLeastMaterial!.Value
                    : Type.Missing;

            definition =
                transitionSymbols.CreateDefinition(
                    symbolIndicationType,
                    combinedArgument,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "TransitionSymbols.CreateDefinition", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndicationType = symbolIndicationTypeText, combinedMaximumAndLeastMaterial, combinedMaximumAndLeastMaterialSupplied });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to create transition symbol definition.",
                diagnostics);
        }

        TransitionSymbol createdTransitionSymbol;

        try
        {
            ObjectCollection leaderPoints =
                _inventor
                    .TransientObjects
                    .CreateObjectCollection();

            foreach ((double leaderX, double leaderY) in leaderPointInputs)
            {
                leaderPoints.Add(
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            leaderX,
                            leaderY));
            }

            createdTransitionSymbol =
                transitionSymbols.Add(
                    leaderPoints,
                    definition);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "TransitionSymbols.Add", message = exception.Message, exceptionType = exception.GetType().FullName, leaderPointCount = leaderPointInputs.Count });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to create transition symbol.",
                diagnostics);
        }

        if (createdTransitionSymbol == null)
        {
            diagnostics.Add(new { scope = "TransitionSymbols.Add", message = "Inventor returned null TransitionSymbol." });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to create transition symbol.",
                diagnostics);
        }

        const string placementApi =
            "TransitionSymbol.Position";

        try
        {
            Point2d requestedPosition =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            createdTransitionSymbol.Position =
                requestedPosition;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Position.CreatePlacement", message = exception.Message, exceptionType = exception.GetType().FullName, requestedPosition = new { x, y } });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to place created transition symbol.",
                diagnostics);
        }

        int? countAfter =
            TransitionSymbolCommandSupport.ReadTransitionSymbolCount(
                sheet,
                diagnostics,
                "Sheet.TransitionSymbols.Count.AfterCreate");

        int createdTransitionSymbolIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object transitionSymbol =
            TransitionSymbolReadSupport.ReadTransitionSymbolSnapshot(
                drawingDocument,
                sheet,
                createdTransitionSymbol,
                createdTransitionSymbolIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Create.Verification.Count", message = "Sheet.TransitionSymbols.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        if (!TransitionSymbolCommandSupport.TryReadPosition(
                createdTransitionSymbol,
                diagnostics,
                "TransitionSymbol.Create.Verification.Position",
                out double? actualX,
                out double? actualY) ||
            !TransitionSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "TransitionSymbol.Create.Verification.Position",
                message =
                    "Inventor did not report the requested transition symbol position after creation placement.",
                placementApi,
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

        if (!TransitionSymbolCommandSupport.TryReadDefinitionFacts(
                createdTransitionSymbol,
                diagnostics,
                "TransitionSymbol.Create.Verification.DefinitionFacts",
                out TransitionSymbolDefinitionFacts actualDefinitionFacts) ||
            actualDefinitionFacts.IndicationType != symbolIndicationType)
        {
            diagnostics.Add(new
            {
                scope =
                    "TransitionSymbol.Create.Verification.SymbolIndicationType",
                message =
                    "Inventor did not report the requested TransitionSymbol indication type after creation.",
                requested =
                    symbolIndicationTypeText,
                actual =
                    actualDefinitionFacts.IndicationType?.ToString()
            });

            verificationPassed =
                false;
        }

        if (combinedMaximumAndLeastMaterialSupplied &&
            actualDefinitionFacts.CombinedMaximumAndLeastMaterial != combinedMaximumAndLeastMaterial)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Create.Verification.CombinedMaximumAndLeastMaterial", message = "Inventor did not report the requested CombinedMaximumAndLeastMaterial after creation.", requested = combinedMaximumAndLeastMaterial, actual = actualDefinitionFacts.CombinedMaximumAndLeastMaterial });
            verificationPassed =
                false;
        }

        TransitionSymbolAttachmentTypeEnum? attachmentType =
            TransitionSymbolCommandSupport.ReadAttachmentType(
                createdTransitionSymbol,
                diagnostics,
                "TransitionSymbol.Create.Verification.AttachmentType");

        if (!verificationPassed)
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Transition symbol creation was not factually verified.",
                diagnostics);
        }

        return TransitionSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_transition_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                requestedLeaderPoints =
                    leaderPointInputs.Select(
                            point => new
                            {
                                x =
                                    point.X,
                                y =
                                    point.Y
                            })
                        .ToList(),
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                requestedDefinition =
                    new
                    {
                        symbolIndicationType =
                            symbolIndicationTypeText,
                        combinedMaximumAndLeastMaterial =
                            combinedMaximumAndLeastMaterialSupplied
                                ? combinedMaximumAndLeastMaterial
                                : null
                    },
                creationApi =
                    "TransitionSymbols.CreateDefinition + TransitionSymbols.Add",
                placementApi,
                attachmentType =
                    attachmentType?.ToString(),
                createdTransitionSymbolIndex,
                countBefore,
                countAfter,
                transitionSymbol,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
