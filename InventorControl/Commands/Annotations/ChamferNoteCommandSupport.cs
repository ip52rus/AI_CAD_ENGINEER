using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ChamferNoteCommandSupport
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
        out int chamferEdgeOneCurveIndex,
        out int chamferEdgeTwoCurveIndex,
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
                "chamferEdgeOneCurveIndex",
                "input.chamferEdgeOneCurveIndex",
                diagnostics,
                out chamferEdgeOneCurveIndex))
        {
            valid =
                false;
        }

        if (!TryGetPositiveIndex(
                root,
                "chamferEdgeTwoCurveIndex",
                "input.chamferEdgeTwoCurveIndex",
                diagnostics,
                out chamferEdgeTwoCurveIndex))
        {
            valid =
                false;
        }

        if (chamferEdgeOneCurveIndex ==
            chamferEdgeTwoCurveIndex)
        {
            diagnostics.Add(new { scope = "input.chamferEdgeTwoCurveIndex", message = "chamferEdgeOneCurveIndex and chamferEdgeTwoCurveIndex must refer to different DrawingCurve objects.", chamferEdgeOneCurveIndex, chamferEdgeTwoCurveIndex });
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
        out int chamferNoteIndex,
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
                "chamferNoteIndex",
                "input.chamferNoteIndex",
                diagnostics,
                out chamferNoteIndex))
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
        out int chamferNoteIndex)
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
                "chamferNoteIndex",
                "input.chamferNoteIndex",
                diagnostics,
                out chamferNoteIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static ChamferNotes? ReadChamferNotesCollection(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .ChamferNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static int? ReadChamferNoteCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .ChamferNotes
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static ChamferNote? ResolveChamferNote(
        Sheet sheet,
        int chamferNoteIndex,
        List<object> diagnostics)
    {
        try
        {
            ChamferNotes notes =
                sheet
                    .DrawingNotes
                    .ChamferNotes;

            int count =
                notes.Count;

            if (chamferNoteIndex < 1 ||
                chamferNoteIndex > count)
            {
                diagnostics.Add(new { scope = "input.chamferNoteIndex", message = $"chamferNoteIndex {chamferNoteIndex} is outside Sheet.DrawingNotes.ChamferNotes range 1..{count}.", chamferNoteIndex, count });
                return null;
            }

            return notes[chamferNoteIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.ChamferNotes.Item", message = exception.Message, exceptionType = exception.GetType().FullName, chamferNoteIndex });
            return null;
        }
    }

    public static DrawingCurve? ResolveDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
        string scope,
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
                diagnostics.Add(new { scope, message = $"curveIndex {curveIndex} is outside DrawingView.DrawingCurves range 1..{count}.", curveIndex, count });
                return null;
            }

            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex });
            return null;
        }
    }

    public static bool TryReadPosition(
        ChamferNote chamferNote,
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
                chamferNote.Position;

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
        ChamferNote chamferNote,
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

            chamferNote.GetReferenceKey(
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
        ChamferNote chamferNote,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return chamferNote.Text;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadChamferEdges(
        ChamferNote chamferNote,
        List<object> diagnostics,
        string scope)
    {
        bool readable =
            true;

        try
        {
            _ =
                chamferNote.ChamferEdgeOne;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.ChamferEdgeOne", message = exception.Message, exceptionType = exception.GetType().FullName });
            readable =
                false;
        }

        try
        {
            _ =
                chamferNote.ChamferEdgeTwo;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.ChamferEdgeTwo", message = exception.Message, exceptionType = exception.GetType().FullName });
            readable =
                false;
        }

        return readable;
    }

    public static bool TryReadAttachedPointOnSheet(
        ChamferNote chamferNote,
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
                chamferNote
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

    public static object ReadPlacementDiagnostics(
        DrawingDocument drawingDocument,
        ChamferNote chamferNote,
        string scope)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint(
                () => chamferNote.Position,
                diagnostics,
                $"{scope}.Position");

        object? rangeBox =
            ReadRangeBox(
                () => chamferNote.RangeBox,
                diagnostics,
                $"{scope}.RangeBox");

        object? horizontalJustification =
            ReadEnumFact(
                () => chamferNote.HorizontalJustification,
                diagnostics,
                $"{scope}.HorizontalJustification");

        object? verticalJustification =
            ReadEnumFact(
                () => chamferNote.VerticalJustification,
                diagnostics,
                $"{scope}.VerticalJustification");

        object? stackedTextPosition =
            ReadEnumFact(
                () => chamferNote.StackedTextPosition,
                diagnostics,
                $"{scope}.StackedTextPosition");

        double? rotation =
            null;

        try
        {
            rotation =
                chamferNote.Rotation;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Rotation", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        object? leader =
            ReadLeaderPlacementFacts(
                () => chamferNote.Leader,
                diagnostics,
                $"{scope}.Leader");

        object? attachedEntity =
            ReadGeometryIntentFacts(
                () => chamferNote._AttachedEntity,
                diagnostics,
                $"{scope}._AttachedEntity");

        string? referenceKey =
            ReadReferenceKeyString(
                drawingDocument,
                chamferNote,
                diagnostics,
                $"{scope}.ReferenceKey");

        string? text =
            ReadText(
                chamferNote,
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
