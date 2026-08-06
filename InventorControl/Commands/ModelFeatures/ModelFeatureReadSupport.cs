using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ModelFeatureReadSupport
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

        return
            (DrawingDocument)document;
    }

    public static Sheet? FindSheet(
        DrawingDocument drawing,
        string name)
    {
        ArgumentNullException.ThrowIfNull(drawing);

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
        ArgumentNullException.ThrowIfNull(sheet);

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

    public static Document? GetReferencedDocument(
        DrawingView view,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(view);

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

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value =
            string.Empty;

        error =
            string.Empty;

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
        Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        string fullFileName =
            string.Empty;

        string internalName =
            string.Empty;

        try
        {
            fullFileName =
                document.FullFileName;
        }
        catch
        {
        }

        try
        {
            internalName =
                document.InternalName;
        }
        catch
        {
        }

        return new
        {
            displayName =
                document.DisplayName,

            fullFileName,

            documentType =
                document.DocumentType.ToString(),

            dirty =
                document.Dirty,

            internalName
        };
    }

    public static List<object> ReadFeatureTree(
        Document document,
        bool includeSuppressed)
    {
        ArgumentNullException.ThrowIfNull(document);

        List<object> result =
            new();

        if (document.DocumentType !=
            DocumentTypeEnum.kPartDocumentObject)
        {
            return result;
        }

        PartDocument partDocument =
            (PartDocument)document;

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
            ReadFeatureCollection(
                featuresObject,
                collectionName,
                includeSuppressed,
                result);
        }

        return result;
    }

    public static List<object> ReadHoleFeatures(
        Document document,
        bool includeSuppressed)
    {
        ArgumentNullException.ThrowIfNull(document);

        List<object> result =
            new();

        if (document.DocumentType !=
            DocumentTypeEnum.kPartDocumentObject)
        {
            return result;
        }

        PartDocument partDocument =
            (PartDocument)document;

        object holeFeaturesObject =
            partDocument
                .ComponentDefinition
                .Features
                .HoleFeatures;

        dynamic holeFeatures =
            holeFeaturesObject;

        int count =
            GetCollectionCount(
                holeFeaturesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object featureObject;

            try
            {
                featureObject =
                    holeFeatures[index];
            }
            catch
            {
                continue;
            }

            bool suppressed =
                GetBooleanProperty(
                    featureObject,
                    "Suppressed");

            if (!includeSuppressed &&
                suppressed)
            {
                continue;
            }

            result.Add(
                ReadHoleFeature(
                    featureObject,
                    index));
        }

        return result;
    }

    private static void ReadFeatureCollection(
        object featuresObject,
        string collectionName,
        bool includeSuppressed,
        List<object> result)
    {
        object? collectionObject =
            GetFeatureCollection(
                featuresObject,
                collectionName);

        if (collectionObject == null)
        {
            return;
        }

        dynamic collection =
            collectionObject;

        int count =
            GetCollectionCount(
                collectionObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object featureObject;

            try
            {
                featureObject =
                    collection[index];
            }
            catch
            {
                continue;
            }

            bool suppressed =
                GetBooleanProperty(
                    featureObject,
                    "Suppressed");

            if (!includeSuppressed &&
                suppressed)
            {
                continue;
            }

            result.Add(
                ReadFeature(
                    featureObject,
                    collectionName,
                    index));
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

    private static object ReadFeature(
        object featureObject,
        string collectionName,
        int index)
    {
        return new
        {
            collection =
                collectionName,

            index,

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

            healthStatus =
                GetEnumStringProperty(
                    featureObject,
                    "HealthStatus"),

            consumed =
                GetBooleanProperty(
                    featureObject,
                    "Consumed"),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "SurfaceBodies"),

            participantBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "ParticipantBodies"),

            parentType =
                GetNestedEnumStringProperty(
                    featureObject,
                    "Parent",
                    "Type")
        };
    }

    private static object ReadHoleFeature(
        object featureObject,
        int index)
    {
        return new
        {
            index,

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

            healthStatus =
                GetEnumStringProperty(
                    featureObject,
                    "HealthStatus"),

            holeType =
                GetEnumStringProperty(
                    featureObject,
                    "HoleType"),

            extentType =
                GetEnumStringProperty(
                    featureObject,
                    "ExtentType"),

            diameter =
                ReadParameterProperty(
                    featureObject,
                    "HoleDiameter"),

            depth =
                ReadParameterProperty(
                    featureObject,
                    "Depth"),

            counterboreDiameter =
                ReadParameterProperty(
                    featureObject,
                    "CBoreDiameter"),

            counterboreDepth =
                ReadParameterProperty(
                    featureObject,
                    "CBoreDepth"),

            countersinkDiameter =
                ReadParameterProperty(
                    featureObject,
                    "CSinkDiameter"),

            countersinkAngle =
                ReadParameterProperty(
                    featureObject,
                    "CSinkAngle"),

            isThreaded =
                GetBooleanProperty(
                    featureObject,
                    "Tapped"),

            threadInfo =
                ReadThreadInfo(
                    featureObject),

            placementDefinitionType =
                GetNestedEnumStringProperty(
                    featureObject,
                    "PlacementDefinition",
                    "Type"),

            holeCenterCount =
                ReadHoleCenterCount(
                    featureObject),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "SurfaceBodies")
        };
    }

    private static object? ReadParameterProperty(
        object ownerObject,
        string propertyName)
    {
        object? parameterObject =
            GetObjectProperty(
                ownerObject,
                propertyName);

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

            units =
                GetStringProperty(
                    parameterObject,
                    "Units"),

            modelValue =
                GetNullableDoubleProperty(
                    parameterObject,
                    "ModelValue")
        };
    }

    private static object? ReadThreadInfo(
        object featureObject)
    {
        object? threadInfoObject =
            GetObjectProperty(
                featureObject,
                "ThreadInfo");

        if (threadInfoObject == null)
        {
            return null;
        }

        return new
        {
            threadType =
                GetStringProperty(
                    threadInfoObject,
                    "ThreadType"),

            designation =
                GetStringProperty(
                    threadInfoObject,
                    "ThreadDesignation"),

            threadClass =
                GetStringProperty(
                    threadInfoObject,
                    "Class"),

            fullThreadDepth =
                GetBooleanProperty(
                    threadInfoObject,
                    "FullThreadDepth"),

            threadDepth =
                ReadParameterProperty(
                    threadInfoObject,
                    "ThreadDepth")
        };
    }

    private static int ReadHoleCenterCount(
        object featureObject)
    {
        object? placementObject =
            GetObjectProperty(
                featureObject,
                "PlacementDefinition");

        if (placementObject == null)
        {
            return 0;
        }

        object? pointsObject =
            GetObjectProperty(
                placementObject,
                "HoleCenterPoints");

        return pointsObject == null
            ? 0
            : GetCollectionCount(
                pointsObject);
    }

    private static int GetCollectionCount(
        object collectionObject)
    {
        dynamic collection =
            collectionObject;

        try
        {
            return
                (int)collection.Count;
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
        object? collectionObject =
            GetObjectProperty(
                ownerObject,
                propertyName);

        return collectionObject == null
            ? 0
            : GetCollectionCount(
                collectionObject);
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
                "SurfaceBodies" =>
                    owner.SurfaceBodies,

                "ParticipantBodies" =>
                    owner.ParticipantBodies,

                "Parent" =>
                    owner.Parent,

                "PlacementDefinition" =>
                    owner.PlacementDefinition,

                "HoleCenterPoints" =>
                    owner.HoleCenterPoints,

                "HoleDiameter" =>
                    owner.HoleDiameter,

                "Depth" =>
                    owner.Depth,

                "CBoreDiameter" =>
                    owner.CBoreDiameter,

                "CBoreDepth" =>
                    owner.CBoreDepth,

                "CSinkDiameter" =>
                    owner.CSinkDiameter,

                "CSinkAngle" =>
                    owner.CSinkAngle,

                "ThreadInfo" =>
                    owner.ThreadInfo,

                "ThreadDepth" =>
                    owner.ThreadDepth,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
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
                    "Name" =>
                        owner.Name,

                    "Expression" =>
                        owner.Expression,

                    "Units" =>
                        owner.Units,

                    "ThreadType" =>
                        owner.ThreadType,

                    "ThreadDesignation" =>
                        owner.ThreadDesignation,

                    "Class" =>
                        owner.Class,

                    _ =>
                        null
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
                    "Type" =>
                        owner.Type,

                    "HealthStatus" =>
                        owner.HealthStatus,

                    "HoleType" =>
                        owner.HoleType,

                    "ExtentType" =>
                        owner.ExtentType,

                    _ =>
                        null
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
        string objectPropertyName,
        string enumPropertyName)
    {
        object? nestedObject =
            GetObjectProperty(
                ownerObject,
                objectPropertyName);

        return nestedObject == null
            ? string.Empty
            : GetEnumStringProperty(
                nestedObject,
                enumPropertyName);
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
                "Suppressed" =>
                    (bool)owner.Suppressed,

                "Consumed" =>
                    (bool)owner.Consumed,

                "Tapped" =>
                    (bool)owner.Tapped,

                "FullThreadDepth" =>
                    (bool)owner.FullThreadDepth,

                _ =>
                    false
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
                "Value" =>
                    (double)owner.Value,

                "ModelValue" =>
                    (double)owner.ModelValue,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

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
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                details
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
