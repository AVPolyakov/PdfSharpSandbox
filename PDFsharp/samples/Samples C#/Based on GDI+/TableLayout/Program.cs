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
	        Process.Start(M2());
	    }

	    private static string M3()
	    {
	        var filename = "temp.png";
	        using (var document = new PdfDocument())
	        {
	            document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
	            const int resolution = 254;
	            var horzPixels = (int) (PageWidth.Inch*resolution);
	            var vertPixels = (int) (PageHeight.Inch*resolution);
	            using (var bitmap = new Bitmap(horzPixels, vertPixels))
	            using (var graphics = Graphics.FromImage(bitmap))
	            {
	                graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, horzPixels, vertPixels);
	                using (var xGraphics = XGraphics.FromGraphics(graphics, new XSize(horzPixels, vertPixels)))
	                {
	                    xGraphics.ScaleTransform(resolution/72d);
	                    Renderer.Draw(document, xGraphics,new []{Table(), Table2()});
	                    bitmap.SetResolution(resolution, resolution);
	                    bitmap.Save(filename, ImageFormat.Png);
	                }
	            }
	        }
	        return filename;
	    }

	    private static string M2()
	    {
	        string filename;
	        using (var document = new PdfDocument())
	        {
	            document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
	            Renderer.Draw(document, new [] {
	                Table(),
	                //Table(),
	                //Table2(),
	                //Table(),
	                //Table(),
	                //Table(),
	                //Table(),
	                //Table(),
	                //Table2(),
	                //Table(),
	                //Table(),
	            });
	            filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
	            document.Save(filename);
	        }
	        return filename;
	    }

	    private static Table Table()
	    {
	        var table = new Table(LeftMargin);
	        var ИНН1 = table.AddColumn(Px(202));
	        var ИНН2 = table.AddColumn(Px(257));
	        var КПП = table.AddColumn(Px(454));
	        var сумма = table.AddColumn(Px(144));
	        var суммаValue = table.AddColumn(PageWidth - LeftMargin - RightMargin
	            - table.Columns.Sum(_ => _.Width));
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[ИНН1];
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Сумма прописью";
	            }
	            {
	                var cell = row[ИНН2];
	                MergeRight(cell, суммаValue);
	                cell.Text = string.Join(" ", Enumerable.Repeat("Сто рублей", 1));
	            }
	        }
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[ИНН1];
	                MergeRight(cell, ИНН2);
	                cell.RightBorder = cell.TopBorder = cell.BottomBorder = BorderWidth;
	                cell.Text = "ИНН";
	            }
	            {
	                var cell = row[КПП];
	                cell.TopBorder = cell.BottomBorder = cell.RightBorder = BorderWidth;
	                cell.Text = "КПП";
	            }
	            {
	                var cell = row[сумма];
	                cell.MergeDown = 1;
	                cell.TopBorder = cell.BottomBorder = cell.RightBorder = BorderWidth;
	                cell.Text = "Сумма";
	            }
	            {
	                var cell = row[суммаValue];
	                cell.MergeDown = 1;
	                cell.BottomBorder = cell.TopBorder = BorderWidth;
	                cell.Text = "777-33";
	            }
	        }
	        {
	            var row = table.AddRow();
	            row.Height = Px(100);
	            {
	                var cell = row[ИНН1];
	                MergeRight(cell, КПП);
	                cell.MergeDown = 1;
	                cell.Text = string.Join(" ", Enumerable.Repeat("Ромашка", 4*5));
	                cell.RightBorder = BorderWidth;
	            }
	        }
	        {
	            var row = table.AddRow();
                row.Height = Px(100);
	            {
	                var cell = row[сумма];
	                cell.MergeDown = 1;
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Сч. №";
	            }
	        }
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[ИНН1];
	                MergeRight(cell, КПП);
	                cell.RightBorder = cell.BottomBorder = BorderWidth;
	                cell.Text = "Плательщик";
	            }
	            row[сумма].BottomBorder = BorderWidth;
	        }
	        return table;
	    }

	    private static Table Table2()
	    {
	        var table = new Table(LeftMargin);
	        var c0 = table.AddColumn(Px(202));
	        var c1 = table.AddColumn(Px(257));
	        var c2 = table.AddColumn(Px(257));
	        var c3 = table.AddColumn(Px(257));
	        var c4 = table.AddColumn(PageWidth - LeftMargin - RightMargin - BorderWidth
	            - table.Columns.Sum(_ => _.Width));
	        for (var i = 0; i < 50; i++)
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[c0];
	                cell.BottomBorder = BorderWidth;
	                cell.LeftBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Text = $"№ {i}";
	            }
	            {
	                var cell = row[c1];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Колонка 2";
	            }
	            {
	                var cell = row[c2];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Колонка 3";
	            }
	            {
	                var cell = row[c3];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Колонка 4";
	            }
	            {
	                var cell = row[c4];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Text = "Колонка 5";
	            }
	        }
	        return table;
	    }

	    public static XUnit PageWidth => FromMillimeter(210);

        public static XUnit PageHeight => FromMillimeter(297);

	    public static XUnit RightMargin => FromCentimeter(1.5);

	    public static XUnit LeftMargin => FromCentimeter(3);

        public static double TopMargin => FromCentimeter(1);

        public static XUnit BottomMargin => FromCentimeter(2);

	    public static void MergeRight(Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

	    public const double BorderWidth = 0.5d*1;
	}
}
