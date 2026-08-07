using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateSectionViewCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateSectionViewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_section_view";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        DrawingDocument? drawingDocument =
            SheetCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "activeDocument",

                    message =
                        documentError
                });

            return CreateError(
                "Active document is not a DrawingDocument.",
                diagnostics);
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sheetName",

                    message =
                        sheetNameError
                });

            return CreateError(
                "sheetName is required.",
                diagnostics);
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "parentViewName",
                    out string parentViewName,
                    out string parentViewNameError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.parentViewName",

                    message =
                        parentViewNameError
                });

            return CreateError(
                "parentViewName is required.",
                diagnostics);
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "sectionSketchName",
                    out string sectionSketchName,
                    out string sectionSketchNameError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sectionSketchName",

                    message =
                        sectionSketchNameError
                });

            return CreateError(
                "sectionSketchName is required.",
                diagnostics);
        }

        if (!TryGetPoint(
                root,
                "position",
                out double x,
                out double y,
                diagnostics))
        {
            return CreateError(
                "Invalid section view position.",
                diagnostics);
        }

        if (!TryGetRequiredViewStyle(
                root,
                out string styleText,
                out DrawingViewStyleEnum viewStyle,
                diagnostics))
        {
            return CreateError(
                "Invalid drawing view style.",
                diagnostics);
        }

        if (!TryGetOptionalPositiveDouble(
                root,
                "scale",
                out double? scale,
                diagnostics))
        {
            return CreateError(
                "Invalid section view scale.",
                diagnostics);
        }

        if (!TryGetOptionalBoolean(
                root,
                "showLabel",
                true,
                out bool showLabel,
                diagnostics))
        {
            return CreateError(
                "Invalid showLabel value.",
                diagnostics);
        }

        if (!TryGetOptionalString(
                root,
                "name",
                out string viewName,
                diagnostics))
        {
            return CreateError(
                "Invalid section view name.",
                diagnostics);
        }

        if (!TryGetOptionalBoolean(
                root,
                "fullDepth",
                true,
                out bool fullDepth,
                diagnostics))
        {
            return CreateError(
                "Invalid fullDepth value.",
                diagnostics);
        }

        if (!TryGetOptionalPositiveDouble(
                root,
                "sectionDepth",
                out double? sectionDepth,
                diagnostics))
        {
            return CreateError(
                "Invalid sectionDepth value.",
                diagnostics);
        }

        if (!fullDepth &&
            !sectionDepth.HasValue)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sectionDepth",

                    message =
                        "sectionDepth is required when fullDepth is false."
                });

            return CreateError(
                "sectionDepth is required when fullDepth is false.",
                diagnostics);
        }

        Sheet? sheet =
            SheetCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sheetName",

                    message =
                        "Sheet was not found.",

                    sheetName
                });

            return CreateError(
                "Sheet was not found.",
                diagnostics);
        }

        if (!BaseViewCommandSupport
                .IsPointInsideSheet(
                    sheet,
                    x,
                    y))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.position",

                    message =
                        "Position is outside sheet bounds.",

                    sheetWidth =
                        sheet.Width,

                    sheetHeight =
                        sheet.Height,

                    x,

                    y
                });

            return CreateError(
                "Section view position is outside sheet bounds.",
                diagnostics);
        }

        DrawingView? parentView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                parentViewName);

        if (parentView == null)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.parentViewName",

                    message =
                        "Parent drawing view was not found on selected sheet.",

                    parentViewName
                });

            return CreateError(
                "Parent drawing view was not found.",
                diagnostics);
        }

        DrawingSketch? sectionSketch =
            FindDrawingSketch(
                parentView,
                sectionSketchName,
                diagnostics);

        if (sectionSketch == null)
        {
            return CreateError(
                "Section sketch was not found.",
                diagnostics);
        }

        try
        {
            Point2d position =
                _inventor.TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            object scaleArgument =
                scale.HasValue
                    ? scale.Value
                    : Type.Missing;

            object sectionDepthArgument =
                sectionDepth.HasValue
                    ? sectionDepth.Value
                    : Type.Missing;

            object moreOptions =
                Type.Missing;

            SectionDrawingView sectionView =
                sheet.DrawingViews
                    .AddSectionView2(
                        parentView,
                        sectionSketch,
                        position,
                        viewStyle,
                        scaleArgument,
                        showLabel,
                        viewName,
                        fullDepth,
                        sectionDepthArgument,
                        moreOptions);

            drawingDocument.Update();

            string? sectionViewReferenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    sectionView,
                    "SectionDrawingView.GetReferenceKey",
                    diagnostics);

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        capability =
                            "create_section_view",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        viewName =
                            sectionView.Name,

                        requestedName =
                            string.IsNullOrWhiteSpace(
                                viewName)
                                ? null
                                : viewName,

                        parentView =
                            parentView.Name,

                        sectionSketch =
                            sectionSketch.Name,

                        position =
                            new
                            {
                                x =
                                    sectionView.Position.X,

                                y =
                                    sectionView.Position.Y
                            },

                        style =
                            styleText,

                        styleRaw =
                            viewStyle.ToString(),

                        scale =
                            sectionView.Scale,

                        scaleString =
                            sectionView.ScaleString,

                        showLabel,

                        fullDepth,

                        sectionDepth,

                        viewType =
                            sectionView.ViewType
                                .ToString(),

                        referenceKey =
                            sectionViewReferenceKey,

                        dirty =
                            drawingDocument.Dirty,

                        diagnostics
                    });
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "DrawingViews.AddSectionView2",

                    message =
                        exception.Message,

                    exceptionType =
                        exception.GetType()
                            .FullName
                });

            return CreateError(
                "Failed to create section view.",
                diagnostics);
        }
    }

    private static DrawingSketch? FindDrawingSketch(
        DrawingView parentView,
        string sketchName,
        List<object> diagnostics)
    {
        DrawingSketch? found =
            null;

        int matches =
            0;

        try
        {
            foreach (DrawingSketch sketch
                     in parentView.Sketches)
            {
                if (string.Equals(
                        sketch.Name,
                        sketchName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    found =
                        sketch;

                    matches++;
                }
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "DrawingView.Sketches",

                    message =
                        exception.Message,

                    exceptionType =
                        exception.GetType()
                            .FullName
                });

            return null;
        }

        if (matches == 0)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sectionSketchName",

                    message =
                        "DrawingSketch was not found on selected sheet.",

                    sectionSketchName =
                        sketchName,

                    parentView =
                        parentView.Name
                });

            return null;
        }

        if (matches > 1)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.sectionSketchName",

                    message =
                        "Multiple DrawingSketch objects matched the same name.",

                    sectionSketchName =
                        sketchName,

                    parentView =
                        parentView.Name,

                    matches
                });

            return null;
        }

        return found;
    }

    private static bool TryGetRequiredViewStyle(
        JsonElement root,
        out string styleText,
        out DrawingViewStyleEnum viewStyle,
        List<object> diagnostics)
    {
        styleText =
            string.Empty;

        viewStyle =
            DrawingViewStyleEnum
                .kHiddenLineRemovedDrawingViewStyle;

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "style",
                    out styleText,
                    out string styleError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.style",

                    message =
                        styleError
                });

            return false;
        }

        string normalized =
            styleText.Trim()
                .ToLowerInvariant()
                .Replace(
                    "-",
                    "_")
                .Replace(
                    " ",
                    "_");

        if (normalized !=
                "hidden_line_removed" &&
            normalized !=
                "hidden_line" &&
            normalized !=
                "shaded" &&
            normalized !=
                "shaded_hidden_line")
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.style",

                    message =
                        "Unsupported drawing view style.",

                    supported =
                        new[]
                        {
                            "hidden_line_removed",
                            "hidden_line",
                            "shaded",
                            "shaded_hidden_line"
                        }
                });

            return false;
        }

        viewStyle =
            BaseViewCommandSupport
                .ParseViewStyle(
                    normalized);

        return true;
    }

    private static bool TryGetPoint(
        JsonElement root,
        string propertyName,
        out double x,
        out double y,
        List<object> diagnostics)
    {
        x =
            0.0;

        y =
            0.0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement pointElement))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Missing required object \"{propertyName}\"."
                });

            return false;
        }

        if (pointElement.ValueKind !=
            JsonValueKind.Object)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Field \"{propertyName}\" must be an object."
                });

            return false;
        }

        return
            TryGetFiniteDouble(
                pointElement,
                "x",
                $"input.{propertyName}.x",
                out x,
                diagnostics) &&
            TryGetFiniteDouble(
                pointElement,
                "y",
                $"input.{propertyName}.y",
                out y,
                diagnostics);
    }

    private static bool TryGetFiniteDouble(
        JsonElement root,
        string propertyName,
        string scope,
        out double value,
        List<object> diagnostics)
    {
        value =
            0.0;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            diagnostics.Add(
                new
                {
                    scope,

                    message =
                        $"Missing required number \"{propertyName}\"."
                });

            return false;
        }

        if (!element.TryGetDouble(
                out value) ||
            double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            diagnostics.Add(
                new
                {
                    scope,

                    message =
                        $"Field \"{propertyName}\" must be a finite number."
                });

            return false;
        }

        return true;
    }

    private static bool TryGetOptionalPositiveDouble(
        JsonElement root,
        string propertyName,
        out double? value,
        List<object> diagnostics)
    {
        value =
            null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (!element.TryGetDouble(
                out double parsed) ||
            double.IsNaN(
                parsed) ||
            double.IsInfinity(
                parsed) ||
            parsed <= 0.0)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Field \"{propertyName}\" must be a positive finite number."
                });

            return false;
        }

        value =
            parsed;

        return true;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue,
        out bool value,
        List<object> diagnostics)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
                JsonValueKind.True &&
            element.ValueKind !=
                JsonValueKind.False)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Field \"{propertyName}\" must be true or false."
                });

            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetOptionalString(
        JsonElement root,
        string propertyName,
        out string value,
        List<object> diagnostics)
    {
        value =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Field \"{propertyName}\" must be a string when provided."
                });

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        $"input.{propertyName}",

                    message =
                        $"Field \"{propertyName}\" must not be empty when provided."
                });

            return false;
        }

        return true;
    }

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        SectionDrawingView sectionView,
        string scope,
        List<object> diagnostics)
    {
        try
        {
            Array referenceKey =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            const int keyContext =
                0;

            sectionView.GetReferenceKey(
                ref referenceKey,
                keyContext);

            return drawingDocument
                .ReferenceKeyManager
                .KeyToString(
                    ref referenceKey);
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope,

                    message =
                        exception.Message,

                    exceptionType =
                        exception.GetType()
                            .FullName
                });

            return null;
        }
    }

    private static string CreateError(
        string message,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                diagnostics
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions
        CreateJsonOptions()
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
