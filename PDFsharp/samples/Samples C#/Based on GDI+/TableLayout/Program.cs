using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using static PdfSharp.Drawing.XUnit;

namespace TableLayout
{
	class Program
	{
	    private static readonly double borderWidth = Util.Px(20);

	    static void Main()
		{
            {
                string filename;
                using (var document = new PdfDocument())
                {
                    document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
                    var page = document.AddPage();
                    using (var xGraphics = XGraphics.FromPdfPage(page))
                    {
                        M1(xGraphics, page);
                    }
                    filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
                    document.Save(filename);
                }
                Process.Start(filename);
            }
            {
				var filename = "temp.png";
				using (var document = new PdfDocument())
				{
					document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
					var page = document.AddPage();
					//see method StoreTempImage to https://pdfsharp.codeplex.com/SourceControl/changeset/view/89150#MigraDoc/code/MigraDoc.RtfRendering/MigraDoc.RtfRendering/ChartRenderer.cs
					const int resolution = 254;
					var pageHeight = FromMillimeter(297);
					var horzPixels = (int) (page.Width.Inch*resolution);
					var vertPixels = (int) (pageHeight.Inch*resolution);
					using (var bitmap = new Bitmap(horzPixels, vertPixels))
					using (var graphics = Graphics.FromImage(bitmap))
					{
						graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, horzPixels, vertPixels);
						using (var xGraphics = XGraphics.FromGraphics(graphics, new XSize(horzPixels, vertPixels)))
						{
							xGraphics.ScaleTransform(resolution/72d);
							M1(xGraphics, page);
							bitmap.SetResolution(resolution, resolution);
							bitmap.Save(filename, ImageFormat.Png);
						}
					}
				}
				Process.Start(filename);
			}
		}

		private static void M1(XGraphics graphics, PdfPage page)
		{
			M2(graphics);
		}

		private static void M2(XGraphics graphics)
		{
			var table = new Table(Util.Px(200), Util.Px(200), highlightCells: true);
			var column1 = table.AddColumn(Util.Px(300));
			var column2 = table.AddColumn(Util.Px(300));
			var column3 = table.AddColumn(Util.Px(300));
			var column4 = table.AddColumn(Util.Px(200));
			var column5 = table.AddColumn(Util.Px(200));
			var column6 = table.AddColumn(Util.Px(200));
			{
				var row = table.AddRow();
			    {
			        var cell = row.Cell(column1);
			        cell.Add("qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe ");
			    }
			    {
			        var cell = row.Cell(column2);
			        cell.BottomBorder = borderWidth;
			        cell.Add("qwe2");
			    }
			    row.Cell(column3).Add("qwe5");
			}
			{
				var row = table.AddRow();
			    {
			        var cell = row.Cell(column1);
			        cell.Add("qwe3");
			    }
			    {
			        var cell = row.Cell(column2);
			        cell.Add("qwe4");
                    cell.TopBorder = borderWidth;
                    cell.LeftBorder = borderWidth * 2;
                    cell.RightBorder = borderWidth * 2;
                }
			    {
			        var cell = row.Cell(column3);
			        cell.Rowspan = 2;
			        cell.Colspan = 3;
			        cell.Add("qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe2 qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe2");
			    }
			}
			{
				var row = table.AddRow();
			    {
			        var cell = row.Cell(column1);
			        cell.Add("qwe2");
			    }
			    {
			        var cell = row.Cell(column2);
                    cell.Add("qwe q q");
			    }
                row.Cell(column5).RightBorder = borderWidth;
			}
			{
				var row = table.AddRow();
			    {
			        var cell = row.Cell(column1);
			        cell.Add("qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe qwe ");
			    }
			    row.Cell(column2).Add("qwe2");
				row.Cell(column3).Add("qwe5");
			}
		    //foreach (var row in table.Rows)
		    //{
		    //    row.Cell(column1).LeftBorder = borderWidth;
		    //}
		    //foreach (var column in table.Columns)
		    //{
		    //    table.Rows[0].Cell(column).TopBorder = borderWidth;
		    //}
		    //foreach (var row in table.Rows)
		    //{
		    //    foreach (var column in table.Columns)
		    //    {
		    //        var cell = row.Cell(column);
		    //        cell.RightBorder = borderWidth;
		    //        cell.BottomBorder = borderWidth;
		    //    }
		    //}
		    table.Draw(graphics);
		}

		private static void M1(XGraphics graphics)
		{
			var pageMarginX = FromMillimeter(20);
			var pageMarginY = FromMillimeter(20);
			var paddingX = FromMillimeter(5);
			var paddingY = FromMillimeter(1);

			var text = "qqqqqqq qw qqqqqqq qw qqqqqqq qw qqqqqqq йцуqу";
			var x0 = Util.Px(100);
			var y0 = Util.Px(100);
			var width = Util.Px(336) - x0;
			var alignment = ParagraphAlignment.Left;
			graphics.DrawRectangle(XBrushes.Aqua, new XRect(x0, y0, width, 
				Util.GetTextBoxHeight(graphics, text, width)));
			Util.DrawTextBox(graphics, text, x0, y0, width, alignment);

			//new XTextFormatter(xGraphics).DrawString(text, Font, XBrushes.Aqua, new XRect(Px(100), Px(100), Px(336) - x0, 500));


			Console.WriteLine();
			//var format = new XStringFormat {LineAlignment = XLineAlignment.BaseLine};
			//xGraphics.DrawString(text, Font, XBrushes.Black, new XPoint(Px(100), Px(100)), 
			//	format);

			//var measureString = xGraphics.MeasureString(text, Font, format);

			//var isWhiteSpace = char.IsWhiteSpace(Environment.NewLine.ToCharArray()[0]);

			//HorizontalLine(xGraphics, pageMarginX, pageMarginY, page.Width - 2*pageMarginX);

			//var textY = pageMarginY + lineWidth + paddingY;
			//var rowHeight = paddingY + Font.GetHeight(xGraphics) + paddingY + 2*lineWidth;

			//VerticalLine(xGraphics, pageMarginX, pageMarginY, rowHeight);

			//var idX = pageMarginX + lineWidth + paddingX;
			//const string id = "Код";
			//xGraphics.DrawString(id, Font, XBrushes.Black,
			//	new XPoint(idX, textY), XStringFormats.TopLeft);

			//VerticalLine(xGraphics,
			//	idX + xGraphics.MeasureString(id, Font).Width + paddingX,
			//	pageMarginY, rowHeight);

			//const string автор = "Автор книги";
			//var авторX = page.Width
			//	- (xGraphics.MeasureString(автор, Font).Width + paddingX + lineWidth + pageMarginX);

			//xGraphics.DrawString("Название книги", Font, XBrushes.Black, new XRect(
			//		new XPoint(
			//			idX + xGraphics.MeasureString(id, Font).Width + paddingX + lineWidth,
			//			textY),
			//		new XPoint(авторX - paddingX - lineWidth, textY)),
			//	XStringFormats.TopCenter);

			//VerticalLine(xGraphics, авторX - paddingX - lineWidth, pageMarginY, rowHeight);

			//xGraphics.DrawString(автор, Font, XBrushes.Black,
			//	new XPoint(авторX, textY), XStringFormats.TopLeft);

			//VerticalLine(xGraphics, page.Width - pageMarginX - lineWidth, pageMarginY, rowHeight);

			//HorizontalLine(xGraphics, pageMarginX, pageMarginY + rowHeight - lineWidth, page.Width - 2*pageMarginX);
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

	    public static readonly double lineWidth = XUnit.FromMillimeter(0.5);
	}
}
