using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class SurfaceTextureSymbolCommandSupport
{
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

    public static bool TryGetLeaderPoints(
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

    public static bool TryResolveAttachmentInput(
        JsonElement root,
        List<object> diagnostics,
        out SurfaceTextureSymbolAttachmentInput? attachment)
    {
        attachment =
            null;

        if (!root.TryGetProperty(
                "attachment",
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = "input.attachment", message = "attachment must be an object when supplied." });
            return false;
        }

        bool valid =
            true;

        if (!TryGetRequiredString(element, "viewName", "input.attachment.viewName", diagnostics, out string viewName))
        {
            valid =
                false;
        }

        if (!TryGetRequiredInt32(element, "curveIndex", "input.attachment.curveIndex", diagnostics, out int curveIndex) ||
            curveIndex < 1)
        {
            diagnostics.Add(new { scope = "input.attachment.curveIndex", message = "curveIndex must be a positive 1-based integer." });
            valid =
                false;
        }

        if (!TryGetRequiredString(element, "intent", "input.attachment.intent", diagnostics, out string intentText))
        {
            valid =
                false;
        }
        else if (!DimensionCommandSupport.TryParsePointIntent(
                     intentText,
                     out PointIntentEnum pointIntent))
        {
            diagnostics.Add(new { scope = "input.attachment.intent", message = "Unsupported point intent.", supported = new[] { "start", "end", "mid", "middle", "center" } });
            valid =
                false;
        }
        else if (valid)
        {
            attachment =
                new SurfaceTextureSymbolAttachmentInput(
                    viewName,
                    curveIndex,
                    intentText,
                    pointIntent);
        }

        return valid;
    }

    public static bool TryGetRequiredBoolean(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out bool value)
    {
        value =
            false;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} is required." });
            return false;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a boolean." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    public static bool TryGetRequiredStringAllowEmpty(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out string value)
    {
        return TryGetRequiredStringAllowEmpty(
            root,
            propertyName,
            $"input.{propertyName}",
            diagnostics,
            out value);
    }

    public static bool TryGetSurfaceTextureType(
        JsonElement root,
        List<object> diagnostics,
        out string requested,
        out SurfaceTextureTypeEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (!TryGetRequiredString(root, "surfaceTextureType", "input.surfaceTextureType", diagnostics, out requested))
        {
            return false;
        }

        if (TryParseSurfaceTextureType(
                requested,
                out value))
        {
            return true;
        }

        diagnostics.Add(new { scope = "input.surfaceTextureType", message = "Unsupported surfaceTextureType.", supported = SupportedSurfaceTextureTypes });
        return false;
    }

    public static bool TryGetLayDirection(
        JsonElement root,
        List<object> diagnostics,
        out string requested,
        out LayDirectionTypeEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (!TryGetRequiredString(root, "layDirection", "input.layDirection", diagnostics, out requested))
        {
            return false;
        }

        if (TryParseLayDirection(
                requested,
                out value))
        {
            return true;
        }

        diagnostics.Add(new { scope = "input.layDirection", message = "Unsupported layDirection.", supported = SupportedLayDirections });
        return false;
    }

    public static SurfaceTextureSymbol? ResolveSurfaceTextureSymbol(
        Sheet sheet,
        int symbolIndex,
        List<object> diagnostics)
    {
        SurfaceTextureSymbols symbols;

        try
        {
            symbols =
                sheet.SurfaceTextureSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SurfaceTextureSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        int count;

        try
        {
            count =
                symbols.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbols.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (symbolIndex < 1 ||
            symbolIndex > count)
        {
            diagnostics.Add(new { scope = "input.symbolIndex", message = "symbolIndex is outside the SurfaceTextureSymbols collection.", symbolIndex, count });
            return null;
        }

        try
        {
            return symbols[symbolIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbols.Item", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return null;
        }
    }

    public static DrawingCurve? ResolveDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
        List<object> diagnostics)
    {
        try
        {
            DrawingCurvesEnumerator curves =
                drawingView.DrawingCurves[Type.Missing];

            int count =
                curves.Count;

            if (curveIndex < 1 ||
                curveIndex > count)
            {
                diagnostics.Add(new { scope = "input.attachment.curveIndex", message = "curveIndex is outside the drawing view curve collection.", curveIndex, count });
                return null;
            }

            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static object FormatLeaderPoints(
        List<(double X, double Y)> points)
    {
        return points
            .Select(
                point => new
                {
                    x =
                        point.X,
                    y =
                        point.Y
                })
            .ToList();
    }

    public static object FormatNativeContent(
        SurfaceTextureSymbolContentInput input)
    {
        return new
        {
            surfaceTextureType =
                input.RequestedSurfaceTextureType,
            surfaceTextureTypeRaw =
                Convert.ToInt32(
                    input.SurfaceTextureType),
            surfaceTextureTypeEnum =
                input.SurfaceTextureType.ToString(),
            forceTail =
                input.ForceTail,
            majority =
                input.Majority,
            allAroundSymbol =
                input.AllAroundSymbol,
            maximumRoughness =
                input.MaximumRoughness,
            minimumRoughness =
                input.MinimumRoughness,
            productionMethod =
                input.ProductionMethod,
            additionalProductionMethod =
                input.AdditionalProductionMethod,
            samplingLength =
                input.SamplingLength,
            additionalSamplingLength =
                input.AdditionalSamplingLength,
            layDirection =
                input.RequestedLayDirection,
            layDirectionRaw =
                Convert.ToInt32(
                    input.LayDirection),
            layDirectionEnum =
                input.LayDirection.ToString(),
            machiningAllowance =
                input.MachiningAllowance,
            additionalRoughness =
                input.AdditionalRoughness,
            surfaceWaviness =
                input.SurfaceWaviness
        };
    }

    public static object ReadSurfaceTextureSymbolFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        SurfaceTextureSymbol symbol,
        int symbolIndex)
    {
        List<object> propertyDiagnostics =
            new();

        return new
        {
            index =
                symbolIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Type",
                    () => symbol.Type),
            objectType =
                ReadObjectTypeName(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Type",
                    () => symbol.Type),
            parentSheet =
                new
                {
                    name =
                        sheet.Name,
                    width =
                        sheet.Width,
                    height =
                        sheet.Height
                },
            position =
                ReadPoint2d(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Position",
                    () => symbol.Position),
            layer =
                ReadNamedObject(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Layer",
                    () => symbol.Layer),
            style =
                ReadNamedObject(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Style",
                    () => symbol.Style),
            leader =
                ReadLeader(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Leader",
                    () => symbol.Leader),
            surfaceTextureTypeRaw =
                ReadEnumRaw(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.SurfaceTextureType",
                    () => symbol.SurfaceTextureType),
            surfaceTextureType =
                ReadEnumName(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.SurfaceTextureType",
                    () => symbol.SurfaceTextureType),
            forceTail =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.ForceTail",
                    () => symbol.ForceTail),
            majority =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Majority",
                    () => symbol.Majority),
            allAroundSymbol =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.AllAroundSymbol",
                    () => symbol.AllAroundSymbol),
            maximumRoughness =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.MaximumRoughness",
                    () => symbol.MaximumRoughness),
            minimumRoughness =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.MinimumRoughness",
                    () => symbol.MinimumRoughness),
            additionalRoughness =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.AdditionalRoughness",
                    () => symbol.AdditionalRoughness),
            productionMethod =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.ProductionMethod",
                    () => symbol.ProductionMethod),
            additionalProductionMethod =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.AdditionalProductionMethod",
                    () => symbol.AdditionalProductionMethod),
            samplingLength =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.SamplingLength",
                    () => symbol.SamplingLength),
            additionalSamplingLength =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.AdditionalSamplingLength",
                    () => symbol.AdditionalSamplingLength),
            layDirectionRaw =
                ReadEnumRaw(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.LayDirection",
                    () => symbol.LayDirection),
            layDirection =
                ReadEnumName(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.LayDirection",
                    () => symbol.LayDirection),
            machiningAllowance =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.MachiningAllowance",
                    () => symbol.MachiningAllowance),
            surfaceWaviness =
                ReadString(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.SurfaceWaviness",
                    () => symbol.SurfaceWaviness),
            definition =
                ReadDefinition(
                    propertyDiagnostics,
                    "SurfaceTextureSymbol.Definition",
                    () => symbol.Definition),
            referenceKey =
                ReadReferenceKey(
                    drawingDocument,
                    propertyDiagnostics,
                    keyContext =>
                    {
                        Array referenceKey =
                            Array.CreateInstance(
                                typeof(byte),
                                0);

                        symbol.GetReferenceKey(
                            ref referenceKey,
                            keyContext);

                        return referenceKey;
                    }),
            propertyDiagnostics
        };
    }

    public static bool TryReadEffectivePosition(
        SurfaceTextureSymbol symbol,
        List<object> diagnostics,
        out double? x,
        out double? y)
    {
        x =
            null;
        y =
            null;

        try
        {
            Leader? leader =
                symbol.Leader;

            if (leader != null &&
                leader.HasRootNode)
            {
                Point2d rootPosition =
                    leader
                        .RootNode
                        .Position;

                x =
                    rootPosition.X;
                y =
                    rootPosition.Y;

                return true;
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbol.Leader.RootNode.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        try
        {
            Point2d symbolPosition =
                symbol.Position;

            x =
                symbolPosition.X;
            y =
                symbolPosition.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbol.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool CoordinatesMatch(
        double? actualX,
        double? actualY,
        double requestedX,
        double requestedY)
    {
        const double tolerance =
            0.000001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - requestedX) <= tolerance &&
               Math.Abs(actualY.Value - requestedY) <= tolerance;
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
                out JsonElement element))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a string." });
            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must not be empty." });
            return false;
        }

        return true;
    }

    private static bool TryGetRequiredStringAllowEmpty(
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
                out JsonElement element))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a string." });
            return false;
        }

        value =
            element.GetString()
            ?? string.Empty;

        return true;
    }

    private static bool TryGetRequiredInt32(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out int value)
    {
        value =
            0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required." });
            return false;
        }

        if (!element.TryGetInt32(
                out value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be an integer." });
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
            0.0;

        if (!element.TryGetProperty(
                propertyName,
                out JsonElement valueElement) ||
            !valueElement.TryGetDouble(
                out value) ||
            double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static bool TryParseSurfaceTextureType(
        string value,
        out SurfaceTextureTypeEnum surfaceTextureType)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "basic":
                surfaceTextureType =
                    SurfaceTextureTypeEnum.kBasicSurfaceType;
                return true;

            case "material_removal_required":
                surfaceTextureType =
                    SurfaceTextureTypeEnum.kMaterialRemovalRequiredSurfaceType;
                return true;

            case "material_removal_prohibited":
                surfaceTextureType =
                    SurfaceTextureTypeEnum.kMaterialRemovalProhibitedSurfaceType;
                return true;

            default:
                surfaceTextureType =
                    default;
                return false;
        }
    }

    private static bool TryParseLayDirection(
        string value,
        out LayDirectionTypeEnum layDirection)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "parallel_to_plane_of_projection":
                layDirection =
                    LayDirectionTypeEnum.kParallelToPlaneOfProjection;
                return true;

            case "perpendicular_to_plane_of_projection":
                layDirection =
                    LayDirectionTypeEnum.kPerpendicularToPlaneOfProjection;
                return true;

            case "angular_in_both_directions":
                layDirection =
                    LayDirectionTypeEnum.kAngularInBothDirections;
                return true;

            case "multidirectional":
                layDirection =
                    LayDirectionTypeEnum.kMultidirectional;
                return true;

            case "circular_relative_to_center":
                layDirection =
                    LayDirectionTypeEnum.kCircularRelativeToCenter;
                return true;

            case "radial_relative_to_center":
                layDirection =
                    LayDirectionTypeEnum.kRadialRelativeToCenter;
                return true;

            case "particulate_nondirectional":
                layDirection =
                    LayDirectionTypeEnum.kParticulateNondirectional;
                return true;

            default:
                layDirection =
                    default;
                return false;
        }
    }

    private static object? ReadPoint2d(
        List<object> diagnostics,
        string scope,
        Func<Point2d> reader)
    {
        try
        {
            Point2d point =
                reader();

            return new
            {
                x =
                    point.X,
                y =
                    point.Y
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadNamedObject<T>(
        List<object> diagnostics,
        string scope,
        Func<T> reader)
    {
        try
        {
            dynamic value =
                reader()!;

            return new
            {
                name =
                    SafeReadDynamicString(
                        value,
                        "Name"),
                internalName =
                    SafeReadDynamicString(
                        value,
                        "InternalName"),
                objectType =
                    value.GetType().FullName
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadLeader(
        List<object> diagnostics,
        string scope,
        Func<Leader> reader)
    {
        try
        {
            Leader leader =
                reader();

            return new
            {
                objectType =
                    SafeReadDynamicString(
                        leader,
                        "Type"),
                hasRootNode =
                    SafeReadDynamicBoolean(
                        leader,
                        "HasRootNode"),
                rootNodePosition =
                    ReadRootNodePosition(
                        diagnostics,
                        $"{scope}.RootNode",
                        leader),
                arrowheadType =
                    SafeReadDynamicString(
                        leader,
                        "ArrowheadType")
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadRootNodePosition(
        List<object> diagnostics,
        string scope,
        Leader leader)
    {
        try
        {
            if (!leader.HasRootNode)
            {
                return null;
            }

            Point2d position =
                leader
                    .RootNode
                    .Position;

            return new
            {
                x =
                    position.X,
                y =
                    position.Y
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadDefinition(
        List<object> diagnostics,
        string scope,
        Func<SurfaceTextureSymbolDefinition> reader)
    {
        try
        {
            SurfaceTextureSymbolDefinition definition =
                reader();

            return new
            {
                apiType =
                    definition
                        .GetType()
                        .FullName,
                objectTypeRaw =
                    ReadObjectTypeRaw(
                        diagnostics,
                        $"{scope}.Type",
                        () => definition.Type),
                objectType =
                    ReadObjectTypeName(
                        diagnostics,
                        $"{scope}.Type",
                        () => definition.Type),
                surfaceTextureTypeRaw =
                    ReadEnumRaw(
                        diagnostics,
                        $"{scope}.SurfaceTextureType",
                        () => definition.SurfaceTextureType),
                surfaceTextureType =
                    ReadEnumName(
                        diagnostics,
                        $"{scope}.SurfaceTextureType",
                        () => definition.SurfaceTextureType),
                allAroundSymbol =
                    ReadNullableBoolean(
                        diagnostics,
                        $"{scope}.AllAroundSymbol",
                        () => definition.AllAroundSymbol),
                isForceTailShown =
                    ReadNullableBoolean(
                        diagnostics,
                        $"{scope}.IsForceTailShown",
                        () => definition.IsForceTailShown),
                isMajority =
                    ReadNullableBoolean(
                        diagnostics,
                        $"{scope}.IsMajority",
                        () => definition.IsMajority)
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? ReadString(
        List<object> diagnostics,
        string scope,
        Func<string> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool? ReadNullableBoolean(
        List<object> diagnostics,
        string scope,
        Func<bool> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? ReadEnumRaw<TEnum>(
        List<object> diagnostics,
        string scope,
        Func<TEnum> reader)
        where TEnum : struct
    {
        try
        {
            return Convert
                .ToInt32(
                    reader())
                .ToString();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? ReadEnumName<TEnum>(
        List<object> diagnostics,
        string scope,
        Func<TEnum> reader)
        where TEnum : struct
    {
        try
        {
            TEnum value =
                reader();

            return Enum.GetName(
                       typeof(TEnum),
                       value)
                   ?? value.ToString();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? ReadObjectTypeRaw(
        List<object> diagnostics,
        string scope,
        Func<ObjectTypeEnum> reader)
    {
        return ReadEnumRaw(
            diagnostics,
            scope,
            reader);
    }

    private static string? ReadObjectTypeName(
        List<object> diagnostics,
        string scope,
        Func<ObjectTypeEnum> reader)
    {
        return ReadEnumName(
            diagnostics,
            scope,
            reader);
    }

    private static string? SafeReadDynamicString(
        dynamic value,
        string propertyName)
    {
        try
        {
            object? property =
                propertyName switch
                {
                    "Name" =>
                        value.Name,
                    "InternalName" =>
                        value.InternalName,
                    "Type" =>
                        value.Type,
                    "ArrowheadType" =>
                        value.ArrowheadType,
                    _ =>
                        null
                };

            return property?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool? SafeReadDynamicBoolean(
        dynamic value,
        string propertyName)
    {
        try
        {
            return propertyName switch
            {
                "HasRootNode" =>
                    value.HasRootNode,
                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadReferenceKey(
        DrawingDocument drawingDocument,
        List<object> diagnostics,
        Func<int, Array> getReferenceKey)
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
                getReferenceKey(
                    keyContext);

            string keyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SurfaceTextureSymbol.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
                    diagnostics.Add(new { scope = "ReferenceKeyManager.ReleaseKeyContext", message = exception.Message, exceptionType = exception.GetType().FullName });
                }
            }
        }
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

    private static readonly string[] SupportedSurfaceTextureTypes =
    {
        "basic",
        "material_removal_required",
        "material_removal_prohibited"
    };

    private static readonly string[] SupportedLayDirections =
    {
        "parallel_to_plane_of_projection",
        "perpendicular_to_plane_of_projection",
        "angular_in_both_directions",
        "multidirectional",
        "circular_relative_to_center",
        "radial_relative_to_center",
        "particulate_nondirectional"
    };
}

internal sealed record SurfaceTextureSymbolAttachmentInput(
    string ViewName,
    int CurveIndex,
    string IntentText,
    PointIntentEnum PointIntent);

internal sealed record SurfaceTextureSymbolContentInput(
    string RequestedSurfaceTextureType,
    SurfaceTextureTypeEnum SurfaceTextureType,
    bool ForceTail,
    bool Majority,
    bool AllAroundSymbol,
    string MaximumRoughness,
    string MinimumRoughness,
    string ProductionMethod,
    string AdditionalProductionMethod,
    string SamplingLength,
    string AdditionalSamplingLength,
    string RequestedLayDirection,
    LayDirectionTypeEnum LayDirection,
    string MachiningAllowance,
    string AdditionalRoughness,
    string SurfaceWaviness);
