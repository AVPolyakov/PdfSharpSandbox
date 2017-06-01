using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using PdfSharp.Drawing;
using Xunit;
using static TableLayout.Util;

namespace TableLayout.Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            Assert("Test1", CreatePng());
        }

        public static Document GetDocument()
        {
            var document = new Document();
            document.Tables.AddRange(new [] {
                Table(),
                Table(),
                Table2(),
                Table(),
                Table(),
                Table(),
                Table(),
                Table(),
                Table2(),
                Table(),
                Table(),
            });
            return document;
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
            for (var i = 0; i < 101; i++)
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

        private static void Assert(string folderName, List<byte[]> pages)
        {
            for (var index = 0; index < pages.Count; index++)
                Xunit.Assert.True(File.ReadAllBytes(GetPageFileName(folderName, index))
                    .SequenceEqual(pages[index]));
        }

        public static List<byte[]> CreatePng()
        {
            var pages = new List<byte[]> {null};
            FillBitmap(xGraphics => Renderer.Draw(xGraphics, GetDocument(),
                    (pageIndex, action) => FillBitmap(action, bitmap => pages.Add(ToBytes(bitmap)))),
                bitmap => pages[0] = ToBytes(bitmap));
            return pages;
        }

        private static void FillBitmap(Action<XGraphics> action, Action<Bitmap> action2)
        {
            const int resolution = 254;
            var horzPixels = (int) (PageWidth.Inch * resolution);
            var vertPixels = (int) (PageHeight.Inch * resolution);
            using (var bitmap = new Bitmap(horzPixels, vertPixels))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, horzPixels, vertPixels);
                    using (var xGraphics = XGraphics.FromGraphics(graphics, new XSize(horzPixels, vertPixels)))
                    {
                        xGraphics.ScaleTransform(resolution / 72d);
                        action(xGraphics);
                    }
                }
                bitmap.SetResolution(resolution, resolution);
                action2(bitmap);
            }
        }

        private static byte[] ToBytes(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static void SavePages(List<byte[]> pages, string folderName)
        {
            for (var index = 0; index < pages.Count; index++)
                File.WriteAllBytes(
                    GetPageFileName(folderName, index),
                    pages[index]);
        }

        private static string GetPageFileName(string folderName, int index)
        {
            return Path.Combine(
                Path.Combine(
                    Path.Combine(GetPath(), "TestData"),
                    folderName),
                $"Page{index + 1}.png");
        }

        public static string GetPath([CallerFilePath] string path = "") => new FileInfo(path).Directory.FullName;
    }
}