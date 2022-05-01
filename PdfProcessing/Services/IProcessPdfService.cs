using PdfProcessing.Entities;

namespace PdfProcessing.Services
{
    public interface IProcessPdfService
    {
        void ProcessFile(CustomFileInfo c);
        DocumentForTextLayerModel GetDataDocument(string fileName);
    }
}