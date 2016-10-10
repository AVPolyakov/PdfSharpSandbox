#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using static PdfSharp.Drawing.XUnit;

namespace HelloWorld
{
    /// <summary>
    /// This sample is the obligatory Hello World program.
    /// </summary>
    class Program
    {
        static void Main()
        {
            M1();
            //M2();
        }

        private static void M2()
        {
// Get a fresh copy of the sample PDF file
            string filename =
                @"..\..\InputFiles\Page1.pdf";

            // Open the file
            PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import);
            PdfDocument inputDocument2 = PdfReader.Open(
                @"..\..\InputFiles\Page3.pdf",
                PdfDocumentOpenMode.Import);

            // Create new document
            PdfDocument outputDocument = new PdfDocument();
            outputDocument.Version = inputDocument.Version;
            outputDocument.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");

            outputDocument.Info.Title = inputDocument.Info.Title;
            outputDocument.Info.Creator = inputDocument.Info.Creator;

            for (int idx = 0; idx < inputDocument.PageCount; idx++)
                outputDocument.AddPage(inputDocument.Pages[idx]);

            var page = outputDocument.AddPage(inputDocument2.Pages[0]);

            var font = new XFont("Times New Roman", 12, XFontStyle.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));

            CreatePage(page, font, "Название книги");

            var allPdf = $"All{Guid.NewGuid():N}.pdf";
            outputDocument.Save(
                allPdf);
            Process.Start(allPdf);
        }

        private static void M1()
        {
// Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";
            document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");

            var font = new XFont("Times New Roman", 12, XFontStyle.Regular,
                new XPdfFontOptions(PdfFontEncoding.Unicode));

            CreatePage(document.AddPage(), font, "Название книги");
            CreatePage(document.AddPage(), font, "Название книги2");

            // Save the document...
            var filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
            document.Save(filename);
            // ...and start a viewer.
            Process.Start(filename);
        }

        private static void CreatePage(PdfPage page, XFont font, string названиеКниги)
        {
            using (var graphics = XGraphics.FromPdfPage(page))
            {
                var pageMarginX = FromMillimeter(20);
                var pageMarginY = FromMillimeter(20);
                var paddingX = FromMillimeter(5);
                var paddingY = FromMillimeter(1);

                HorizontalLine(graphics, pageMarginX, pageMarginY, page.Width - 2*pageMarginX);

                var textY = pageMarginY + lineWidth + paddingY;
                var rowHeight = paddingY + font.GetHeight(graphics) + paddingY + 2*lineWidth;

                VerticalLine(graphics, pageMarginX, pageMarginY, rowHeight);

                var idX = pageMarginX + lineWidth + paddingX;
                const string id = "Код";
                graphics.DrawString(id, font, XBrushes.Black,
                    new XPoint(idX, textY), XStringFormats.TopLeft);

                VerticalLine(graphics,
                    idX + graphics.MeasureString(id, font).Width + paddingX,
                    pageMarginY, rowHeight);

                const string автор = "Автор книги";
                var авторX = page.Width
                             - (graphics.MeasureString(автор, font).Width + paddingX + lineWidth + pageMarginX);

                graphics.DrawString(названиеКниги, font, XBrushes.Black, new XRect(
                        new XPoint(
                            idX + graphics.MeasureString(id, font).Width + paddingX + lineWidth,
                            textY),
                        new XPoint(авторX - paddingX - lineWidth, textY)),
                    XStringFormats.TopCenter);

                VerticalLine(graphics, авторX - paddingX - lineWidth, pageMarginY, rowHeight);

                graphics.DrawString(автор, font, XBrushes.Black,
                    new XPoint(авторX, textY), XStringFormats.TopLeft);

                VerticalLine(graphics, page.Width - pageMarginX - lineWidth, pageMarginY, rowHeight);

                HorizontalLine(graphics, pageMarginX, pageMarginY + rowHeight - lineWidth, page.Width - 2*pageMarginX);
            }
        }

        private static void HorizontalLine(XGraphics graphics, double x, double y, double width)
        {
            graphics.DrawRectangle(new XPen(XColors.Black, lineWidth/2), new XRect(
                new XPoint(x + lineWidth/4, y + lineWidth/4),
                new XPoint(x + width - lineWidth/4, y + lineWidth/2 + lineWidth/4)));
        }

        private static void VerticalLine(XGraphics graphics, double x, double y, double height)
        {
            graphics.DrawRectangle(new XPen(XColors.Black, lineWidth/2), new XRect(
                new XPoint(x + lineWidth/4, y + lineWidth/4),
                new XPoint(x + lineWidth/2 + lineWidth/4, y + height - lineWidth/4)));
        }

        private static readonly double lineWidth = FromMillimeter(0.5);
    }
}
