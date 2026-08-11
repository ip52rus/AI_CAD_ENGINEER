using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class SketchedSymbolCommandSupport
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

    public static bool TryGetCreateInputs(
        JsonElement root,
        List<object> diagnostics,
        out SketchedSymbolCreateInput input)
    {
        input =
            new SketchedSymbolCreateInput();

        bool valid =
            true;

        if (!TryGetRequiredString(
                root,
                "sheetName",
                "input.sheetName",
                diagnostics,
                out string sheetName))
        {
            valid =
                false;
        }

        if (!TryGetRequiredString(
                root,
                "definitionName",
                "input.definitionName",
                diagnostics,
                out string definitionName))
        {
            valid =
                false;
        }

        bool hasLeaderPoints =
            root.TryGetProperty(
                "leaderPoints",
                out JsonElement leaderPointsElement);

        bool hasX =
            root.TryGetProperty(
                "x",
                out _);

        bool hasY =
            root.TryGetProperty(
                "y",
                out _);

        if (hasLeaderPoints &&
            (hasX || hasY))
        {
            diagnostics.Add(new { scope = "input", message = "Input is ambiguous: provide either free x/y or leaderPoints, not both." });
            valid =
                false;
        }

        List<(double X, double Y)>? leaderPoints =
            null;

        double? x =
            null;
        double? y =
            null;

        if (hasLeaderPoints)
        {
            if (!TryReadLeaderPoints(
                    leaderPointsElement,
                    "input.leaderPoints",
                    diagnostics,
                    out List<(double X, double Y)> parsedLeaderPoints))
            {
                valid =
                    false;
            }
            else
            {
                leaderPoints =
                    parsedLeaderPoints;
            }
        }
        else
        {
            if (!TryGetFiniteDouble(
                    root,
                    "x",
                    "input.x",
                    diagnostics,
                    out double parsedX))
            {
                valid =
                    false;
            }
            else
            {
                x =
                    parsedX;
            }

            if (!TryGetFiniteDouble(
                    root,
                    "y",
                    "input.y",
                    diagnostics,
                    out double parsedY))
            {
                valid =
                    false;
            }
            else
            {
                y =
                    parsedY;
            }
        }

        if (root.TryGetProperty(
                "attachment",
                out JsonElement attachmentElement) &&
            !hasLeaderPoints)
        {
            diagnostics.Add(new { scope = "input.attachment", message = "attachment is only valid with leaderPoints/AddWithLeader input." });
            valid =
                false;
        }

        SketchedSymbolAttachmentInput? attachment =
            null;

        if (hasLeaderPoints)
        {
            if (!TryResolveAttachmentInput(
                    root,
                    diagnostics,
                    out attachment))
            {
                valid =
                    false;
            }
        }

        if (!TryGetOptionalFiniteDouble(
                root,
                "rotation",
                "input.rotation",
                diagnostics,
                0.0,
                out double rotation))
        {
            valid =
                false;
        }

        if (!TryGetOptionalFiniteDouble(
                root,
                "scale",
                "input.scale",
                diagnostics,
                1.0,
                out double scale))
        {
            valid =
                false;
        }

        if (!TryGetPromptedValues(
                root,
                diagnostics,
                out List<string> promptedValues))
        {
            valid =
                false;
        }

        if (valid)
        {
            input =
                new SketchedSymbolCreateInput
                {
                    SheetName =
                        sheetName,
                    DefinitionName =
                        definitionName,
                    X =
                        x,
                    Y =
                        y,
                    LeaderPoints =
                        leaderPoints,
                    Attachment =
                        attachment,
                    Rotation =
                        rotation,
                    Scale =
                        scale,
                    PromptedValues =
                        promptedValues
                };
        }

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int symbolIndex,
        out double x,
        out double y)
    {
        bool valid =
            true;

        if (!TryGetRequiredString(
                root,
                "sheetName",
                "input.sheetName",
                diagnostics,
                out sheetName))
        {
            valid =
                false;
        }

        if (!TryGetSymbolIndex(
                root,
                diagnostics,
                out symbolIndex))
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

        return valid;
    }

    public static bool TryGetDeleteInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int symbolIndex)
    {
        bool valid =
            true;

        if (!TryGetRequiredString(
                root,
                "sheetName",
                "input.sheetName",
                diagnostics,
                out sheetName))
        {
            valid =
                false;
        }

        if (!TryGetSymbolIndex(
                root,
                diagnostics,
                out symbolIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static SketchedSymbolDefinition? ResolveDefinition(
        DrawingDocument drawingDocument,
        string definitionName,
        List<object> diagnostics)
    {
        try
        {
            SketchedSymbolDefinitions definitions =
                drawingDocument.SketchedSymbolDefinitions;

            List<SketchedSymbolDefinition> matches =
                new();

            for (int index = 1;
                 index <= definitions.Count;
                 index++)
            {
                SketchedSymbolDefinition definition =
                    definitions[index];

                if (string.Equals(
                        definition.Name,
                        definitionName,
                        StringComparison.Ordinal))
                {
                    matches.Add(
                        definition);
                }
            }

            if (matches.Count == 1)
            {
                return matches[0];
            }

            diagnostics.Add(new
            {
                scope =
                    "DrawingDocument.SketchedSymbolDefinitions",
                message =
                    matches.Count == 0
                        ? $"SketchedSymbolDefinition \"{definitionName}\" was not found by exact Name match."
                        : $"SketchedSymbolDefinition \"{definitionName}\" matched more than one definition.",
                definitionName,
                matchCount =
                    matches.Count
            });

            return null;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingDocument.SketchedSymbolDefinitions", message = exception.Message, exceptionType = exception.GetType().FullName, definitionName });
            return null;
        }
    }

    public static SketchedSymbol? ResolveSketchedSymbol(
        Sheet sheet,
        int symbolIndex,
        List<object> diagnostics)
    {
        try
        {
            SketchedSymbols symbols =
                sheet.SketchedSymbols;

            int count =
                symbols.Count;

            if (symbolIndex < 1 ||
                symbolIndex > count)
            {
                diagnostics.Add(new { scope = "input.symbolIndex", message = $"symbolIndex {symbolIndex} is outside Sheet.SketchedSymbols range 1..{count}.", symbolIndex, count });
                return null;
            }

            return symbols[symbolIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.SketchedSymbols.Item", message = exception.Message, exceptionType = exception.GetType().FullName, symbolIndex });
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
                drawingView.DrawingCurves[
                    Type.Missing];

            int count =
                curves.Count;

            if (curveIndex < 1 ||
                curveIndex > count)
            {
                diagnostics.Add(new { scope = "input.attachment.curveIndex", message = $"curveIndex {curveIndex} is outside DrawingView.DrawingCurves range 1..{count}.", curveIndex, count });
                return null;
            }

            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex });
            return null;
        }
    }

    public static object CreatePromptStringsArgument(
        IReadOnlyCollection<string> promptedValues)
    {
        if (promptedValues.Count == 0)
        {
            return Type.Missing;
        }

        return promptedValues
            .ToArray();
    }

    public static object FormatPoints(
        IEnumerable<(double X, double Y)> points)
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
        SketchedSymbolAttachmentInput? attachment)
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

    public static object ReadSketchedSymbolFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        SketchedSymbol symbol,
        int symbolIndex)
    {
        List<object> diagnostics =
            new();

        string name =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Name",
                    () => symbol.Name));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => symbol.Position));

        object? referenceKey =
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
                });

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
            name,
            position,
            origin =
                position,
            rangeBox =
                ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "RangeBox",
                        () => symbol.RangeBox)),
            layer =
                ReadNamedObject(
                    diagnostics,
                    "Layer",
                    () => symbol.Layer),
            leader =
                ReadLeader(
                    diagnostics,
                    "Leader",
                    () => symbol.Leader),
            leaderVisible =
                ReadNullableBoolean(
                    diagnostics,
                    "LeaderVisible",
                    () => symbol.LeaderVisible),
            symbolClipping =
                ReadNullableBoolean(
                    diagnostics,
                    "SymbolClipping",
                    () => symbol.SymbolClipping),
            staticSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "Static",
                    () => symbol.Static),
            rotation =
                ReadNullableDouble(
                    diagnostics,
                    "Rotation",
                    () => symbol.Rotation),
            scale =
                ReadNullableDouble(
                    diagnostics,
                    "Scale",
                    () => symbol.Scale),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => symbol._AttachedEntity),
            resultTexts =
                ReadResultTexts(
                    symbol,
                    diagnostics),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index =
                        symbolIndex,
                    indexIsStable =
                        false,
                    type =
                        "sketched_symbol",
                    name,
                    position
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    public static Leader? TryReadLeader(
        SketchedSymbol symbol,
        List<object> diagnostics)
    {
        try
        {
            return symbol.Leader;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SketchedSymbol.Leader.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadHasRootNode(
        Leader leader,
        List<object> diagnostics)
    {
        try
        {
            return leader.HasRootNode;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "SketchedSymbol.Leader.HasRootNode.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool TryReadEffectivePosition(
        SketchedSymbol symbol,
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
            diagnostics.Add(new { scope = "SketchedSymbol.Leader.RootNode.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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
            diagnostics.Add(new { scope = "SketchedSymbol.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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

    private static bool TryGetSymbolIndex(
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

    private static bool TryResolveAttachmentInput(
        JsonElement root,
        List<object> diagnostics,
        out SketchedSymbolAttachmentInput? attachment)
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

        if (!TryGetRequiredString(
                element,
                "viewName",
                "input.attachment.viewName",
                diagnostics,
                out string viewName))
        {
            valid =
                false;
        }

        if (!TryGetRequiredInt32(
                element,
                "curveIndex",
                "input.attachment.curveIndex",
                diagnostics,
                out int curveIndex) ||
            curveIndex < 1)
        {
            diagnostics.Add(new { scope = "input.attachment.curveIndex", message = "curveIndex must be a positive 1-based integer." });
            valid =
                false;
        }

        if (!TryGetRequiredString(
                element,
                "intent",
                "input.attachment.intent",
                diagnostics,
                out string intentText))
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
                new SketchedSymbolAttachmentInput(
                    viewName,
                    curveIndex,
                    intentText,
                    pointIntent);
        }

        return valid;
    }

    private static bool TryReadLeaderPoints(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out List<(double X, double Y)> points)
    {
        points =
            new List<(double X, double Y)>();

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope, message = "leaderPoints must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement pointElement in element.EnumerateArray())
        {
            if (pointElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"{scope}[{index}]", message = "Leader point must be an object." });
                return false;
            }

            if (!TryGetFiniteDouble(
                    pointElement,
                    "x",
                    $"{scope}[{index}].x",
                    diagnostics,
                    out double x) ||
                !TryGetFiniteDouble(
                    pointElement,
                    "y",
                    $"{scope}[{index}].y",
                    diagnostics,
                    out double y))
            {
                return false;
            }

            points.Add(
                (x, y));

            index++;
        }

        if (points.Count == 0)
        {
            diagnostics.Add(new { scope, message = "leaderPoints must contain at least one point." });
            return false;
        }

        return true;
    }

    private static bool TryGetPromptedValues(
        JsonElement root,
        List<object> diagnostics,
        out List<string> values)
    {
        values =
            new List<string>();

        if (!root.TryGetProperty(
                "promptedValues",
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.promptedValues", message = "promptedValues must be an array of strings when supplied." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement valueElement in element.EnumerateArray())
        {
            if (valueElement.ValueKind !=
                JsonValueKind.String)
            {
                diagnostics.Add(new { scope = $"input.promptedValues[{index}]", message = "Each promptedValues item must be a string." });
                return false;
            }

            values.Add(
                valueElement.GetString()
                ?? string.Empty);

            index++;
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
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out double value)
    {
        value =
            0.0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} is required." });
            return false;
        }

        if (!element.TryGetDouble(
                out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalFiniteDouble(
        JsonElement root,
        string propertyName,
        string scope,
        List<object> diagnostics,
        double defaultValue,
        out double value)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (!element.TryGetDouble(
                out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number when supplied." });
            return false;
        }

        return true;
    }

    private static object ReadResultTexts(
        SketchedSymbol symbol,
        List<object> symbolDiagnostics)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        SketchedSymbolDefinition? definition =
            ReadComObject(
                diagnostics,
                "Definition",
                () => symbol.Definition);

        if (definition == null)
        {
            AddNestedDiagnostics(
                symbolDiagnostics,
                "SketchedSymbol.ResultTexts",
                diagnostics);

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        string definitionName =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Definition.Name",
                    () => definition.Name));

        DrawingSketch? sketch =
            ReadComObject(
                diagnostics,
                "Definition.Sketch",
                () => definition.Sketch);

        if (sketch == null)
        {
            AddNestedDiagnostics(
                symbolDiagnostics,
                "SketchedSymbol.ResultTexts",
                diagnostics);

            return new
            {
                definitionName,
                rawCount =
                    (int?)null,
                count =
                    items.Count,
                items,
                diagnostics
            };
        }

        TextBoxes? textBoxes =
            ReadComObject(
                diagnostics,
                "Definition.Sketch.TextBoxes",
                () => sketch.TextBoxes);

        if (textBoxes == null)
        {
            AddNestedDiagnostics(
                symbolDiagnostics,
                "SketchedSymbol.ResultTexts",
                diagnostics);

            return new
            {
                definitionName,
                rawCount =
                    (int?)null,
                count =
                    items.Count,
                items,
                diagnostics
            };
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "Definition.Sketch.TextBoxes.Count",
                () => textBoxes.Count);

        if (rawCount.HasValue)
        {
            for (int index = 1;
                 index <= rawCount.Value;
                 index++)
            {
                TextBox? textBox =
                    ReadComObject(
                        diagnostics,
                        $"Definition.Sketch.TextBoxes.Item[{index}]",
                        () => textBoxes[index]);

                if (textBox == null)
                {
                    continue;
                }

                List<object> itemDiagnostics =
                    new();

                items.Add(
                    new
                    {
                        index,
                        definitionText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "Text",
                                    () => textBox.Text)),
                        definitionFormattedText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "FormattedText",
                                    () => textBox.FormattedText)),
                        resultText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "GetResultText",
                                    () => symbol.GetResultText(
                                        textBox))),
                        propertyDiagnostics =
                            itemDiagnostics
                    });
            }
        }

        AddNestedDiagnostics(
            symbolDiagnostics,
            "SketchedSymbol.ResultTexts",
            diagnostics);

        return new
        {
            definitionName,
            rawCount,
            count =
                items.Count,
            items,
            diagnostics
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

        object? rootNode =
            null;

        if (hasRootNode == true)
        {
            rootNode =
                ReadLeaderNode(
                    diagnostics,
                    $"{propertyName}.RootNode",
                    () => leader.RootNode);
        }

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
            arrowheadType =
                ReadEnumName(
                    ReadNullableEnum(
                        diagnostics,
                        $"{propertyName}.ArrowheadType",
                        () => leader.ArrowheadType)),
            hasRootNode,
            rootNode
        };
    }

    private static object? ReadLeaderNode(
        List<object> diagnostics,
        string propertyName,
        Func<LeaderNode> reader)
    {
        LeaderNode? node =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (node == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => node.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => node.Type),
            position =
                ReadPoint2d(
                    ReadComObject(
                        diagnostics,
                        $"{propertyName}.Position",
                        () => node.Position)),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    $"{propertyName}.AttachedEntity",
                    () => node.AttachedEntity)
        };
    }

    private static object? ReadGeometryIntent(
        List<object> diagnostics,
        string propertyName,
        Func<GeometryIntent> reader)
    {
        GeometryIntent? intent =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (intent == null)
        {
            return null;
        }

        IntentTypeEnum? intentType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.IntentType",
                () => intent.IntentType);

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => intent.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => intent.Type),
            intentTypeRaw =
                ReadEnumRaw(
                    intentType),
            intentType =
                ReadEnumName(
                    intentType),
            pointOnSheet =
                ReadPoint2d(
                    ReadComObject(
                        diagnostics,
                        $"{propertyName}.PointOnSheet",
                        () => intent.PointOnSheet)),
            geometry =
                ReadGenericObjectMetadata(
                    diagnostics,
                    $"{propertyName}.Geometry",
                    () => intent.Geometry),
            intent =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Intent",
                        () => intent.Intent))
        };
    }

    private static object? ReadGenericObjectMetadata(
        List<object> diagnostics,
        string propertyName,
        Func<object> reader)
    {
        object? value =
            ReadProperty(
                diagnostics,
                propertyName,
                reader);

        if (value == null)
        {
            return null;
        }

        return new
        {
            runtimeType =
                value.GetType().FullName,
            objectTypeRaw =
                ReadString(
                    ReadReflectionProperty(
                        value,
                        diagnostics,
                        $"{propertyName}.Type",
                        "Type"))
        };
    }

    private static object? ReadNamedObject(
        List<object> diagnostics,
        string propertyName,
        Func<object> reader)
    {
        object? value =
            ReadProperty(
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
                        "Type")),
            runtimeType =
                value.GetType().FullName
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
            AddDiagnostic(
                diagnostics,
                "GetReferenceKey",
                exception);

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
                    AddDiagnostic(
                        diagnostics,
                        "ReleaseKeyContext",
                        exception);
                }
            }
        }
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

    private static object? ReadBox2d(
        Box2d? box)
    {
        if (box == null)
        {
            return null;
        }

        return new
        {
            minPoint =
                ReadPoint2d(
                    box.MinPoint),
            maxPoint =
                ReadPoint2d(
                    box.MaxPoint)
        };
    }

    private static object CreateCollectionResult(
        int? rawCount,
        List<object> items,
        List<object> diagnostics)
    {
        return new
        {
            rawCount,
            count =
                items.Count,
            items,
            diagnostics
        };
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
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

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
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static object? ReadReflectionProperty(
        object target,
        List<object> diagnostics,
        string diagnosticName,
        string propertyName)
    {
        try
        {
            return target
                .GetType()
                .InvokeMember(
                    propertyName,
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    target,
                    Array.Empty<object>());
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                diagnosticName,
                exception);

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
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static double? ReadNullableDouble(
        List<object> diagnostics,
        string propertyName,
        Func<double> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

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
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

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
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

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
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Convert
            .ToInt32(
                value.Value)
            .ToString();
    }

    private static string ReadEnumName<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Enum.GetName(
                   typeof(TEnum),
                   value.Value)
               ?? value.Value.ToString()
               ?? string.Empty;
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static void AddNestedDiagnostics(
        List<object> targetDiagnostics,
        string propertyName,
        List<object> nestedDiagnostics)
    {
        if (nestedDiagnostics.Count == 0)
        {
            return;
        }

        targetDiagnostics.Add(
            new
            {
                property =
                    propertyName,
                diagnostics =
                    nestedDiagnostics
            });
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string propertyName,
        Exception exception)
    {
        diagnostics.Add(
            new
            {
                property =
                    propertyName,
                available =
                    false,
                error =
                    exception.Message
            });
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}

internal sealed class SketchedSymbolCreateInput
{
    public string SheetName { get; set; } =
        string.Empty;

    public string DefinitionName { get; set; } =
        string.Empty;

    public double? X { get; set; }

    public double? Y { get; set; }

    public List<(double X, double Y)>? LeaderPoints { get; set; }

    public SketchedSymbolAttachmentInput? Attachment { get; set; }

    public double Rotation { get; set; }

    public double Scale { get; set; } =
        1.0;

    public List<string> PromptedValues { get; set; } =
        new();
}

internal sealed record SketchedSymbolAttachmentInput(
    string ViewName,
    int CurveIndex,
    string IntentText,
    PointIntentEnum PointIntent);
