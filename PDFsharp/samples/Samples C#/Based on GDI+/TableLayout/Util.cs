using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TableLayout
{
	public static class Util
	{
	    public static void MergeRight(Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

	    public const double BorderWidth = 0.5d*1;

		public static readonly XFont Font = new XFont("Times New Roman", 10, XFontStyle.Regular,
			new XPdfFontOptions(PdfFontEncoding.Unicode));

	    public static double Px(double value) => XUnit.FromCentimeter(value/100d);

		public static void DrawTextBox(XGraphics graphics, string text, XUnit x0, XUnit y0, double width,
			ParagraphAlignment alignment)
		{
		    var lineSpace = LineSpace(graphics);
		    var height = Ascent(graphics);
			foreach (var line in GetLines(graphics, GetWords(text).ToList(), width))
			{
				double x;
				switch (alignment)
				{
					case ParagraphAlignment.Left:
						x = x0;
						break;
					case ParagraphAlignment.Center:
						x = x0 + (width - GetWordsWidth(graphics, line, GetSpaceWidth(graphics)))/2;
						break;
					case ParagraphAlignment.Right:
						x = x0 + width - GetWordsWidth(graphics, line, GetSpaceWidth(graphics));
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
				}
				foreach (var word in line)
				{
				    graphics.DrawString(word, Font, XBrushes.Black, x, height + y0);
					x += GetSpaceWidth(graphics) + graphics.MeasureString(word, Font).Width;
				}
				height += lineSpace;
			}
		}

		public static double GetTextBoxHeight(XGraphics graphics, string text, double width)
        {
            var lineCount = GetLines(graphics, GetWords(text).ToList(), width).Count();
            return LineSpace(graphics) * lineCount;
        }

	    private static double Ascent(XGraphics graphics) => LineSpace(graphics)*CellAscent/CellSpace;

	    private static int CellSpace => Font.FontFamily.GetLineSpacing(Font.Style);

	    private static int CellAscent => Font.FontFamily.GetCellAscent(Font.Style);

	    private static double LineSpace(XGraphics graphics) => Font.GetHeight(graphics);

	    private static double GetWordsWidth(XGraphics graphics, List<string> line, double spaceWidth) 
			=> line.Sum(word => graphics.MeasureString(word, Font).Width) + (line.Count - 1)*spaceWidth;

		private static IEnumerable<List<string>> GetLines(XGraphics xGraphics, List<string> words, XUnit width)
		{
			var firstLineWordIndex = 0;
			var currentWidth = xGraphics.MeasureString(words[0], Font).Width;
		    var spaceWidth = GetSpaceWidth(xGraphics);
			for (var index = 1; index < words.Count; index++)
			{
				var wordWidth = xGraphics.MeasureString(words[index], Font).Width;
			    var newWidth = currentWidth + spaceWidth + wordWidth;
				if (newWidth >= width)
				{
					yield return words.GetRange(firstLineWordIndex, index - firstLineWordIndex);
					firstLineWordIndex = index;
					currentWidth = wordWidth;
				}
				else
					currentWidth = newWidth;
			}
			yield return words.GetRange(firstLineWordIndex, words.Count - firstLineWordIndex);
		}

		private static IEnumerable<string> GetWords(string text)
		{
			var lastNonWhiteIndex = new Option<int>();
			for (var index = 0; index < text.Length; index++)
				if (char.IsWhiteSpace(text[index]))
				{
					if (!lastNonWhiteIndex.HasValue)
					{
						if (index > 0)
							yield return text.Substring(0, index);
					}
					else
					{
						var length = index - lastNonWhiteIndex.Value - 1;
						if (length > 0)
							yield return text.Substring(lastNonWhiteIndex.Value + 1, length);
					}
					lastNonWhiteIndex = index;
				}
			if (!lastNonWhiteIndex.HasValue)
				yield return text;
			else
			{
				var length = text.Length - lastNonWhiteIndex.Value - 1;
				if (length > 0)
					yield return text.Substring(lastNonWhiteIndex.Value + 1, length);
			}
		}

	    public static double GetSpaceWidth(XGraphics graphics)
	    {
	        var xStringFormat = XStringFormats.Default;
	        xStringFormat.FormatFlags |= XStringFormatFlags.MeasureTrailingSpaces;
	        return graphics.MeasureString(" ", Font, xStringFormat).Width;
	    }

	    public static void Add<TKey, TValue>(this Dictionary<TKey, List<TValue>> it, TKey key, TValue value)
        {
            List<TValue> list;
            if (it.TryGetValue(key, out list))
                list.Add(value);
            else
                it.Add(key, new List<TValue> {value});
        }
	}
}