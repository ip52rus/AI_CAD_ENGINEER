using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class WeldingSymbolCommandSupport
{
    private static readonly string[] SupportedWeldSymbolTypes =
    {
        "none",
        "fillet",
        "plug",
        "slot",
        "stud",
        "spot_or_projection",
        "seam",
        "back_or_backing_or_bead",
        "surfacing_or_cladding",
        "square_groove",
        "v_groove",
        "bevel_groove",
        "u_groove",
        "j_groove"
    };

    private static readonly string[] SupportedIdentificationLinePlacements =
    {
        "no_identification_line",
        "above",
        "below"
    };

    private static readonly string[] SupportedStaggerTypes =
    {
        "none",
        "mirror",
        "ansi_move",
        "iso_move"
    };

    private static readonly string[] SupportedContourTypes =
    {
        "none",
        "flush_or_flat",
        "convex",
        "concave",
        "toes_blended_smooth",
        "flush_finished"
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
        out WeldingSymbolAttachmentInput? attachment)
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
                new WeldingSymbolAttachmentInput(
                    viewName,
                    curveIndex,
                    intentText,
                    pointIntent);
        }

        return valid;
    }

    public static bool TryGetDefinitionsInput(
        JsonElement root,
        List<object> diagnostics,
        out List<WeldingSymbolDefinitionInput> definitions)
    {
        definitions =
            new List<WeldingSymbolDefinitionInput>();

        if (!root.TryGetProperty(
                "definitions",
                out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.definitions", message = "definitions is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.definitions", message = "definitions must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement definitionElement in element.EnumerateArray())
        {
            if (definitionElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.definitions[{index}]", message = "Definition must be an object." });
                return false;
            }

            if (!TryReadDefinitionInput(
                    definitionElement,
                    $"input.definitions[{index}]",
                    diagnostics,
                    out WeldingSymbolDefinitionInput definition))
            {
                return false;
            }

            definitions.Add(
                definition);

            index++;
        }

        if (definitions.Count == 0)
        {
            diagnostics.Add(new { scope = "input.definitions", message = "definitions must contain at least one item." });
            return false;
        }

        return true;
    }

    public static DrawingCurve? ResolveDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
        List<object> diagnostics)
    {
        DrawingCurvesEnumerator curves;

        try
        {
            curves =
                drawingView.DrawingCurves;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        int count;

        try
        {
            count =
                curves.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingCurves.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (curveIndex < 1 ||
            curveIndex > count)
        {
            diagnostics.Add(new { scope = "input.attachment.curveIndex", message = "curveIndex is outside DrawingCurves range.", curveIndex, count });
            return null;
        }

        try
        {
            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingCurves.Item", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex });
            return null;
        }
    }

    public static DrawingWeldingSymbol? ResolveWeldingSymbol(
        Sheet sheet,
        int symbolIndex,
        List<object> diagnostics)
    {
        DrawingWeldingSymbols symbols;

        try
        {
            symbols =
                sheet.WeldingSymbols;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.WeldingSymbols", message = exception.Message, exceptionType = exception.GetType().FullName });
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
            diagnostics.Add(new { scope = "DrawingWeldingSymbols.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (symbolIndex < 1 ||
            symbolIndex > count)
        {
            diagnostics.Add(new { scope = "input.symbolIndex", message = "symbolIndex is outside WeldingSymbols range.", symbolIndex, count });
            return null;
        }

        try
        {
            return symbols[symbolIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbols.Item", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
            return null;
        }
    }

    public static void ApplyDefinitionInput(
        DrawingWeldingSymbolDefinition target,
        WeldingSymbolDefinitionInput input)
    {
        if (input.AllAroundSymbol.HasValue)
        {
            target.AllAroundSymbol =
                input.AllAroundSymbol.Value;
        }

        if (input.ClosedNoteTail.HasValue)
        {
            target.ClosedNoteTail =
                input.ClosedNoteTail.Value;
        }

        if (input.FieldWeldingSymbol.HasValue)
        {
            target.FieldWeldingSymbol =
                input.FieldWeldingSymbol.Value;
        }

        if (input.IdentificationLinePlacement.HasValue)
        {
            target.IdentificationLinePlacement =
                input.IdentificationLinePlacement.Value;
        }

        if (input.StaggerType.HasValue)
        {
            target.StaggerType =
                input.StaggerType.Value;
        }

        if (input.SwapArrowAndSymbols.HasValue)
        {
            target.SwapArrowAndSymbols =
                input.SwapArrowAndSymbols.Value;
        }

        if (input.TailNote != null)
        {
            target.TailNote =
                input.TailNote;
        }

        if (input.UseSpacer.HasValue)
        {
            target.UseSpacer =
                input.UseSpacer.Value;
        }

        if (input.WeldSymbolOne != null)
        {
            ApplyWeldSymbolInput(
                target.WeldSymbolOne,
                input.WeldSymbolOne);
        }

        if (input.WeldSymbolTwo != null)
        {
            ApplyWeldSymbolInput(
                target.WeldSymbolTwo,
                input.WeldSymbolTwo);
        }
    }

    public static object FormatLeaderPoints(
        List<(double X, double Y)> points)
    {
        return points
            .Select(point => new
            {
                x =
                    point.X,
                y =
                    point.Y
            })
            .ToList();
    }

    public static object FormatAttachmentSelector(
        WeldingSymbolAttachmentInput? attachment)
    {
        if (attachment == null)
        {
            return new
            {
                supplied =
                    false
            };
        }

        return new
        {
            supplied =
                true,
            viewName =
                attachment.ViewName,
            curveIndex =
                attachment.CurveIndex,
            intent =
                attachment.IntentText,
            intentRaw =
                attachment.PointIntent.ToString()
        };
    }

    public static object FormatDefinitionsInput(
        List<WeldingSymbolDefinitionInput> definitions)
    {
        return definitions
            .Select(definition => new
            {
                definition.AllAroundSymbol,
                definition.ClosedNoteTail,
                definition.FieldWeldingSymbol,
                identificationLinePlacement =
                    definition.RequestedIdentificationLinePlacement,
                definition.StaggerType,
                staggerType =
                    definition.RequestedStaggerType,
                definition.SwapArrowAndSymbols,
                definition.TailNote,
                definition.UseSpacer,
                weldSymbolOne =
                    FormatWeldSymbolInput(
                        definition.WeldSymbolOne),
                weldSymbolTwo =
                    FormatWeldSymbolInput(
                        definition.WeldSymbolTwo)
            })
            .ToList();
    }

    public static object ReadWeldingSymbolFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingWeldingSymbol symbol,
        int symbolIndex)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => symbol.Position));

        return new
        {
            index =
                symbolIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => symbol.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
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
            position,
            origin =
                position,
            layer =
                ReadNamedObject(
                    diagnostics,
                    "Layer",
                    () => symbol.Layer),
            style =
                ReadNamedObject(
                    diagnostics,
                    "Style",
                    () => symbol.Style),
            leader =
                ReadLeader(
                    diagnostics,
                    "Leader",
                    () => symbol.Leader),
            retrieved =
                ReadNullableBoolean(
                    diagnostics,
                    "Retrieved",
                    () => symbol.Retrieved),
            definitions =
                ReadDefinitions(
                    symbol,
                    diagnostics),
            referenceKey =
                ReadReferenceKey(
                    drawingDocument,
                    diagnostics,
                    keyContext =>
                    {
                        Array referenceKeyArray =
                            Array.CreateInstance(
                                typeof(byte),
                                0);

                        symbol.GetReferenceKey(
                            ref referenceKeyArray,
                            keyContext);

                        return referenceKeyArray;
                    }),
            selectorSnapshot =
                new
                {
                    index =
                        symbolIndex,
                    indexIsStable =
                        false,
                    type =
                        "welding_symbol",
                    position
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    public static object ReadCreationInputFacts(
        ObjectCollection leaderPoints,
        DrawingWeldingSymbolDefinitions definitions,
        object styleArgument)
    {
        List<object> diagnostics =
            new();

        List<object> leaderItems =
            new();

        int? leaderPointCount =
            ReadNullableInt32(
                diagnostics,
                "LeaderPoints.Count",
                () => leaderPoints.Count);

        if (leaderPointCount.HasValue)
        {
            for (int index = 1;
                 index <= leaderPointCount.Value;
                 index++)
            {
                object? item =
                    ReadProperty(
                        diagnostics,
                        $"LeaderPoints.Item[{index}]",
                        () => leaderPoints[index]);

                leaderItems.Add(new
                {
                    index,
                    runtimeType =
                        item?.GetType().FullName,
                    objectTypeRaw =
                        item == null
                            ? null
                            : ReadString(
                                ReadReflectionProperty(
                                    item,
                                    diagnostics,
                                    $"LeaderPoints.Item[{index}].Type",
                                    "Type"))
                });
            }
        }

        List<object> definitionItems =
            new();

        int? definitionCount =
            ReadNullableInt32(
                diagnostics,
                "Definitions.Count",
                () => definitions.Count);

        if (definitionCount.HasValue)
        {
            for (int index = 1;
                 index <= definitionCount.Value;
                 index++)
            {
                DrawingWeldingSymbolDefinition? definition =
                    ReadComObject(
                        diagnostics,
                        $"Definitions.Item[{index}]",
                        () => definitions[index]);

                if (definition == null)
                {
                    continue;
                }

                definitionItems.Add(
                    ReadDefinition(
                        definition,
                        index));
            }
        }

        return new
        {
            scope =
                "DrawingWeldingSymbols.Add.PreCall",
            leaderPointCount,
            leaderItems,
            definitionCount,
            definitions =
                definitionItems,
            styleArgument =
                styleArgument == Type.Missing
                    ? "Type.Missing"
                    : styleArgument.GetType().FullName,
            diagnostics
        };
    }

    public static bool TryReadEffectivePosition(
        DrawingWeldingSymbol symbol,
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
                Point2d point =
                    leader.RootNode.Position;

                x =
                    point.X;
                y =
                    point.Y;

                return true;
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbol.Leader.RootNode.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        try
        {
            Point2d point =
                symbol.Position;

            x =
                point.X;
            y =
                point.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingWeldingSymbol.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool CoordinatesMatch(
        double? actualX,
        double? actualY,
        double expectedX,
        double expectedY)
    {
        const double tolerance =
            0.0001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - expectedX) <= tolerance &&
               Math.Abs(actualY.Value - expectedY) <= tolerance;
    }

    public static bool TryGetSheetName(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName)
    {
        return TryGetRequiredString(
            root,
            "sheetName",
            "input.sheetName",
            diagnostics,
            out sheetName);
    }

    public static bool TryGetSymbolIndex(
        JsonElement root,
        List<object> diagnostics,
        out int symbolIndex)
    {
        if (!TryGetRequiredInt32(
                root,
                "symbolIndex",
                "input.symbolIndex",
                diagnostics,
                out symbolIndex))
        {
            return false;
        }

        if (symbolIndex < 1)
        {
            diagnostics.Add(new { scope = "input.symbolIndex", message = "symbolIndex must be a positive 1-based integer." });
            return false;
        }

        return true;
    }

    public static bool TryGetPosition(
        JsonElement root,
        List<object> diagnostics,
        out double x,
        out double y)
    {
        bool valid =
            true;

        if (!TryGetFiniteDouble(root, "x", "input.x", diagnostics, out x))
        {
            valid =
                false;
        }

        if (!TryGetFiniteDouble(root, "y", "input.y", diagnostics, out y))
        {
            valid =
                false;
        }

        return valid;
    }

    private static bool TryReadDefinitionInput(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out WeldingSymbolDefinitionInput input)
    {
        input =
            new WeldingSymbolDefinitionInput();

        bool valid =
            true;

        if (TryGetOptionalBoolean(element, "allAroundSymbol", $"{scope}.allAroundSymbol", diagnostics, out bool? allAroundSymbol))
        {
            input.AllAroundSymbol =
                allAroundSymbol;
        }
        else
        {
            valid =
                false;
        }

        if (TryGetOptionalBoolean(element, "closedNoteTail", $"{scope}.closedNoteTail", diagnostics, out bool? closedNoteTail))
        {
            input.ClosedNoteTail =
                closedNoteTail;
        }
        else
        {
            valid =
                false;
        }

        if (TryGetOptionalBoolean(element, "fieldWeldingSymbol", $"{scope}.fieldWeldingSymbol", diagnostics, out bool? fieldWeldingSymbol))
        {
            input.FieldWeldingSymbol =
                fieldWeldingSymbol;
        }
        else
        {
            valid =
                false;
        }

        if (element.TryGetProperty("identificationLinePlacement", out JsonElement identificationLinePlacementElement))
        {
            if (!TryReadIdentificationLinePlacement(
                    identificationLinePlacementElement,
                    $"{scope}.identificationLinePlacement",
                    diagnostics,
                    out string requested,
                    out IdentificationLinePlacementEnum value))
            {
                valid =
                    false;
            }
            else
            {
                input.RequestedIdentificationLinePlacement =
                    requested;
                input.IdentificationLinePlacement =
                    value;
            }
        }

        if (element.TryGetProperty("staggerType", out JsonElement staggerTypeElement))
        {
            if (!TryReadStaggerType(
                    staggerTypeElement,
                    $"{scope}.staggerType",
                    diagnostics,
                    out string requested,
                    out StaggerTypeEnum value))
            {
                valid =
                    false;
            }
            else
            {
                input.RequestedStaggerType =
                    requested;
                input.StaggerType =
                    value;
            }
        }

        if (TryGetOptionalBoolean(element, "swapArrowAndSymbols", $"{scope}.swapArrowAndSymbols", diagnostics, out bool? swapArrowAndSymbols))
        {
            input.SwapArrowAndSymbols =
                swapArrowAndSymbols;
        }
        else
        {
            valid =
                false;
        }

        if (TryGetOptionalStringAllowEmpty(element, "tailNote", $"{scope}.tailNote", diagnostics, out string? tailNote))
        {
            input.TailNote =
                tailNote;
        }
        else
        {
            valid =
                false;
        }

        if (TryGetOptionalBoolean(element, "useSpacer", $"{scope}.useSpacer", diagnostics, out bool? useSpacer))
        {
            input.UseSpacer =
                useSpacer;
        }
        else
        {
            valid =
                false;
        }

        if (element.TryGetProperty("weldSymbolOne", out JsonElement weldSymbolOneElement))
        {
            if (!TryReadWeldSymbolInput(
                    weldSymbolOneElement,
                    $"{scope}.weldSymbolOne",
                    diagnostics,
                    out WeldingSymbolInput weldSymbolOne))
            {
                valid =
                    false;
            }
            else
            {
                input.WeldSymbolOne =
                    weldSymbolOne;
            }
        }

        if (element.TryGetProperty("weldSymbolTwo", out JsonElement weldSymbolTwoElement))
        {
            if (!TryReadWeldSymbolInput(
                    weldSymbolTwoElement,
                    $"{scope}.weldSymbolTwo",
                    diagnostics,
                    out WeldingSymbolInput weldSymbolTwo))
            {
                valid =
                    false;
            }
            else
            {
                input.WeldSymbolTwo =
                    weldSymbolTwo;
            }
        }

        return valid;
    }

    private static bool TryReadWeldSymbolInput(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out WeldingSymbolInput input)
    {
        input =
            new WeldingSymbolInput();

        if (element.ValueKind !=
            JsonValueKind.Object)
        {
            diagnostics.Add(new { scope, message = "Weld symbol must be an object." });
            return false;
        }

        bool valid =
            true;

        if (element.TryGetProperty("weldSymbolType", out JsonElement weldSymbolTypeElement))
        {
            if (!TryReadWeldSymbolType(
                    weldSymbolTypeElement,
                    $"{scope}.weldSymbolType",
                    diagnostics,
                    out string requested,
                    out WeldSymbolTypeEnum value))
            {
                valid =
                    false;
            }
            else
            {
                input.RequestedWeldSymbolType =
                    requested;
                input.WeldSymbolType =
                    value;
            }
        }

        if (element.TryGetProperty("contour", out JsonElement contourElement))
        {
            if (!TryReadContour(
                    contourElement,
                    $"{scope}.contour",
                    diagnostics,
                    out string requested,
                    out ContourSymbolTypeEnum value))
            {
                valid =
                    false;
            }
            else
            {
                input.RequestedContour =
                    requested;
                input.Contour =
                    value;
            }
        }

        valid =
            TryReadOptionalWeldString(element, "sizeOrStrength", scope, diagnostics, out string? sizeOrStrength) &&
            valid;
        input.SizeOrStrength =
            sizeOrStrength;

        valid =
            TryReadOptionalWeldString(element, "length", scope, diagnostics, out string? length) &&
            valid;
        input.Length =
            length;

        valid =
            TryReadOptionalWeldString(element, "pitch", scope, diagnostics, out string? pitch) &&
            valid;
        input.Pitch =
            pitch;

        valid =
            TryReadOptionalWeldString(element, "method", scope, diagnostics, out string? method) &&
            valid;
        input.Method =
            method;

        valid =
            TryReadOptionalWeldString(element, "prefix", scope, diagnostics, out string? prefix) &&
            valid;
        input.Prefix =
            prefix;

        valid =
            TryReadOptionalWeldString(element, "root", scope, diagnostics, out string? root) &&
            valid;
        input.Root =
            root;

        valid =
            TryReadOptionalWeldString(element, "gap", scope, diagnostics, out string? gap) &&
            valid;
        input.Gap =
            gap;

        valid =
            TryReadOptionalWeldString(element, "depth", scope, diagnostics, out string? depth) &&
            valid;
        input.Depth =
            depth;

        valid =
            TryReadOptionalWeldString(element, "leg1", scope, diagnostics, out string? leg1) &&
            valid;
        input.Leg1 =
            leg1;

        valid =
            TryReadOptionalWeldString(element, "leg2", scope, diagnostics, out string? leg2) &&
            valid;
        input.Leg2 =
            leg2;

        valid =
            TryReadOptionalWeldString(element, "test", scope, diagnostics, out string? test) &&
            valid;
        input.Test =
            test;

        if (TryGetOptionalBoolean(element, "enableSecondaryFilletWeld", $"{scope}.enableSecondaryFilletWeld", diagnostics, out bool? enableSecondaryFilletWeld))
        {
            input.EnableSecondaryFilletWeld =
                enableSecondaryFilletWeld;
        }
        else
        {
            valid =
                false;
        }

        return valid;
    }

    private static void ApplyWeldSymbolInput(
        WeldSymbol target,
        WeldingSymbolInput input)
    {
        if (input.WeldSymbolType.HasValue)
        {
            target.WeldSymbolType =
                (int)input.WeldSymbolType.Value;
        }

        if (input.SizeOrStrength != null)
        {
            target.SizeOrStrength =
                input.SizeOrStrength;
        }

        if (input.Length != null)
        {
            target.Length =
                input.Length;
        }

        if (input.Pitch != null)
        {
            target.Pitch =
                input.Pitch;
        }

        if (input.Contour.HasValue)
        {
            target.Contour =
                input.Contour.Value;
        }

        if (input.Method != null)
        {
            target.Method =
                input.Method;
        }

        if (input.Prefix != null)
        {
            target.Prefix =
                input.Prefix;
        }

        if (input.Root != null)
        {
            target.Root =
                input.Root;
        }

        if (input.Gap != null)
        {
            target.Gap =
                input.Gap;
        }

        if (input.Depth != null)
        {
            target.Depth =
                input.Depth;
        }

        if (input.Leg1 != null)
        {
            target.Leg1 =
                input.Leg1;
        }

        if (input.Leg2 != null)
        {
            target.Leg2 =
                input.Leg2;
        }

        if (input.Test != null)
        {
            target.Test =
                input.Test;
        }

        if (input.EnableSecondaryFilletWeld.HasValue)
        {
            target.EnableSecondaryFilletWeld =
                input.EnableSecondaryFilletWeld.Value;
        }
    }

    private static object? FormatWeldSymbolInput(
        WeldingSymbolInput? input)
    {
        if (input == null)
        {
            return null;
        }

        return new
        {
            weldSymbolType =
                input.RequestedWeldSymbolType,
            input.SizeOrStrength,
            input.Length,
            input.Pitch,
            contour =
                input.RequestedContour,
            input.Method,
            input.Prefix,
            input.Root,
            input.Gap,
            input.Depth,
            input.Leg1,
            input.Leg2,
            input.Test,
            input.EnableSecondaryFilletWeld
        };
    }

    private static bool TryReadOptionalWeldString(
        JsonElement element,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out string? value)
    {
        if (!TryGetOptionalStringAllowEmpty(
                element,
                propertyName,
                $"{scope}.{propertyName}",
                diagnostics,
                out value))
        {
            return false;
        }

        return true;
    }

    private static bool TryReadWeldSymbolType(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out string requested,
        out WeldSymbolTypeEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = "weldSymbolType must be a string." });
            return false;
        }

        requested =
            element.GetString()?.Trim() ??
            string.Empty;

        return TryParseWeldSymbolType(
            requested,
            diagnostics,
            scope,
            out value);
    }

    private static bool TryReadIdentificationLinePlacement(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out string requested,
        out IdentificationLinePlacementEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = "identificationLinePlacement must be a string." });
            return false;
        }

        requested =
            element.GetString()?.Trim() ??
            string.Empty;

        switch (requested)
        {
            case "no_identification_line":
                value =
                    IdentificationLinePlacementEnum.kNoIdentificationLine;
                return true;
            case "above":
                value =
                    IdentificationLinePlacementEnum.kIdentificationLineAbove;
                return true;
            case "below":
                value =
                    IdentificationLinePlacementEnum.kIdentificationLineBelow;
                return true;
            default:
                diagnostics.Add(new { scope, message = "Unsupported identificationLinePlacement.", supported = SupportedIdentificationLinePlacements });
                return false;
        }
    }

    private static bool TryReadStaggerType(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out string requested,
        out StaggerTypeEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = "staggerType must be a string." });
            return false;
        }

        requested =
            element.GetString()?.Trim() ??
            string.Empty;

        switch (requested)
        {
            case "none":
                value =
                    StaggerTypeEnum.kNoWeldStagger;
                return true;
            case "mirror":
                value =
                    StaggerTypeEnum.kMirrorWeldStagger;
                return true;
            case "ansi_move":
                value =
                    StaggerTypeEnum.kANSIMoveWeldStagger;
                return true;
            case "iso_move":
                value =
                    StaggerTypeEnum.kISOMoveWeldStagger;
                return true;
            default:
                diagnostics.Add(new { scope, message = "Unsupported staggerType.", supported = SupportedStaggerTypes });
                return false;
        }
    }

    private static bool TryReadContour(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out string requested,
        out ContourSymbolTypeEnum value)
    {
        requested =
            string.Empty;
        value =
            default;

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = "contour must be a string." });
            return false;
        }

        requested =
            element.GetString()?.Trim() ??
            string.Empty;

        switch (requested)
        {
            case "none":
                value =
                    ContourSymbolTypeEnum.kNoContourSymbolType;
                return true;
            case "flush_or_flat":
                value =
                    ContourSymbolTypeEnum.kFlushOrFlatContourSymbolType;
                return true;
            case "convex":
                value =
                    ContourSymbolTypeEnum.kConvexContourSymbolType;
                return true;
            case "concave":
                value =
                    ContourSymbolTypeEnum.kConcaveContourSymbolType;
                return true;
            case "toes_blended_smooth":
                value =
                    ContourSymbolTypeEnum.kToesShallBeBlendedSmoothContourSymbolType;
                return true;
            case "flush_finished":
                value =
                    ContourSymbolTypeEnum.kFlushFinishedContourSymbolType;
                return true;
            default:
                diagnostics.Add(new { scope, message = "Unsupported contour.", supported = SupportedContourTypes });
                return false;
        }
    }

    private static bool TryParseWeldSymbolType(
        string text,
        List<object> diagnostics,
        string scope,
        out WeldSymbolTypeEnum value)
    {
        switch (text)
        {
            case "none":
                value =
                    WeldSymbolTypeEnum.kNoneWeldSymbolType;
                return true;
            case "fillet":
                value =
                    WeldSymbolTypeEnum.kFilletWeldSymbolType;
                return true;
            case "plug":
                value =
                    WeldSymbolTypeEnum.kPlugWeldSymbolType;
                return true;
            case "slot":
                value =
                    WeldSymbolTypeEnum.kSlotWeldSymbolType;
                return true;
            case "stud":
                value =
                    WeldSymbolTypeEnum.kStudWeldSymbolType;
                return true;
            case "spot_or_projection":
                value =
                    WeldSymbolTypeEnum.kSpotOrProjectionWeldSymbolType;
                return true;
            case "seam":
                value =
                    WeldSymbolTypeEnum.kSeamWeldSymbolType;
                return true;
            case "back_or_backing_or_bead":
                value =
                    WeldSymbolTypeEnum.kBackOrBackingOrBeadWeldSymbolType;
                return true;
            case "surfacing_or_cladding":
                value =
                    WeldSymbolTypeEnum.kSurfacingOrCladdingWeldSymbolType;
                return true;
            case "square_groove":
                value =
                    WeldSymbolTypeEnum.kSquareGrooveOrSquareButtWeldSymbolType;
                return true;
            case "v_groove":
                value =
                    WeldSymbolTypeEnum.kVGrooveOrButtWeldSymbolType;
                return true;
            case "bevel_groove":
                value =
                    WeldSymbolTypeEnum.kBevelGrooveOrButtWeldSymbolType;
                return true;
            case "u_groove":
                value =
                    WeldSymbolTypeEnum.kUGrooveOrButtWeldSymbolType;
                return true;
            case "j_groove":
                value =
                    WeldSymbolTypeEnum.kJGrooveOrButtWeldSymbolType;
                return true;
            default:
                value =
                    default;
                diagnostics.Add(new { scope, message = "Unsupported weldSymbolType.", supported = SupportedWeldSymbolTypes });
                return false;
        }
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out bool? value)
    {
        value =
            null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a boolean when supplied." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetOptionalStringAllowEmpty(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out string? value)
    {
        value =
            null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a string when supplied." });
            return false;
        }

        value =
            element.GetString() ??
            string.Empty;

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
            diagnostics.Add(new { scope, message = $"{propertyName} is required and must be a string." });
            return false;
        }

        value =
            element.GetString()?.Trim() ??
            string.Empty;

        if (value.Length == 0)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must not be empty." });
            return false;
        }

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
                out JsonElement element) ||
            element.ValueKind !=
                JsonValueKind.Number ||
            !element.TryGetInt32(
                out value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required and must be an integer." });
            return false;
        }

        return true;
    }

    private static bool TryGetFiniteDouble(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out double value)
    {
        value =
            0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            element.ValueKind !=
                JsonValueKind.Number ||
            !element.TryGetDouble(
                out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required and must be a finite number." });
            return false;
        }

        return true;
    }

    private static List<object> ReadDefinitions(
        DrawingWeldingSymbol symbol,
        List<object> parentDiagnostics)
    {
        List<object> items =
            new();

        DrawingWeldingSymbolDefinitions? definitions =
            ReadComObject(
                parentDiagnostics,
                "Definitions",
                () => symbol.Definitions);

        if (definitions == null)
        {
            return items;
        }

        int? count =
            ReadNullableInt32(
                parentDiagnostics,
                "Definitions.Count",
                () => definitions.Count);

        if (!count.HasValue)
        {
            return items;
        }

        for (int index = 1;
             index <= count.Value;
             index++)
        {
            DrawingWeldingSymbolDefinition? definition =
                ReadComObject(
                    parentDiagnostics,
                    $"Definitions.Item[{index}]",
                    () => definitions[index]);

            if (definition == null)
            {
                continue;
            }

            items.Add(
                ReadDefinition(
                    definition,
                    index));
        }

        return items;
    }

    private static object ReadDefinition(
        DrawingWeldingSymbolDefinition definition,
        int index)
    {
        List<object> diagnostics =
            new();

        IdentificationLinePlacementEnum? identificationLinePlacement =
            ReadNullableEnum(
                diagnostics,
                "IdentificationLinePlacement",
                () => definition.IdentificationLinePlacement);

        StaggerTypeEnum? staggerType =
            ReadNullableEnum(
                diagnostics,
                "StaggerType",
                () => definition.StaggerType);

        return new
        {
            index,
            allAroundSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "AllAroundSymbol",
                    () => definition.AllAroundSymbol),
            closedNoteTail =
                ReadNullableBoolean(
                    diagnostics,
                    "ClosedNoteTail",
                    () => definition.ClosedNoteTail),
            fieldWeldingSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "FieldWeldingSymbol",
                    () => definition.FieldWeldingSymbol),
            identificationLinePlacementRaw =
                ReadEnumRaw(
                    identificationLinePlacement),
            identificationLinePlacement =
                ReadEnumName(
                    identificationLinePlacement),
            staggerTypeRaw =
                ReadEnumRaw(
                    staggerType),
            staggerType =
                ReadEnumName(
                    staggerType),
            swapArrowAndSymbols =
                ReadNullableBoolean(
                    diagnostics,
                    "SwapArrowAndSymbols",
                    () => definition.SwapArrowAndSymbols),
            tailNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "TailNote",
                        () => definition.TailNote)),
            useSpacer =
                ReadNullableBoolean(
                    diagnostics,
                    "UseSpacer",
                    () => definition.UseSpacer),
            weldSymbolOne =
                ReadWeldSymbol(
                    diagnostics,
                    "WeldSymbolOne",
                    () => definition.WeldSymbolOne),
            weldSymbolTwo =
                ReadWeldSymbol(
                    diagnostics,
                    "WeldSymbolTwo",
                    () => definition.WeldSymbolTwo),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object? ReadWeldSymbol(
        List<object> diagnostics,
        string propertyName,
        Func<WeldSymbol> reader)
    {
        WeldSymbol? symbol =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (symbol == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        ContourSymbolTypeEnum? contour =
            ReadNullableEnum(
                propertyDiagnostics,
                "Contour",
                () => symbol.Contour);

        return new
        {
            weldSymbolTypeRaw =
                ReadNullableInt32(
                    propertyDiagnostics,
                    "WeldSymbolType",
                    () => symbol.WeldSymbolType),
            sizeOrStrength =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "SizeOrStrength",
                        () => symbol.SizeOrStrength)),
            length =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Length",
                        () => symbol.Length)),
            pitch =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Pitch",
                        () => symbol.Pitch)),
            contourRaw =
                ReadEnumRaw(
                    contour),
            contour =
                ReadEnumName(
                    contour),
            method =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Method",
                        () => symbol.Method)),
            prefix =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Prefix",
                        () => symbol.Prefix)),
            root =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Root",
                        () => symbol.Root)),
            gap =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Gap",
                        () => symbol.Gap)),
            depth =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Depth",
                        () => symbol.Depth)),
            leg1 =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Leg1",
                        () => symbol.Leg1)),
            leg2 =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Leg2",
                        () => symbol.Leg2)),
            test =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Test",
                        () => symbol.Test)),
            enableSecondaryFilletWeld =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "EnableSecondaryFilletWeld",
                    () => symbol.EnableSecondaryFilletWeld),
            propertyDiagnostics
        };
    }

    private static object? ReadLeader(
        List<object> diagnostics,
        string propertyName,
        Func<Leader> reader)
    {
        Leader? leader =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (leader == null)
        {
            return null;
        }

        bool? hasRootNode =
            ReadNullableBoolean(
                diagnostics,
                $"{propertyName}.HasRootNode",
                () => leader.HasRootNode);

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => leader.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => leader.Type),
            hasRootNode,
            rootNodePosition =
                hasRootNode == true
                    ? ReadPoint2d(
                        ReadComObject(
                            diagnostics,
                            $"{propertyName}.RootNode.Position",
                            () => leader.RootNode.Position))
                    : null
        };
    }

    private static object? ReadNamedObject<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> reader)
        where T : class
    {
        T? value =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (value == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadReflectionProperty(
                        value,
                        diagnostics,
                        $"{propertyName}.Name",
                        "Name")),
            internalName =
                ReadString(
                    ReadReflectionProperty(
                        value,
                        diagnostics,
                        $"{propertyName}.InternalName",
                        "InternalName")),
            objectTypeRaw =
                ReadString(
                    ReadReflectionProperty(
                        value,
                        diagnostics,
                        $"{propertyName}.Type",
                        "Type"))
        };
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

            string referenceKeyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString =
                    referenceKeyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
                    diagnostics.Add(new { scope = "ReleaseKeyContext", message = exception.Message, exceptionType = exception.GetType().FullName });
                }
            }
        }
    }

    private static object? ReadReflectionProperty(
        object ownerObject,
        List<object> diagnostics,
        string diagnosticName,
        string propertyName)
    {
        try
        {
            return ownerObject
                .GetType()
                .InvokeMember(
                    propertyName,
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    ownerObject,
                    null);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = diagnosticName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadProperty(
        List<object> diagnostics,
        string propertyName,
        Func<object?> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = propertyName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static T? ReadComObject<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> reader)
        where T : class
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = propertyName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static int? ReadNullableInt32(
        List<object> diagnostics,
        string propertyName,
        Func<int> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = propertyName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool? ReadNullableBoolean(
        List<object> diagnostics,
        string propertyName,
        Func<bool> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = propertyName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static TEnum? ReadNullableEnum<TEnum>(
        List<object> diagnostics,
        string propertyName,
        Func<TEnum> reader)
        where TEnum : struct
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = propertyName, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string ReadObjectTypeRaw(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumRaw(
            value);
    }

    private static string ReadObjectTypeName(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumName(
            value);
    }

    private static string ReadEnumRaw<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        return value.HasValue
            ? Convert.ToInt32(value.Value).ToString()
            : string.Empty;
    }

    private static string ReadEnumName<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        return value.HasValue
            ? value.Value.ToString() ??
              string.Empty
            : string.Empty;
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString() ??
               string.Empty;
    }

    private static object? ReadPoint2d(
        Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x =
                point.X,
            y =
                point.Y
        };
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

internal sealed class WeldingSymbolDefinitionInput
{
    public bool? AllAroundSymbol { get; set; }
    public bool? ClosedNoteTail { get; set; }
    public bool? FieldWeldingSymbol { get; set; }
    public string? RequestedIdentificationLinePlacement { get; set; }
    public IdentificationLinePlacementEnum? IdentificationLinePlacement { get; set; }
    public string? RequestedStaggerType { get; set; }
    public StaggerTypeEnum? StaggerType { get; set; }
    public bool? SwapArrowAndSymbols { get; set; }
    public string? TailNote { get; set; }
    public bool? UseSpacer { get; set; }
    public WeldingSymbolInput? WeldSymbolOne { get; set; }
    public WeldingSymbolInput? WeldSymbolTwo { get; set; }
}

internal sealed class WeldingSymbolInput
{
    public string? RequestedWeldSymbolType { get; set; }
    public WeldSymbolTypeEnum? WeldSymbolType { get; set; }
    public string? SizeOrStrength { get; set; }
    public string? Length { get; set; }
    public string? Pitch { get; set; }
    public string? RequestedContour { get; set; }
    public ContourSymbolTypeEnum? Contour { get; set; }
    public string? Method { get; set; }
    public string? Prefix { get; set; }
    public string? Root { get; set; }
    public string? Gap { get; set; }
    public string? Depth { get; set; }
    public string? Leg1 { get; set; }
    public string? Leg2 { get; set; }
    public string? Test { get; set; }
    public bool? EnableSecondaryFilletWeld { get; set; }
}

internal sealed record WeldingSymbolAttachmentInput(
    string ViewName,
    int CurveIndex,
    string IntentText,
    PointIntentEnum PointIntent);
