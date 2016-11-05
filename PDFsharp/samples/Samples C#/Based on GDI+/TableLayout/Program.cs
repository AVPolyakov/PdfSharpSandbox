using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static PdfSharp.Drawing.XUnit;
using static TableLayout.Util;

namespace TableLayout
{
	class Program
	{
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

		public static XUnit PageWidth => FromMillimeter(210);

	    public static XUnit RightMargin => FromCentimeter(1.5);

	    public static XUnit LeftMargin => FromCentimeter(3);

	    public static void MergeRight(Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

		private static void M1(XGraphics graphics, PdfPage page)
		{
		    var table = new Table(LeftMargin, Px(200), highlightCells: true);
		    var ИНН1 = table.AddColumn(Px(202));
		    var ИНН2 = table.AddColumn(Px(257));
		    var КПП = table.AddColumn(Px(454));
		    var сумма = table.AddColumn(Px(144));
		    var суммаValue1 = table.AddColumn(Px(194));
		    //var суммаValue2 = table.AddColumn(Px(181));
		    var суммаValue3 = table.AddColumn(PageWidth - LeftMargin - RightMargin
                - table.Columns.Sum(_ => _.Width));
		    {
		        var row = table.AddRow();
		        {
		            var cell = row[ИНН1];
		            MergeRight(cell, ИНН2);
		            cell.RightBorder = cell.TopBorder = cell.BottomBorder = BorderWidth;
		            cell.Add("ИНН");
		        }
		        {
		            var cell = row[КПП];
		            cell.TopBorder = cell.BottomBorder = cell.RightBorder = BorderWidth;
		            cell.Add("КПП");
		        }
		        {
		            var cell = row[сумма];
		            cell.MergeDown = 1;
		            cell.TopBorder = cell.BottomBorder = cell.RightBorder = BorderWidth;
		            cell.Add("Сумма Temp");
		        }
		        {
		            var cell = row[суммаValue1];
		            MergeRight(cell, суммаValue3);
		            cell.MergeDown = 1;
		            cell.BottomBorder = cell.TopBorder = BorderWidth;
		            cell.Add("777-33");
		        }
		    }
		    {
		        var row = table.AddRow();
		        {
		            var cell = row[ИНН1];
		            MergeRight(cell, КПП);
		            cell.MergeDown = 1;
		            cell.Add("Ромашка Ромашка Ромашка Ромашка Ромашка Ромашка Ромашка Ромашка");
		            cell.RightBorder = BorderWidth;
		        }
		    }
		    {
		        var row = table.AddRow();
		        {
		            var cell = row[сумма];
		            cell.MergeDown = 1;
		            cell.RightBorder = BorderWidth;
		            cell.Add("Сч. №");
		        }
		    }
		    {
		        var row = table.AddRow();
		        {
		            var cell = row[ИНН1];
		            MergeRight(cell, КПП);
		            cell.RightBorder = cell.BottomBorder = BorderWidth;
		            cell.Add("Плательщик");
		        }
		        row[сумма].BottomBorder = BorderWidth;
		    }
		    table.Draw(graphics);
		}

	    public const double BorderWidth = 0.5d;
	}
}
