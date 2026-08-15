using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateEdgeSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateEdgeSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_edge_symbol";

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

        if (!EdgeSymbolCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out List<(double X, double Y)> leaderPointInputs,
                out EdgeSymbolValuePositionTypeEnum valuePositionType,
                out string valuePositionTypeText,
                out EdgeSymbolIndicationTypeEnum indicationType,
                out string indicationTypeText,
                out EdgeSymbolDefinitionInput definitionInput))
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid create_edge_symbol input.",
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
                "Invalid create_edge_symbol input.",
                diagnostics);
        }

        EdgeSymbols edgeSymbols;

        try
        {
            edgeSymbols =
                sheet.EdgeSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.EdgeSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
            return EdgeSymbolCommandSupport.CreateError(
                "Unable to access EdgeSymbols collection.",
                diagnostics);
        }

        int? countBefore =
            EdgeSymbolCommandSupport.ReadEdgeSymbolCount(
                sheet,
                diagnostics,
                "Sheet.EdgeSymbols.Count.BeforeCreate");

        EdgeSymbolDefinition definition;

        try
        {
            definition =
                edgeSymbols.CreateDefinition(
                    valuePositionType,
                    indicationType,
                    Type.Missing);

            EdgeSymbolCommandSupport.ApplyDefinitionInput(
                definition,
                definitionInput);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "EdgeSymbols.CreateDefinition", message = exception.Message, exceptionType = exception.GetType().FullName, valuePositionType = valuePositionTypeText, indicationType = indicationTypeText });
            return EdgeSymbolCommandSupport.CreateError(
                "Failed to create edge symbol definition.",
                diagnostics);
        }

        EdgeSymbol createdEdgeSymbol;

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

            createdEdgeSymbol =
                edgeSymbols.Add(
                    leaderPoints,
                    definition);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "EdgeSymbols.Add", message = exception.Message, exceptionType = exception.GetType().FullName, leaderPointCount = leaderPointInputs.Count });
            return EdgeSymbolCommandSupport.CreateError(
                "Failed to create edge symbol.",
                diagnostics);
        }

        if (createdEdgeSymbol == null)
        {
            diagnostics.Add(new { scope = "EdgeSymbols.Add", message = "Inventor returned null EdgeSymbol." });
            return EdgeSymbolCommandSupport.CreateError(
                "Failed to create edge symbol.",
                diagnostics);
        }

        int? countAfter =
            EdgeSymbolCommandSupport.ReadEdgeSymbolCount(
                sheet,
                diagnostics,
                "Sheet.EdgeSymbols.Count.AfterCreate");

        int createdEdgeSymbolIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object edgeSymbol =
            EdgeSymbolReadSupport.ReadEdgeSymbolSnapshot(
                drawingDocument,
                sheet,
                createdEdgeSymbol,
                createdEdgeSymbolIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Create.Verification.Count", message = "Sheet.EdgeSymbols.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        if (!EdgeSymbolCommandSupport.TryReadDefinitionEnums(
                createdEdgeSymbol,
                diagnostics,
                out EdgeSymbolValuePositionTypeEnum? actualValuePositionType,
                out EdgeSymbolIndicationTypeEnum? actualIndicationType) ||
            actualValuePositionType != valuePositionType ||
            actualIndicationType != indicationType)
        {
            diagnostics.Add(new
            {
                scope =
                    "EdgeSymbol.Create.Verification.DefinitionEnums",
                message =
                    "Inventor did not report the requested EdgeSymbol definition enum values after creation.",
                requested =
                    new
                    {
                        valuePositionType =
                            valuePositionTypeText,
                        indicationType =
                            indicationTypeText
                    },
                actual =
                    new
                    {
                        valuePositionType =
                            actualValuePositionType?.ToString(),
                        indicationType =
                            actualIndicationType?.ToString()
                    }
            });

            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Edge symbol creation was not factually verified.",
                diagnostics);
        }

        return EdgeSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_edge_symbol",
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
                requestedDefinition =
                    EdgeSymbolCommandSupport.CreateRequestedDefinitionSnapshot(
                        valuePositionTypeText,
                        indicationTypeText,
                        definitionInput),
                creationApi =
                    "EdgeSymbols.CreateDefinition + EdgeSymbols.Add",
                createdEdgeSymbolIndex,
                countBefore,
                countAfter,
                edgeSymbol,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
