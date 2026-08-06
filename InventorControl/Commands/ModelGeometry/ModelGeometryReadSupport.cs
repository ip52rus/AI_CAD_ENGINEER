using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ModelGeometryReadSupport
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

    public static Document? GetReferencedDocument(
        DrawingView view,
        out string? error)
    {
        error = null;

        try
        {
            return
                view
                    .ReferencedDocumentDescriptor
                    .ReferencedDocument;
        }
        catch (Exception exception)
        {
            error =
                exception.Message;

            return null;
        }
    }

    public static PartDocument? GetReferencedPartDocument(
        DrawingView view,
        out string? error)
    {
        Document? document =
            GetReferencedDocument(
                view,
                out error);

        if (document == null)
        {
            return null;
        }

        if (document.DocumentType !=
            DocumentTypeEnum.kPartDocumentObject)
        {
            error =
                "Документ, на который ссылается вид, не является деталью.";

            return null;
        }

        return (PartDocument)document;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value = string.Empty;
        error = string.Empty;

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

    public static bool TryGetRequiredInt32(
        JsonElement root,
        string propertyName,
        out int value,
        out string error)
    {
        value = 0;
        error = string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            !element.TryGetInt32(
                out value))
        {
            error =
                $"Поле \"{propertyName}\" должно быть целым числом.";

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

        if (element.ValueKind ==
            JsonValueKind.True)
        {
            return true;
        }

        if (element.ValueKind ==
            JsonValueKind.False)
        {
            return false;
        }

        return defaultValue;
    }

    public static object ReadDocument(
        object documentObject)
    {
        ArgumentNullException.ThrowIfNull(
            documentObject);

        dynamic document =
            documentObject;

        string displayName =
            string.Empty;

        string fullFileName =
            string.Empty;

        string internalName =
            string.Empty;

        string documentType =
            string.Empty;

        bool dirty =
            false;

        try
        {
            displayName =
                (string)document.DisplayName;
        }
        catch
        {
        }

        try
        {
            fullFileName =
                (string)document.FullFileName;
        }
        catch
        {
        }

        try
        {
            internalName =
                (string)document.InternalName;
        }
        catch
        {
        }

        try
        {
            documentType =
                document.DocumentType.ToString();
        }
        catch
        {
        }

        try
        {
            dirty =
                (bool)document.Dirty;
        }
        catch
        {
        }

        return new
        {
            displayName,
            fullFileName,
            internalName,
            documentType,
            dirty
        };
    }

    public static object? FindFeatureByName(
        PartDocument partDocument,
        string featureName)
    {
        object featuresObject =
            partDocument
                .ComponentDefinition
                .Features;

        string[] collectionNames =
        {
            "ExtrudeFeatures",
            "RevolveFeatures",
            "HoleFeatures",
            "FilletFeatures",
            "ChamferFeatures",
            "ThreadFeatures",
            "ShellFeatures",
            "RectangularPatternFeatures",
            "CircularPatternFeatures",
            "MirrorFeatures",
            "SweepFeatures",
            "LoftFeatures",
            "CoilFeatures",
            "SplitFeatures",
            "MoveFaceFeatures",
            "DirectEditFeatures"
        };

        foreach (string collectionName in collectionNames)
        {
            object? collection =
                GetFeatureCollection(
                    featuresObject,
                    collectionName);

            if (collection == null)
            {
                continue;
            }

            int count =
                GetCollectionCount(
                    collection);

            dynamic dynamicCollection =
                collection;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object featureObject;

                try
                {
                    featureObject =
                        dynamicCollection[index];
                }
                catch
                {
                    continue;
                }

                string name =
                    GetStringProperty(
                        featureObject,
                        "Name");

                if (string.Equals(
                        name,
                        featureName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return featureObject;
                }
            }
        }

        return null;
    }

    public static object ReadFeatureDetails(
        object featureObject)
    {
        return new
        {
            name =
                GetStringProperty(
                    featureObject,
                    "Name"),

            objectType =
                GetEnumStringProperty(
                    featureObject,
                    "Type"),

            suppressed =
                GetBooleanProperty(
                    featureObject,
                    "Suppressed"),

            consumed =
                GetBooleanProperty(
                    featureObject,
                    "Consumed"),

            healthStatus =
                GetEnumStringProperty(
                    featureObject,
                    "HealthStatus"),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "SurfaceBodies"),

            participantBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "ParticipantBodies"),

            faceCount =
                GetNestedCollectionCount(
                    featureObject,
                    "Faces"),

            edgeCount =
                GetNestedCollectionCount(
                    featureObject,
                    "Edges"),

            parent =
                ReadNamedObject(
                    GetObjectProperty(
                        featureObject,
                        "Parent")),

            profile =
                ReadNamedObject(
                    GetObjectProperty(
                        featureObject,
                        "Profile")),

            sketch =
                ReadNamedObject(
                    GetObjectProperty(
                        featureObject,
                        "Sketch")),

            parameters =
                ReadFeatureParameters(
                    featureObject)
        };
    }

    public static List<object> ReadAllParameters(
        PartDocument partDocument,
        bool includeModel,
        bool includeUser,
        bool includeReference)
    {
        List<object> result = new();

        object parametersObject =
            partDocument
                .ComponentDefinition
                .Parameters;

        if (includeModel)
        {
            ReadParameterCollection(
                parametersObject,
                "ModelParameters",
                "model",
                result);
        }

        if (includeUser)
        {
            ReadParameterCollection(
                parametersObject,
                "UserParameters",
                "user",
                result);
        }

        if (includeReference)
        {
            ReadParameterCollection(
                parametersObject,
                "ReferenceParameters",
                "reference",
                result);
        }

        return result;
    }

    public static List<object> ReadSurfaceBodies(
        PartDocument partDocument)
    {
        List<object> result = new();

        object bodiesObject =
            partDocument
                .ComponentDefinition
                .SurfaceBodies;

        dynamic bodies =
            bodiesObject;

        int count =
            GetCollectionCount(
                bodiesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object bodyObject;

            try
            {
                bodyObject =
                    bodies[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadSurfaceBody(
                    bodyObject,
                    index));
        }

        return result;
    }

    public static object? ReadBodyFaces(
        PartDocument partDocument,
        int bodyIndex)
    {
        object bodiesObject =
            partDocument
                .ComponentDefinition
                .SurfaceBodies;

        int bodyCount =
            GetCollectionCount(
                bodiesObject);

        if (bodyIndex < 1 ||
            bodyIndex > bodyCount)
        {
            return null;
        }

        dynamic bodies =
            bodiesObject;

        object bodyObject =
            bodies[bodyIndex];

        object? facesObject =
            GetObjectProperty(
                bodyObject,
                "Faces");

        if (facesObject == null)
        {
            return new
            {
                bodyIndex,
                faceCount = 0,
                faces =
                    Array.Empty<object>()
            };
        }

        dynamic faces =
            facesObject;

        int faceCount =
            GetCollectionCount(
                facesObject);

        List<object> result = new();

        for (int faceIndex = 1;
             faceIndex <= faceCount;
             faceIndex++)
        {
            object faceObject;

            try
            {
                faceObject =
                    faces[faceIndex];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadFace(
                    faceObject,
                    faceIndex));
        }

        return new
        {
            bodyIndex,
            body =
                ReadSurfaceBody(
                    bodyObject,
                    bodyIndex),

            faceCount =
                result.Count,

            faces =
                result
        };
    }

    public static object? ReadFaceEdges(
        PartDocument partDocument,
        int bodyIndex,
        int faceIndex)
    {
        object bodiesObject =
            partDocument
                .ComponentDefinition
                .SurfaceBodies;

        int bodyCount =
            GetCollectionCount(
                bodiesObject);

        if (bodyIndex < 1 ||
            bodyIndex > bodyCount)
        {
            return null;
        }

        dynamic bodies =
            bodiesObject;

        object bodyObject =
            bodies[bodyIndex];

        object? facesObject =
            GetObjectProperty(
                bodyObject,
                "Faces");

        if (facesObject == null)
        {
            return null;
        }

        int faceCount =
            GetCollectionCount(
                facesObject);

        if (faceIndex < 1 ||
            faceIndex > faceCount)
        {
            return null;
        }

        dynamic faces =
            facesObject;

        object faceObject =
            faces[faceIndex];

        object? edgesObject =
            GetObjectProperty(
                faceObject,
                "Edges");

        if (edgesObject == null)
        {
            return new
            {
                bodyIndex,
                faceIndex,
                edgeCount = 0,
                edges =
                    Array.Empty<object>()
            };
        }

        dynamic edges =
            edgesObject;

        int edgeCount =
            GetCollectionCount(
                edgesObject);

        List<object> result = new();

        for (int edgeIndex = 1;
             edgeIndex <= edgeCount;
             edgeIndex++)
        {
            object edgeObject;

            try
            {
                edgeObject =
                    edges[edgeIndex];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadEdge(
                    edgeObject,
                    edgeIndex));
        }

        return new
        {
            bodyIndex,
            faceIndex,
            face =
                ReadFace(
                    faceObject,
                    faceIndex),

            edgeCount =
                result.Count,

            edges =
                result
        };
    }

    private static object ReadSurfaceBody(
        object bodyObject,
        int index)
    {
        return new
        {
            index,

            name =
                GetStringProperty(
                    bodyObject,
                    "Name"),

            objectType =
                GetEnumStringProperty(
                    bodyObject,
                    "Type"),

            visible =
                GetBooleanProperty(
                    bodyObject,
                    "Visible"),

            faceCount =
                GetNestedCollectionCount(
                    bodyObject,
                    "Faces"),

            edgeCount =
                GetNestedCollectionCount(
                    bodyObject,
                    "Edges"),

            vertexCount =
                GetNestedCollectionCount(
                    bodyObject,
                    "Vertices"),

            rangeBox =
                ReadBox3d(
                    GetObjectProperty(
                        bodyObject,
                        "RangeBox")),

            volume =
                GetNullableDoubleProperty(
                    bodyObject,
                    "Volume"),

            area =
                GetNullableDoubleProperty(
                    bodyObject,
                    "Area")
        };
    }

    private static object ReadFace(
        object faceObject,
        int index)
    {
        return new
        {
            index,

            objectType =
                GetEnumStringProperty(
                    faceObject,
                    "Type"),

            surfaceType =
                GetEnumStringProperty(
                    faceObject,
                    "SurfaceType"),

            edgeCount =
                GetNestedCollectionCount(
                    faceObject,
                    "Edges"),

            loopCount =
                GetNestedCollectionCount(
                    faceObject,
                    "EdgeLoops"),

            area =
                ReadArea(
                    faceObject),

            geometry =
                ReadGeometry(
                    GetObjectProperty(
                        faceObject,
                        "Geometry")),

            createdByFeature =
                ReadNamedObject(
                    GetObjectProperty(
                        faceObject,
                        "CreatedByFeature")),

            rangeBox =
                ReadBox3d(
                    GetObjectProperty(
                        faceObject,
                        "RangeBox"))
        };
    }

    private static object ReadEdge(
        object edgeObject,
        int index)
    {
        return new
        {
            index,

            objectType =
                GetEnumStringProperty(
                    edgeObject,
                    "Type"),

            geometryType =
                GetEnumStringProperty(
                    edgeObject,
                    "GeometryType"),

            startPoint =
                ReadPoint3d(
                    GetNestedObjectProperty(
                        edgeObject,
                        "StartVertex",
                        "Point")),

            endPoint =
                ReadPoint3d(
                    GetNestedObjectProperty(
                        edgeObject,
                        "StopVertex",
                        "Point")),

            geometry =
                ReadGeometry(
                    GetObjectProperty(
                        edgeObject,
                        "Geometry")),

            adjacentFaceCount =
                GetNestedCollectionCount(
                    edgeObject,
                    "Faces"),

            length =
                ReadLength(
                    edgeObject)
        };
    }

    private static void ReadParameterCollection(
        object parametersObject,
        string collectionName,
        string category,
        List<object> result)
    {
        object? collection =
            GetParameterCollection(
                parametersObject,
                collectionName);

        if (collection == null)
        {
            return;
        }

        dynamic dynamicCollection =
            collection;

        int count =
            GetCollectionCount(
                collection);

        for (int index = 1;
             index <= count;
             index++)
        {
            object parameterObject;

            try
            {
                parameterObject =
                    dynamicCollection[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadParameter(
                    parameterObject,
                    category,
                    index));
        }
    }

    private static object ReadParameter(
        object parameterObject,
        string category,
        int index)
    {
        return new
        {
            category,
            index,

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
                    "ExposedAsProperty"),

            disabledActionTypes =
                GetEnumStringProperty(
                    parameterObject,
                    "DisabledActionTypes"),

            tolerance =
                ReadTolerance(
                    GetObjectProperty(
                        parameterObject,
                        "Tolerance"))
        };
    }

    private static List<object> ReadFeatureParameters(
        object featureObject)
    {
        List<object> result = new();

        string[] parameterPropertyNames =
        {
            "Distance",
            "DistanceTwo",
            "Angle",
            "HoleDiameter",
            "Depth",
            "CBoreDiameter",
            "CBoreDepth",
            "CSinkDiameter",
            "CSinkAngle",
            "Radius"
        };

        foreach (string propertyName in parameterPropertyNames)
        {
            object? parameterObject =
                GetObjectProperty(
                    featureObject,
                    propertyName);

            if (parameterObject == null)
            {
                continue;
            }

            result.Add(
                new
                {
                    property =
                        propertyName,

                    parameter =
                        ReadParameter(
                            parameterObject,
                            "feature",
                            result.Count + 1)
                });
        }

        return result;
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

    private static object? ReadTolerance(
        object? toleranceObject)
    {
        if (toleranceObject == null)
        {
            return null;
        }

        return new
        {
            toleranceType =
                GetEnumStringProperty(
                    toleranceObject,
                    "ToleranceType"),

            upper =
                GetNullableDoubleProperty(
                    toleranceObject,
                    "Upper"),

            lower =
                GetNullableDoubleProperty(
                    toleranceObject,
                    "Lower")
        };
    }

    private static object? ReadGeometry(
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

            objectType =
                GetEnumStringProperty(
                    geometryObject,
                    "Type"),

            center =
                ReadPoint3d(
                    GetObjectProperty(
                        geometryObject,
                        "Center")),

            origin =
                ReadPoint3d(
                    GetObjectProperty(
                        geometryObject,
                        "Origin")),

            axisVector =
                ReadVector3d(
                    GetObjectProperty(
                        geometryObject,
                        "AxisVector")),

            normal =
                ReadVector3d(
                    GetObjectProperty(
                        geometryObject,
                        "Normal")),

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

            halfAngle =
                GetNullableDoubleProperty(
                    geometryObject,
                    "HalfAngle")
        };
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

    private static object? ReadBox3d(
        object? boxObject)
    {
        if (boxObject == null)
        {
            return null;
        }

        return new
        {
            minPoint =
                ReadPoint3d(
                    GetObjectProperty(
                        boxObject,
                        "MinPoint")),

            maxPoint =
                ReadPoint3d(
                    GetObjectProperty(
                        boxObject,
                        "MaxPoint"))
        };
    }

    private static double? ReadArea(
        object faceObject)
    {
        object? evaluatorObject =
            GetObjectProperty(
                faceObject,
                "Evaluator");

        if (evaluatorObject == null)
        {
            return null;
        }

        dynamic evaluator =
            evaluatorObject;

        try
        {
            double area = 0.0;
            evaluator.GetArea(ref area);

            return area;
        }
        catch
        {
        }

        try
        {
            return (double)evaluator.Area;
        }
        catch
        {
            return null;
        }
    }

    private static double? ReadLength(
        object edgeObject)
    {
        object? evaluatorObject =
            GetObjectProperty(
                edgeObject,
                "Evaluator");

        if (evaluatorObject == null)
        {
            return null;
        }

        dynamic evaluator =
            evaluatorObject;

        try
        {
            double minParameter = 0.0;
            double maxParameter = 0.0;

            evaluator.GetParamExtents(
                ref minParameter,
                ref maxParameter);

            double length = 0.0;

            evaluator.GetLengthAtParam(
                minParameter,
                maxParameter,
                ref length);

            return length;
        }
        catch
        {
            return null;
        }
    }

    private static object? GetFeatureCollection(
        object featuresObject,
        string collectionName)
    {
        dynamic features =
            featuresObject;

        try
        {
            return collectionName switch
            {
                "ExtrudeFeatures" =>
                    features.ExtrudeFeatures,

                "RevolveFeatures" =>
                    features.RevolveFeatures,

                "HoleFeatures" =>
                    features.HoleFeatures,

                "FilletFeatures" =>
                    features.FilletFeatures,

                "ChamferFeatures" =>
                    features.ChamferFeatures,

                "ThreadFeatures" =>
                    features.ThreadFeatures,

                "ShellFeatures" =>
                    features.ShellFeatures,

                "RectangularPatternFeatures" =>
                    features.RectangularPatternFeatures,

                "CircularPatternFeatures" =>
                    features.CircularPatternFeatures,

                "MirrorFeatures" =>
                    features.MirrorFeatures,

                "SweepFeatures" =>
                    features.SweepFeatures,

                "LoftFeatures" =>
                    features.LoftFeatures,

                "CoilFeatures" =>
                    features.CoilFeatures,

                "SplitFeatures" =>
                    features.SplitFeatures,

                "MoveFaceFeatures" =>
                    features.MoveFaceFeatures,

                "DirectEditFeatures" =>
                    features.DirectEditFeatures,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? GetParameterCollection(
        object parametersObject,
        string collectionName)
    {
        dynamic parameters =
            parametersObject;

        try
        {
            return collectionName switch
            {
                "ModelParameters" =>
                    parameters.ModelParameters,

                "UserParameters" =>
                    parameters.UserParameters,

                "ReferenceParameters" =>
                    parameters.ReferenceParameters,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
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
                "SurfaceBodies" => owner.SurfaceBodies,
                "ParticipantBodies" => owner.ParticipantBodies,
                "Faces" => owner.Faces,
                "Edges" => owner.Edges,
                "Vertices" => owner.Vertices,
                "EdgeLoops" => owner.EdgeLoops,
                "RangeBox" => owner.RangeBox,
                "Parent" => owner.Parent,
                "Profile" => owner.Profile,
                "Sketch" => owner.Sketch,
                "CreatedByFeature" => owner.CreatedByFeature,
                "Geometry" => owner.Geometry,
                "Evaluator" => owner.Evaluator,
                "StartVertex" => owner.StartVertex,
                "StopVertex" => owner.StopVertex,
                "Point" => owner.Point,
                "Tolerance" => owner.Tolerance,
                "Center" => owner.Center,
                "Origin" => owner.Origin,
                "AxisVector" => owner.AxisVector,
                "Normal" => owner.Normal,
                "MinPoint" => owner.MinPoint,
                "MaxPoint" => owner.MaxPoint,
                "Distance" => owner.Distance,
                "DistanceTwo" => owner.DistanceTwo,
                "Angle" => owner.Angle,
                "HoleDiameter" => owner.HoleDiameter,
                "Depth" => owner.Depth,
                "CBoreDiameter" => owner.CBoreDiameter,
                "CBoreDepth" => owner.CBoreDepth,
                "CSinkDiameter" => owner.CSinkDiameter,
                "CSinkAngle" => owner.CSinkAngle,
                "Radius" => owner.Radius,
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
        string firstPropertyName,
        string secondPropertyName)
    {
        object? first =
            GetObjectProperty(
                ownerObject,
                firstPropertyName);

        return first == null
            ? null
            : GetObjectProperty(
                first,
                secondPropertyName);
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
                    "HealthStatus" => owner.HealthStatus,
                    "SurfaceType" => owner.SurfaceType,
                    "GeometryType" => owner.GeometryType,
                    "DisabledActionTypes" => owner.DisabledActionTypes,
                    "ToleranceType" => owner.ToleranceType,
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
                "Suppressed" => (bool)owner.Suppressed,
                "Consumed" => (bool)owner.Consumed,
                "Visible" => (bool)owner.Visible,
                "ExposedAsProperty" => (bool)owner.ExposedAsProperty,
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
                "Value" => (double)owner.Value,
                "ModelValue" => (double)owner.ModelValue,
                "Volume" => (double)owner.Volume,
                "Area" => (double)owner.Area,
                "Radius" => (double)owner.Radius,
                "MajorRadius" => (double)owner.MajorRadius,
                "MinorRadius" => (double)owner.MinorRadius,
                "HalfAngle" => (double)owner.HalfAngle,
                "X" => (double)owner.X,
                "Y" => (double)owner.Y,
                "Z" => (double)owner.Z,
                "Length" => (double)owner.Length,
                "Upper" => (double)owner.Upper,
                "Lower" => (double)owner.Lower,
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
