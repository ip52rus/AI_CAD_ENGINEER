using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CaptureModelPreviewCommand : IInventorCommand
{
    private const int DefaultPixelWidth = 1600;
    private const int DefaultPixelHeight = 1200;
    private const int MinimumPixelSize = 16;
    private const int MaximumPixelSize = 16000;

    private readonly Inventor.Application _inventor;

    public CaptureModelPreviewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "capture_model_preview";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            diagnostics.Add(new
            {
                scope = "activeDocument",
                message = "Inventor has no active document."
            });

            return CreateError(
                "Active document is not a PartDocument.",
                diagnostics);
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kPartDocumentObject)
        {
            diagnostics.Add(new
            {
                scope = "activeDocument.DocumentType",
                message = "Package 62A supports active PartDocument only.",
                documentType = activeDocument.DocumentType.ToString()
            });

            return CreateError(
                "Active document is not a PartDocument.",
                diagnostics);
        }

        PartDocument partDocument =
            (PartDocument)activeDocument;

        if (!TryGetRequiredString(
                root,
                "orientation",
                out string orientationText,
                out string orientationError))
        {
            diagnostics.Add(new { scope = "input.orientation", message = orientationError });
            return CreateError("Invalid capture_model_preview input.", diagnostics);
        }

        if (!BaseViewCommandSupport.TryParseOrientation(
                orientationText,
                out ViewOrientationTypeEnum orientation,
                out string orientationParseError))
        {
            diagnostics.Add(new
            {
                scope = "input.orientation",
                message = orientationParseError,
                orientation = orientationText
            });

            return CreateError("Invalid capture_model_preview input.", diagnostics);
        }

        if (!TryGetRequiredString(
                root,
                "outputPath",
                out string outputPath,
                out string outputPathError))
        {
            diagnostics.Add(new { scope = "input.outputPath", message = outputPathError });
            return CreateError("Invalid capture_model_preview input.", diagnostics);
        }

        if (!TryGetOptionalBoolean(
                root,
                "overwrite",
                false,
                diagnostics,
                out bool overwrite))
        {
            return CreateError("Invalid capture_model_preview input.", diagnostics);
        }

        if (!TryResolvePixelSize(
                root,
                diagnostics,
                out int pixelWidth,
                out int pixelHeight,
                out int? requestedPixelWidth,
                out int? requestedPixelHeight))
        {
            return CreateError("Invalid capture_model_preview input.", diagnostics);
        }

        if (!TryPrepareOutputFile(
                outputPath,
                overwrite,
                diagnostics,
                out bool fileExistedBefore,
                out long? fileSizeBefore,
                out DateTime? lastWriteUtcBefore))
        {
            return CreateError("Invalid preview outputPath.", diagnostics);
        }

        diagnostics.Add(new
        {
            scope = "capture.strategy",
            message = "TransientObjects.CreateCamera with SceneObject = PartDocument.ComponentDefinition was live-tested during Package 62A validation and produced a bitmap without visible model geometry; final implementation uses the active view camera path with camera-state restoration."
        });

        CaptureResult? captureResult =
            TryCaptureWithActiveView(
                partDocument,
                orientation,
                outputPath,
                pixelWidth,
                pixelHeight,
                diagnostics);

        if (captureResult == null)
        {
            return CreateError("Failed to capture model preview.", diagnostics);
        }

        FileInfo outputFile =
            new(outputPath);

        outputFile.Refresh();

        if (!outputFile.Exists)
        {
            diagnostics.Add(new
            {
                scope = "output.file",
                message = "Expected preview file does not exist after capture.",
                outputPath
            });

            return CreateError("Preview capture did not create the expected file.", diagnostics);
        }

        ImageFacts imageFacts =
            ReadImageFacts(outputPath);

        if (imageFacts.Format == null)
        {
            diagnostics.Add(new
            {
                scope = "output.file",
                message = "Preview file was created, but image format could not be recognized.",
                outputPath,
                fileSizeBytes = outputFile.Length
            });

            return CreateError("Preview image format is unknown.", diagnostics);
        }

        string? extensionFormat =
            FormatFromExtension(outputPath);

        if (extensionFormat != null &&
            !string.Equals(
                extensionFormat,
                imageFacts.Format,
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new
            {
                scope = "output.file",
                message = "Output file extension does not match detected image format.",
                extensionFormat,
                detectedFormat = imageFacts.Format
            });
        }

        return CreateSuccess(new
        {
            capability = "capture_model_preview",
            document = new
            {
                name = partDocument.DisplayName,
                fullFileName = partDocument.FullFileName,
                documentType = partDocument.DocumentType.ToString(),
                dirty = partDocument.Dirty
            },
            source = "activePartDocument",
            orientation = NormalizeOrientationForResponse(orientationText),
            orientationRaw = (int)orientation,
            orientationName = orientation.ToString(),
            captureSource = captureResult.CaptureSource,
            transientCameraAttempted = captureResult.TransientCameraAttempted,
            transientCameraSceneObject = captureResult.TransientCameraSceneObject,
            outputPath,
            fileExists = true,
            fileSizeBytes = outputFile.Length,
            lastWriteUtc = outputFile.LastWriteTimeUtc,
            imageFormat = imageFacts.Format,
            format = imageFacts.Format,
            requestedPixelWidth,
            requestedPixelHeight,
            pixelWidth = imageFacts.Width ?? pixelWidth,
            pixelHeight = imageFacts.Height ?? pixelHeight,
            actualPixelWidth = imageFacts.Width ?? pixelWidth,
            actualPixelHeight = imageFacts.Height ?? pixelHeight,
            activeViewStateTouched = captureResult.ActiveViewStateTouched,
            activeViewStateRestored = captureResult.ActiveViewStateRestored,
            fileExistedBefore,
            fileSizeBefore,
            lastWriteUtcBefore,
            dirty = partDocument.Dirty,
            diagnostics
        });
    }

    private CaptureResult? TryCaptureWithActiveView(
        PartDocument partDocument,
        ViewOrientationTypeEnum orientation,
        string outputPath,
        int pixelWidth,
        int pixelHeight,
        List<object> diagnostics)
    {
        CameraState? previousCameraState =
            null;

        bool restored =
            false;

        try
        {
            DeleteExistingOutputForAttempt(outputPath);

            View activeView =
                _inventor.ActiveView;

            Camera camera =
                activeView.Camera;

            previousCameraState =
                CameraState.Capture(camera);

            camera.Perspective =
                false;

            if (orientation !=
                ViewOrientationTypeEnum.kCurrentViewOrientation)
            {
                camera.ViewOrientationType =
                    orientation;
            }

            camera.Fit();
            camera.ApplyWithoutTransition();

            activeView.SaveAsBitmap(
                outputPath,
                pixelWidth,
                pixelHeight);

            restored =
                TryRestoreActiveViewState(
                    _inventor.TransientGeometry,
                    camera,
                    previousCameraState,
                    diagnostics);

            diagnostics.Add(new
            {
                scope = "Application.ActiveView.SaveAsBitmap",
                message = "Captured model preview with active view camera and restored previous camera state."
            });

            return new CaptureResult(
                "Application.ActiveView",
                true,
                restored,
                false,
                null);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Application.ActiveView.SaveAsBitmap",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                attemptedCaptureSource = "Application.ActiveView"
            });

            TryRestoreActiveViewState(
                _inventor.TransientGeometry,
                _inventor.ActiveView?.Camera,
                previousCameraState,
                diagnostics);

            return null;
        }
    }

    private static bool TryRestoreActiveViewState(
        TransientGeometry transientGeometry,
        Camera? camera,
        CameraState? cameraState,
        List<object> diagnostics)
    {
        if (camera == null ||
            cameraState == null)
        {
            return true;
        }

        return cameraState.TryRestore(
            transientGeometry,
            camera,
            diagnostics);
    }

    private static bool TryPrepareOutputFile(
        string outputPath,
        bool overwrite,
        List<object> diagnostics,
        out bool fileExistedBefore,
        out long? fileSizeBefore,
        out DateTime? lastWriteUtcBefore)
    {
        fileExistedBefore =
            System.IO.File.Exists(outputPath);

        fileSizeBefore =
            null;

        lastWriteUtcBefore =
            null;

        string? directory =
            System.IO.Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "Output path must include a parent directory.",
                outputPath
            });

            return false;
        }

        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath.parentDirectory",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                directory
            });

            return false;
        }

        if (!fileExistedBefore)
        {
            return true;
        }

        FileInfo existingFile =
            new(outputPath);

        fileSizeBefore =
            existingFile.Length;

        lastWriteUtcBefore =
            existingFile.LastWriteTimeUtc;

        if (!overwrite)
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "Destination preview already exists and overwrite is false.",
                outputPath
            });

            return false;
        }

        return true;
    }

    private static bool TryResolvePixelSize(
        JsonElement root,
        List<object> diagnostics,
        out int pixelWidth,
        out int pixelHeight,
        out int? requestedPixelWidth,
        out int? requestedPixelHeight)
    {
        requestedPixelWidth =
            null;

        requestedPixelHeight =
            null;

        if (!TryGetOptionalInt(
                root,
                "widthPixels",
                diagnostics,
                out requestedPixelWidth) ||
            !TryGetOptionalInt(
                root,
                "heightPixels",
                diagnostics,
                out requestedPixelHeight))
        {
            pixelWidth =
                0;

            pixelHeight =
                0;

            return false;
        }

        pixelWidth =
            requestedPixelWidth ?? DefaultPixelWidth;

        pixelHeight =
            requestedPixelHeight ?? DefaultPixelHeight;

        return true;
    }

    private static bool TryGetRequiredString(
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
                out JsonElement element))
        {
            error =
                $"Required field \"{propertyName}\" was not found.";

            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            error =
                $"Field \"{propertyName}\" must be a string.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error =
                $"Field \"{propertyName}\" must not be empty.";

            return false;
        }

        return true;
    }

    private static bool TryGetOptionalInt(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out int? value)
    {
        value =
            null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (!element.TryGetInt32(
                out int parsed))
        {
            diagnostics.Add(new
            {
                scope = $"input.{propertyName}",
                message = $"{propertyName} must be an integer."
            });

            return false;
        }

        if (parsed < MinimumPixelSize ||
            parsed > MaximumPixelSize)
        {
            diagnostics.Add(new
            {
                scope = $"input.{propertyName}",
                message = $"{propertyName} must be between {MinimumPixelSize} and {MaximumPixelSize}.",
                value = parsed
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
        List<object> diagnostics,
        out bool value)
    {
        value =
            defaultValue;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            diagnostics.Add(new
            {
                scope = $"input.{propertyName}",
                message = $"{propertyName} must be true or false."
            });

            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static void DeleteExistingOutputForAttempt(
        string outputPath)
    {
        if (System.IO.File.Exists(outputPath))
        {
            System.IO.File.Delete(outputPath);
        }
    }

    private static T? SafeRead<T>(
        Func<T> reader)
    {
        try
        {
            return reader();
        }
        catch
        {
            return default;
        }
    }

    private static ImageFacts ReadImageFacts(
        string outputPath)
    {
        byte[] bytes =
            System.IO.File.ReadAllBytes(outputPath);

        if (bytes.Length >= 24 &&
            bytes[0] == 0x89 &&
            bytes[1] == 0x50 &&
            bytes[2] == 0x4E &&
            bytes[3] == 0x47)
        {
            int width =
                ReadBigEndianInt32(bytes, 16);

            int height =
                ReadBigEndianInt32(bytes, 20);

            return new ImageFacts(
                "png",
                width,
                height);
        }

        if (bytes.Length >= 26 &&
            bytes[0] == 0x42 &&
            bytes[1] == 0x4D)
        {
            int width =
                BitConverter.ToInt32(
                    bytes,
                    18);

            int height =
                Math.Abs(
                    BitConverter.ToInt32(
                        bytes,
                        22));

            return new ImageFacts(
                "bmp",
                width,
                height);
        }

        if (bytes.Length >= 10 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xD8)
        {
            ImageFacts? jpegFacts =
                TryReadJpegFacts(bytes);

            if (jpegFacts != null)
            {
                return jpegFacts;
            }

            return new ImageFacts(
                "jpeg",
                null,
                null);
        }

        if (bytes.Length >= 10 &&
            bytes[0] == 0x47 &&
            bytes[1] == 0x49 &&
            bytes[2] == 0x46)
        {
            int width =
                BitConverter.ToUInt16(
                    bytes,
                    6);

            int height =
                BitConverter.ToUInt16(
                    bytes,
                    8);

            return new ImageFacts(
                "gif",
                width,
                height);
        }

        return new ImageFacts(
            null,
            null,
            null);
    }

    private static ImageFacts? TryReadJpegFacts(
        byte[] bytes)
    {
        int index =
            2;

        while (index + 9 < bytes.Length)
        {
            if (bytes[index] != 0xFF)
            {
                index++;
                continue;
            }

            byte marker =
                bytes[index + 1];

            index +=
                2;

            if (marker == 0xD9 ||
                marker == 0xDA)
            {
                break;
            }

            if (index + 1 >= bytes.Length)
            {
                break;
            }

            int segmentLength =
                (bytes[index] << 8) +
                bytes[index + 1];

            if (segmentLength < 2 ||
                index + segmentLength > bytes.Length)
            {
                break;
            }

            if ((marker >= 0xC0 && marker <= 0xC3) ||
                (marker >= 0xC5 && marker <= 0xC7) ||
                (marker >= 0xC9 && marker <= 0xCB) ||
                (marker >= 0xCD && marker <= 0xCF))
            {
                int height =
                    (bytes[index + 3] << 8) +
                    bytes[index + 4];

                int width =
                    (bytes[index + 5] << 8) +
                    bytes[index + 6];

                return new ImageFacts(
                    "jpeg",
                    width,
                    height);
            }

            index +=
                segmentLength;
        }

        return null;
    }

    private static int ReadBigEndianInt32(
        byte[] bytes,
        int offset)
    {
        return
            (bytes[offset] << 24) |
            (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) |
            bytes[offset + 3];
    }

    private static string? FormatFromExtension(
        string outputPath)
    {
        string extension =
            System.IO.Path.GetExtension(outputPath)
                .TrimStart('.')
                .ToLowerInvariant();

        return extension switch
        {
            "png" => "png",
            "bmp" => "bmp",
            "jpg" => "jpeg",
            "jpeg" => "jpeg",
            "gif" => "gif",
            _ => null
        };
    }

    private static string NormalizeOrientationForResponse(
        string orientation)
    {
        return orientation
            .Trim()
            .ToLowerInvariant()
            .Replace("-", "_")
            .Replace(" ", "_");
    }

    private static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            Options());
    }

    private static string CreateError(
        string error,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error,
                diagnostics
            },
            Options());
    }

    private static JsonSerializerOptions Options()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    private sealed record CaptureResult(
        string CaptureSource,
        bool ActiveViewStateTouched,
        bool ActiveViewStateRestored,
        bool TransientCameraAttempted,
        string? TransientCameraSceneObject);

    private sealed record ImageFacts(
        string? Format,
        int? Width,
        int? Height);

    private sealed class CameraState
    {
        private CameraState(
            PointFacts? eye,
            PointFacts? target,
            UnitVectorFacts? upVector,
            bool? perspective,
            double? perspectiveAngle,
            ViewOrientationTypeEnum? viewOrientationType)
        {
            Eye = eye;
            Target = target;
            UpVector = upVector;
            Perspective = perspective;
            PerspectiveAngle = perspectiveAngle;
            ViewOrientationType = viewOrientationType;
        }

        private PointFacts? Eye { get; }

        private PointFacts? Target { get; }

        private UnitVectorFacts? UpVector { get; }

        private bool? Perspective { get; }

        private double? PerspectiveAngle { get; }

        private ViewOrientationTypeEnum? ViewOrientationType { get; }

        public static CameraState Capture(
            Camera camera)
        {
            return new CameraState(
                ReadPoint(SafeRead(() => camera.Eye)),
                ReadPoint(SafeRead(() => camera.Target)),
                ReadUnitVector(SafeRead(() => camera.UpVector)),
                SafeRead(() => camera.Perspective),
                SafeRead(() => camera.PerspectiveAngle),
                SafeRead(() => camera.ViewOrientationType));
        }

        public bool TryRestore(
            TransientGeometry transientGeometry,
            Camera camera,
            List<object> diagnostics)
        {
            bool restored =
                true;

            if (ViewOrientationType.HasValue)
            {
                diagnostics.Add(new
                {
                    scope = "Camera.ViewOrientationType.restore",
                    message = "ViewOrientationType was read for diagnostics but is not restored directly; exact camera Eye/Target/UpVector/projection facts are restored instead.",
                    previousViewOrientationType = ViewOrientationType.Value.ToString(),
                    previousViewOrientationTypeRaw = (int)ViewOrientationType.Value
                });
            }

            restored &=
                TrySet(
                    "Camera.Eye.restore",
                    diagnostics,
                    () =>
                    {
                        if (Eye != null)
                        {
                            camera.Eye =
                                transientGeometry.CreatePoint(
                                    Eye.X,
                                    Eye.Y,
                                    Eye.Z);
                        }
                    });

            restored &=
                TrySet(
                    "Camera.Target.restore",
                    diagnostics,
                    () =>
                    {
                        if (Target != null)
                        {
                            camera.Target =
                                transientGeometry.CreatePoint(
                                    Target.X,
                                    Target.Y,
                                    Target.Z);
                        }
                    });

            restored &=
                TrySet(
                    "Camera.UpVector.restore",
                    diagnostics,
                    () =>
                    {
                        if (UpVector != null)
                        {
                            camera.UpVector =
                                transientGeometry.CreateUnitVector(
                                    UpVector.X,
                                    UpVector.Y,
                                    UpVector.Z);
                        }
                    });

            restored &=
                TrySet(
                    "Camera.Perspective.restore",
                    diagnostics,
                    () =>
                    {
                        if (Perspective.HasValue)
                        {
                            camera.Perspective =
                                Perspective.Value;
                        }
                    });

            restored &=
                TrySet(
                    "Camera.PerspectiveAngle.restore",
                    diagnostics,
                    () =>
                    {
                        if (PerspectiveAngle.HasValue)
                        {
                            camera.PerspectiveAngle =
                                PerspectiveAngle.Value;
                        }
                    });

            restored &=
                TrySet(
                    "Camera.ApplyWithoutTransition.restore",
                    diagnostics,
                    camera.ApplyWithoutTransition);

            return restored;
        }

        private static PointFacts? ReadPoint(
            Point? point)
        {
            if (point == null)
            {
                return null;
            }

            return new PointFacts(
                point.X,
                point.Y,
                point.Z);
        }

        private static UnitVectorFacts? ReadUnitVector(
            UnitVector? unitVector)
        {
            if (unitVector == null)
            {
                return null;
            }

            return new UnitVectorFacts(
                unitVector.X,
                unitVector.Y,
                unitVector.Z);
        }

        private static bool TrySet(
            string scope,
            List<object> diagnostics,
            Action setter)
        {
            try
            {
                setter();
                return true;
            }
            catch (Exception exception)
            {
                diagnostics.Add(new
                {
                    scope,
                    message = exception.Message,
                    exceptionType = exception.GetType().FullName
                });

                return false;
            }
        }
    }

    private sealed record PointFacts(
        double X,
        double Y,
        double Z);

    private sealed record UnitVectorFacts(
        double X,
        double Y,
        double Z);
}
