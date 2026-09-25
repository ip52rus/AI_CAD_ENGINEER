using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class PunchNoteCommandSupport
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
        out string sheetName,
        out string viewName,
        out int curveIndex,
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

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "viewName", out viewName, out string viewNameError))
        {
            diagnostics.Add(new { scope = "input.viewName", message = viewNameError });
            valid =
                false;
        }

        if (!TryGetPositiveIndex(
                root,
                "curveIndex",
                "input.curveIndex",
                diagnostics,
                out curveIndex))
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

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int punchNoteIndex,
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

        if (!TryGetPositiveIndex(
                root,
                "punchNoteIndex",
                "input.punchNoteIndex",
                diagnostics,
                out punchNoteIndex))
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
        out int punchNoteIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetPositiveIndex(
                root,
                "punchNoteIndex",
                "input.punchNoteIndex",
                diagnostics,
                out punchNoteIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static PunchNotes? ReadPunchNotesCollection(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .PunchNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static int? ReadPunchNoteCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .PunchNotes
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static PunchNote? ResolvePunchNote(
        Sheet sheet,
        int punchNoteIndex,
        List<object> diagnostics)
    {
        try
        {
            PunchNotes notes =
                sheet
                    .DrawingNotes
                    .PunchNotes;

            int count =
                notes.Count;

            if (punchNoteIndex < 1 ||
                punchNoteIndex > count)
            {
                diagnostics.Add(new { scope = "input.punchNoteIndex", message = $"punchNoteIndex {punchNoteIndex} is outside Sheet.DrawingNotes.PunchNotes range 1..{count}.", punchNoteIndex, count });
                return null;
            }

            return notes[punchNoteIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.PunchNotes.Item", message = exception.Message, exceptionType = exception.GetType().FullName, punchNoteIndex });
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
                diagnostics.Add(new { scope = "input.curveIndex", message = $"curveIndex {curveIndex} is outside DrawingView.DrawingCurves range 1..{count}.", curveIndex, count });
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

    public static bool TryReadEdgeType(
        DrawingCurve drawingCurve,
        List<object> diagnostics,
        string scope,
        out int? edgeTypeRaw,
        out string? edgeType,
        out DrawingEdgeTypeEnum? nativeEdgeType)
    {
        edgeTypeRaw =
            null;
        edgeType =
            null;
        nativeEdgeType =
            null;

        try
        {
            DrawingEdgeTypeEnum value =
                drawingCurve.EdgeType;

            edgeTypeRaw =
                (int)value;
            edgeType =
                value.ToString();
            nativeEdgeType =
                value;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool IsPunchEdge(
        DrawingEdgeTypeEnum edgeType)
    {
        return edgeType == DrawingEdgeTypeEnum.kPunchUpEdge ||
               edgeType == DrawingEdgeTypeEnum.kPunchDownEdge;
    }

    public static bool TryReadPosition(
        PunchNote punchNote,
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
                punchNote.Position;

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
        PunchNote punchNote,
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

            punchNote.GetReferenceKey(
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

    public static string? ReadText(
        PunchNote punchNote,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return punchNote.Text;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadPunchEdge(
        PunchNote punchNote,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            _ =
                punchNote.PunchEdge;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.PunchEdge", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool TryReadAttachedPointOnSheet(
        PunchNote punchNote,
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
            Point2d point =
                punchNote
                    ._AttachedEntity
                    .PointOnSheet;

            x =
                point.X;
            y =
                point.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static object? ReadGeometryIntentFacts(
        GeometryIntent? intent,
        List<object> diagnostics,
        string scope)
    {
        if (intent == null)
        {
            diagnostics.Add(new { scope, message = "GeometryIntent is null or unavailable." });
            return null;
        }

        return ReadGeometryIntentFacts(
            () => intent,
            diagnostics,
            scope);
    }

    public static object ReadPlacementDiagnostics(
        DrawingDocument drawingDocument,
        PunchNote punchNote,
        string scope)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint(
                () => punchNote.Position,
                diagnostics,
                $"{scope}.Position");

        object? rangeBox =
            ReadRangeBox(
                () => punchNote.RangeBox,
                diagnostics,
                $"{scope}.RangeBox");

        object? horizontalJustification =
            ReadEnumFact(
                () => punchNote.HorizontalJustification,
                diagnostics,
                $"{scope}.HorizontalJustification");

        object? verticalJustification =
            ReadEnumFact(
                () => punchNote.VerticalJustification,
                diagnostics,
                $"{scope}.VerticalJustification");

        object? stackedTextPosition =
            ReadEnumFact(
                () => punchNote.StackedTextPosition,
                diagnostics,
                $"{scope}.StackedTextPosition");

        double? rotation =
            null;

        try
        {
            rotation =
                punchNote.Rotation;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Rotation", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        object? leader =
            ReadLeaderPlacementFacts(
                () => punchNote.Leader,
                diagnostics,
                $"{scope}.Leader");

        object? attachedEntity =
            ReadGeometryIntentFacts(
                () => punchNote._AttachedEntity,
                diagnostics,
                $"{scope}._AttachedEntity");

        object? punchEdge =
            ReadGeometryIntentFacts(
                () => punchNote.PunchEdge,
                diagnostics,
                $"{scope}.PunchEdge");

        string? referenceKey =
            ReadReferenceKeyString(
                drawingDocument,
                punchNote,
                diagnostics,
                $"{scope}.ReferenceKey");

        string? text =
            ReadText(
                punchNote,
                diagnostics,
                $"{scope}.Text");

        return new
        {
            position,
            rangeBox,
            horizontalJustification,
            verticalJustification,
            stackedTextPosition,
            rotation,
            leader,
            attachedEntity,
            punchEdge,
            referenceKey,
            text,
            diagnostics
        };
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

    private static bool TryGetPositiveIndex(
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
            !element.TryGetInt32(
                out value) ||
            value < 1)
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a positive 1-based integer." });
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
            !element.TryGetDouble(
                out value) ||
            !double.IsFinite(
                value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static object? ReadLeaderPlacementFacts(
        Func<Leader> reader,
        List<object> diagnostics,
        string scope)
    {
        Leader? leader;

        try
        {
            leader =
                reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (leader == null)
        {
            diagnostics.Add(new { scope, message = "Leader is null or unavailable." });
            return null;
        }

        bool? hasRootNode =
            null;

        try
        {
            hasRootNode =
                leader.HasRootNode;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.HasRootNode", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        object? rootNodePosition =
            null;

        if (hasRootNode == true)
        {
            try
            {
                rootNodePosition =
                    ReadPoint(
                        leader.RootNode.Position);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = $"{scope}.RootNode.Position", message = exception.Message, exceptionType = exception.GetType().FullName });
            }
        }

        object allNodes =
            ReadLeaderNodes(
                () => leader.AllNodes,
                diagnostics,
                $"{scope}.AllNodes");

        object allLeafNodes =
            ReadLeaderNodes(
                () => leader.AllLeafNodes,
                diagnostics,
                $"{scope}.AllLeafNodes");

        return new
        {
            hasRootNode,
            rootNodePosition,
            allNodes,
            allLeafNodes
        };
    }

    private static object ReadLeaderNodes(
        Func<LeaderNodesEnumerator> reader,
        List<object> diagnostics,
        string scope)
    {
        List<object> items =
            new();

        LeaderNodesEnumerator? nodes;

        try
        {
            nodes =
                reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return new
            {
                rawCount =
                    (int?)null,
                count =
                    0,
                items
            };
        }

        if (nodes == null)
        {
            diagnostics.Add(new { scope, message = "LeaderNodesEnumerator is null or unavailable." });
            return new
            {
                rawCount =
                    (int?)null,
                count =
                    0,
                items
            };
        }

        int? rawCount =
            null;

        try
        {
            rawCount =
                nodes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        try
        {
            int index =
                0;

            foreach (LeaderNode node
                     in nodes)
            {
                index++;

                items.Add(
                    ReadLeaderNode(
                        node,
                        index,
                        diagnostics,
                        $"{scope}.Item"));
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Enumeration", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return new
        {
            rawCount,
            count =
                items.Count,
            items
        };
    }

    private static object ReadLeaderNode(
        LeaderNode node,
        int index,
        List<object> diagnostics,
        string scope)
    {
        object? position =
            ReadPoint(
                () => node.Position,
                diagnostics,
                $"{scope}[{index}].Position");

        object? attachedEntity =
            ReadGeometryIntentFacts(
                () => node.AttachedEntity,
                diagnostics,
                $"{scope}[{index}].AttachedEntity");

        int? childNodeCount =
            null;

        try
        {
            childNodeCount =
                node.ChildNodes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}[{index}].ChildNodes.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return new
        {
            index,
            position,
            attachedEntity,
            childNodeCount
        };
    }

    private static object? ReadGeometryIntentFacts(
        Func<GeometryIntent> reader,
        List<object> diagnostics,
        string scope)
    {
        GeometryIntent? intent;

        try
        {
            intent =
                reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (intent == null)
        {
            diagnostics.Add(new { scope, message = "GeometryIntent is null or unavailable." });
            return null;
        }

        object? objectType =
            ReadEnumFact(
                () => intent.Type,
                diagnostics,
                $"{scope}.Type");

        object? pointOnSheet =
            ReadPoint(
                () => intent.PointOnSheet,
                diagnostics,
                $"{scope}.PointOnSheet");

        string? intentType =
            null;

        try
        {
            intentType =
                intent.IntentType.ToString();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.IntentType", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        string? geometryType =
            null;

        try
        {
            geometryType =
                intent.Geometry?
                    .GetType()
                    .Name;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Geometry", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return new
        {
            objectType,
            intentType,
            pointOnSheet,
            geometryType
        };
    }

    private static object? ReadEnumFact<TEnum>(
        Func<TEnum> reader,
        List<object> diagnostics,
        string scope)
        where TEnum : struct, Enum
    {
        try
        {
            TEnum value =
                reader();

            return new
            {
                raw =
                    Convert.ToInt32(
                        value),
                name =
                    value.ToString()
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadPoint(
        Func<Point2d> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return ReadPoint(
                reader());
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadPoint(
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

    private static object? ReadRangeBox(
        Func<Box2d> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            Box2d rangeBox =
                reader();

            return new
            {
                minPoint =
                    ReadPoint(
                        rangeBox.MinPoint),
                maxPoint =
                    ReadPoint(
                        rangeBox.MaxPoint)
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
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
}
