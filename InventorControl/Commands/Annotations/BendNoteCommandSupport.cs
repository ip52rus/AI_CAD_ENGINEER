using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class BendNoteCommandSupport
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
        out int curveIndex)
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

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int bendNoteIndex,
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
                "bendNoteIndex",
                "input.bendNoteIndex",
                diagnostics,
                out bendNoteIndex))
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
        out int bendNoteIndex)
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
                "bendNoteIndex",
                "input.bendNoteIndex",
                diagnostics,
                out bendNoteIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static BendNotes? ReadBendNotesCollection(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .BendNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static int? ReadBendNoteCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .DrawingNotes
                .BendNotes
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static BendNote? ResolveBendNote(
        Sheet sheet,
        int bendNoteIndex,
        List<object> diagnostics)
    {
        try
        {
            BendNotes notes =
                sheet
                    .DrawingNotes
                    .BendNotes;

            int count =
                notes.Count;

            if (bendNoteIndex < 1 ||
                bendNoteIndex > count)
            {
                diagnostics.Add(new { scope = "input.bendNoteIndex", message = $"bendNoteIndex {bendNoteIndex} is outside Sheet.DrawingNotes.BendNotes range 1..{count}.", bendNoteIndex, count });
                return null;
            }

            return notes[bendNoteIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.BendNotes.Item", message = exception.Message, exceptionType = exception.GetType().FullName, bendNoteIndex });
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

    public static bool TryReadPosition(
        BendNote bendNote,
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
                bendNote.Position;

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

    public static bool TryReadEffectivePlacement(
        BendNote bendNote,
        List<object> diagnostics,
        string scope,
        out double? effectiveX,
        out double? effectiveY,
        out string? verificationSource,
        out object? effectivePosition,
        out object? bendNotePosition,
        out object? leaderRootPosition)
    {
        effectiveX =
            null;
        effectiveY =
            null;
        verificationSource =
            null;
        effectivePosition =
            null;
        bendNotePosition =
            null;
        leaderRootPosition =
            null;

        bool bendNotePositionReadable =
            TryReadPosition(
                bendNote,
                diagnostics,
                $"{scope}.BendNote.Position",
                out double? bendNoteX,
                out double? bendNoteY);

        if (bendNotePositionReadable &&
            bendNoteX.HasValue &&
            bendNoteY.HasValue)
        {
            bendNotePosition =
                new
                {
                    x =
                        bendNoteX.Value,
                    y =
                        bendNoteY.Value
                };
        }

        try
        {
            Leader leader =
                bendNote.Leader;

            if (leader != null &&
                leader.HasRootNode)
            {
                Point2d rootNodePosition =
                    leader.RootNode.Position;

                effectiveX =
                    rootNodePosition.X;
                effectiveY =
                    rootNodePosition.Y;
                verificationSource =
                    "Leader.RootNode.Position";
                leaderRootPosition =
                    new
                    {
                        x =
                            rootNodePosition.X,
                        y =
                            rootNodePosition.Y
                    };
                effectivePosition =
                    leaderRootPosition;

                return true;
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Leader.RootNode.Position", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        if (bendNotePositionReadable &&
            bendNoteX.HasValue &&
            bendNoteY.HasValue)
        {
            effectiveX =
                bendNoteX.Value;
            effectiveY =
                bendNoteY.Value;
            verificationSource =
                "BendNote.Position";
            effectivePosition =
                bendNotePosition;

            return true;
        }

        diagnostics.Add(new { scope, message = "Unable to read factual BendNote effective placement from Leader.RootNode.Position or BendNote.Position." });
        return false;
    }

    public static string? ReadText(
        BendNote bendNote,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return bendNote.Text;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static bool TryReadAttachedPointOnSheet(
        BendNote bendNote,
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
                bendNote
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

    public static string? ReadReferenceKeyString(
        DrawingDocument drawingDocument,
        BendNote bendNote,
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

            bendNote.GetReferenceKey(
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

    public static bool TryReadBendEdge(
        BendNote bendNote,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            _ =
                bendNote.BendEdge;

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
        BendNote bendNote,
        string scope)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint(
                () => bendNote.Position,
                diagnostics,
                $"{scope}.Position");

        object? rangeBox =
            ReadRangeBox(
                () => bendNote.RangeBox,
                diagnostics,
                $"{scope}.RangeBox");

        object? leader =
            ReadLeaderPlacementFacts(
                () => bendNote.Leader,
                diagnostics,
                $"{scope}.Leader");

        object? attachedEntity =
            ReadGeometryIntentFacts(
                () => bendNote._AttachedEntity,
                diagnostics,
                $"{scope}._AttachedEntity");

        string? referenceKey =
            ReadReferenceKeyString(
                drawingDocument,
                bendNote,
                diagnostics,
                $"{scope}.ReferenceKey");

        string? text =
            null;

        try
        {
            text =
                bendNote.Text;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{scope}.Text", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return new
        {
            position,
            rangeBox,
            leader,
            attachedEntity,
            referenceKey,
            text,
            diagnostics
        };
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

        return new
        {
            hasRootNode,
            rootNodePosition
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
