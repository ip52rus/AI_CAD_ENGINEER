using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class EdgeSymbolCommandSupport
{
    private static readonly string[] SupportedValuePositionTypes =
    {
        "no_values",
        "horizontal",
        "vertical",
        "vertical_and_horizontal",
        "undefined"
    };

    private static readonly string[] SupportedIndicationTypes =
    {
        "all_edges",
        "outer_edge",
        "inner_edge",
        "majority_one_exception",
        "majority_more_exception"
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
        out EdgeSymbolValuePositionTypeEnum valuePositionType,
        out string valuePositionTypeText,
        out EdgeSymbolIndicationTypeEnum indicationType,
        out string indicationTypeText,
        out EdgeSymbolDefinitionInput definitionInput)
    {
        valuePositionType =
            0;
        valuePositionTypeText =
            string.Empty;
        indicationType =
            0;
        indicationTypeText =
            string.Empty;

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

        if (!TryGetRequiredString(
                root,
                "valuePositionType",
                "input.valuePositionType",
                diagnostics,
                out valuePositionTypeText) ||
            !TryParseValuePositionType(
                valuePositionTypeText,
                out valuePositionType))
        {
            diagnostics.Add(new { scope = "input.valuePositionType", message = "Unsupported valuePositionType.", supported = SupportedValuePositionTypes });
            valid =
                false;
        }

        if (!TryGetRequiredString(
                root,
                "indicationType",
                "input.indicationType",
                diagnostics,
                out indicationTypeText) ||
            !TryParseIndicationType(
                indicationTypeText,
                out indicationType))
        {
            diagnostics.Add(new { scope = "input.indicationType", message = "Unsupported indicationType.", supported = SupportedIndicationTypes });
            valid =
                false;
        }

        definitionInput =
            ReadOptionalDefinitionInput(
                root,
                diagnostics,
                ref valid);

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int edgeSymbolIndex,
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

        if (!TryGetEdgeSymbolIndex(
                root,
                diagnostics,
                out edgeSymbolIndex))
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
        out int edgeSymbolIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetEdgeSymbolIndex(
                root,
                diagnostics,
                out edgeSymbolIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static EdgeSymbol? ResolveEdgeSymbol(
        Sheet sheet,
        int edgeSymbolIndex,
        List<object> diagnostics)
    {
        try
        {
            EdgeSymbols symbols =
                sheet.EdgeSymbols;

            int count =
                symbols.Count;

            if (edgeSymbolIndex < 1 ||
                edgeSymbolIndex > count)
            {
                diagnostics.Add(new { scope = "input.edgeSymbolIndex", message = $"edgeSymbolIndex {edgeSymbolIndex} is outside Sheet.EdgeSymbols range 1..{count}.", edgeSymbolIndex, count });
                return null;
            }

            return symbols[edgeSymbolIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.EdgeSymbols.Item", message = exception.Message, exceptionType = exception.GetType().FullName, edgeSymbolIndex });
            return null;
        }
    }

    public static int? ReadEdgeSymbolCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .EdgeSymbols
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadPosition(
        EdgeSymbol symbol,
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
        EdgeSymbol symbol,
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

    public static bool TryReadDefinitionEnums(
        EdgeSymbol symbol,
        List<object> diagnostics,
        out EdgeSymbolValuePositionTypeEnum? valuePositionType,
        out EdgeSymbolIndicationTypeEnum? indicationType)
    {
        valuePositionType =
            null;
        indicationType =
            null;

        try
        {
            EdgeSymbolDefinition definition =
                symbol.Definition;

            valuePositionType =
                definition.ValuePositionType;
            indicationType =
                definition.IndicationType;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Definition.ReadEnums", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static void ApplyDefinitionInput(
        EdgeSymbolDefinition definition,
        EdgeSymbolDefinitionInput input)
    {
        if (input.EdgesSupplied)
        {
            definition.Edges =
                input.Edges!;
        }

        if (input.HorizontalValueSupplied)
        {
            definition.HorizontalValue =
                input.HorizontalValue!;
        }

        if (input.HorizontalValueLowerSupplied)
        {
            definition.HorizontalValueLower =
                input.HorizontalValueLower!;
        }

        if (input.VerticalValueSupplied)
        {
            definition.VerticalValue =
                input.VerticalValue!;
        }

        if (input.VerticalValueLowerSupplied)
        {
            definition.VerticalValueLower =
                input.VerticalValueLower!;
        }

        if (input.UndefinedValueSupplied)
        {
            definition.UndefinedValue =
                input.UndefinedValue!;
        }

        if (input.UndefinedValueLowerSupplied)
        {
            definition.UndefinedValueLower =
                input.UndefinedValueLower!;
        }

        if (input.RangeOfValuesSupplied)
        {
            definition.RangeOfValues =
                input.RangeOfValues!.Value;
        }

        if (input.StatesOfAllEdgesAroundProfileSupplied)
        {
            definition.StatesOfAllEdgesAroundProfile =
                input.StatesOfAllEdgesAroundProfile!.Value;
        }

        if (input.SidesDefinedSupplied)
        {
            definition.SidesDefined =
                input.SidesDefined!.Value;
        }

        if (input.ReferenceToISOSupplied)
        {
            definition.ReferenceToISO =
                input.ReferenceToISO!.Value;
        }
    }

    public static object CreateRequestedDefinitionSnapshot(
        string valuePositionType,
        string indicationType,
        EdgeSymbolDefinitionInput input)
    {
        return new
        {
            valuePositionType,
            indicationType,
            edges =
                input.EdgesSupplied ? input.Edges : null,
            horizontalValue =
                input.HorizontalValueSupplied ? input.HorizontalValue : null,
            horizontalValueLower =
                input.HorizontalValueLowerSupplied ? input.HorizontalValueLower : null,
            verticalValue =
                input.VerticalValueSupplied ? input.VerticalValue : null,
            verticalValueLower =
                input.VerticalValueLowerSupplied ? input.VerticalValueLower : null,
            undefinedValue =
                input.UndefinedValueSupplied ? input.UndefinedValue : null,
            undefinedValueLower =
                input.UndefinedValueLowerSupplied ? input.UndefinedValueLower : null,
            rangeOfValues =
                input.RangeOfValuesSupplied ? input.RangeOfValues : null,
            statesOfAllEdgesAroundProfile =
                input.StatesOfAllEdgesAroundProfileSupplied ? input.StatesOfAllEdgesAroundProfile : null,
            sidesDefined =
                input.SidesDefinedSupplied ? input.SidesDefined : null,
            referenceToISO =
                input.ReferenceToISOSupplied ? input.ReferenceToISO : null
        };
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

    private static EdgeSymbolDefinitionInput ReadOptionalDefinitionInput(
        JsonElement root,
        List<object> diagnostics,
        ref bool valid)
    {
        EdgeSymbolDefinitionInput input =
            new();

        ReadOptionalString(root, "edges", "input.edges", diagnostics, ref valid, out input.Edges, out input.EdgesSupplied);
        ReadOptionalString(root, "horizontalValue", "input.horizontalValue", diagnostics, ref valid, out input.HorizontalValue, out input.HorizontalValueSupplied);
        ReadOptionalString(root, "horizontalValueLower", "input.horizontalValueLower", diagnostics, ref valid, out input.HorizontalValueLower, out input.HorizontalValueLowerSupplied);
        ReadOptionalString(root, "verticalValue", "input.verticalValue", diagnostics, ref valid, out input.VerticalValue, out input.VerticalValueSupplied);
        ReadOptionalString(root, "verticalValueLower", "input.verticalValueLower", diagnostics, ref valid, out input.VerticalValueLower, out input.VerticalValueLowerSupplied);
        ReadOptionalString(root, "undefinedValue", "input.undefinedValue", diagnostics, ref valid, out input.UndefinedValue, out input.UndefinedValueSupplied);
        ReadOptionalString(root, "undefinedValueLower", "input.undefinedValueLower", diagnostics, ref valid, out input.UndefinedValueLower, out input.UndefinedValueLowerSupplied);

        ReadOptionalBoolean(root, "rangeOfValues", "input.rangeOfValues", diagnostics, ref valid, out input.RangeOfValues, out input.RangeOfValuesSupplied);
        ReadOptionalBoolean(root, "statesOfAllEdgesAroundProfile", "input.statesOfAllEdgesAroundProfile", diagnostics, ref valid, out input.StatesOfAllEdgesAroundProfile, out input.StatesOfAllEdgesAroundProfileSupplied);
        ReadOptionalBoolean(root, "sidesDefined", "input.sidesDefined", diagnostics, ref valid, out input.SidesDefined, out input.SidesDefinedSupplied);
        ReadOptionalBoolean(root, "referenceToISO", "input.referenceToISO", diagnostics, ref valid, out input.ReferenceToISO, out input.ReferenceToISOSupplied);

        return input;
    }

    private static void ReadOptionalString(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        ref bool valid,
        out string? value,
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
            return;
        }

        supplied =
            true;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a string when supplied." });
            valid =
                false;
            return;
        }

        value =
            element.GetString() ??
            string.Empty;
    }

    private static void ReadOptionalBoolean(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        ref bool valid,
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
            return;
        }

        supplied =
            true;

        if (element.ValueKind !=
            JsonValueKind.True &&
            element.ValueKind !=
            JsonValueKind.False)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a boolean when supplied." });
            valid =
                false;
            return;
        }

        value =
            element.GetBoolean();
    }

    private static bool TryGetEdgeSymbolIndex(
        JsonElement root,
        List<object> diagnostics,
        out int edgeSymbolIndex)
    {
        edgeSymbolIndex =
            0;

        if (!root.TryGetProperty(
                "edgeSymbolIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out edgeSymbolIndex))
        {
            diagnostics.Add(new { scope = "input.edgeSymbolIndex", message = "edgeSymbolIndex must be a positive 1-based integer." });
            return false;
        }

        if (edgeSymbolIndex < 1)
        {
            diagnostics.Add(new { scope = "input.edgeSymbolIndex", message = "edgeSymbolIndex must be a positive 1-based integer.", edgeSymbolIndex });
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

    private static bool TryParseValuePositionType(
        string value,
        out EdgeSymbolValuePositionTypeEnum parsed)
    {
        parsed =
            value.Trim().ToLowerInvariant() switch
            {
                "no_values" =>
                    EdgeSymbolValuePositionTypeEnum.kEdgeSymbolValueNoValues,
                "horizontal" =>
                    EdgeSymbolValuePositionTypeEnum.kEdgeSymbolValueDirectionHorizontal,
                "vertical" =>
                    EdgeSymbolValuePositionTypeEnum.kEdgeSymbolValueDirectionVertical,
                "vertical_and_horizontal" =>
                    EdgeSymbolValuePositionTypeEnum.kEdgeSymbolValueDirectionVerticalAndHorizontal,
                "undefined" =>
                    EdgeSymbolValuePositionTypeEnum.kEdgeSymbolValueDirectionUndefined,
                _ =>
                    0
            };

        return parsed !=
               0;
    }

    private static bool TryParseIndicationType(
        string value,
        out EdgeSymbolIndicationTypeEnum parsed)
    {
        parsed =
            value.Trim().ToLowerInvariant() switch
            {
                "all_edges" =>
                    EdgeSymbolIndicationTypeEnum.kAllEdgesIndicationType,
                "outer_edge" =>
                    EdgeSymbolIndicationTypeEnum.kOuterEdgeIndicationType,
                "inner_edge" =>
                    EdgeSymbolIndicationTypeEnum.kInnerEdgeIndicationType,
                "majority_one_exception" =>
                    EdgeSymbolIndicationTypeEnum.kMajoritySymbolWithOneExceptionIndicationType,
                "majority_more_exception" =>
                    EdgeSymbolIndicationTypeEnum.kMajoritySymbolWithMoreExceptionIndicationType,
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

internal sealed class EdgeSymbolDefinitionInput
{
    public string? Edges;
    public bool EdgesSupplied;
    public string? HorizontalValue;
    public bool HorizontalValueSupplied;
    public string? HorizontalValueLower;
    public bool HorizontalValueLowerSupplied;
    public string? VerticalValue;
    public bool VerticalValueSupplied;
    public string? VerticalValueLower;
    public bool VerticalValueLowerSupplied;
    public string? UndefinedValue;
    public bool UndefinedValueSupplied;
    public string? UndefinedValueLower;
    public bool UndefinedValueLowerSupplied;
    public bool? RangeOfValues;
    public bool RangeOfValuesSupplied;
    public bool? StatesOfAllEdgesAroundProfile;
    public bool StatesOfAllEdgesAroundProfileSupplied;
    public bool? SidesDefined;
    public bool SidesDefinedSupplied;
    public bool? ReferenceToISO;
    public bool ReferenceToISOSupplied;
}
