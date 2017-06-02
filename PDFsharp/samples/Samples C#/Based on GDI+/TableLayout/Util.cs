using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;

namespace TableLayout
{
	public static class Util
	{
	    public static void MergeRight(Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

	    public const double BorderWidth = 0.5d*1;

	    public static double Px(double value) => XUnit.FromCentimeter(value/100d);

		public static void DrawTextBox(XGraphics graphics, Chunk chunk, XUnit x0, XUnit y0, double width,
			ParagraphAlignment alignment)
		{
		    var lineSpace = LineSpace(graphics, chunk.Font);
		    var height = (lineSpace +
		            lineSpace * (chunk.Font.FontFamily.GetCellAscent(chunk.Font.Style) -
		                chunk.Font.FontFamily.GetCellDescent(chunk.Font.Style))
		            / chunk.Font.FontFamily.GetLineSpacing(chunk.Font.Style)) /
		        2;
			foreach (var line in GetLines(graphics, GetWords(chunk.Text).ToList(), width, chunk.Font))
			{
				double x;
				switch (alignment)
				{
					case ParagraphAlignment.Left:
						x = x0;
						break;
					case ParagraphAlignment.Center:
						x = x0 + (width - GetWordsWidth(graphics, line, GetSpaceWidth(graphics, chunk.Font), chunk.Font))/2;
						break;
					case ParagraphAlignment.Right:
						x = x0 + width - GetWordsWidth(graphics, line, GetSpaceWidth(graphics, chunk.Font), chunk.Font);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
				}
				foreach (var word in line)
				{
				    graphics.DrawString(word, chunk.Font, XBrushes.Black, x, height + y0);
					x += GetSpaceWidth(graphics, chunk.Font) + graphics.MeasureString(word, chunk.Font).Width;
				}
				height += lineSpace;
			}
		}

		public static double GetTextBoxHeight(XGraphics graphics, Chunk chunk, double width)
        {
            var lineCount = GetLines(graphics, GetWords(chunk.Text).ToList(), width, chunk.Font).Count();
            return LineSpace(graphics, chunk.Font) * lineCount;
        }

	    private static double LineSpace(XGraphics graphics, XFont font) => font.GetHeight(graphics);

	    private static double GetWordsWidth(XGraphics graphics, List<string> line, double spaceWidth, XFont font) 
			=> line.Sum(word => graphics.MeasureString(word, font).Width) + (line.Count - 1)*spaceWidth;

		private static IEnumerable<List<string>> GetLines(XGraphics xGraphics, List<string> words, XUnit width, XFont font)
		{
			var firstLineWordIndex = 0;
			var currentWidth = xGraphics.MeasureString(words[0], font).Width;
		    var spaceWidth = GetSpaceWidth(xGraphics, font);
			for (var index = 1; index < words.Count; index++)
			{
				var wordWidth = xGraphics.MeasureString(words[index], font).Width;
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

	    public static double GetSpaceWidth(XGraphics graphics, XFont font)
	    {
	        var xStringFormat = XStringFormats.Default;
	        xStringFormat.FormatFlags |= XStringFormatFlags.MeasureTrailingSpaces;
	        return graphics.MeasureString(" ", font, xStringFormat).Width;
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