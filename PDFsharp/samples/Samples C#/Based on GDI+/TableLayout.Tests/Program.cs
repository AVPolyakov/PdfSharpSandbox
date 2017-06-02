using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static TableLayout.Tests.Tests;

namespace TableLayout.Tests
{
	class Program
	{
	    static void Main()
	    {
	        var document = new Document {
                LeftMargin = XUnit.FromCentimeter(3),
                RightMargin = XUnit.FromCentimeter(1.5),
	        };
	        document.Tables.AddRange(new [] {
	            Table(document),
	            Table(document),
	            Table2(document),
	            Table(document),
	            Table(document),
	            Table(document),
	            Table(document),
	            Table(document),
	            Table2(document),
	            Table(document),
	            Table(document),
	        });
	        Process.Start(CreatePdf(document));
	        Process.Start(SavePng(document, 0));
	    }

	    public static string CreatePdf(Document document)
	    {
	        string filename;
	        using (var pdfDocument = new PdfDocument())
	        {
	            pdfDocument.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
	            using (var xGraphics = XGraphics.FromPdfPage(pdfDocument.AddPage()))
	                Renderer.Draw(xGraphics, document, (pageIndex, action) =>  {
	                    using (var xGraphics2 = XGraphics.FromPdfPage(pdfDocument.AddPage()))
	                        action(xGraphics2);
	                });
	            filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
	            pdfDocument.Save(filename);
	        }
	        return filename;
	    }

	    public static string SavePng(Document document, int pageNumber)
	    {
	        const string filename = "temp.png";
            File.WriteAllBytes(filename, CreatePng(document)[pageNumber]);
	        return filename;
	    }
	}
}
