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
                    index,
                    document));
        }

        return result;
    }

    public static List<object> ReadThreadFeatures(
        Document document,
        bool includeSuppressed,
        List<object> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diagnostics);

        List<object> result =
            new();

        if (document.DocumentType !=
            DocumentTypeEnum.kPartDocumentObject)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "ReferencedDocument",
                    message =
                        "Referenced document is not a PartDocument.",
                    documentType =
                        document.DocumentType.ToString()
                });

            return result;
        }

        PartDocument partDocument =
            (PartDocument)document;

        object threadFeaturesObject;

        try
        {
            threadFeaturesObject =
                partDocument
                    .ComponentDefinition
                    .Features
                    .ThreadFeatures;
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "PartDocument.ComponentDefinition.Features.ThreadFeatures",
                    message =
                        exception.Message,
                    exceptionType =
                        exception
                            .GetType()
                            .FullName
                });

            return result;
        }

        dynamic threadFeatures =
            threadFeaturesObject;

        int count =
            GetCollectionCount(
                threadFeaturesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object featureObject;

            try
            {
                featureObject =
                    threadFeatures[index];
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new
                    {
                        scope =
                            "ThreadFeatures.Item",
                        index,
                        message =
                            exception.Message,
                        exceptionType =
                            exception
                                .GetType()
                                .FullName
                    });

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
                ReadThreadFeature(
                    featureObject,
                    index,
                    document));
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
        int index,
        Document document)
    {
        List<object> diagnostics =
            new();

        bool isThreaded =
            GetBooleanProperty(
                featureObject,
                "Tapped",
                diagnostics);

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

            referenceKey =
                ReadReferenceKey(
                    document,
                    featureObject,
                    "HoleFeature.GetReferenceKey",
                    diagnostics),

            suppressed =
                GetBooleanProperty(
                    featureObject,
                    "Suppressed"),

            healthStatus =
                GetEnumStringProperty(
                    featureObject,
                    "HealthStatus"),

            extendedName =
                GetStringProperty(
                    featureObject,
                    "ExtendedName",
                    diagnostics),

            holeType =
                GetEnumStringProperty(
                    featureObject,
                    "HoleType"),

            extentType =
                GetEnumStringProperty(
                    featureObject,
                    "ExtentType"),

            placementType =
                GetEnumStringProperty(
                    featureObject,
                    "PlacementType",
                    diagnostics),

            drillPointType =
                GetEnumStringProperty(
                    featureObject,
                    "DrillPointType",
                    diagnostics),

            isClearanceHole =
                GetNullableBooleanProperty(
                    featureObject,
                    "IsClearanceHole",
                    diagnostics),

            diameter =
                ReadParameterProperty(
                    featureObject,
                    "HoleDiameter",
                    diagnostics),

            depth =
                ReadParameterProperty(
                    featureObject,
                    "Depth",
                    diagnostics),

            counterboreDiameter =
                ReadParameterProperty(
                    featureObject,
                    "CBoreDiameter",
                    diagnostics),

            counterboreDepth =
                ReadParameterProperty(
                    featureObject,
                    "CBoreDepth",
                    diagnostics),

            countersinkDiameter =
                ReadParameterProperty(
                    featureObject,
                    "CSinkDiameter",
                    diagnostics),

            countersinkAngle =
                ReadParameterProperty(
                    featureObject,
                    "CSinkAngle",
                    diagnostics),

            countersinkDepth =
                ReadParameterProperty(
                    featureObject,
                    "CSinkDepth",
                    diagnostics),

            spotFaceDiameter =
                ReadParameterProperty(
                    featureObject,
                    "SpotFaceDiameter",
                    diagnostics),

            spotFaceDepth =
                ReadParameterProperty(
                    featureObject,
                    "SpotFaceDepth",
                    diagnostics),

            isThreaded,

            threadInfo =
                isThreaded
                    ? ReadThreadInfo(
                        featureObject,
                        diagnostics)
                    : null,

            placementDefinitionType =
                GetNestedEnumStringProperty(
                    featureObject,
                    "PlacementDefinition",
                    "Type",
                    diagnostics),

            holeCenterCount =
                ReadHoleCenterCount(
                    featureObject,
                    diagnostics),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "SurfaceBodies",
                    diagnostics),

            rangeBox =
                ReadBox(
                    GetObjectProperty(
                        featureObject,
                        "RangeBox",
                        diagnostics),
                    diagnostics),

            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadThreadFeature(
        object featureObject,
        int index,
        Document document)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index,

            name =
                GetStringProperty(
                    featureObject,
                    "Name",
                    diagnostics),

            extendedName =
                GetStringProperty(
                    featureObject,
                    "ExtendedName",
                    diagnostics),

            objectType =
                GetEnumStringProperty(
                    featureObject,
                    "Type",
                    diagnostics),

            referenceKey =
                ReadReferenceKey(
                    document,
                    featureObject,
                    "ThreadFeature.GetReferenceKey",
                    diagnostics),

            suppressed =
                GetBooleanProperty(
                    featureObject,
                    "Suppressed",
                    diagnostics),

            healthStatus =
                GetEnumStringProperty(
                    featureObject,
                    "HealthStatus",
                    diagnostics),

            directionReversed =
                GetNullableBooleanProperty(
                    featureObject,
                    "DirectionReversed",
                    diagnostics),

            fullDepth =
                GetNullableBooleanProperty(
                    featureObject,
                    "FullDepth",
                    diagnostics),

            threadDepth =
                ReadParameterProperty(
                    featureObject,
                    "ThreadDepth",
                    diagnostics),

            threadOffset =
                ReadParameterProperty(
                    featureObject,
                    "ThreadOffset",
                    diagnostics),

            threadInfoType =
                GetEnumStringProperty(
                    featureObject,
                    "ThreadInfoType",
                    diagnostics),

            threadInfo =
                ReadThreadInfoFromObject(
                    GetObjectProperty(
                        featureObject,
                        "ThreadInfo",
                        diagnostics),
                    diagnostics),

            threadedFaceCount =
                GetNestedCollectionCount(
                    featureObject,
                    "ThreadedFace",
                    diagnostics),

            faceCount =
                GetNestedCollectionCount(
                    featureObject,
                    "Faces",
                    diagnostics),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    featureObject,
                    "SurfaceBodies",
                    diagnostics),

            participantCount =
                GetNestedCollectionCount(
                    featureObject,
                    "Participants",
                    diagnostics),

            isOwnedByFeature =
                GetNullableBooleanProperty(
                    featureObject,
                    "IsOwnedByFeature",
                    diagnostics),

            featureDimensionCount =
                GetNestedCollectionCount(
                    featureObject,
                    "FeatureDimensions",
                    diagnostics),

            rangeBox =
                ReadBox(
                    GetObjectProperty(
                        featureObject,
                        "RangeBox",
                        diagnostics),
                    diagnostics),

            propertyDiagnostics =
                diagnostics
        };
    }


    private static object? ReadParameterProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
    {
        object? parameterObject =
            GetObjectProperty(
                ownerObject,
                propertyName,
                diagnostics);

        if (parameterObject == null)
        {
            return null;
        }

        if (TryConvertDouble(
                parameterObject,
                out double scalarValue))
        {
            return new
            {
                value =
                    scalarValue
            };
        }

        return new
        {
            name =
                GetStringProperty(
                    parameterObject,
                    "Name",
                    diagnostics),

            expression =
                GetStringProperty(
                    parameterObject,
                    "Expression",
                    diagnostics),

            value =
                GetNullableDoubleProperty(
                    parameterObject,
                    "Value",
                    diagnostics),

            units =
                GetStringProperty(
                    parameterObject,
                    "Units",
                    diagnostics),

            modelValue =
                GetNullableDoubleProperty(
                    parameterObject,
                    "ModelValue",
                    diagnostics)
        };
    }

    private static object? ReadThreadInfo(
        object featureObject,
        List<object> diagnostics)
    {
        object? threadInfoObject =
            GetObjectProperty(
                featureObject,
                "TapInfo",
                diagnostics);

        if (threadInfoObject == null)
        {
            threadInfoObject =
                GetObjectProperty(
                    featureObject,
                    "ThreadInfo",
                    diagnostics);
        }

        if (threadInfoObject == null)
        {
            return null;
        }

        return ReadThreadInfoFromObject(
            threadInfoObject,
            diagnostics);
    }

    private static object? ReadThreadInfoFromObject(
        object? threadInfoObject,
        List<object> diagnostics)
    {
        if (threadInfoObject == null)
        {
            return null;
        }

        return new
        {
            objectType =
                GetEnumStringProperty(
                    threadInfoObject,
                    "Type",
                    diagnostics),

            threadType =
                GetStringProperty(
                    threadInfoObject,
                    "ThreadType",
                    diagnostics),

            threadTypeIdentifier =
                GetStringProperty(
                    threadInfoObject,
                    "ThreadTypeIdentifier",
                    diagnostics),

            designation =
                GetStringProperty(
                    threadInfoObject,
                    "ThreadDesignation",
                    diagnostics),

            customDesignation =
                GetStringProperty(
                    threadInfoObject,
                    "CustomThreadDesignation",
                    diagnostics),

            threadClass =
                GetStringProperty(
                    threadInfoObject,
                    "Class",
                    diagnostics),

            fullThreadDepth =
                GetBooleanProperty(
                    threadInfoObject,
                    "FullThreadDepth",
                    diagnostics),

            metric =
                GetNullableBooleanProperty(
                    threadInfoObject,
                    "Metric",
                    diagnostics),

            internalThread =
                GetNullableBooleanProperty(
                    threadInfoObject,
                    "Internal",
                    diagnostics),

            threadDirection =
                ReadVector(
                    GetObjectProperty(
                        threadInfoObject,
                        "ThreadDirection",
                        diagnostics),
                    diagnostics),

            threadDirectionUnit =
                ReadVector(
                    GetObjectProperty(
                        threadInfoObject,
                        "_ThreadDirection",
                        diagnostics),
                    diagnostics),

            threadBasePointCount =
                GetNestedCollectionCount(
                    threadInfoObject,
                    "ThreadBasePoints",
                    diagnostics),

            threadDepth =
                ReadParameterProperty(
                    threadInfoObject,
                    "ThreadDepth",
                    diagnostics)
        };
    }

    private static int ReadHoleCenterCount(
        object featureObject,
        List<object>? diagnostics = null)
    {
        object? placementObject =
            GetObjectProperty(
                featureObject,
                "PlacementDefinition",
                diagnostics);

        if (placementObject == null)
        {
            return 0;
        }

        object? pointsObject =
            GetObjectProperty(
                placementObject,
                "HoleCenterPoints",
                diagnostics);

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
        string propertyName,
        List<object>? diagnostics = null)
    {
        object? collectionObject =
            GetObjectProperty(
                ownerObject,
                propertyName,
                diagnostics);

        return collectionObject == null
            ? 0
            : GetCollectionCount(
                collectionObject);
    }

    private static object? GetObjectProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "SurfaceBodies" =>
                    owner.SurfaceBodies,

                "SurfaceBody" =>
                    owner.SurfaceBody,

                "ParticipantBodies" =>
                    owner.ParticipantBodies,

                "Participants" =>
                    owner.Participants,

                "Parent" =>
                    owner.Parent,

                "RangeBox" =>
                    owner.RangeBox,

                "PlacementDefinition" =>
                    owner.PlacementDefinition,

                "HoleCenterPoints" =>
                    owner.HoleCenterPoints,

                "HoleDiameter" =>
                    owner.HoleDiameter,

                "Depth" =>
                    owner.Depth,

                "ExtendedName" =>
                    owner.ExtendedName,

                "PlacementType" =>
                    owner.PlacementType,

                "DrillPointType" =>
                    owner.DrillPointType,

                "IsClearanceHole" =>
                    owner.IsClearanceHole,

                "CBoreDiameter" =>
                    owner.CBoreDiameter,

                "CBoreDepth" =>
                    owner.CBoreDepth,

                "CSinkDiameter" =>
                    owner.CSinkDiameter,

                "CSinkAngle" =>
                    owner.CSinkAngle,

                "CSinkDepth" =>
                    owner.CSinkDepth,

                "SpotFaceDiameter" =>
                    owner.SpotFaceDiameter,

                "SpotFaceDepth" =>
                    owner.SpotFaceDepth,

                "ThreadInfo" =>
                    owner.ThreadInfo,

                "TapInfo" =>
                    owner.TapInfo,

                "ThreadDepth" =>
                    owner.ThreadDepth,

                "ThreadOffset" =>
                    owner.ThreadOffset,

                "ThreadedFace" =>
                    owner.ThreadedFace,

                "Faces" =>
                    owner.Faces,

                "FeatureDimensions" =>
                    owner.FeatureDimensions,

                "ThreadDirection" =>
                    owner.ThreadDirection,

                "_ThreadDirection" =>
                    owner._ThreadDirection,

                "ThreadBasePoints" =>
                    owner.ThreadBasePoints,

                _ =>
                    null
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static string GetStringProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
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

                    "ExtendedName" =>
                        owner.ExtendedName,

                    "ThreadType" =>
                        owner.ThreadType,

                    "ThreadTypeIdentifier" =>
                        owner.ThreadTypeIdentifier,

                    "ThreadDesignation" =>
                        owner.ThreadDesignation,

                    "CustomThreadDesignation" =>
                        owner.CustomThreadDesignation,

                    "Class" =>
                        owner.Class,

                    _ =>
                        null
                };

            return value?.ToString() ??
                   string.Empty;
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return string.Empty;
        }
    }

    private static string GetEnumStringProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
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

                    "PlacementType" =>
                        owner.PlacementType,

                    "DrillPointType" =>
                        owner.DrillPointType,

                    "ThreadInfoType" =>
                        owner.ThreadInfoType,

                    _ =>
                        null
                };

            return value?.ToString() ??
                   string.Empty;
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return string.Empty;
        }
    }

    private static string GetNestedEnumStringProperty(
        object ownerObject,
        string objectPropertyName,
        string enumPropertyName,
        List<object>? diagnostics = null)
    {
        object? nestedObject =
            GetObjectProperty(
                ownerObject,
                objectPropertyName,
                diagnostics);

        return nestedObject == null
            ? string.Empty
            : GetEnumStringProperty(
                nestedObject,
                enumPropertyName,
                diagnostics);
    }

    private static bool GetBooleanProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
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
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return false;
        }
    }

    private static double? GetNullableDoubleProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
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
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static bool? GetNullableBooleanProperty(
        object ownerObject,
        string propertyName,
        List<object>? diagnostics = null)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "IsClearanceHole" =>
                    (bool)owner.IsClearanceHole,

                "Metric" =>
                    (bool)owner.Metric,

                "Internal" =>
                    (bool)owner.Internal,

                "DirectionReversed" =>
                    (bool)owner.DirectionReversed,

                "FullDepth" =>
                    (bool)owner.FullDepth,

                "IsOwnedByFeature" =>
                    (bool)owner.IsOwnedByFeature,

                _ =>
                    null
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static object? ReadReferenceKey(
        Document document,
        object featureObject,
        string scope,
        List<object> diagnostics)
    {
        int keyContext =
            0;

        try
        {
            ReferenceKeyManager manager =
                document.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                Array.Empty<byte>();

            dynamic feature =
                featureObject;

            feature.GetReferenceKey(
                ref referenceKey,
                keyContext);

            string keyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
        finally
        {
            if (keyContext !=
                0)
            {
                try
                {
                    document
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch (Exception exception)
                {
                    AddDiagnostic(
                        diagnostics,
                        "ReferenceKeyManager.ReleaseKeyContext",
                        exception);
                }
            }
        }
    }

    private static object? ReadBox(
        object? boxObject,
        List<object>? diagnostics = null)
    {
        if (boxObject == null)
        {
            return null;
        }

        dynamic box =
            boxObject;

        try
        {
            return new
            {
                minPoint =
                    ReadPoint3d(
                        box.MinPoint,
                        diagnostics),

                maxPoint =
                    ReadPoint3d(
                        box.MaxPoint,
                        diagnostics)
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "RangeBox",
                exception);

            return null;
        }
    }

    private static object? ReadPoint3d(
        object? pointObject,
        List<object>? diagnostics = null)
    {
        if (pointObject == null)
        {
            return null;
        }

        dynamic point =
            pointObject;

        try
        {
            return new
            {
                x =
                    (double)point.X,

                y =
                    (double)point.Y,

                z =
                    (double)point.Z
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Point3d",
                exception);

            return null;
        }
    }

    private static object? ReadVector(
        object? vectorObject,
        List<object>? diagnostics = null)
    {
        if (vectorObject == null)
        {
            return null;
        }

        dynamic vector =
            vectorObject;

        try
        {
            return new
            {
                x =
                    (double)vector.X,

                y =
                    (double)vector.Y,

                z =
                    (double)vector.Z
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "ThreadDirection",
                exception);

            return null;
        }
    }

    private static bool TryConvertDouble(
        object value,
        out double result)
    {
        try
        {
            if (value is double doubleValue)
            {
                result =
                    doubleValue;

                return true;
            }

            if (value is float floatValue)
            {
                result =
                    floatValue;

                return true;
            }

            if (value is int intValue)
            {
                result =
                    intValue;

                return true;
            }

            if (value is decimal decimalValue)
            {
                result =
                    (double)decimalValue;

                return true;
            }
        }
        catch
        {
        }

        result =
            0;

        return false;
    }

    private static void AddDiagnostic(
        List<object>? diagnostics,
        string propertyName,
        Exception exception)
    {
        diagnostics?.Add(
            new
            {
                property =
                    propertyName,

                available =
                    false,

                error =
                    exception.Message,

                exceptionType =
                    exception
                        .GetType()
                        .FullName
            });
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
