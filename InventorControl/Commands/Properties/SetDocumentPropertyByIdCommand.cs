using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDocumentPropertyByIdCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDocumentPropertyByIdCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_document_property_by_id";

    public string Execute(
        JsonElement root)
    {
        Document? document =
            PropertyByIdSupport
                .GetTargetDocument(
                    _inventor,
                    root,
                    out string? documentError);

        if (document == null)
        {
            return PropertyByIdSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить документ.");
        }

        if (!PropertyByIdSupport
                .TryGetRequiredString(
                    root,
                    "propertySetInternalName",
                    out string propertySetInternalName,
                    out string propertySetError))
        {
            return PropertyByIdSupport
                .CreateError(
                    propertySetError);
        }

        if (!PropertyByIdSupport
                .TryGetRequiredInt32(
                    root,
                    "propertyId",
                    out int propertyId,
                    out string propertyIdError))
        {
            return PropertyByIdSupport
                .CreateError(
                    propertyIdError);
        }

        if (!root.TryGetProperty(
                "value",
                out JsonElement valueElement))
        {
            return PropertyByIdSupport
                .CreateError(
                    "Не найдено обязательное поле \"value\".");
        }

        if (!PropertyByIdSupport
                .TryResolveProperty(
                    document,
                    propertySetInternalName,
                    propertyId,
                    out Inventor.Property? property,
                    out string source,
                    out string resolveDetails) ||
            property == null)
        {
            return PropertyByIdSupport
                .CreateError(
                    "Свойство по указанным идентификаторам не найдено.",
                    resolveDetails);
        }

        object? previousValue;

        try
        {
            previousValue =
                PropertyByIdSupport
                    .NormalizePropertyValue(
                        property.Value);
        }
        catch
        {
            previousValue =
                null;
        }

        object? requestedValue =
            PropertyByIdSupport
                .ConvertJsonValue(
                    valueElement);

        try
        {
            property.Value =
                requestedValue;

            document.Update();

            object? actualValue =
                PropertyByIdSupport
                    .NormalizePropertyValue(
                        property.Value);

            return PropertyByIdSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Свойство по идентификатору изменено.",

                        document =
                            document.DisplayName,

                        fullFileName =
                            document.FullFileName,

                        propertySetInternalName,

                        propertyId,

                        source,

                        property =
                            new
                            {
                                name =
                                    property.Name,

                                displayName =
                                    property.DisplayName
                            },

                        previousValue,

                        requestedValue,

                        actualValue,

                        dirty =
                            document.Dirty
                    });
        }
        catch (Exception exception)
        {
            return PropertyByIdSupport
                .CreateError(
                    "Не удалось изменить свойство по идентификатору.",
                    exception.Message);
        }
    }
}
