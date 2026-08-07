using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateSectionLineCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateSectionLineCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_section_line";

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

        string? requestedName =
            null;

        if (root.TryGetProperty(
                "name",
                out JsonElement nameElement))
        {
            if (nameElement.ValueKind !=
                JsonValueKind.String)
            {
                diagnostics.Add(
                    new
                    {
                        scope =
                            "input.name",

                        message =
                            "Field \"name\" must be a string when provided."
                    });

                return CreateError(
                    "Invalid section line sketch name.",
                    diagnostics);
            }

            requestedName =
                nameElement.GetString()?
                    .Trim();

            if (string.IsNullOrWhiteSpace(
                    requestedName))
            {
                diagnostics.Add(
                    new
                    {
                        scope =
                            "input.name",

                        message =
                            "Field \"name\" must not be empty when provided."
                    });

                return CreateError(
                    "Invalid section line sketch name.",
                    diagnostics);
            }
        }

        if (!TryGetPoint(
                root,
                "start",
                out double startX,
                out double startY,
                diagnostics) ||
            !TryGetPoint(
                root,
                "end",
                out double endX,
                out double endY,
                diagnostics))
        {
            return CreateError(
                "Invalid section line points.",
                diagnostics);
        }

        if (Math.Abs(
                startX - endX) < double.Epsilon &&
            Math.Abs(
                startY - endY) < double.Epsilon)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input",

                    message =
                        "start and end points must be different."
                });

            return CreateError(
                "Section line cannot have zero length.",
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

        DrawingSketch? sketch =
            null;

        SketchLine? line =
            null;

        bool editStarted =
            false;

        Exception? creationException =
            null;

        try
        {
            Point2d startSheetPoint =
                _inventor.TransientGeometry.CreatePoint2d(
                    startX,
                    startY);

            Point2d endSheetPoint =
                _inventor.TransientGeometry.CreatePoint2d(
                    endX,
                    endY);

            sketch =
                parentView.Sketches.Add();

            if (!string.IsNullOrWhiteSpace(
                    requestedName))
            {
                sketch.Name =
                    requestedName;
            }

            sketch.Edit();

            editStarted =
                true;

            Point2d startSketchPoint =
                sketch.SheetToSketchSpace(
                    startSheetPoint);

            Point2d endSketchPoint =
                sketch.SheetToSketchSpace(
                    endSheetPoint);

            line =
                sketch.SketchLines.AddByTwoPoints(
                    startSketchPoint,
                    endSketchPoint);
        }
        catch (Exception exception)
        {
            creationException =
                exception;
        }
        finally
        {
            if (editStarted &&
                sketch != null)
            {
                try
                {
                    sketch.ExitEdit();
                }
                catch (Exception exception)
                {
                    diagnostics.Add(
                        new
                        {
                            scope =
                                "DrawingSketch.ExitEdit",

                            message =
                                exception.Message,

                            exceptionType =
                                exception.GetType()
                                    .FullName
                        });
                }
            }
        }

        if (creationException != null)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "create_section_line",

                    message =
                        creationException.Message,

                    exceptionType =
                        creationException.GetType()
                            .FullName
                });

            return CreateError(
                "Failed to create section line.",
                diagnostics);
        }

        if (sketch == null ||
            line == null)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "create_section_line",

                    message =
                        "Sketch or line was not created."
                });

            return CreateError(
                "Failed to create section line.",
                diagnostics);
        }

        string? sketchReferenceKey =
            TryGetReferenceKey(
                drawingDocument,
                sketch,
                "DrawingSketch.GetReferenceKey",
                diagnostics);

        string? lineReferenceKey =
            TryGetReferenceKey(
                drawingDocument,
                line,
                "SketchLine.GetReferenceKey",
                diagnostics);

        return DocumentCommandSupport
            .CreateSuccess(
                new
                {
                    capability =
                        "create_section_line",

                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    parentView =
                        parentView.Name,

                    sketchName =
                        sketch.Name,

                    requestedName,

                    line =
                        new
                        {
                            start =
                                new
                                {
                                    x =
                                        startX,

                                    y =
                                        startY
                                },

                            end =
                                new
                                {
                                    x =
                                        endX,

                                    y =
                                        endY
                                },

                            referenceKey =
                                lineReferenceKey
                        },

                    coordinateSystem =
                        "sheet",

                    sketchReferenceKey,

                    dirty =
                        drawingDocument.Dirty,

                    diagnostics
                });
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

        bool valid =
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

        return valid;
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

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        object entity,
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

            switch (entity)
            {
                case DrawingSketch drawingSketch:
                    drawingSketch.GetReferenceKey(
                        ref referenceKey,
                        keyContext);
                    break;

                case SketchLine sketchLine:
                    sketchLine.GetReferenceKey(
                        ref referenceKey,
                        keyContext);
                    break;

                default:
                    diagnostics.Add(
                        new
                        {
                            scope,

                            message =
                                "Unsupported reference key entity type.",

                            entityType =
                                entity.GetType()
                                    .FullName
                        });

                    return null;
            }

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
