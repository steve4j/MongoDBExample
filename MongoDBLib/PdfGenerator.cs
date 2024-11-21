using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace MongoDBLib
{
    #region PdfGenerator class

    public class PdfGenerator
    {
        public void Generate(Dictionary<string,string> data, string outputFile)
        {
            PdfDocumentBuilder builder = new PdfDocumentBuilder();

            PdfPageBuilder page = builder.AddPage(PageSize.A4);

            // Fonts must be registered with the document builder prior to use to prevent duplication.
            PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);

            int y = 700;

            foreach (var entry in data)
            {
                page.AddText(entry.Key + " --> "+ entry.Value, 12, new PdfPoint(25, y), font);
                y += 25;
            }

            byte[] documentBytes = builder.Build();

            File.WriteAllBytes(outputFile, documentBytes);
        }


    }

    #endregion
}
