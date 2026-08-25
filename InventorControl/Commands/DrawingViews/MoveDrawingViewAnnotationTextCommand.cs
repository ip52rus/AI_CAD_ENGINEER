using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveDrawingViewAnnotationTextCommand : IInventorCommand
{
    private const double PositionTolerance = 0.000001;

    private readonly Inventor.Application _inventor;

    public MoveDrawingViewAnnotationTextCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_drawing_view_annotation_text";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DrawingViewCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return DrawingViewCommandSupport.CreateError(
                documentError ??
                "Unable to get active drawing document.");
        }

        if (!DrawingViewCommandSupport.TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetError))
        {
            return DrawingViewCommandSupport.CreateError(
                sheetError);
        }

        if (!DrawingViewCommandSupport.TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out string viewError))
        {
            return DrawingViewCommandSupport.CreateError(
                viewError);
        }

        if (!DrawingViewCommandSupport.TryGetRequiredString(
                root,
                "textSlot",
                out string textSlot,
                out string textSlotError))
        {
            return DrawingViewCommandSupport.CreateError(
                textSlotError);
        }

        string normalizedTextSlot =
            textSlot.Trim().ToLowerInvariant();

        if (normalizedTextSlot != "primary" &&
            normalizedTextSlot != "second")
        {
            return DrawingViewCommandSupport.CreateError(
                "Field \"textSlot\" must be either \"primary\" or \"second\".");
        }

        if (!TryGetPosition(
                root,
                out double x,
                out double y,
                out string positionError))
        {
            return DrawingViewCommandSupport.CreateError(
                positionError);
        }

        Sheet? sheet =
            FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Sheet \"{sheetName}\" was not found.");
        }

        if (!IsPointInsideSheet(
                sheet,
                x,
                y))
        {
            return DrawingViewCommandSupport.CreateError(
                "Requested position is outside the sheet bounds.");
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Drawing view \"{viewName}\" was not found.");
        }

        object? annotation =
            TryGetViewAnnotation(
                drawingView,
                out string? annotationError);

        if (annotation == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Drawing view \"{drawingView.Name}\" does not expose ViewAnnotation.",
                annotationError);
        }

        bool drawingDirtyBefore =
            drawingDocument.Dirty;

        object before =
            ReadAnnotationSnapshot(
                drawingDocument,
                drawingView,
                annotation);

        try
        {
            Point2d requestedPoint =
                _inventor.TransientGeometry.CreatePoint2d(
                    x,
                    y);

            dynamic dynamicAnnotation =
                annotation;

            if (normalizedTextSlot == "primary")
            {
                dynamicAnnotation.TextPosition =
                    requestedPoint;
            }
            else
            {
                dynamicAnnotation.SecondTextPosition =
                    requestedPoint;
            }

            drawingDocument.Update();

            object after =
                ReadAnnotationSnapshot(
                    drawingDocument,
                    drawingView,
                    annotation);

            Point2d actualPosition =
                normalizedTextSlot == "primary"
                    ? dynamicAnnotation.TextPosition
                    : dynamicAnnotation.SecondTextPosition;

            object? referenceKeyBefore =
                TryGetReferenceKey(
                    drawingDocument,
                    annotation);

            object? referenceKeyAfter =
                TryGetReferenceKey(
                    drawingDocument,
                    annotation);

            bool? referenceKeyPreserved =
                AreReferenceKeysEqual(
                    referenceKeyBefore,
                    referenceKeyAfter);

            return DrawingViewCommandSupport.CreateSuccess(
                new
                {
                    command =
                        Name,
                    sheet =
                        sheet.Name,
                    view =
                        new
                        {
                            name =
                                drawingView.Name,
                            viewType =
                                drawingView.ViewType.ToString(),
                            objectType =
                                drawingView.Type.ToString()
                        },
                    textSlot =
                        normalizedTextSlot,
                    requestedPosition =
                        new
                        {
                            x,
                            y
                        },
                    actualPosition =
                        new
                        {
                            x =
                                actualPosition.X,
                            y =
                                actualPosition.Y
                        },
                    positionNormalizedByInventor =
                        Math.Abs(
                            actualPosition.X - x) >
                        PositionTolerance ||
                        Math.Abs(
                            actualPosition.Y - y) >
                        PositionTolerance,
                    annotation =
                        new
                        {
                            before,
                            after
                        },
                    referenceKeyBefore,
                    referenceKeyAfter,
                    referenceKeyPreserved,
                    dirty =
                        new
                        {
                            before =
                                drawingDirtyBefore,
                            after =
                                drawingDocument.Dirty
                        }
                });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Unable to move drawing view annotation text for view \"{drawingView.Name}\".",
                exception.Message);
        }
    }

    private static bool TryGetPosition(
        JsonElement root,
        out double x,
        out double y,
        out string error)
    {
        x =
            0.0;

        y =
            0.0;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                "position",
                out JsonElement positionElement) ||
            positionElement.ValueKind != JsonValueKind.Object)
        {
            error =
                "Field \"position\" must be an object with numeric x and y fields.";

            return false;
        }

        if (!TryGetFiniteDouble(
                positionElement,
                "x",
                out x))
        {
            error =
                "Field \"position.x\" must be a finite number.";

            return false;
        }

        if (!TryGetFiniteDouble(
                positionElement,
                "y",
                out y))
        {
            error =
                "Field \"position.y\" must be a finite number.";

            return false;
        }

        return true;
    }

    private static bool TryGetFiniteDouble(
        JsonElement root,
        string propertyName,
        out double value)
    {
        value =
            0.0;

        return root.TryGetProperty(
                propertyName,
                out JsonElement element) &&
            element.ValueKind == JsonValueKind.Number &&
            element.TryGetDouble(
                out value) &&
            !double.IsNaN(
                value) &&
            !double.IsInfinity(
                value);
    }

    private static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
        foreach (Sheet sheet in drawingDocument.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    sheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        return null;
    }

    private static object? TryGetViewAnnotation(
        DrawingView drawingView,
        out string? error)
    {
        error =
            null;

        try
        {
            return drawingView.ViewAnnotation;
        }
        catch (Exception exception)
        {
            error =
                exception.Message;

            return null;
        }
    }

    private static object ReadAnnotationSnapshot(
        DrawingDocument drawingDocument,
        DrawingView drawingView,
        object annotation)
    {
        List<object> diagnostics = new();

        dynamic dynamicAnnotation =
            annotation;

        return new
        {
            referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    annotation),
            text =
                TryReadString(
                    () => dynamicAnnotation.Text,
                    diagnostics,
                    "DrawingViewAnnotation.Text"),
            formattedText =
                TryReadString(
                    () => dynamicAnnotation.FormattedText,
                    diagnostics,
                    "DrawingViewAnnotation.FormattedText"),
            textPosition =
                TryReadPoint(
                    () => dynamicAnnotation.TextPosition,
                    diagnostics,
                    "DrawingViewAnnotation.TextPosition"),
            secondText =
                TryReadString(
                    () => dynamicAnnotation.SecondText,
                    diagnostics,
                    "DrawingViewAnnotation.SecondText"),
            secondFormattedText =
                TryReadString(
                    () => dynamicAnnotation.SecondFormattedText,
                    diagnostics,
                    "DrawingViewAnnotation.SecondFormattedText"),
            secondTextPosition =
                TryReadPoint(
                    () => dynamicAnnotation.SecondTextPosition,
                    diagnostics,
                    "DrawingViewAnnotation.SecondTextPosition"),
            parentView =
                new
                {
                    name =
                        drawingView.Name,
                    objectType =
                        drawingView.Type.ToString(),
                    viewType =
                        drawingView.ViewType.ToString()
                },
            sourceView =
                ReadParentView(
                    drawingView),
            detailDefinition =
                ReadDetailDefinition(
                    drawingView,
                    diagnostics),
            diagnostics
        };
    }

    private static object? ReadParentView(
        DrawingView drawingView)
    {
        try
        {
            DrawingView? parentView =
                drawingView.ParentView;

            if (parentView == null)
            {
                return null;
            }

            return new
            {
                name =
                    parentView.Name,
                objectType =
                    parentView.Type.ToString(),
                viewType =
                    parentView.ViewType.ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadDetailDefinition(
        DrawingView drawingView,
        List<object> diagnostics)
    {
        if (drawingView.ViewType !=
            DrawingViewTypeEnum.kDetailDrawingViewType)
        {
            return null;
        }

        dynamic dynamicView =
            drawingView;

        return new
        {
            displayDefinitionInBase =
                TryReadBoolean(
                    () => dynamicView.DisplayDefinitionInBase,
                    diagnostics,
                    "DetailDrawingView.DisplayDefinitionInBase"),
            circularFence =
                TryReadBoolean(
                    () => dynamicView.CircularFence,
                    diagnostics,
                    "DetailDrawingView.CircularFence"),
            fenceCenter =
                TryReadPoint(
                    () => dynamicView.FenceCenter,
                    diagnostics,
                    "DetailDrawingView.FenceCenter"),
            fenceRadius =
                TryReadDouble(
                    () => dynamicView.FenceRadius,
                    diagnostics,
                    "DetailDrawingView.FenceRadius"),
            fenceCornerOne =
                TryReadPoint(
                    () => dynamicView.FenceCornerOne,
                    diagnostics,
                    "DetailDrawingView.FenceCornerOne"),
            fenceCornerTwo =
                TryReadPoint(
                    () => dynamicView.FenceCornerTwo,
                    diagnostics,
                    "DetailDrawingView.FenceCornerTwo")
        };
    }

    private static object? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        object owner)
    {
        int keyContext = 0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            byte[] referenceKey =
                Array.Empty<byte>();

            dynamic dynamicOwner =
                owner;

            dynamicOwner.GetReferenceKey(
                ref referenceKey,
                keyContext);

            Array referenceKeyArray =
                referenceKey;

            string keyString =
                manager.KeyToString(
                    ref referenceKeyArray);

            return new
            {
                keyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch
        {
            return null;
        }
        finally
        {
            if (keyContext != 0)
            {
                try
                {
                    drawingDocument
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch
                {
                }
            }
        }
    }

    private static bool? AreReferenceKeysEqual(
        object? first,
        object? second)
    {
        string? firstKey =
            TryExtractKeyString(
                first);

        string? secondKey =
            TryExtractKeyString(
                second);

        if (firstKey == null ||
            secondKey == null)
        {
            return null;
        }

        return string.Equals(
            firstKey,
            secondKey,
            StringComparison.Ordinal);
    }

    private static string? TryExtractKeyString(
        object? referenceKey)
    {
        if (referenceKey == null)
        {
            return null;
        }

        try
        {
            dynamic dynamicReferenceKey =
                referenceKey;

            return dynamicReferenceKey.keyString;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadString(
        Func<object?> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return reader()?.ToString();
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope,
                    available =
                        false,
                    message =
                        exception.Message,
                    exceptionType =
                        exception.GetType().FullName
                });

            return null;
        }
    }

    private static double? TryReadDouble(
        Func<object?> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            object? value =
                reader();

            if (value == null)
            {
                return null;
            }

            return Convert.ToDouble(
                value);
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope,
                    available =
                        false,
                    message =
                        exception.Message,
                    exceptionType =
                        exception.GetType().FullName
                });

            return null;
        }
    }

    private static bool? TryReadBoolean(
        Func<object?> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            object? value =
                reader();

            if (value == null)
            {
                return null;
            }

            return Convert.ToBoolean(
                value);
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope,
                    available =
                        false,
                    message =
                        exception.Message,
                    exceptionType =
                        exception.GetType().FullName
                });

            return null;
        }
    }

    private static object? TryReadPoint(
        Func<object?> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            object? value =
                reader();

            if (value == null)
            {
                return null;
            }

            dynamic point =
                value;

            return new
            {
                x =
                    (double)point.X,
                y =
                    (double)point.Y
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope,
                    available =
                        false,
                    message =
                        exception.Message,
                    exceptionType =
                        exception.GetType().FullName
                });

            return null;
        }
    }

    private static bool IsPointInsideSheet(
        Sheet sheet,
        double x,
        double y)
    {
        return
            x >= 0.0 &&
            y >= 0.0 &&
            x <= sheet.Width &&
            y <= sheet.Height;
    }
}
