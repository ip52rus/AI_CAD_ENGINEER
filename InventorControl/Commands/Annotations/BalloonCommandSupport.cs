using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class BalloonCommandSupport
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

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int balloonIndex,
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

        if (!TryGetBalloonIndex(
                root,
                diagnostics,
                out balloonIndex))
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
        out int balloonIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetBalloonIndex(
                root,
                diagnostics,
                out balloonIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static Balloon? ResolveBalloon(
        Sheet sheet,
        int balloonIndex,
        List<object> diagnostics)
    {
        try
        {
            Balloons balloons =
                sheet.Balloons;

            int count =
                balloons.Count;

            if (balloonIndex < 1 ||
                balloonIndex > count)
            {
                diagnostics.Add(new { scope = "input.balloonIndex", message = $"balloonIndex {balloonIndex} is outside Sheet.Balloons range 1..{count}.", balloonIndex, count });
                return null;
            }

            return balloons[balloonIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.Balloons.Item", message = exception.Message, exceptionType = exception.GetType().FullName, balloonIndex });
            return null;
        }
    }

    public static Leader? TryReadLeader(
        Balloon balloon,
        List<object> diagnostics)
    {
        try
        {
            return balloon.Leader;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Balloon.Leader.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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
            diagnostics.Add(new { scope = "Balloon.Leader.HasRootNode.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static bool TryReadReportedPosition(
        Balloon balloon,
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
            Point2d position =
                balloon.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Balloon.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
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
            0.0001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - requestedX) <= tolerance &&
               Math.Abs(actualY.Value - requestedY) <= tolerance;
    }

    private static bool TryGetBalloonIndex(
        JsonElement root,
        List<object> diagnostics,
        out int balloonIndex)
    {
        balloonIndex =
            0;

        if (!root.TryGetProperty(
                "balloonIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out balloonIndex))
        {
            diagnostics.Add(new { scope = "input.balloonIndex", message = "balloonIndex must be a positive 1-based integer." });
            return false;
        }

        if (balloonIndex < 1)
        {
            diagnostics.Add(new { scope = "input.balloonIndex", message = "balloonIndex must be a positive 1-based integer.", balloonIndex });
            return false;
        }

        return true;
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
