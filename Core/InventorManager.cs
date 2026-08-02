using Inventor;
using System.Runtime.InteropServices;

namespace AI_CAD_ENGINEER.Core;

public class InventorManager
{
    private Inventor.Application? _inventor;

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    private static extern int CLSIDFromProgID(
        string progId,
        out Guid clsid);

    [DllImport("oleaut32.dll", PreserveSig = true)]
    private static extern int GetActiveObject(
        ref Guid clsid,
        IntPtr reserved,
        [MarshalAs(UnmanagedType.Interface)] out object activeObject);

    public bool Connect()
    {
        if (TryConnectToRunningInventor())
        {
            return true;
        }

        return StartNewInventor();
    }

    private bool TryConnectToRunningInventor()
    {
        try
        {
            int result = CLSIDFromProgID(
                "Inventor.Application",
                out Guid clsid);

            if (result != 0)
            {
                return false;
            }

            result = GetActiveObject(
                ref clsid,
                IntPtr.Zero,
                out object activeObject);

            if (result != 0)
            {
                return false;
            }

            _inventor = activeObject as Inventor.Application;

            return _inventor != null;
        }
        catch
        {
            return false;
        }
    }

    private bool StartNewInventor()
    {
        try
        {
            Type? inventorType =
                Type.GetTypeFromProgID("Inventor.Application");

            if (inventorType == null)
            {
                return false;
            }

            _inventor =
                Activator.CreateInstance(inventorType)
                as Inventor.Application;

            if (_inventor == null)
            {
                return false;
            }

            _inventor.Visible = true;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Inventor.Application? GetInventorApplication()
    {
        return _inventor;
    }

    public Document? GetActiveDocument()
    {
        if (_inventor == null)
        {
            return null;
        }

        return _inventor.ActiveDocument;
    }

    public string GetDocumentName()
    {
        Document? document = GetActiveDocument();

        if (document == null)
        {
            return "Документ не открыт.";
        }

        return document.DisplayName;
    }

    public DocumentTypeEnum? GetDocumentType()
    {
        Document? document = GetActiveDocument();

        if (document == null)
        {
            return null;
        }

        return document.DocumentType;
    }

    public bool IsPart()
    {
        return GetDocumentType() ==
               DocumentTypeEnum.kPartDocumentObject;
    }

    public bool IsAssembly()
    {
        return GetDocumentType() ==
               DocumentTypeEnum.kAssemblyDocumentObject;
    }

    public bool IsDrawing()
    {
        return GetDocumentType() ==
               DocumentTypeEnum.kDrawingDocumentObject;
    }
}