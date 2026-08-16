using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CaptureDrawingSheetPreviewCommand : IInventorCommand
{
    private const int DefaultPixelWidth = 2400;
    private const int MinimumPixelSize = 16;
    private const int MaximumPixelSize = 16000;

    private readonly Inventor.Application _inventor;

    public CaptureDrawingSheetPreviewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "capture_drawing_sheet_preview";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            SheetCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new
            {
                scope = "activeDocument",
                message = documentError ?? "Unable to resolve active DrawingDocument."
            });

            return CreateError(
                "Active document is not a DrawingDocument.",
                diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            return CreateError("Invalid capture_drawing_sheet_preview input.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(
                root,
                "outputPath",
                out string outputPath,
                out string outputPathError))
        {
            diagnostics.Add(new { scope = "input.outputPath", message = outputPathError });
            return CreateError("Invalid capture_drawing_sheet_preview input.", diagnostics);
        }

        if (!TryGetOptionalBoolean(
                root,
                "overwrite",
                false,
                diagnostics,
                out bool overwrite))
        {
            return CreateError("Invalid capture_drawing_sheet_preview input.", diagnostics);
        }

        Sheet? sheet =
            SheetCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new
            {
                scope = "input.sheetName",
                message = "Sheet was not found.",
                sheetName
            });

            return CreateError("Sheet was not found.", diagnostics);
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

        double sheetWidth =
            sheet.Width;

        double sheetHeight =
            sheet.Height;

        if (sheetWidth <= 0 ||
            sheetHeight <= 0)
        {
            diagnostics.Add(new
            {
                scope = "Sheet.WidthHeight",
                message = "Sheet dimensions must be positive.",
                sheetWidth,
                sheetHeight
            });

            return CreateError("Invalid sheet dimensions.", diagnostics);
        }

        if (!TryResolvePixelSize(
                root,
                sheetWidth,
                sheetHeight,
                diagnostics,
                out int pixelWidth,
                out int pixelHeight,
                out int? requestedPixelWidth,
                out int? requestedPixelHeight,
                out bool pixelSizeAdjustedForSheetAspect))
        {
            return CreateError("Invalid capture_drawing_sheet_preview input.", diagnostics);
        }

        diagnostics.Add(new
        {
            scope = "capture.strategy",
            message = "Transient sheet camera is not used as the primary path because live validation showed it captures a cropped sheet image in Inventor 2027."
        });

        CaptureResult? captureResult =
            TryCaptureWithActiveViewCamera(
                drawingDocument,
                sheet,
                outputPath,
                pixelWidth,
                pixelHeight,
                sheetWidth,
                sheetHeight,
                diagnostics);

        if (captureResult == null)
        {
            captureResult =
                TryCaptureWithActiveViewBitmap(
                    drawingDocument,
                    sheet,
                    outputPath,
                    pixelWidth,
                    pixelHeight,
                    diagnostics);
        }

        if (captureResult == null)
        {
            return CreateError("Failed to capture drawing sheet preview.", diagnostics);
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
            capability = "capture_drawing_sheet_preview",
            document = drawingDocument.DisplayName,
            documentFullFileName = drawingDocument.FullFileName,
            sheet = sheet.Name,
            outputPath,
            fileExists = true,
            fileSizeBytes = outputFile.Length,
            lastWriteUtc = outputFile.LastWriteTimeUtc,
            imageFormat = imageFacts.Format,
            pixelWidth = imageFacts.Width ?? pixelWidth,
            pixelHeight = imageFacts.Height ?? pixelHeight,
            requestedPixelWidth,
            requestedPixelHeight,
            pixelSizeAdjustedForSheetAspect,
            sheetWidth,
            sheetHeight,
            captureSource = captureResult.CaptureSource,
            activeViewStateTouched = captureResult.ActiveViewStateTouched,
            activeViewStateRestored = captureResult.ActiveViewStateRestored,
            selectedSheetActivated = captureResult.SelectedSheetActivated,
            previousActiveSheet = captureResult.PreviousActiveSheet,
            fileExistedBefore,
            fileSizeBefore,
            lastWriteUtcBefore,
            dirty = drawingDocument.Dirty,
            diagnostics
        });
    }

    private CaptureResult? TryCaptureWithTransientSheetCamera(
        Sheet sheet,
        string outputPath,
        int pixelWidth,
        int pixelHeight,
        double sheetWidth,
        double sheetHeight,
        List<object> diagnostics)
    {
        try
        {
            DeleteExistingOutputForAttempt(outputPath);

            Camera camera =
                _inventor.TransientObjects.CreateCamera();

            camera.SceneObject =
                sheet;

            double centerX =
                sheetWidth / 2.0;

            double centerY =
                sheetHeight / 2.0;

            double eyeDistance =
                Math.Max(
                    sheetWidth,
                    sheetHeight);

            TransientGeometry transientGeometry =
                _inventor.TransientGeometry;

            camera.Perspective =
                false;

            camera.Target =
                transientGeometry.CreatePoint(
                    centerX,
                    centerY,
                    0);

            camera.Eye =
                transientGeometry.CreatePoint(
                    centerX,
                    centerY,
                    eyeDistance);

            camera.UpVector =
                transientGeometry.CreateUnitVector(
                    0,
                    1,
                    0);

            camera.SetExtents(
                sheetWidth,
                sheetHeight);

            camera.SaveAsBitmap(
                outputPath,
                pixelWidth,
                pixelHeight);

            diagnostics.Add(new
            {
                scope = "TransientObjects.CreateCamera.Camera.SaveAsBitmap",
                message = "Captured preview with transient camera scene object set to selected Sheet."
            });

            return new CaptureResult(
                "TransientCamera.Sheet",
                false,
                true,
                false,
                null);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "TransientObjects.CreateCamera.Camera.SaveAsBitmap",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                attemptedCaptureSource = "TransientCamera.Sheet"
            });

            return null;
        }
    }

    private CaptureResult? TryCaptureWithActiveViewCamera(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string outputPath,
        int pixelWidth,
        int pixelHeight,
        double sheetWidth,
        double sheetHeight,
        List<object> diagnostics)
    {
        string? previousActiveSheet =
            SafeRead(() => drawingDocument.ActiveSheet.Name);

        bool selectedSheetActivated =
            false;

        CameraState? previousCameraState =
            null;

        bool restored =
            false;

        try
        {
            DeleteExistingOutputForAttempt(outputPath);

            if (!string.Equals(
                    previousActiveSheet,
                    sheet.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                sheet.Activate();
                selectedSheetActivated = true;
            }

            View activeView =
                _inventor.ActiveView;

            Camera camera =
                activeView.Camera;

            previousCameraState =
                CameraState.Capture(camera);

            camera.SceneObject =
                sheet;

            camera.Fit();
            camera.SetExtents(
                sheetWidth,
                sheetHeight);

            camera.ApplyWithoutTransition();

            camera.SaveAsBitmap(
                outputPath,
                pixelWidth,
                pixelHeight);

            restored =
                TryRestoreActiveViewState(
                    drawingDocument,
                    previousActiveSheet,
                    selectedSheetActivated,
                    camera,
                    previousCameraState,
                    diagnostics);

            diagnostics.Add(new
            {
                scope = "Application.ActiveView.Camera.SaveAsBitmap",
                message = "Captured preview with active view camera."
            });

            return new CaptureResult(
                "ActiveView.Camera",
                true,
                restored,
                selectedSheetActivated,
                previousActiveSheet);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Application.ActiveView.Camera.SaveAsBitmap",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                attemptedCaptureSource = "ActiveView.Camera"
            });

            TryRestoreActiveViewState(
                drawingDocument,
                previousActiveSheet,
                selectedSheetActivated,
                null,
                previousCameraState,
                diagnostics);

            return null;
        }
    }

    private CaptureResult? TryCaptureWithActiveViewBitmap(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string outputPath,
        int pixelWidth,
        int pixelHeight,
        List<object> diagnostics)
    {
        string? previousActiveSheet =
            SafeRead(() => drawingDocument.ActiveSheet.Name);

        bool selectedSheetActivated =
            false;

        try
        {
            DeleteExistingOutputForAttempt(outputPath);

            if (!string.Equals(
                    previousActiveSheet,
                    sheet.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                sheet.Activate();
                selectedSheetActivated = true;
            }

            View activeView =
                _inventor.ActiveView;

            activeView.Fit(true);
            activeView.SaveAsBitmap(
                outputPath,
                pixelWidth,
                pixelHeight);

            bool activeSheetRestored =
                TryRestoreActiveSheet(
                    drawingDocument,
                    previousActiveSheet,
                    selectedSheetActivated,
                    diagnostics);

            diagnostics.Add(new
            {
                scope = "Application.ActiveView.SaveAsBitmap",
                message = "Captured preview with active view bitmap after ActiveView.Fit. Previous zoom/pan camera state is not restored by this fallback.",
                activeSheetRestored
            });

            return new CaptureResult(
                "Application.ActiveView",
                true,
                false,
                selectedSheetActivated,
                previousActiveSheet);
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

            TryRestoreActiveSheet(
                drawingDocument,
                previousActiveSheet,
                selectedSheetActivated,
                diagnostics);

            return null;
        }
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
        double sheetWidth,
        double sheetHeight,
        List<object> diagnostics,
        out int pixelWidth,
        out int pixelHeight,
        out int? requestedPixelWidth,
        out int? requestedPixelHeight,
        out bool adjustedForSheetAspect)
    {
        requestedPixelWidth =
            null;

        requestedPixelHeight =
            null;

        adjustedForSheetAspect =
            false;

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

        double sheetRatio =
            sheetWidth / sheetHeight;

        if (requestedPixelWidth.HasValue &&
            requestedPixelHeight.HasValue)
        {
            int width =
                requestedPixelWidth.Value;

            int height =
                requestedPixelHeight.Value;

            double requestedRatio =
                (double)width / height;

            if (Math.Abs(requestedRatio - sheetRatio) <= 0.001)
            {
                pixelWidth =
                    width;

                pixelHeight =
                    height;

                return true;
            }

            adjustedForSheetAspect =
                true;

            if (requestedRatio > sheetRatio)
            {
                pixelHeight =
                    height;

                pixelWidth =
                    Math.Max(
                        MinimumPixelSize,
                        (int)Math.Round(height * sheetRatio));
            }
            else
            {
                pixelWidth =
                    width;

                pixelHeight =
                    Math.Max(
                        MinimumPixelSize,
                        (int)Math.Round(width / sheetRatio));
            }

            return true;
        }

        if (requestedPixelWidth.HasValue)
        {
            pixelWidth =
                requestedPixelWidth.Value;

            pixelHeight =
                Math.Max(
                    MinimumPixelSize,
                    (int)Math.Round(pixelWidth / sheetRatio));

            adjustedForSheetAspect =
                true;

            return true;
        }

        if (requestedPixelHeight.HasValue)
        {
            pixelHeight =
                requestedPixelHeight.Value;

            pixelWidth =
                Math.Max(
                    MinimumPixelSize,
                    (int)Math.Round(pixelHeight * sheetRatio));

            adjustedForSheetAspect =
                true;

            return true;
        }

        pixelWidth =
            DefaultPixelWidth;

        pixelHeight =
            Math.Max(
                MinimumPixelSize,
                (int)Math.Round(pixelWidth / sheetRatio));

        adjustedForSheetAspect =
            true;

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

    private static bool TryRestoreActiveViewState(
        DrawingDocument drawingDocument,
        string? previousActiveSheet,
        bool selectedSheetActivated,
        Camera? camera,
        CameraState? cameraState,
        List<object> diagnostics)
    {
        bool cameraRestored =
            true;

        if (camera != null &&
            cameraState != null)
        {
            cameraRestored =
                cameraState.TryRestore(
                    camera,
                    diagnostics);
        }

        bool sheetRestored =
            TryRestoreActiveSheet(
                drawingDocument,
                previousActiveSheet,
                selectedSheetActivated,
                diagnostics);

        return cameraRestored &&
               sheetRestored;
    }

    private static bool TryRestoreActiveSheet(
        DrawingDocument drawingDocument,
        string? previousActiveSheet,
        bool selectedSheetActivated,
        List<object> diagnostics)
    {
        if (!selectedSheetActivated ||
            string.IsNullOrWhiteSpace(previousActiveSheet))
        {
            return true;
        }

        try
        {
            Sheet? sheet =
                SheetCommandSupport.FindSheet(
                    drawingDocument,
                    previousActiveSheet);

            if (sheet == null)
            {
                diagnostics.Add(new
                {
                    scope = "activeSheet.restore",
                    message = "Previous active sheet was not found.",
                    previousActiveSheet
                });

                return false;
            }

            sheet.Activate();
            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "activeSheet.restore",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                previousActiveSheet
            });

            return false;
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
        bool SelectedSheetActivated,
        string? PreviousActiveSheet);

    private sealed record ImageFacts(
        string? Format,
        int? Width,
        int? Height);

    private sealed class CameraState
    {
        private CameraState(
            object? sceneObject,
            Point? eye,
            Point? target,
            UnitVector? upVector,
            bool? perspective,
            double? perspectiveAngle)
        {
            SceneObject = sceneObject;
            Eye = eye;
            Target = target;
            UpVector = upVector;
            Perspective = perspective;
            PerspectiveAngle = perspectiveAngle;
        }

        private object? SceneObject { get; }

        private Point? Eye { get; }

        private Point? Target { get; }

        private UnitVector? UpVector { get; }

        private bool? Perspective { get; }

        private double? PerspectiveAngle { get; }

        public static CameraState Capture(
            Camera camera)
        {
            return new CameraState(
                SafeRead(() => camera.SceneObject),
                SafeRead(() => camera.Eye),
                SafeRead(() => camera.Target),
                SafeRead(() => camera.UpVector),
                SafeRead(() => camera.Perspective),
                SafeRead(() => camera.PerspectiveAngle));
        }

        public bool TryRestore(
            Camera camera,
            List<object> diagnostics)
        {
            bool restored =
                true;

            restored &=
                TrySet(
                    "Camera.SceneObject.restore",
                    diagnostics,
                    () =>
                    {
                        if (SceneObject != null)
                        {
                            camera.SceneObject = SceneObject;
                        }
                    });

            restored &=
                TrySet(
                    "Camera.Eye.restore",
                    diagnostics,
                    () =>
                    {
                        if (Eye != null)
                        {
                            camera.Eye = Eye;
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
                            camera.Target = Target;
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
                            camera.UpVector = UpVector;
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
                            camera.Perspective = Perspective.Value;
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
                            camera.PerspectiveAngle = PerspectiveAngle.Value;
                        }
                    });

            TrySet(
                "Camera.ApplyWithoutTransition.restore",
                diagnostics,
                camera.ApplyWithoutTransition);

            return restored;
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
}
