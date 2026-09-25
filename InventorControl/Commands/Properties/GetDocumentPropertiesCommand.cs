using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDocumentPropertiesCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDocumentPropertiesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_document_properties";

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

        string? requestedPropertySet =
            null;

        if (root.TryGetProperty(
                "propertySetName",
                out JsonElement propertySetElement))
        {
            if (propertySetElement.ValueKind !=
                JsonValueKind.String)
            {
                return PropertyCommandSupport
                    .CreateError(
                        "Поле \"propertySetName\" должно быть строкой.");
            }

            requestedPropertySet =
                propertySetElement
                    .GetString()?
                    .Trim();

            if (string.IsNullOrWhiteSpace(
                    requestedPropertySet))
            {
                requestedPropertySet =
                    null;
            }
        }

        List<object> propertySets =
            new();

        foreach (PropertySet propertySet
                 in document.PropertySets)
        {
            if (requestedPropertySet != null &&
                !string.Equals(
                    propertySet.DisplayName,
                    requestedPropertySet,
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    propertySet.Name,
                    requestedPropertySet,
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    propertySet.InternalName,
                    requestedPropertySet,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            List<object> properties =
                new();

            foreach (Inventor.Property property
                     in propertySet)
            {
                object? normalizedValue;

                try
                {
                    normalizedValue =
                        PropertyCommandSupport
                            .NormalizePropertyValue(
                                property.Value);
                }
                catch
                {
                    normalizedValue =
                        null;
                }

                properties.Add(
                    new
                    {
                        name =
                            property.Name,

                        displayName =
                            property.DisplayName,

                        propertyId =
                            property.PropId,

                        value =
                            normalizedValue,

                        valueType =
                            normalizedValue?
                                .GetType()
                                .Name
                    });
            }

            propertySets.Add(
                new
                {
                    name =
                        propertySet.Name,

                    displayName =
                        propertySet.DisplayName,

                    internalName =
                        propertySet.InternalName,

                    propertyCount =
                        properties.Count,

                    properties
                });
        }

        if (requestedPropertySet != null &&
            propertySets.Count == 0)
        {
            return PropertyCommandSupport
                .CreateError(
                    $"Набор свойств " +
                    $"\"{requestedPropertySet}\" не найден.");
        }

        return PropertyCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        document.DisplayName,

                    fullFileName =
                        document.FullFileName,

                    propertySetCount =
                        propertySets.Count,

                    propertySets
                });
    }
}
