using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class FeatureControlFrameCommandSupport
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

    public static bool TryGetRows(
        JsonElement root,
        List<object> diagnostics,
        out List<FeatureControlFrameRowInput> rows)
    {
        rows =
            new List<FeatureControlFrameRowInput>();

        if (!root.TryGetProperty(
                "rows",
                out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.rows", message = "rows is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.rows", message = "rows must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement rowElement in element.EnumerateArray())
        {
            if (rowElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.rows[{index}]", message = "Row must be an object." });
                return false;
            }

            if (!TryGetRequiredString(rowElement, "geometricCharacteristic", $"input.rows[{index}].geometricCharacteristic", diagnostics, out string characteristicText) ||
                !TryParseGeometricCharacteristic(characteristicText, out GeometricCharacteristicEnum characteristic))
            {
                diagnostics.Add(new { scope = $"input.rows[{index}].geometricCharacteristic", message = "Unsupported geometricCharacteristic.", supported = SupportedGeometricCharacteristics });
                return false;
            }

            if (!TryGetRequiredStringAllowEmpty(rowElement, "tolerance", $"input.rows[{index}].tolerance", diagnostics, out string tolerance) ||
                !TryGetRequiredStringAllowEmpty(rowElement, "lowerTolerance", $"input.rows[{index}].lowerTolerance", diagnostics, out string lowerTolerance) ||
                !TryGetRequiredStringAllowEmpty(rowElement, "datumOne", $"input.rows[{index}].datumOne", diagnostics, out string datumOne) ||
                !TryGetRequiredStringAllowEmpty(rowElement, "datumTwo", $"input.rows[{index}].datumTwo", diagnostics, out string datumTwo) ||
                !TryGetRequiredStringAllowEmpty(rowElement, "datumThree", $"input.rows[{index}].datumThree", diagnostics, out string datumThree))
            {
                return false;
            }

            rows.Add(
                new FeatureControlFrameRowInput(
                    characteristicText,
                    characteristic,
                    tolerance,
                    lowerTolerance,
                    datumOne,
                    datumTwo,
                    datumThree));

            index++;
        }

        if (rows.Count == 0)
        {
            diagnostics.Add(new { scope = "input.rows", message = "rows must contain at least one row." });
            return false;
        }

        return true;
    }

    public static bool TryResolveAttachmentInput(
        JsonElement root,
        List<object> diagnostics,
        out FeatureControlFrameAttachmentInput? attachment)
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
                new FeatureControlFrameAttachmentInput(
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

    public static FeatureControlFrame? ResolveFeatureControlFrame(
        Sheet sheet,
        int frameIndex,
        List<object> diagnostics)
    {
        FeatureControlFrames frames;

        try
        {
            frames =
                sheet.FeatureControlFrames;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.FeatureControlFrames", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        int count;

        try
        {
            count =
                frames.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrames.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (frameIndex < 1 ||
            frameIndex > count)
        {
            diagnostics.Add(new { scope = "input.frameIndex", message = "frameIndex is outside the FeatureControlFrames collection.", frameIndex, count });
            return null;
        }

        try
        {
            return frames[frameIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrames.Item", message = exception.Message, exceptionType = exception.GetType().FullName, frameIndex });
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

    public static object FormatRows(
        List<FeatureControlFrameRowInput> rows)
    {
        return rows
            .Select(
                row => new
                {
                    geometricCharacteristic =
                        row.RequestedGeometricCharacteristic,
                    geometricCharacteristicRaw =
                        Convert.ToInt32(
                            row.GeometricCharacteristic),
                    geometricCharacteristicEnum =
                        row.GeometricCharacteristic.ToString(),
                    tolerance =
                        row.Tolerance,
                    lowerTolerance =
                        row.LowerTolerance,
                    datumOne =
                        row.DatumOne,
                    datumTwo =
                        row.DatumTwo,
                    datumThree =
                        row.DatumThree
                })
            .ToList();
    }

    public static object ReadFeatureControlFrameFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        FeatureControlFrame frame,
        int frameIndex)
    {
        List<object> propertyDiagnostics =
            new();

        FeatureControlFrameCommandRowsReadResult rows =
            ReadRows(
                frame);

        return new
        {
            index =
                frameIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    propertyDiagnostics,
                    "FeatureControlFrame.Type",
                    () => frame.Type),
            objectType =
                ReadObjectTypeName(
                    propertyDiagnostics,
                    "FeatureControlFrame.Type",
                    () => frame.Type),
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
                    "FeatureControlFrame.Position",
                    () => frame.Position),
            layer =
                ReadNamedObject(
                    propertyDiagnostics,
                    "FeatureControlFrame.Layer",
                    () => frame.Layer),
            style =
                ReadNamedObject(
                    propertyDiagnostics,
                    "FeatureControlFrame.Style",
                    () => frame.Style),
            leader =
                ReadLeader(
                    propertyDiagnostics,
                    "FeatureControlFrame.Leader",
                    () => frame.Leader),
            datumIdentifier =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrame.DatumIdentifier",
                    () => frame.DatumIdentifier),
            notes =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrame.Notes",
                    () => frame.Notes),
            topNotes =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrame.TopNotes",
                    () => frame.TopNotes),
            profileTypeRaw =
                ReadEnumRaw(
                    propertyDiagnostics,
                    "FeatureControlFrame.ProfileType",
                    () => frame.ProfileType),
            profileType =
                ReadEnumName(
                    propertyDiagnostics,
                    "FeatureControlFrame.ProfileType",
                    () => frame.ProfileType),
            allAroundSymbol =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "FeatureControlFrame.AllAroundSymbol",
                    () => frame.AllAroundSymbol),
            overrideMergeSymbol =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "FeatureControlFrame.OverrideMergeSymbol",
                    () => frame.OverrideMergeSymbol),
            mergeSymbolOverridden =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "FeatureControlFrame.MergeSymbolOverridden",
                    () => frame.MergeSymbolOverridden),
            rowRawCount =
                rows.RawCount,
            rowCount =
                rows.Items.Count,
            rows =
                rows.Items,
            rowDiagnostics =
                rows.Diagnostics,
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

                        frame.GetReferenceKey(
                            ref referenceKey,
                            keyContext);

                        return referenceKey;
                    }),
            propertyDiagnostics
        };
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

    private static bool TryParseGeometricCharacteristic(
        string value,
        out GeometricCharacteristicEnum characteristic)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "straightness":
                characteristic =
                    GeometricCharacteristicEnum.kStraightness;
                return true;

            case "flatness":
                characteristic =
                    GeometricCharacteristicEnum.kFlatness;
                return true;

            case "circularity":
                characteristic =
                    GeometricCharacteristicEnum.kCircularity;
                return true;

            case "profile_of_any_line":
                characteristic =
                    GeometricCharacteristicEnum.kProfileOfAnyLine;
                return true;

            case "profile_of_any_surface":
                characteristic =
                    GeometricCharacteristicEnum.kProfileOfAnySurface;
                return true;

            case "angularity":
                characteristic =
                    GeometricCharacteristicEnum.kAngularity;
                return true;

            case "perpendicularity":
                characteristic =
                    GeometricCharacteristicEnum.kPerpendicularity;
                return true;

            case "parallelism":
                characteristic =
                    GeometricCharacteristicEnum.kParallelism;
                return true;

            case "position":
                characteristic =
                    GeometricCharacteristicEnum.kPosition;
                return true;

            case "concentricity_and_coaxiality":
                characteristic =
                    GeometricCharacteristicEnum.kConcentricityAndCoaxiality;
                return true;

            case "circular_runout":
                characteristic =
                    GeometricCharacteristicEnum.kCircularRunout;
                return true;

            case "symmetry":
                characteristic =
                    GeometricCharacteristicEnum.kSymmetry;
                return true;

            case "total_runout":
                characteristic =
                    GeometricCharacteristicEnum.kTotalRunout;
                return true;

            case "cylindricity":
                characteristic =
                    GeometricCharacteristicEnum.kCylindricity;
                return true;

            default:
                characteristic =
                    default;
                return false;
        }
    }

    private static FeatureControlFrameCommandRowsReadResult ReadRows(
        FeatureControlFrame frame)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        FeatureControlFrameRows? rows;

        try
        {
            rows =
                frame.FeatureControlFrameRows;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrame.FeatureControlFrameRows", message = exception.Message, exceptionType = exception.GetType().FullName });
            return new FeatureControlFrameCommandRowsReadResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            null;

        try
        {
            rawCount =
                rows.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrameRows.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        try
        {
            int index =
                0;

            foreach (FeatureControlFrameRow row
                     in rows)
            {
                index++;

                items.Add(
                    ReadRow(
                        row,
                        index));
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrameRows.Enumeration", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return new FeatureControlFrameCommandRowsReadResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadRow(
        FeatureControlFrameRow row,
        int index)
    {
        List<object> propertyDiagnostics =
            new();

        return new
        {
            index,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.Type",
                    () => row.Type),
            objectType =
                ReadObjectTypeName(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.Type",
                    () => row.Type),
            geometricCharacteristicRaw =
                ReadEnumRaw(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.GeometricCharacteristic",
                    () => row.GeometricCharacteristic),
            geometricCharacteristic =
                ReadEnumName(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.GeometricCharacteristic",
                    () => row.GeometricCharacteristic),
            tolerance =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.Tolerance",
                    () => row.Tolerance),
            lowerTolerance =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.LowerTolerance",
                    () => row.LowerTolerance),
            datumOne =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.DatumOne",
                    () => row.DatumOne),
            datumTwo =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.DatumTwo",
                    () => row.DatumTwo),
            datumThree =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.DatumThree",
                    () => row.DatumThree),
            inlineNote =
                ReadString(
                    propertyDiagnostics,
                    "FeatureControlFrameRow.InlineNote",
                    () => row.InlineNote),
            propertyDiagnostics
        };
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
            return propertyName switch
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
            diagnostics.Add(new { scope = "FeatureControlFrame.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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

    private static readonly string[] SupportedGeometricCharacteristics =
    {
        "straightness",
        "flatness",
        "circularity",
        "profile_of_any_line",
        "profile_of_any_surface",
        "angularity",
        "perpendicularity",
        "parallelism",
        "position",
        "concentricity_and_coaxiality",
        "circular_runout",
        "symmetry",
        "total_runout",
        "cylindricity"
    };
}

internal sealed record FeatureControlFrameRowInput(
    string RequestedGeometricCharacteristic,
    GeometricCharacteristicEnum GeometricCharacteristic,
    string Tolerance,
    string LowerTolerance,
    string DatumOne,
    string DatumTwo,
    string DatumThree);

internal sealed record FeatureControlFrameAttachmentInput(
    string ViewName,
    int CurveIndex,
    string IntentText,
    PointIntentEnum PointIntent);

internal sealed record FeatureControlFrameCommandRowsReadResult(
    int? RawCount,
    List<object> Items,
    List<object> Diagnostics);
