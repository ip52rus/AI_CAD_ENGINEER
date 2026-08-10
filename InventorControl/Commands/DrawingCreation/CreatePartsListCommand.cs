using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreatePartsListCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreatePartsListCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_parts_list";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            SheetCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out string sheetError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetError });
            return CreateError("sheetName is required.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "viewName", out string viewName, out string viewError))
        {
            diagnostics.Add(new { scope = "input.viewName", message = viewError });
            return CreateError("viewName is required.", diagnostics);
        }

        if (!TryGetPoint(root, "position", out double positionX, out double positionY, diagnostics) ||
            !TryGetOptionalLevel(root, out PartsListLevelEnum level, out string levelText, diagnostics) ||
            !TryGetOptionalNumberingScheme(root, out object numberingScheme, out string? numberingSchemeText, diagnostics) ||
            !TryGetOptionalPositiveInt(root, "numberOfSections", 1, out int numberOfSections, diagnostics) ||
            !TryGetOptionalBoolean(root, "wrapLeft", true, out bool wrapLeft, diagnostics))
        {
            return CreateError("Invalid parts list input.", diagnostics);
        }

        Sheet? sheet =
            SheetCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
        }

        if (!BaseViewCommandSupport.IsPointInsideSheet(sheet, positionX, positionY))
        {
            diagnostics.Add(new { scope = "input.position", message = "Position is outside sheet bounds.", x = positionX, y = positionY });
            return CreateError("Parts list position is outside sheet bounds.", diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = "Drawing view was not found on selected sheet.", viewName });
            return CreateError("Drawing view was not found.", diagnostics);
        }

        try
        {
            Point2d placementPoint =
                _inventor.TransientGeometry.CreatePoint2d(
                    positionX,
                    positionY);

            PartsList partsList =
                sheet.PartsLists.Add(
                    drawingView,
                    placementPoint,
                    level,
                    numberingScheme,
                    numberOfSections,
                    wrapLeft);

            string? referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    partsList,
                    diagnostics);

            return CreateSuccess(new
            {
                capability = "create_parts_list",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = drawingView.Name,
                title = SafeRead(() => partsList.Title),
                position = ReadPoint(SafeRead(() => partsList.Position)),
                requestedPosition = new { x = positionX, y = positionY },
                level = SafeRead(() => partsList.Level.ToString()),
                requestedLevel = levelText,
                numberingScheme = numberingSchemeText,
                numberOfSections,
                wrapLeft,
                rowCount = SafeReadNullableInt(() => partsList.PartsListRows.Count),
                columnCount = SafeReadNullableInt(() => partsList.PartsListColumns.Count),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "PartsLists.Add",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("Failed to create parts list.", diagnostics);
        }
    }

    private static bool TryGetOptionalLevel(
        JsonElement root,
        out PartsListLevelEnum level,
        out string levelText,
        List<object> diagnostics)
    {
        level =
            PartsListLevelEnum.kFirstLevelComponents;

        levelText =
            "first_level_components";

        if (!root.TryGetProperty("level", out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = "input.level", message = "Value must be a string." });
            return false;
        }

        levelText =
            element.GetString()?
                .Trim()
                .ToLowerInvariant()
                .Replace("-", "_")
                .Replace(" ", "_")
            ?? string.Empty;

        switch (levelText)
        {
            case "first_level_components":
                level =
                    PartsListLevelEnum.kFirstLevelComponents;
                return true;

            case "parts_only":
                level =
                    PartsListLevelEnum.kPartsOnly;
                return true;

            case "structured_all_levels":
                level =
                    PartsListLevelEnum.kStructuredAllLevels;
                return true;

            default:
                diagnostics.Add(new
                {
                    scope = "input.level",
                    message = "Unsupported parts list level.",
                    supported = new[]
                    {
                        "first_level_components",
                        "parts_only",
                        "structured_all_levels"
                    }
                });
                return false;
        }
    }

    private static bool TryGetOptionalNumberingScheme(
        JsonElement root,
        out object numberingScheme,
        out string? numberingSchemeText,
        List<object> diagnostics)
    {
        numberingScheme =
            Type.Missing;

        numberingSchemeText =
            null;

        if (!root.TryGetProperty("numberingScheme", out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = "input.numberingScheme", message = "Value must be a string." });
            return false;
        }

        numberingSchemeText =
            element.GetString()?
                .Trim()
                .ToLowerInvariant()
                .Replace("-", "_")
                .Replace(" ", "_");

        switch (numberingSchemeText)
        {
            case "numeric":
                numberingScheme =
                    NumberingSchemeEnum.kNumericNumbering;
                return true;

            case "lowercase_alpha":
                numberingScheme =
                    NumberingSchemeEnum.kLowercaseAlphaNumbering;
                return true;

            case "uppercase_alpha":
                numberingScheme =
                    NumberingSchemeEnum.kUppercaseAlphaNumbering;
                return true;

            default:
                diagnostics.Add(new
                {
                    scope = "input.numberingScheme",
                    message = "Unsupported numbering scheme.",
                    supported = new[]
                    {
                        "numeric",
                        "lowercase_alpha",
                        "uppercase_alpha"
                    }
                });
                return false;
        }
    }

    private static bool TryGetOptionalPositiveInt(
        JsonElement root,
        string name,
        int defaultValue,
        out int value,
        List<object> diagnostics)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(name, out JsonElement element))
        {
            return true;
        }

        if (!element.TryGetInt32(out int parsed) ||
            parsed <= 0)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be a positive 32-bit integer." });
            return false;
        }

        value =
            parsed;

        return true;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string name,
        bool defaultValue,
        out bool value,
        List<object> diagnostics)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(name, out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be boolean." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetPoint(
        JsonElement root,
        string name,
        out double x,
        out double y,
        List<object> diagnostics)
    {
        x =
            0.0;

        y =
            0.0;

        if (!root.TryGetProperty(name, out JsonElement point) ||
            point.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = $"Required point object {name} is missing." });
            return false;
        }

        return TryGetFiniteDouble(point, "x", $"input.{name}.x", out x, diagnostics) &&
               TryGetFiniteDouble(point, "y", $"input.{name}.y", out y, diagnostics);
    }

    private static bool TryGetFiniteDouble(
        JsonElement root,
        string name,
        string scope,
        out double value,
        List<object> diagnostics)
    {
        value =
            0.0;

        if (!root.TryGetProperty(name, out JsonElement element) ||
            !element.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"Field {name} must be a finite number." });
            return false;
        }

        return true;
    }

    private static string? TryGetReferenceKey(
        DrawingDocument document,
        PartsList partsList,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            partsList.GetReferenceKey(
                ref key,
                0);

            return document.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "PartsList.GetReferenceKey",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return null;
        }
    }

    private static object? ReadPoint(Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x = point.X,
            y = point.Y
        };
    }

    private static T? SafeRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return default;
        }
    }

    private static int? SafeReadNullableInt(Func<int> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                diagnostics
            },
            CreateJsonOptions());

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
