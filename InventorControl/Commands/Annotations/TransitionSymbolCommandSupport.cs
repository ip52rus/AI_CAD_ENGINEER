using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class TransitionSymbolCommandSupport
{
    private static readonly string[] SupportedSymbolIndicationTypes =
    {
        "no_symbol",
        "horizontal_adjacent",
        "vertical_adjacent",
        "for_all_transitions",
        "for_external_transitions",
        "for_internal_transitions",
        "additional_indications_presented",
        "additional_indications_parentheses"
    };

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,
                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
        string error,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,
                error,
                diagnostics
            },
            CreateJsonOptions());
    }

    public static bool TryGetCreateInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out List<(double X, double Y)> leaderPoints,
        out double x,
        out double y,
        out TransitionSymbolIndicationTypeEnum symbolIndicationType,
        out string symbolIndicationTypeText,
        out bool? combinedMaximumAndLeastMaterial,
        out bool combinedMaximumAndLeastMaterialSupplied)
    {
        x =
            0;
        y =
            0;
        symbolIndicationType =
            0;
        symbolIndicationTypeText =
            string.Empty;
        combinedMaximumAndLeastMaterial =
            null;
        combinedMaximumAndLeastMaterialSupplied =
            false;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetLeaderPoints(
                root,
                diagnostics,
                out leaderPoints))
        {
            valid =
            false;
        }

        if (!TryGetFiniteDouble(
                root,
                "x",
                "input.x",
                diagnostics,
                out x))
        {
            valid =
                false;
        }

        if (!TryGetFiniteDouble(
                root,
                "y",
                "input.y",
                diagnostics,
                out y))
        {
            valid =
                false;
        }

        if (!TryGetRequiredString(
                root,
                "symbolIndicationType",
                "input.symbolIndicationType",
                diagnostics,
                out symbolIndicationTypeText) ||
            !TryParseSymbolIndicationType(
                symbolIndicationTypeText,
                out symbolIndicationType))
        {
            diagnostics.Add(new { scope = "input.symbolIndicationType", message = "Unsupported symbolIndicationType.", supported = SupportedSymbolIndicationTypes });
            valid =
                false;
        }

        if (!TryGetOptionalBoolean(
                root,
                "combinedMaximumAndLeastMaterial",
                "input.combinedMaximumAndLeastMaterial",
                diagnostics,
                out combinedMaximumAndLeastMaterial,
                out combinedMaximumAndLeastMaterialSupplied))
        {
            valid =
                false;
        }

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int transitionSymbolIndex,
        out double x,
        out double y)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetTransitionSymbolIndex(
                root,
                diagnostics,
                out transitionSymbolIndex))
        {
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

    public static bool TryGetDeleteInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int transitionSymbolIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetTransitionSymbolIndex(
                root,
                diagnostics,
                out transitionSymbolIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static TransitionSymbol? ResolveTransitionSymbol(
        Sheet sheet,
        int transitionSymbolIndex,
        List<object> diagnostics)
    {
        try
        {
            TransitionSymbols symbols =
                sheet.TransitionSymbols;

            int count =
                symbols.Count;

            if (transitionSymbolIndex < 1 ||
                transitionSymbolIndex > count)
            {
                diagnostics.Add(new { scope = "input.transitionSymbolIndex", message = $"transitionSymbolIndex {transitionSymbolIndex} is outside Sheet.TransitionSymbols range 1..{count}.", transitionSymbolIndex, count });
                return null;
            }

            return symbols[transitionSymbolIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.TransitionSymbols.Item", message = exception.Message, exceptionType = exception.GetType().FullName, transitionSymbolIndex });
            return null;
        }
    }

    public static int? ReadTransitionSymbolCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .TransitionSymbols
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadPosition(
        TransitionSymbol symbol,
        List<object> diagnostics,
        string scope,
        out double? x,
        out double? y)
    {
        x =
            null;
        y =
            null;

        try
        {
            Point2d position =
                symbol.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static string? ReadReferenceKeyString(
        DrawingDocument drawingDocument,
        TransitionSymbol symbol,
        List<object> diagnostics,
        string scope)
    {
        int keyContext =
            0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            symbol.GetReferenceKey(
                ref referenceKey,
                keyContext);

            return manager.KeyToString(
                ref referenceKey);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
        finally
        {
            if (keyContext !=
                0)
            {
                try
                {
                    drawingDocument
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new { scope = $"{scope}.ReleaseKeyContext", message = exception.Message, exceptionType = exception.GetType().FullName });
                }
            }
        }
    }

    public static bool CoordinatesMatch(
        double? actualX,
        double? actualY,
        double requestedX,
        double requestedY)
    {
        const double tolerance =
            0.0001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - requestedX) <= tolerance &&
               Math.Abs(actualY.Value - requestedY) <= tolerance;
    }

    public static bool TryReadDefinitionFacts(
        TransitionSymbol symbol,
        List<object> diagnostics,
        string scope,
        out TransitionSymbolDefinitionFacts facts)
    {
        facts =
            new TransitionSymbolDefinitionFacts(
                null,
                null);

        try
        {
            TransitionSymbolDefinition definition =
                symbol.Definition;

            facts =
                new TransitionSymbolDefinitionFacts(
                    definition.IndicationType,
                    definition.CombinedMaximumAndLeastMaterial);

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static TransitionSymbolAttachmentTypeEnum? ReadAttachmentType(
        TransitionSymbol symbol,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return symbol.AttachmentType;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static int? ReadLeaderNodeCount(
        TransitionSymbol symbol,
        List<object> diagnostics,
        string scope)
    {
        Leader? leader;

        try
        {
            leader =
                symbol.Leader;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (leader == null)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader", message = "TransitionSymbol.Leader is null or unavailable." });
            return null;
        }

        LeaderNodesEnumerator? allNodes;

        try
        {
            allNodes =
                leader.AllNodes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader.AllNodes", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (allNodes == null)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader.AllNodes", message = "TransitionSymbol.Leader.AllNodes is null or unavailable." });
            return null;
        }

        try
        {
            return allNodes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader.AllNodes.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool TryGetLeaderPoints(
        JsonElement root,
        List<object> diagnostics,
        out List<(double X, double Y)> points)
    {
        points =
            new List<(double X, double Y)>();

        if (!root.TryGetProperty(
                "leaderPoints",
                out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement pointElement in element.EnumerateArray())
        {
            if (pointElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.leaderPoints[{index}]", message = "Leader point must be an object." });
                return false;
            }

            if (!TryGetFiniteDouble(pointElement, "x", $"input.leaderPoints[{index}].x", diagnostics, out double x) ||
                !TryGetFiniteDouble(pointElement, "y", $"input.leaderPoints[{index}].y", diagnostics, out double y))
            {
                return false;
            }

            points.Add(
                (x, y));

            index++;
        }

        if (points.Count == 0)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must contain at least one point." });
            return false;
        }

        return true;
    }

    private static bool TryGetTransitionSymbolIndex(
        JsonElement root,
        List<object> diagnostics,
        out int transitionSymbolIndex)
    {
        transitionSymbolIndex =
            0;

        if (!root.TryGetProperty(
                "transitionSymbolIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out transitionSymbolIndex))
        {
            diagnostics.Add(new { scope = "input.transitionSymbolIndex", message = "transitionSymbolIndex must be a positive 1-based integer." });
            return false;
        }

        if (transitionSymbolIndex < 1)
        {
            diagnostics.Add(new { scope = "input.transitionSymbolIndex", message = "transitionSymbolIndex must be a positive 1-based integer.", transitionSymbolIndex });
            return false;
        }

        return true;
    }

    private static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out string value)
    {
        value =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a non-empty string." });
            return false;
        }

        value =
            element.GetString()?
                .Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a non-empty string." });
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out bool? value,
        out bool supplied)
    {
        value =
            null;
        supplied =
            false;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        supplied =
            true;

        if (element.ValueKind !=
            JsonValueKind.True &&
            element.ValueKind !=
            JsonValueKind.False)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a boolean when supplied." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetFiniteDouble(
        JsonElement element,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out double value)
    {
        value =
            0;

        if (!element.TryGetProperty(
                propertyName,
                out JsonElement propertyElement) ||
            !propertyElement.TryGetDouble(
                out value) ||
            !double.IsFinite(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static bool TryParseSymbolIndicationType(
        string value,
        out TransitionSymbolIndicationTypeEnum parsed)
    {
        parsed =
            value.Trim().ToLowerInvariant() switch
            {
                "no_symbol" =>
                    TransitionSymbolIndicationTypeEnum.kNoSymbolIndication,
                "horizontal_adjacent" =>
                    TransitionSymbolIndicationTypeEnum.kHorizontallyAdjacentSymbolIndication,
                "vertical_adjacent" =>
                    TransitionSymbolIndicationTypeEnum.kVerticallyAdjacentSymbolIndication,
                "for_all_transitions" =>
                    TransitionSymbolIndicationTypeEnum.kForAllTransitionsSymbolIndication,
                "for_external_transitions" =>
                    TransitionSymbolIndicationTypeEnum.kForExternalTransitionsSymbolIndication,
                "for_internal_transitions" =>
                    TransitionSymbolIndicationTypeEnum.kForInternalTransitionsSymbolIndication,
                "additional_indications_presented" =>
                    TransitionSymbolIndicationTypeEnum.kAdditionalIndicationsPresentedSymbolIndication,
                "additional_indications_parentheses" =>
                    TransitionSymbolIndicationTypeEnum.kAdditionalIndicationsWithinParenthesesSymbolIndication,
                _ =>
                    0
            };

        return parsed !=
               0;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}

internal sealed record TransitionSymbolDefinitionFacts(
    TransitionSymbolIndicationTypeEnum? IndicationType,
    bool? CombinedMaximumAndLeastMaterial);
