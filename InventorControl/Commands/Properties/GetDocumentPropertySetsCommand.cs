using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDocumentPropertySetsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDocumentPropertySetsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_document_property_sets";

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

        List<object> propertySets =
            new();

        int index =
            1;

        foreach (PropertySet propertySet
                 in document.PropertySets)
        {
            propertySets.Add(
                new
                {
                    index,

                    name =
                        propertySet.Name,

                    displayName =
                        propertySet.DisplayName,

                    internalName =
                        propertySet.InternalName,

                    propertyCount =
                        propertySet.Count
                });

            index++;
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
