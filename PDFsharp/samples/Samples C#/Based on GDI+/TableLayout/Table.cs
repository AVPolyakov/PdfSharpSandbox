using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using static System.Linq.Enumerable;

namespace TableLayout
{
    public class Table
	{
		private readonly XUnit x0;
		private readonly XUnit y0;
        private readonly bool highlightCells;
        public readonly List<Column> Columns = new List<Column>();
	    public readonly List<Row> Rows = new List<Row>();

	    public Table(XUnit x0, XUnit y0, bool highlightCells = false)
		{
			this.x0 = x0;
			this.y0 = y0;
	        this.highlightCells = highlightCells;
		}

		public Column AddColumn(XUnit width)
		{
		    var column = new Column(width, Columns.Count);
            Columns.Add(column);
		    return column;
		}

	    public Row AddRow()
	    {
	        var row = new Row(this, Rows.Count);
            Rows.Add(row);
	        return row;
	    }

	    public void Draw(XGraphics graphics)
	    {
	        var maxLeftBorder = Rows.Max(row => leftBorders.Get(new CellInfo(row.Index, 0)).ValueOr(0));
	        var maxTopBorder = Columns.Max(column => topBorders.Get(new CellInfo(0, column.Index)).ValueOr(0));
	        var maxBottomBorders = Rows.ToDictionary(row => row.Index, 
	            row => Columns.Max(column => BottomBorder(new CellInfo(row, column)).ValueOr(0)));
	        var maxHeights = MaxHeights(graphics, maxBottomBorders);
	        {
                var x = x0 + maxLeftBorder;
	            foreach (var column in Columns)
	            {
	                double topBorder;
	                if (topBorders.TryGetValue(new CellInfo(0, column.Index), out topBorder))
	                {
	                    double leftBorder;
	                    if (column.Index == 0)
	                        leftBorder = leftBorders.Get(new CellInfo(0, 0)).ValueOr(0);
	                    else
	                        leftBorder = RightBorder(new CellInfo(0, column.Index - 1)).ValueOr(0);
	                    graphics.DrawLine(new XPen(XColors.Black, topBorder),
	                        x - leftBorder, y0 + maxTopBorder - topBorder/2,
	                        x + column.Width, y0 + maxTopBorder - topBorder/2);
	                }
                    x += column.Width;
	            }
	        }
	        var y = y0 + maxTopBorder;
	        foreach (var row in Rows)
	        {
	            {
	                double leftBorder;
	                if (leftBorders.TryGetValue(new CellInfo(row.Index, 0), out leftBorder))
	                {
	                    var borderX = x0 + maxLeftBorder - leftBorder/2;
	                    graphics.DrawLine(new XPen(XColors.Black, leftBorder),
	                        borderX, y, borderX, y + maxHeights[row.Index]);
	                }
	            }
	            var x = x0 + maxLeftBorder;
	            foreach (var column in Columns)
	            {
	                string text;
	                if (texts.TryGetValue(new CellInfo(row, column), out text))
	                    Util.DrawTextBox(graphics, text, x, y, ContentWidth(row, column), ParagraphAlignment.Left);
	                var rightBorder = RightBorder(new CellInfo(row, column));
	                if (rightBorder.HasValue)
	                {
	                    var borderX = x + column.Width - rightBorder.Value/2;
	                    graphics.DrawLine(new XPen(XColors.Black, rightBorder.Value),
	                        borderX, y, borderX, y + maxHeights[row.Index]);
	                }
	                var bottomBorder = BottomBorder(new CellInfo(row, column));
	                if (bottomBorder.HasValue)
	                {
	                    var borderY = y + maxHeights[row.Index] + bottomBorder.Value/2;
	                    double leftBorder;
	                    if (column.Index == 0)
	                        leftBorder = Max(leftBorders.Get(new CellInfo(row.Index, 0)),
	                            leftBorders.Get(new CellInfo(row.Index + 1, 0)));
	                    else
	                        leftBorder = Max(RightBorder(new CellInfo(row.Index, column.Index - 1)),
	                            RightBorder(new CellInfo(row.Index + 1, column.Index - 1)));
	                    graphics.DrawLine(new XPen(XColors.Black, bottomBorder.Value),
	                        x - leftBorder,
	                        borderY, x + column.Width, borderY);
	                }
	                if (highlightCells)
	                    graphics.DrawRectangle(HighlightBrush(row, column), new XRect(x, y,
	                        column.Width - RightBorder(new CellInfo(row, column)).ValueOr(0),
	                        maxHeights[row.Index]));
	                x += column.Width;
	            }
	            y += maxHeights[row.Index] + maxBottomBorders[row.Index];
	        }
		}

        private double ContentWidth(Row row, Column column)
            => colspans.Get(new CellInfo(row, column)).Match(
                colspan => column.Width
                           + Range(column.Index + 1, colspan - 1).Sum(i => Columns[i].Width)
                           - BorderWidth(row, column, column.Index + colspan - 1),
                () => column.Width - BorderWidth(row, column, column.Index));

