using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetSheetTitleBlockCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetSheetTitleBlockCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_sheet_title_block";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            SheetCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return SheetCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    sheetNameError);
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "definitionName",
                    out string definitionName,
                    out string definitionNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    definitionNameError);
        }

        string locationText =
            "bottom_right";

        if (root.TryGetProperty(
                "location",
                out JsonElement locationElement))
        {
            if (locationElement.ValueKind !=
                JsonValueKind.String)
            {
                return SheetCommandSupport
                    .CreateError(
                        "Поле \"location\" должно быть строкой.");
            }

            locationText =
                locationElement
                    .GetString()?
                    .Trim()
                    .ToLowerInvariant()
                ?? "bottom_right";
        }

        if (!TryParseLocation(
                locationText,
                out TitleBlockLocationEnum location))
        {
            return SheetCommandSupport
                .CreateError(
                    "Неизвестное положение основной надписи.",
                    "Допустимые значения: bottom_right, bottom_left, " +
                    "top_right, top_left.");
        }

        Sheet? sheet =
            SheetCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlockDefinition? definition =
            FindDefinition(
                drawingDocument,
                definitionName);

        if (definition == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Определение основной надписи " +
                    $"\"{definitionName}\" не найдено.");
        }

        string? previousDefinitionName =
            null;

        try
        {
            if (sheet.TitleBlock != null)
            {
                previousDefinitionName =
                    sheet.TitleBlock.Definition.Name;

                sheet.TitleBlock.Delete();
            }

            TitleBlock titleBlock =
                sheet.AddTitleBlock(
                    definition,
                    location,
                    Type.Missing);

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Основная надпись " +
                            $"\"{titleBlock.Definition.Name}\" установлена.",

                        sheet =
                            sheet.Name,

                        previousDefinitionName,

                        definitionName =
                            titleBlock.Definition.Name,

                        location =
                            location.ToString()
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось установить основную надпись " +
                    $"на лист \"{sheet.Name}\".",
                    exception.Message);
        }
    }

    private static TitleBlockDefinition? FindDefinition(
        DrawingDocument drawingDocument,
        string definitionName)
    {
        foreach (TitleBlockDefinition definition
                 in drawingDocument.TitleBlockDefinitions)
        {
            if (string.Equals(
                    definition.Name,
                    definitionName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return definition;
            }
        }

        return null;
    }

    private static bool TryParseLocation(
        string value,
        out TitleBlockLocationEnum location)
    {
        switch (value)
        {
            case "bottom_right":
                location =
                    TitleBlockLocationEnum
                        .kBottomRightPosition;

                return true;

            case "bottom_left":
                location =
                    TitleBlockLocationEnum
                        .kBottomLeftPosition;

                return true;

            case "top_right":
                location =
                    TitleBlockLocationEnum
                        .kTopRightPosition;

                return true;

            case "top_left":
                location =
                    TitleBlockLocationEnum
                        .kTopLeftPosition;

                return true;

            default:
                location =
                    default;

                return false;
        }
    }
}
