using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Rendering.Skia;
using UglyToad.PdfPig.Graphics.Colors;
using SkiaSharp;

namespace MongoDBIndexer
{
    #region PdfExtractor class

    public class PdfExtractor : IDisposable
    {
        private PdfDocument pdfDocument;
        private string fileName;

        public PdfExtractor(string fileName)
        {
            pdfDocument = PdfDocument.Open(fileName);
            this.fileName = fileName;
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

        public void ExtractImages(string exportDir)
        {
            string fn = Path.GetFileNameWithoutExtension(fileName);

            pdfDocument.AddSkiaPageFactory();

            for (int p = 1; p <= pdfDocument.NumberOfPages; p++)
            {
                var picture = pdfDocument.GetPage<SKPicture>(p);
                var outputFile = Path.Combine(exportDir, $"{fn}_{p}.png");

                using (var fs = new FileStream(outputFile, FileMode.Create))
                using (var ms = pdfDocument.GetPageAsPng(p, 3f, RGBColor.White))
                {
                    ms.WriteTo(fs);
                }
            }
        }
    }

    #endregion
}
