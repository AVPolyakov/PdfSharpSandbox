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
            var document = new Document {
                LeftMargin = XUnit.FromCentimeter(3),
                RightMargin = XUnit.FromCentimeter(1.5),
                TopMargin = XUnit.FromCentimeter(0),
                BottomMargin = XUnit.FromCentimeter(0),
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
            Assert("Test1", CreatePng(document));
        }

        public static Table Table(Document document)
        {
            var table = new Table(document.LeftMargin);
            var ИНН1 = table.AddColumn(Px(202));
            var ИНН2 = table.AddColumn(Px(257));
            var КПП = table.AddColumn(Px(454));
            var сумма = table.AddColumn(Px(144));
            var суммаValue = table.AddColumn(document.PageWidth - document.LeftMargin - document.RightMargin
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

        public static Table Table2(Document document)
        {
            var table = new Table(document.LeftMargin);
            var c0 = table.AddColumn(Px(202));
            var c1 = table.AddColumn(Px(257));
            var c2 = table.AddColumn(Px(257));
            var c3 = table.AddColumn(Px(257));
            var c4 = table.AddColumn(document.PageWidth - document.LeftMargin - document.RightMargin - BorderWidth
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

        public static List<byte[]> CreatePng(Document document)
        {
            var pages = new List<byte[]> {null};
            FillBitmap(xGraphics => Renderer.Draw(xGraphics, document,
                    (pageIndex, action) => FillBitmap(action, bitmap => pages.Add(ToBytes(bitmap)), document)),
                bitmap => pages[0] = ToBytes(bitmap),
                document);
            return pages;
        }

        private static void FillBitmap(Action<XGraphics> action, Action<Bitmap> action2, Document document)
        {
            const int resolution = 254;
            var horzPixels = (int) (new XUnit(document.PageWidth).Inch * resolution);
            var vertPixels = (int) (new XUnit(document.PageHeight).Inch * resolution);
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