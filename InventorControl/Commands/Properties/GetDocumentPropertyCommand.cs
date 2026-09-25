using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDocumentPropertyCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDocumentPropertyCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_document_property";

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

        object? value;

        try
        {
            value =
                PropertyCommandSupport
                    .NormalizePropertyValue(
                        property.Value);
        }
        catch
        {
            value =
                null;
        }

        return PropertyCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        document.DisplayName,

                    fullFileName =
                        document.FullFileName,

                    propertySet =
                        new
                        {
                            name =
                                propertySet.Name,

                            displayName =
                                propertySet.DisplayName,

                            internalName =
                                propertySet.InternalName
                        },

                    property =
                        new
                        {
                            name =
                                property.Name,

                            displayName =
                                property.DisplayName,

                            propertyId =
                                property.PropId,

                            value,

                            valueType =
                                value?
                                    .GetType()
                                    .Name
                        }
                });
    }
}
