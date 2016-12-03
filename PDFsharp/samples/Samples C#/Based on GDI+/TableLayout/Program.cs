﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using PdfSharp;
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
	        //var tuples = Enumerable.Range(0, 10*1000*1000).Select((i, i1) => Tuple.Create(i, i1)).ToDictionary(_ => _);
	        //return;
		    {
		        Process.Start(M2());
		    }
	        //{
            //    var filename = "temp.png";
            //    using (var document = new PdfDocument())
            //    {
            //        document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
            //        var page = document.AddPage();
            //        //see method StoreTempImage to https://pdfsharp.codeplex.com/SourceControl/changeset/view/89150#MigraDoc/code/MigraDoc.RtfRendering/MigraDoc.RtfRendering/ChartRenderer.cs
            //        const int resolution = 254;
            //        var pageHeight = FromMillimeter(297);
            //        var horzPixels = (int)(page.Width.Inch * resolution);
            //        var vertPixels = (int)(pageHeight.Inch * resolution);
            //        using (var bitmap = new Bitmap(horzPixels, vertPixels))
            //        using (var graphics = Graphics.FromImage(bitmap))
            //        {
            //            graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, horzPixels, vertPixels);
            //            using (var xGraphics = XGraphics.FromGraphics(graphics, new XSize(horzPixels, vertPixels)))
            //            {
            //                xGraphics.ScaleTransform(resolution / 72d);
            //                M1(xGraphics, page);
            //                bitmap.SetResolution(resolution, resolution);
            //                bitmap.Save(filename, ImageFormat.Png);
            //            }
            //        }
            //    }
            //    Process.Start(filename);
            //}
        }

	    private static string M2()
	    {
	        string filename;
	        using (var document = new PdfDocument())
	        {
	            document.ViewerPreferences.Elements.SetName("/PrintScaling", "/None");
	            using (var xGraphics = XGraphics.FromPdfPage(document.AddPage()))
	            {
	                Table().Draw(xGraphics, document);
	            }
	            filename = $"HelloWorld_tempfile{Guid.NewGuid():N}.pdf";
	            document.Save(filename);
	        }
	        return filename;
	    }

	    private static Table Table2()
	    {
	        var table = new Table(LeftMargin, Px(200), highlightCells: false);
	        var c0 = table.AddColumn(Px(202));
	        var c1 = table.AddColumn(Px(257));
	        var c2 = table.AddColumn(Px(257));
	        var c3 = table.AddColumn(Px(257));
	        var c4 = table.AddColumn(Px(257));
	        for (var i = 0; i < 15*1000; i++)
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[c0];
	                cell.BottomBorder = BorderWidth;
	                cell.LeftBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Add($"qwe{i}");
	            }
	            {
	                var cell = row[c1];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Add("qwe2");
	            }
	            {
	                var cell = row[c2];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Add("qwe2");
	            }
	            {
	                var cell = row[c3];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Add("qwe2");
	            }
	            {
	                var cell = row[c4];
	                cell.BottomBorder = BorderWidth;
	                cell.RightBorder = BorderWidth;
	                cell.Add("qwe2");
	            }
	        }
	        return table;
	    }

	    private static Table Table()
	    {
	        var table = new Table(LeftMargin, Px(200), highlightCells: false);
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
	                cell.Add("Сумма прописью");
	            }
	            {
	                var cell = row[ИНН2];
	                MergeRight(cell, суммаValue);
	                cell.Add(string.Join(" ", Enumerable.Repeat("Сто рублей", 15)));
	            }
	        }
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
	                var cell = row[суммаValue];
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
	                cell.Add(string.Join(" ", Enumerable.Repeat("Ромашка", 355)));
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
	        {
	            var row = table.AddRow();
	            {
	                var cell = row[ИНН1];
	                cell.RightBorder = BorderWidth;
	                cell.Add("Сумма прописью");
	            }
	            {
	                var cell = row[ИНН2];
	                MergeRight(cell, суммаValue);
	                cell.Add(string.Join(" ", Enumerable.Repeat("Сто рублей", 15)));
	            }
	        }
	        return table;
	    }

		public static XUnit PageWidth => FromMillimeter(210);

        public static XUnit PageHeight => FromMillimeter(297);

	    public static XUnit RightMargin => FromCentimeter(1.5);

	    public static XUnit LeftMargin => FromCentimeter(3);

        public static XUnit TopMargin => FromCentimeter(1);

        public static XUnit BottomMargin => FromCentimeter(2);

	    public static void MergeRight(Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

	    public const double BorderWidth = 0.5d*1;
	}
}
