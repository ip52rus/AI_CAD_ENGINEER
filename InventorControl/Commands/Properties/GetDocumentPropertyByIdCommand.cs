using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDocumentPropertyByIdCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDocumentPropertyByIdCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_document_property_by_id";

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

        object? value;

        try
        {
            value =
                PropertyByIdSupport
                    .NormalizePropertyValue(
                        property.Value);
        }
        catch
        {
            value =
                null;
        }

        return PropertyByIdSupport
            .CreateSuccess(
                new
                {
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
                                property.DisplayName,

                            value,

                            valueType =
                                value?
                                    .GetType()
                                    .Name
                        }
                });
    }
}
