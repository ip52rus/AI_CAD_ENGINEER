using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetOpenDocumentsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetOpenDocumentsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_open_documents";

    public string Execute(
        JsonElement root)
    {
        List<object> documents =
            new();

        int index =
            1;

        foreach (Document document
                 in _inventor.Documents)
        {
            bool isActive =
                ReferenceEquals(
                    document,
                    _inventor.ActiveDocument);

            documents.Add(
                new
                {
                    index,

                    name =
                        document.DisplayName,

                    fullFileName =
                        document.FullFileName,

                    documentType =
                        document.DocumentType
                            .ToString(),

                    dirty =
                        document.Dirty,

                    isActive
                });

            index++;
        }

        return DocumentCommandSupport
            .CreateSuccess(
                new
                {
                    documentCount =
                        documents.Count,

                    documents
                });
    }
}
