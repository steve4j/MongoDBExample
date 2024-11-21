using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace MongoDBIndexer
{
    #region PdfExtractor class

    public class PdfExtractor : IDisposable
    {
        private PdfDocument pdfDocument;

        public PdfExtractor(string fileName)
        {
            pdfDocument = PdfDocument.Open(fileName);
        }

        public void Dispose()
        {
            if (pdfDocument != null)
            {
                pdfDocument.Dispose();
                pdfDocument = null;
            }
        }

        public string ExtractText()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Page page in pdfDocument.GetPages())
            {
                string pageText = page.Text;
                    sb.AppendLine(pageText);
            }

            return sb.ToString();
        }
    }

    #endregion
}