        private double BorderWidth(Row row, Column column, int columnIndex)
            => rowspans.Get(new CellInfo(row, column)).Match(
                rowspan => Range(row.Index, rowspan)
                    .Max(i => RightBorder(new CellInfo(i, columnIndex)).ValueOr(0)),
                () => RightBorder(new CellInfo(row.Index, columnIndex)).ValueOr(0));

        private Dictionary<int, double> MaxHeights(XGraphics graphics, Dictionary<int, double> maxBottomBorders)
	    {
	        var cellContentsByBottomRow = new Dictionary<CellInfo, Tuple<string, Option<int>, Row>>();
	        foreach (var row in Rows)
	            foreach (var column in Columns)
	            {
	                string text;
	                if (texts.TryGetValue(new CellInfo(row, column), out text))
	                {
	                    var rowspan = rowspans.Get(new CellInfo(row, column));
	                    var rowIndex = rowspan.Match(value => row.Index + value - 1, () => row.Index);
	                    cellContentsByBottomRow.Add(new CellInfo(rowIndex, column.Index), 
                            Tuple.Create(text, rowspan, row));
	                }
	            }
	        var result = new Dictionary<int, double>();
	        foreach (var row in Rows)
	        {
	            var maxHeight = 0d;
	            foreach (var column in Columns)
	            {
	                Tuple<string, Option<int>, Row> tuple;
	                if (cellContentsByBottomRow.TryGetValue(new CellInfo(row, column), out tuple))
	                {
	                    var textHeight = Util.GetTextBoxHeight(graphics, tuple.Item1, 
                            ContentWidth(tuple.Item3, column));
	                    var height = tuple.Item2.Match(
	                        value => textHeight - Range(1, value - 1)
	                            .Sum(i => result[row.Index - i] + maxBottomBorders.Get(row.Index - i).ValueOr(0)),
	                        () => textHeight);
	                    if (maxHeight < height)
	                        maxHeight = height;
	                }
	            }
	            result.Add(row.Index, maxHeight);
	        }
	        return result;
	    }

        private Option<double> RightBorder(CellInfo cell)
        {
            var value1 = rightBorders.Get(cell);
            var value2 = leftBorders.Get(new CellInfo(cell.RowIndex, cell.ColumnIndex + 1));
            if (value1.HasValue)
                if (value2.HasValue)
                    throw new Exception($"The right border is ambiguous Cell=({cell.RowIndex},{cell.ColumnIndex}).");
                else
                    return value1.Value;
            else
            {
                if (value2.HasValue)
                    return value2.Value;
                else
                    return new Option<double>();
            }
        }

        private Option<double> BottomBorder(CellInfo cell)
        {
            var value1 = bottomBorders.Get(cell);
            var value2 = topBorders.Get(new CellInfo(cell.RowIndex + 1, cell.ColumnIndex));
            if (value1.HasValue)
                if (value2.HasValue)
                    throw new Exception($"The bottom border is ambiguous Cell=({cell.RowIndex},{cell.ColumnIndex}).");
                else
                    return value1.Value;
            else
            {
                if (value2.HasValue)
                    return value2.Value;
                else
                    return new Option<double>();
            }
        }

	    private static double Max(Option<double> x, Option<double> y)
	    {
	        if (x.HasValue)
	            return y.HasValue ? Math.Max(x.Value, y.Value) : x.Value;
	        else
	            return y.HasValue ? y.Value : 0;
	    }

	    private static XSolidBrush HighlightBrush(Row row, Column column)
	    {
	        if ((row.Index + column.Index)%2 == 1)
	            return new XSolidBrush(XColor.FromArgb(32, 127, 127, 127));
	        else
	            return new XSolidBrush(XColor.FromArgb(32, 0, 255, 0));
	    }

	    public Cell Cell(int rowIndex, int columnIndex) => new Cell(this, rowIndex, columnIndex);

	    private readonly Dictionary<CellInfo, string> texts = new Dictionary<CellInfo, string>();

	    public void Add(Cell cell, string text) => texts.Add(cell, text);

	    private readonly Dictionary<CellInfo, int> rowspans = new Dictionary<CellInfo, int>();

	    public void Rowspan(Cell cell, int value) => rowspans.Add(cell, value);

	    private readonly Dictionary<CellInfo, int> colspans = new Dictionary<CellInfo, int>();

	    public void Colspan(Cell cell, int value) => colspans.Add(cell, value);

	    private readonly Dictionary<CellInfo, double> leftBorders = new Dictionary<CellInfo, double>();

	    public void LeftBorder(Cell cell, double value) => leftBorders.Add(cell, value);

	    private readonly Dictionary<CellInfo, double> rightBorders = new Dictionary<CellInfo, double>();

	    public void RightBorder(Cell cell, double value) => rightBorders.Add(cell, value);

	    private readonly Dictionary<CellInfo, double> topBorders = new Dictionary<CellInfo, double>();

	    public void TopBorder(Cell cell, double value) => topBorders.Add(cell, value);

	    private readonly Dictionary<CellInfo, double> bottomBorders = new Dictionary<CellInfo, double>();

	    public void BottomBorder(Cell cell, double value) => bottomBorders.Add(cell, value);
	}
}