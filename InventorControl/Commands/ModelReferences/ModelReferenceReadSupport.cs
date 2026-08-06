using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ModelReferenceReadSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        error = null;
        Document? document = inventor.ActiveDocument;

        if (document == null)
        {
            error = "В Inventor нет активного документа.";
            return null;
        }

        if (document.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Активный документ не является чертежом.";
            return null;
        }

        return (DrawingDocument)document;
    }

    public static Sheet? FindSheet(
        DrawingDocument drawing,
        string name)
    {
        foreach (Sheet sheet in drawing.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        return null;
    }

    public static DrawingView? FindView(
        Sheet sheet,
        string name)
    {
        foreach (DrawingView view in sheet.DrawingViews)
        {
            if (string.Equals(
                    view.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return view;
            }
        }

        return null;
    }

    public static List<DrawingCurve> GetCurves(
        DrawingView view)
    {
        List<DrawingCurve> result = new();
        DrawingCurvesEnumerator curves =
            view.DrawingCurves[Type.Missing];

        foreach (DrawingCurve curve in curves)
        {
            result.Add(curve);
        }

        return result;
    }

    public static bool TryGetString(
        JsonElement root,
        string name,
        out string value,
        out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (!root.TryGetProperty(name, out JsonElement element) ||
            element.ValueKind != JsonValueKind.String)
        {
            error = $"Поле \"{name}\" должно быть строкой.";
            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            error = $"Поле \"{name}\" не должно быть пустым.";
            return false;
        }

        return true;
    }

    public static bool TryGetInt(
        JsonElement root,
        string name,
        out int value,
        out string error)
    {
        value = 0;
        error = string.Empty;

        if (!root.TryGetProperty(name, out JsonElement element) ||
            !element.TryGetInt32(out value))
        {
            error = $"Поле \"{name}\" должно быть целым числом.";
            return false;
        }

        return true;
    }

    public static int GetOptionalInt(
        JsonElement root,
        string name,
        int defaultValue)
    {
        return root.TryGetProperty(name, out JsonElement element) &&
               element.TryGetInt32(out int value)
            ? value
            : defaultValue;
    }

    public static object ReadViewDescriptor(
        DrawingView view)
    {
        string displayName = string.Empty;
        string fullDocumentName = string.Empty;
        string documentType = string.Empty;
        bool missing = false;
        bool suppressed = false;

        try
        {
            DocumentDescriptor descriptor =
                view.ReferencedDocumentDescriptor;

            try
            {
                displayName =
                    descriptor.ReferencedDocument.DisplayName;
            }
            catch
            {
            }

            try
            {
                fullDocumentName =
                    descriptor.FullDocumentName;
            }
            catch
            {
            }

            try
            {
                documentType =
                    descriptor.ReferencedDocumentType.ToString();
            }
            catch
            {
            }

            try
            {
                missing =
                    descriptor.ReferenceMissing;
            }
            catch
            {
            }

            try
            {
                suppressed =
                    descriptor.ReferenceSuppressed;
            }
            catch
            {
            }
        }
        catch
        {
        }

        return new
        {
            displayName,
            fullDocumentName,
            documentType,
            referenceMissing = missing,
            referenceSuppressed = suppressed
        };
    }

    public static object ReadCurve(
        DrawingCurve curve,
        int index)
    {
        object? modelGeometry = null;
        string? modelGeometryError = null;

        try
        {
            modelGeometry = curve.ModelGeometry;
        }
        catch (Exception exception)
        {
            modelGeometryError = exception.Message;
        }

        return new
        {
            curveIndex = index,
            curveType = curve.CurveType.ToString(),
            projectedCurveType = SafeProjectedType(curve),
            edgeType = SafeEdgeType(curve),
            startPoint = ReadPoint(curve, "start"),
            endPoint = ReadPoint(curve, "end"),
            midPoint = ReadPoint(curve, "mid"),
            centerPoint = ReadPoint(curve, "center"),
            segmentCount = SafeSegmentCount(curve),
            modelGeometryAvailable = modelGeometry != null,
            modelGeometryError,
            modelReference = ReadModelObject(modelGeometry)
        };
    }

    public static object? ReadModelObject(
        object? modelObject)
    {
        if (modelObject == null)
        {
            return null;
        }

        dynamic value = modelObject;

        string inventorType = string.Empty;
        string name = string.Empty;
        string nativeObjectType = string.Empty;
        string parentType = string.Empty;
        string parentName = string.Empty;
        string occurrenceName = string.Empty;
        string occurrencePath = string.Empty;
        string geometryType = string.Empty;
        int faceCount = 0;

        try
        {
            inventorType = value.Type.ToString();
        }
        catch
        {
        }

        try
        {
            name = (string)value.Name;
        }
        catch
        {
        }

        try
        {
            object? nativeObject = value.NativeObject;

            if (nativeObject != null)
            {
                dynamic nativeDynamic = nativeObject;

                try
                {
                    nativeObjectType =
                        nativeDynamic.Type.ToString();
                }
                catch
                {
                    nativeObjectType =
                        nativeObject.GetType().Name;
                }
            }
        }
        catch
        {
        }

        try
        {
            object? parent = value.Parent;

            if (parent != null)
            {
                dynamic parentDynamic = parent;

                try
                {
                    parentType =
                        parentDynamic.Type.ToString();
                }
                catch
                {
                    parentType =
                        parent.GetType().Name;
                }

                try
                {
                    parentName =
                        (string)parentDynamic.Name;
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        try
        {
            dynamic occurrence = value.ContainingOccurrence;

            if (occurrence != null)
            {
                try
                {
                    occurrenceName = (string)occurrence.Name;
                }
                catch
                {
                }

                try
                {
                    occurrencePath =
                        (string)occurrence.OccurrencePath;
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        try
        {
            object? geometry = value.Geometry;

            if (geometry != null)
            {
                dynamic geometryDynamic = geometry;

                try
                {
                    geometryType =
                        geometryDynamic.Type.ToString();
                }
                catch
                {
                    geometryType =
                        geometry.GetType().Name;
                }
            }
        }
        catch
        {
        }

        try
        {
            faceCount = (int)value.Faces.Count;
        }
        catch
        {
        }

        return new
        {
            runtimeType = modelObject.GetType().Name,
            inventorType = EmptyToNull(inventorType),
            name = EmptyToNull(name),
            nativeObjectType = EmptyToNull(nativeObjectType),
            parent =
                string.IsNullOrWhiteSpace(parentType) &&
                string.IsNullOrWhiteSpace(parentName)
                    ? null
                    : new
                    {
                        objectType = EmptyToNull(parentType),
                        name = EmptyToNull(parentName)
                    },
            containingOccurrence =
                string.IsNullOrWhiteSpace(occurrenceName)
                    ? null
                    : new
                    {
                        name = occurrenceName,
                        occurrencePath =
                            EmptyToNull(occurrencePath)
                    },
            geometryType = EmptyToNull(geometryType),
            adjacentFaceCount = faceCount
        };
    }

    public static string Success(object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            Options());
    }

    public static string Error(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                details
            },
            Options());
    }

    private static object? ReadPoint(
        DrawingCurve curve,
        string kind)
    {
        Point2d? point = null;

        try
        {
            point = kind switch
            {
                "start" => curve.StartPoint,
                "end" => curve.EndPoint,
                "mid" => curve.MidPoint,
                "center" => curve.CenterPoint,
                _ => null
            };
        }
        catch
        {
        }

        return point == null
            ? null
            : new
            {
                x = point.X,
                y = point.Y
            };
    }

    private static string SafeProjectedType(
        DrawingCurve curve)
    {
        try
        {
            return curve.ProjectedCurveType.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeEdgeType(
        DrawingCurve curve)
    {
        try
        {
            return curve.EdgeType.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static int SafeSegmentCount(
        DrawingCurve curve)
    {
        try
        {
            return curve.Segments.Count;
        }
        catch
        {
            return 0;
        }
    }

    private static string? EmptyToNull(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    private static JsonSerializerOptions Options()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
