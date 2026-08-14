using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class CustomTableCommandSupport
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
        out string title,
        out double x,
        out double y,
        out int numberOfColumns,
        out int numberOfRows,
        out string[] columnTitles,
        out bool contentsWasSupplied)
    {
        bool valid =
            true;

        columnTitles =
            Array.Empty<string>();

        contentsWasSupplied =
            root.TryGetProperty(
                "contents",
                out _);

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "title", out title, out string titleError))
        {
            diagnostics.Add(new { scope = "input.title", message = titleError });
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

        if (!TryGetPositiveInt32(
                root,
                "numberOfColumns",
                diagnostics,
                out numberOfColumns))
        {
            valid =
                false;
        }

        if (!TryGetPositiveInt32(
                root,
                "numberOfRows",
                diagnostics,
                out numberOfRows))
        {
            valid =
                false;
        }

        if (!TryGetColumnTitles(
                root,
                numberOfColumns,
                diagnostics,
                out columnTitles))
        {
            valid =
                false;
        }

        if (contentsWasSupplied)
        {
            diagnostics.Add(new
            {
                scope =
                    "input.contents",
                message =
                    "contents is deliberately not supported in Package 51A because native COM contents array marshaling has not been validated. Omit contents and populate cells through a separately audited capability."
            });
            valid =
                false;
        }

        return valid;
    }

    public static bool TryGetMoveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int customTableIndex,
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

        if (!TryGetCustomTableIndex(
                root,
                diagnostics,
                out customTableIndex))
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
        out int customTableIndex)
    {
        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!TryGetCustomTableIndex(
                root,
                diagnostics,
                out customTableIndex))
        {
            valid =
                false;
        }

        return valid;
    }

    public static CustomTable? ResolveCustomTable(
        Sheet sheet,
        int customTableIndex,
        List<object> diagnostics)
    {
        try
        {
            CustomTables customTables =
                sheet.CustomTables;

            int count =
                customTables.Count;

            if (customTableIndex < 1 ||
                customTableIndex > count)
            {
                diagnostics.Add(new { scope = "input.customTableIndex", message = $"customTableIndex {customTableIndex} is outside Sheet.CustomTables range 1..{count}.", customTableIndex, count });
                return null;
            }

            return customTables[customTableIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.CustomTables.Item", message = exception.Message, exceptionType = exception.GetType().FullName, customTableIndex });
            return null;
        }
    }

    public static Array CreateColumnTitlesArray(
        string[] columnTitles)
    {
        string[] titles =
            new string[columnTitles.Length];

        Array.Copy(
            columnTitles,
            titles,
            columnTitles.Length);

        return titles;
    }

    public static bool TryReadPosition(
        CustomTable customTable,
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
                customTable.Position;

            x =
                position.X;
            y =
                position.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    public static int? ReadCustomTableCount(
        Sheet sheet,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return sheet
                .CustomTables
                .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
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

    public static bool TryReadStructure(
        CustomTable customTable,
        List<object> diagnostics,
        out int? rowCount,
        out int? columnCount)
    {
        bool success =
            true;

        rowCount =
            null;
        columnCount =
            null;

        try
        {
            rowCount =
                customTable
                    .Rows
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Rows.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            success =
                false;
        }

        try
        {
            columnCount =
                customTable
                    .Columns
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Columns.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            success =
                false;
        }

        return success;
    }

    public static string? TryReadTitle(
        CustomTable customTable,
        List<object> diagnostics)
    {
        try
        {
            return customTable.Title;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Title.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool TryGetColumnTitles(
        JsonElement root,
        int numberOfColumns,
        List<object> diagnostics,
        out string[] columnTitles)
    {
        columnTitles =
            Array.Empty<string>();

        if (!root.TryGetProperty(
                "columnTitles",
                out JsonElement element) ||
            element.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.columnTitles", message = "columnTitles must be an array of strings with count exactly equal to numberOfColumns." });
            return false;
        }

        List<string> values =
            new();

        int index =
            0;

        foreach (JsonElement item in element.EnumerateArray())
        {
            index++;

            if (item.ValueKind != JsonValueKind.String)
            {
                diagnostics.Add(new { scope = "input.columnTitles", message = $"columnTitles[{index}] must be a string.", index });
                return false;
            }

            values.Add(
                item.GetString() ??
                string.Empty);
        }

        if (values.Count != numberOfColumns)
        {
            diagnostics.Add(new { scope = "input.columnTitles", message = $"columnTitles count must equal numberOfColumns. Expected {numberOfColumns}, got {values.Count}.", numberOfColumns, actualCount = values.Count });
            return false;
        }

        columnTitles =
            values.ToArray();

        return true;
    }

    private static bool TryGetPositiveInt32(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out int value)
    {
        value =
            0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            !element.TryGetInt32(
                out value))
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a positive integer." });
            return false;
        }

        if (value < 1)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a positive integer.", value });
            return false;
        }

        return true;
    }

    private static bool TryGetCustomTableIndex(
        JsonElement root,
        List<object> diagnostics,
        out int customTableIndex)
    {
        customTableIndex =
            0;

        if (!root.TryGetProperty(
                "customTableIndex",
                out JsonElement element) ||
            !element.TryGetInt32(
                out customTableIndex))
        {
            diagnostics.Add(new { scope = "input.customTableIndex", message = "customTableIndex must be a positive 1-based integer." });
            return false;
        }

        if (customTableIndex < 1)
        {
            diagnostics.Add(new { scope = "input.customTableIndex", message = "customTableIndex must be a positive 1-based integer.", customTableIndex });
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
