using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ModelConstraintReadSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        Document? document =
            inventor.ActiveDocument;

        if (document == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        if (document.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Активный документ не является чертежом.";

            return null;
        }

        return (DrawingDocument)document;
    }

    public static PartDocument? ResolvePartDocument(
        Inventor.Application inventor,
        JsonElement root,
        out DrawingDocument? drawing,
        out Sheet? sheet,
        out DrawingView? view,
        out string? error)
    {
        drawing = null;
        sheet = null;
        view = null;
        error = null;

        drawing =
            GetActiveDrawingDocument(
                inventor,
                out error);

        if (drawing == null)
        {
            return null;
        }

        if (!TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out error))
        {
            return null;
        }

        if (!TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out error))
        {
            return null;
        }

        sheet =
            FindSheet(
                drawing,
                sheetName);

        if (sheet == null)
        {
            error =
                $"Лист \"{sheetName}\" не найден.";

            return null;
        }

        view =
            FindView(
                sheet,
                viewName);

        if (view == null)
        {
            error =
                $"Вид \"{viewName}\" не найден.";

            return null;
        }

        try
        {
            Document referencedDocument =
                view
                    .ReferencedDocumentDescriptor
                    .ReferencedDocument;

            if (referencedDocument.DocumentType !=
                DocumentTypeEnum.kPartDocumentObject)
            {
                error =
                    "Документ, на который ссылается вид, не является деталью.";

                return null;
            }

            return
                (PartDocument)referencedDocument;
        }
        catch (Exception exception)
        {
            error =
                exception.Message;

            return null;
        }
    }

    public static Sheet? FindSheet(
        DrawingDocument drawing,
        string sheetName)
    {
        foreach (Sheet sheet in drawing.Sheets)
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

    public static DrawingView? FindView(
        Sheet sheet,
        string viewName)
    {
        foreach (DrawingView view in sheet.DrawingViews)
        {
            if (string.Equals(
                    view.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return view;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string? error)
    {
        value = string.Empty;
        error = null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            element.ValueKind !=
                JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (value.Length == 0)
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static bool GetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => defaultValue
        };
    }

    public static object ReadDocument(
        object documentObject)
    {
        dynamic document =
            documentObject;

        return new
        {
            displayName =
                GetStringProperty(
                    documentObject,
                    "DisplayName"),

            fullFileName =
                GetStringProperty(
                    documentObject,
                    "FullFileName"),

            internalName =
                GetStringProperty(
                    documentObject,
                    "InternalName"),

            documentType =
                GetEnumStringProperty(
                    documentObject,
                    "DocumentType"),

            dirty =
                GetBooleanProperty(
                    documentObject,
                    "Dirty")
        };
    }

    public static List<object> ReadSketches(
        PartDocument partDocument)
    {
        List<object> result = new();

        object sketchesObject =
            partDocument
                .ComponentDefinition
                .Sketches;

        dynamic sketches =
            sketchesObject;

        int count =
            GetCollectionCount(
                sketchesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object sketchObject;

            try
            {
                sketchObject =
                    sketches[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadSketch(
                    sketchObject,
                    index));
        }

        return result;
    }

    public static object? FindSketchByName(
        PartDocument partDocument,
        string sketchName)
    {
        object sketchesObject =
            partDocument
                .ComponentDefinition
                .Sketches;

        dynamic sketches =
            sketchesObject;

        int count =
            GetCollectionCount(
                sketchesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object sketchObject;

            try
            {
                sketchObject =
                    sketches[index];
            }
            catch
            {
                continue;
            }

            string name =
                GetStringProperty(
                    sketchObject,
                    "Name");

            if (string.Equals(
                    name,
                    sketchName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sketchObject;
            }
        }

        return null;
    }

    public static List<object> ReadSketchGeometry(
        object sketchObject)
    {
        List<object> result = new();

        object? entitiesObject =
            GetObjectProperty(
                sketchObject,
                "SketchEntities");

        if (entitiesObject == null)
        {
            return result;
        }

        dynamic entities =
            entitiesObject;

        int count =
            GetCollectionCount(
                entitiesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object entityObject;

            try
            {
                entityObject =
                    entities[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadSketchEntity(
                    entityObject,
                    index));
        }

        return result;
    }

    public static List<object> ReadSketchConstraints(
        object sketchObject)
    {
        List<object> result = new();

        object? constraintsObject =
            GetObjectProperty(
                sketchObject,
                "GeometricConstraints");

        if (constraintsObject == null)
        {
            return result;
        }

        dynamic constraints =
            constraintsObject;

        int count =
            GetCollectionCount(
                constraintsObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object constraintObject;

            try
            {
                constraintObject =
                    constraints[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                new
                {
                    index,

                    objectTypeRaw =
                        GetEnumRawValue(
                            constraintObject,
                            "Type"),

                    objectType =
                        GetObjectTypeName(
                            constraintObject),

                    constraintType =
                        GetConstraintTypeName(
                            constraintObject),

                    suppressed =
                        GetBooleanProperty(
                            constraintObject,
                            "Suppressed"),

                    deletable =
                        GetBooleanProperty(
                            constraintObject,
                            "Deletable"),

                    geometryOne =
                        ReadNamedObject(
                            GetObjectProperty(
                                constraintObject,
                                "EntityOne")),

                    geometryTwo =
                        ReadNamedObject(
                            GetObjectProperty(
                                constraintObject,
                                "EntityTwo")),

                    geometryThree =
                        ReadNamedObject(
                            GetObjectProperty(
                                constraintObject,
                                "EntityThree"))
                });
        }

        return result;
    }

    public static List<object> ReadSketchDimensions(
        object sketchObject)
    {
        List<object> result = new();

        object? dimensionsObject =
            GetObjectProperty(
                sketchObject,
                "DimensionConstraints");

        if (dimensionsObject == null)
        {
            return result;
        }

        dynamic dimensions =
            dimensionsObject;

        int count =
            GetCollectionCount(
                dimensionsObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object dimensionObject;

            try
            {
                dimensionObject =
                    dimensions[index];
            }
            catch
            {
                continue;
            }

            object? parameterObject =
                GetObjectProperty(
                    dimensionObject,
                    "Parameter");

            result.Add(
                new
                {
                    index,

                    objectTypeRaw =
                        GetEnumRawValue(
                            dimensionObject,
                            "Type"),

                    objectType =
                        GetObjectTypeName(
                            dimensionObject),

                    dimensionType =
                        GetDimensionTypeName(
                            dimensionObject),

                    driven =
                        GetBooleanProperty(
                            dimensionObject,
                            "Driven"),

                    suppressed =
                        GetBooleanProperty(
                            dimensionObject,
                            "Suppressed"),

                    textPoint =
                        ReadPoint2d(
                            GetObjectProperty(
                                dimensionObject,
                                "TextPoint")),

                    parameter =
                        ReadParameter(
                            parameterObject),

                    geometryOne =
                        ReadNamedObject(
                            GetObjectProperty(
                                dimensionObject,
                                "EntityOne")),

                    geometryTwo =
                        ReadNamedObject(
                            GetObjectProperty(
                                dimensionObject,
                                "EntityTwo"))
                });
        }

        return result;
    }

    public static object ReadWorkFeatures(
        PartDocument partDocument)
    {
        PartComponentDefinition definition =
            partDocument.ComponentDefinition;

        List<object> planes =
            ReadWorkFeatureCollection(
                definition.WorkPlanes,
                "work_plane");

        List<object> axes =
            ReadWorkFeatureCollection(
                definition.WorkAxes,
                "work_axis");

        List<object> points =
            ReadWorkFeatureCollection(
                definition.WorkPoints,
                "work_point");

        return new
        {
            workPlaneCount =
                planes.Count,

            workPlanes =
                planes,

            workAxisCount =
                axes.Count,

            workAxes =
                axes,

            workPointCount =
                points.Count,

            workPoints =
                points
        };
    }

    private static object ReadSketch(
        object sketchObject,
        int index)
    {
        return new
        {
            index,

            name =
                GetStringProperty(
                    sketchObject,
                    "Name"),

            objectType =
                GetEnumStringProperty(
                    sketchObject,
                    "Type"),

            visible =
                GetBooleanProperty(
                    sketchObject,
                    "Visible"),

            consumed =
                GetBooleanProperty(
                    sketchObject,
                    "Consumed"),

            shared =
                GetBooleanProperty(
                    sketchObject,
                    "Shared"),

            isActive =
                GetBooleanProperty(
                    sketchObject,
                    "Edit"),

            geometryCount =
                GetNestedCollectionCount(
                    sketchObject,
                    "SketchEntities"),

            geometricConstraintCount =
                GetNestedCollectionCount(
                    sketchObject,
                    "GeometricConstraints"),

            dimensionConstraintCount =
                GetNestedCollectionCount(
                    sketchObject,
                    "DimensionConstraints"),

            profileCount =
                GetNestedCollectionCount(
                    sketchObject,
                    "Profiles"),

            planarity =
                GetEnumStringProperty(
                    sketchObject,
                    "PlanarSketchType"),

            reference =
                GetBooleanProperty(
                    sketchObject,
                    "Reference")
        };
    }

    private static object ReadSketchEntity(
        object entityObject,
        int index)
    {
        object? geometryObject =
            GetObjectProperty(
                entityObject,
                "Geometry");

        return new
        {
            index,

            name =
                GetStringProperty(
                    entityObject,
                    "Name"),

            objectTypeRaw =
                GetEnumRawValue(
                    entityObject,
                    "Type"),

            objectType =
                GetObjectTypeName(
                    entityObject),

            geometryTypeRaw =
                GetEnumRawValue(
                    entityObject,
                    "GeometryType"),

            geometryType =
                GetGeometryTypeName(
                    entityObject),

            construction =
                GetBooleanProperty(
                    entityObject,
                    "Construction"),

            reference =
                GetBooleanProperty(
                    entityObject,
                    "Reference"),

            grounded =
                GetBooleanProperty(
                    entityObject,
                    "Grounded"),

            startPoint =
                ReadPoint2d(
                    GetNestedObjectProperty(
                        entityObject,
                        "StartSketchPoint",
                        "Geometry")),

            endPoint =
                ReadPoint2d(
                    GetNestedObjectProperty(
                        entityObject,
                        "EndSketchPoint",
                        "Geometry")),

            centerPoint =
                ReadPoint2d(
                    GetNestedObjectProperty(
                        entityObject,
                        "CenterSketchPoint",
                        "Geometry")),

            geometry =
                ReadGeometry2d(
                    geometryObject)
        };
    }

    private static object? ReadParameter(
        object? parameterObject)
    {
        if (parameterObject == null)
        {
            return null;
        }

        return new
        {
            name =
                GetStringProperty(
                    parameterObject,
                    "Name"),

            expression =
                GetStringProperty(
                    parameterObject,
                    "Expression"),

            value =
                GetNullableDoubleProperty(
                    parameterObject,
                    "Value"),

            modelValue =
                GetNullableDoubleProperty(
                    parameterObject,
                    "ModelValue"),

            units =
                GetStringProperty(
                    parameterObject,
                    "Units"),

            comment =
                GetStringProperty(
                    parameterObject,
                    "Comment"),

            exposedAsProperty =
                GetBooleanProperty(
                    parameterObject,
                    "ExposedAsProperty")
        };
    }

    private static object? ReadGeometry2d(
        object? geometryObject)
    {
        if (geometryObject == null)
        {
            return null;
        }

        return new
        {
            runtimeType =
                geometryObject
                    .GetType()
                    .Name,

            objectTypeRaw =
                GetEnumRawValue(
                    geometryObject,
                    "Type"),

            objectType =
                GetGeometryObjectTypeName(
                    geometryObject),

            startPoint =
                ReadPoint2d(
                    GetObjectProperty(
                        geometryObject,
                        "StartPoint")),

            endPoint =
                ReadPoint2d(
                    GetObjectProperty(
                        geometryObject,
                        "EndPoint")),

            center =
                ReadPoint2d(
                    GetObjectProperty(
                        geometryObject,
                        "Center")),

            radius =
                GetNullableDoubleProperty(
                    geometryObject,
                    "Radius"),

            majorRadius =
                GetNullableDoubleProperty(
                    geometryObject,
                    "MajorRadius"),

            minorRadius =
                GetNullableDoubleProperty(
                    geometryObject,
                    "MinorRadius"),

            startAngle =
                GetNullableDoubleProperty(
                    geometryObject,
                    "StartAngle"),

            sweepAngle =
                GetNullableDoubleProperty(
                    geometryObject,
                    "SweepAngle"),

            direction =
                ReadVector2d(
                    GetObjectProperty(
                        geometryObject,
                        "Direction"))
        };
    }

    private static object? ReadPoint2d(
        object? pointObject)
    {
        if (pointObject == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDoubleProperty(
                    pointObject,
                    "X"),

            y =
                GetNullableDoubleProperty(
                    pointObject,
                    "Y")
        };
    }

    private static object? ReadVector2d(
        object? vectorObject)
    {
        if (vectorObject == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDoubleProperty(
                    vectorObject,
                    "X"),

            y =
                GetNullableDoubleProperty(
                    vectorObject,
                    "Y"),

            length =
                GetNullableDoubleProperty(
                    vectorObject,
                    "Length")
        };
    }

    private static object? ReadNamedObject(
        object? value)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            name =
                GetStringProperty(
                    value,
                    "Name"),

            objectType =
                GetEnumStringProperty(
                    value,
                    "Type")
        };
    }

    private static List<object> ReadWorkFeatureCollection(
        object collectionObject,
        string kind)
    {
        List<object> result = new();

        dynamic collection =
            collectionObject;

        int count =
            GetCollectionCount(
                collectionObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object itemObject;

            try
            {
                itemObject =
                    collection[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                new
                {
                    kind,
                    index,

                    name =
                        GetStringProperty(
                            itemObject,
                            "Name"),

                    objectTypeRaw =
                        GetEnumRawValue(
                            itemObject,
                            "Type"),

                    objectType =
                        GetObjectTypeName(
                            itemObject),

                    visible =
                        GetBooleanProperty(
                            itemObject,
                            "Visible"),

                    grounded =
                        GetBooleanProperty(
                            itemObject,
                            "Grounded"),

                    consumed =
                        GetBooleanProperty(
                            itemObject,
                            "Consumed"),

                    construction =
                        GetBooleanProperty(
                            itemObject,
                            "Construction"),

                    definitionTypeRaw =
                        GetNestedEnumRawValue(
                            itemObject,
                            "Definition",
                            "Type"),

                    definitionType =
                        GetNestedObjectTypeName(
                            itemObject,
                            "Definition"),

                    point =
                        ReadPoint3d(
                            GetObjectProperty(
                                itemObject,
                                "Point")),

                    line =
                        ReadLine3d(
                            GetObjectProperty(
                                itemObject,
                                "Line")),

                    plane =
                        ReadPlane3d(
                            GetObjectProperty(
                                itemObject,
                                "Plane"))
                });
        }

        return result;
    }

    private static object? ReadPoint3d(
        object? pointObject)
    {
        if (pointObject == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDoubleProperty(
                    pointObject,
                    "X"),

            y =
                GetNullableDoubleProperty(
                    pointObject,
                    "Y"),

            z =
                GetNullableDoubleProperty(
                    pointObject,
                    "Z")
        };
    }

    private static object? ReadLine3d(
        object? lineObject)
    {
        if (lineObject == null)
        {
            return null;
        }

        return new
        {
            rootPoint =
                ReadPoint3d(
                    GetObjectProperty(
                        lineObject,
                        "RootPoint")),

            direction =
                ReadVector3d(
                    GetObjectProperty(
                        lineObject,
                        "Direction"))
        };
    }

    private static object? ReadPlane3d(
        object? planeObject)
    {
        if (planeObject == null)
        {
            return null;
        }

        return new
        {
            rootPoint =
                ReadPoint3d(
                    GetObjectProperty(
                        planeObject,
                        "RootPoint")),

            normal =
                ReadVector3d(
                    GetObjectProperty(
                        planeObject,
                        "Normal"))
        };
    }

    private static object? ReadVector3d(
        object? vectorObject)
    {
        if (vectorObject == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDoubleProperty(
                    vectorObject,
                    "X"),

            y =
                GetNullableDoubleProperty(
                    vectorObject,
                    "Y"),

            z =
                GetNullableDoubleProperty(
                    vectorObject,
                    "Z"),

            length =
                GetNullableDoubleProperty(
                    vectorObject,
                    "Length")
        };
    }

    private static object? GetObjectProperty(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "SketchEntities" => owner.SketchEntities,
                "GeometricConstraints" => owner.GeometricConstraints,
                "DimensionConstraints" => owner.DimensionConstraints,
                "Profiles" => owner.Profiles,
                "Geometry" => owner.Geometry,
                "StartSketchPoint" => owner.StartSketchPoint,
                "EndSketchPoint" => owner.EndSketchPoint,
                "CenterSketchPoint" => owner.CenterSketchPoint,
                "Parameter" => owner.Parameter,
                "TextPoint" => owner.TextPoint,
                "EntityOne" => owner.EntityOne,
                "EntityTwo" => owner.EntityTwo,
                "EntityThree" => owner.EntityThree,
                "StartPoint" => owner.StartPoint,
                "EndPoint" => owner.EndPoint,
                "Center" => owner.Center,
                "Direction" => owner.Direction,
                "Definition" => owner.Definition,
                "Point" => owner.Point,
                "Line" => owner.Line,
                "Plane" => owner.Plane,
                "RootPoint" => owner.RootPoint,
                "Normal" => owner.Normal,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? GetNestedObjectProperty(
        object ownerObject,
        string firstProperty,
        string secondProperty)
    {
        object? first =
            GetObjectProperty(
                ownerObject,
                firstProperty);

        return first == null
            ? null
            : GetObjectProperty(
                first,
                secondProperty);
    }

    private static string GetStringProperty(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            object? value =
                propertyName switch
                {
                    "Name" => owner.Name,
                    "DisplayName" => owner.DisplayName,
                    "FullFileName" => owner.FullFileName,
                    "InternalName" => owner.InternalName,
                    "Expression" => owner.Expression,
                    "Units" => owner.Units,
                    "Comment" => owner.Comment,
                    _ => null
                };

            return value?.ToString() ??
                   string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetObjectTypeName(
        object ownerObject)
    {
        object? value =
            GetRawPropertyValue(
                ownerObject,
                "Type");

        return GetEnumName(
            typeof(ObjectTypeEnum),
            value);
    }

    private static string GetNestedObjectTypeName(
        object ownerObject,
        string nestedPropertyName)
    {
        object? nested =
            GetObjectProperty(
                ownerObject,
                nestedPropertyName);

        return nested == null
            ? string.Empty
            : GetObjectTypeName(
                nested);
    }

    private static string GetConstraintTypeName(
        object constraintObject)
    {
        string explicitName =
            GetEnumNameFromProperty(
                constraintObject,
                "ConstraintType");

        if (!string.IsNullOrWhiteSpace(
                explicitName))
        {
            return explicitName;
        }

        string objectTypeName =
            GetObjectTypeName(
                constraintObject);

        return SimplifyInventorTypeName(
            objectTypeName,
            "Constraint");
    }

    private static string GetDimensionTypeName(
        object dimensionObject)
    {
        string explicitName =
            GetEnumNameFromProperty(
                dimensionObject,
                "ConstraintType");

        if (!string.IsNullOrWhiteSpace(
                explicitName))
        {
            return explicitName;
        }

        string objectTypeName =
            GetObjectTypeName(
                dimensionObject);

        return SimplifyInventorTypeName(
            objectTypeName,
            "Constraint");
    }

    private static string GetGeometryTypeName(
        object entityObject)
    {
        string explicitName =
            GetEnumNameFromProperty(
                entityObject,
                "GeometryType");

        if (!string.IsNullOrWhiteSpace(
                explicitName))
        {
            return explicitName;
        }

        string objectTypeName =
            GetObjectTypeName(
                entityObject);

        return SimplifyInventorTypeName(
            objectTypeName,
            "Object");
    }

    private static string GetGeometryObjectTypeName(
        object geometryObject)
    {
        object? value =
            GetRawPropertyValue(
                geometryObject,
                "Type");

        if (value == null)
        {
            return geometryObject
                .GetType()
                .Name;
        }

        string objectTypeName =
            GetEnumName(
                typeof(ObjectTypeEnum),
                value);

        return string.IsNullOrWhiteSpace(
                objectTypeName)
            ? value.ToString() ??
              geometryObject.GetType().Name
            : objectTypeName;
    }

    private static string GetEnumNameFromProperty(
        object ownerObject,
        string propertyName)
    {
        object? value =
            GetRawPropertyValue(
                ownerObject,
                propertyName);

        if (value == null)
        {
            return string.Empty;
        }

        Type valueType =
            value.GetType();

        if (valueType.IsEnum)
        {
            return Enum.GetName(
                       valueType,
                       value) ??
                   value.ToString() ??
                   string.Empty;
        }

        return string.Empty;
    }

    private static string GetEnumRawValue(
        object ownerObject,
        string propertyName)
    {
        object? value =
            GetRawPropertyValue(
                ownerObject,
                propertyName);

        return value?.ToString() ??
               string.Empty;
    }

    private static string GetNestedEnumRawValue(
        object ownerObject,
        string nestedPropertyName,
        string enumPropertyName)
    {
        object? nested =
            GetObjectProperty(
                ownerObject,
                nestedPropertyName);

        return nested == null
            ? string.Empty
            : GetEnumRawValue(
                nested,
                enumPropertyName);
    }

    private static object? GetRawPropertyValue(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "Type" => owner.Type,
                "DocumentType" => owner.DocumentType,
                "ConstraintType" => owner.ConstraintType,
                "GeometryType" => owner.GeometryType,
                "PlanarSketchType" => owner.PlanarSketchType,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static string GetEnumName(
        Type enumType,
        object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            int numericValue =
                Convert.ToInt32(value);

            return Enum.GetName(
                       enumType,
                       numericValue) ??
                   numericValue.ToString();
        }
        catch
        {
            return value.ToString() ??
                   string.Empty;
        }
    }

    private static string SimplifyInventorTypeName(
        string value,
        string suffixToRemove)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        string result =
            value;

        if (result.StartsWith(
                "k",
                StringComparison.Ordinal))
        {
            result =
                result[1..];
        }

        if (result.EndsWith(
                suffixToRemove,
                StringComparison.Ordinal))
        {
            result =
                result[
                    ..^suffixToRemove.Length];
        }

        return result;
    }

    private static string GetEnumStringProperty(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            object? value =
                propertyName switch
                {
                    "Type" => owner.Type,
                    "DocumentType" => owner.DocumentType,
                    "ConstraintType" => owner.ConstraintType,
                    "GeometryType" => owner.GeometryType,
                    "PlanarSketchType" => owner.PlanarSketchType,
                    _ => null
                };

            return value?.ToString() ??
                   string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetNestedEnumStringProperty(
        object ownerObject,
        string objectProperty,
        string enumProperty)
    {
        object? nestedObject =
            GetObjectProperty(
                ownerObject,
                objectProperty);

        return nestedObject == null
            ? string.Empty
            : GetEnumStringProperty(
                nestedObject,
                enumProperty);
    }

    private static bool GetBooleanProperty(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "Visible" => (bool)owner.Visible,
                "Consumed" => (bool)owner.Consumed,
                "Shared" => (bool)owner.Shared,
                "Edit" => (bool)owner.Edit,
                "Reference" => (bool)owner.Reference,
                "Construction" => (bool)owner.Construction,
                "Grounded" => (bool)owner.Grounded,
                "Suppressed" => (bool)owner.Suppressed,
                "Deletable" => (bool)owner.Deletable,
                "Driven" => (bool)owner.Driven,
                "ExposedAsProperty" => (bool)owner.ExposedAsProperty,
                "Dirty" => (bool)owner.Dirty,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private static double? GetNullableDoubleProperty(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "X" => (double)owner.X,
                "Y" => (double)owner.Y,
                "Z" => (double)owner.Z,
                "Length" => (double)owner.Length,
                "Radius" => (double)owner.Radius,
                "MajorRadius" => (double)owner.MajorRadius,
                "MinorRadius" => (double)owner.MinorRadius,
                "StartAngle" => (double)owner.StartAngle,
                "SweepAngle" => (double)owner.SweepAngle,
                "Value" => (double)owner.Value,
                "ModelValue" => (double)owner.ModelValue,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static int GetCollectionCount(
        object collectionObject)
    {
        dynamic collection =
            collectionObject;

        try
        {
            return (int)collection.Count;
        }
        catch
        {
            return 0;
        }
    }

    private static int GetNestedCollectionCount(
        object ownerObject,
        string propertyName)
    {
        object? collection =
            GetObjectProperty(
                ownerObject,
                propertyName);

        return collection == null
            ? 0
            : GetCollectionCount(
                collection);
    }

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
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
            CreateJsonOptions());
    }

    private static JsonSerializerOptions
        CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}
