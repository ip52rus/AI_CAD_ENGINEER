using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDocumentPropertyCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDocumentPropertyCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_document_property";

    public string Execute(
        JsonElement root)
    {
        Document? document =
            PropertyCommandSupport
                .GetTargetDocument(
                    _inventor,
                    root,
                    out string? documentError);

        if (document == null)
        {
            return PropertyCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить документ.");
        }

        if (!PropertyCommandSupport
                .TryGetRequiredString(
                    root,
                    "propertySetName",
                    out string propertySetName,
                    out string propertySetNameError))
        {
            return PropertyCommandSupport
                .CreateError(
                    propertySetNameError);
        }

        if (!PropertyCommandSupport
                .TryGetRequiredString(
                    root,
                    "propertyName",
                    out string propertyName,
                    out string propertyNameError))
        {
            return PropertyCommandSupport
                .CreateError(
                    propertyNameError);
        }

        if (!root.TryGetProperty(
                "value",
                out JsonElement valueElement))
        {
            return PropertyCommandSupport
                .CreateError(
                    "Не найдено обязательное поле \"value\".");
        }

        PropertySet? propertySet =
            PropertyCommandSupport
                .FindPropertySet(
                    document,
                    propertySetName);

        if (propertySet == null)
        {
            return PropertyCommandSupport
                .CreateError(
                    $"Набор свойств " +
                    $"\"{propertySetName}\" не найден.");
        }

        Inventor.Property? property =
            PropertyCommandSupport
                .FindProperty(
                    propertySet,
                    propertyName);

        if (property == null)
        {
            return PropertyCommandSupport
                .CreateError(
                    $"Свойство \"{propertyName}\" " +
                    $"не найдено в наборе " +
                    $"\"{propertySet.DisplayName}\".");
        }

        object? previousValue;

        try
        {
            previousValue =
                PropertyCommandSupport
                    .NormalizePropertyValue(
                        property.Value);
        }
        catch
        {
            previousValue =
                null;
        }

        object? newValue =
            PropertyCommandSupport
                .ConvertJsonValue(
                    valueElement);

        try
        {
            property.Value =
                newValue;

            document.Update();

            object? actualValue =
                PropertyCommandSupport
                    .NormalizePropertyValue(
                        property.Value);

            return PropertyCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Свойство \"{property.DisplayName}\" изменено.",

                        document =
                            document.DisplayName,

                        propertySet =
                            propertySet.DisplayName,

                        property =
                            property.DisplayName,

                        previousValue,

                        requestedValue =
                            newValue,

                        actualValue,

                        dirty =
                            document.Dirty
                    });
        }
        catch (Exception exception)
        {
            return PropertyCommandSupport
                .CreateError(
                    $"Не удалось изменить свойство " +
                    $"\"{property.DisplayName}\".",
                    exception.Message);
        }
    }
}
