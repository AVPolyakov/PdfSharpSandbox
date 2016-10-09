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

                {
                    var y = pageMarginY + lineWidth/2;
                    graphics.DrawLine(new XPen(XColors.Black, lineWidth),
                        new XPoint(pageMarginX, y),
                        new XPoint(page.Width - pageMarginX, y));
                }

                var rowY = pageMarginY + lineWidth;
                var textY = rowY + paddingY;
                var rowHeight = paddingY + font.GetHeight(graphics) + paddingY;

                DrawVerticalLine(graphics, pageMarginX, rowY, rowHeight);

                var idX = pageMarginX + lineWidth + paddingX;
                const string id = "Код";
                graphics.DrawString(id, font, XBrushes.Black,
                    new XPoint(idX, textY),
                    XStringFormats.TopLeft);

                DrawVerticalLine(graphics,
                    idX + graphics.MeasureString(id, font).Width + paddingX,
                    rowY, rowHeight);

                const string автор = "Автор книги";
                var авторX = page.Width
                             - (graphics.MeasureString(автор, font).Width + paddingX + lineWidth + pageMarginX);

                graphics.DrawString(названиеКниги, font, XBrushes.Black,
                    new XRect(new XPoint(
                            idX + graphics.MeasureString(id, font).Width + paddingX + lineWidth,
                            textY),
                        new XPoint(авторX - paddingX - lineWidth, textY)),
                    new XStringFormat {
                        Alignment = XStringAlignment.Center,
                        LineAlignment = XLineAlignment.Near
                    });

                DrawVerticalLine(graphics, авторX - paddingX - lineWidth, rowY, rowHeight);

                graphics.DrawString(автор, font, XBrushes.Black,
                    new XPoint(
                        авторX,
                        textY),
                    XStringFormats.TopLeft);

                DrawVerticalLine(graphics, page.Width - pageMarginX - lineWidth, rowY, rowHeight);

                {
                    var y = rowY + rowHeight + lineWidth/2;
                    graphics.DrawLine(new XPen(XColors.Black, lineWidth),
                        new XPoint(pageMarginX, y),
                        new XPoint(page.Width - pageMarginX, y));
                }
            }
        }

        private static void DrawVerticalLine(XGraphics graphics, double x, double y, double rowHeight)
        {
            graphics.DrawLine(new XPen(XColors.Black, lineWidth),
                new XPoint(x + lineWidth/2, y),
                new XPoint(x + lineWidth/2, y + rowHeight));
        }

        private const double lineWidth = 1d;
    }
}
