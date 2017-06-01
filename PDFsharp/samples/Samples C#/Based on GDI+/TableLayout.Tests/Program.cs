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
	        Process.Start(CreatePdf());
	        //Process.Start(SavePng(1));
	    }

	    public static string CreatePdf()
	    {
	        string filename;
	        using (var document = new PdfDocument())
	        {
	            document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
	            using (var xGraphics = XGraphics.FromPdfPage(document.AddPage()))
	                Renderer.Draw(xGraphics, GetDocument(), (pageIndex, action) =>  {
	                    using (var xGraphics2 = XGraphics.FromPdfPage(document.AddPage()))
	                        action(xGraphics2);
	                });
	            filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
	            document.Save(filename);
	        }
	        return filename;
	    }

	    public static string SavePng(int pageNumber)
	    {
	        const string filename = "temp.png";
            File.WriteAllBytes(filename, CreatePng()[pageNumber]);
	        return filename;
	    }
	}
}
