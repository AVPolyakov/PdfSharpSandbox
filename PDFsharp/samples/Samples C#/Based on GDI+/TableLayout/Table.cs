using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static System.Linq.Enumerable;
using static TableLayout.Program;

namespace TableLayout
{
    public class Table
    {
        private readonly double x0;
        private readonly Option<double> y0;
        private readonly bool highlightCells;
        public readonly List<Column> Columns = new List<Column>();
        private readonly List<Row> Rows = new List<Row>();

        public Table(double x0, Option<double> y0, bool highlightCells = false)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.highlightCells = highlightCells;
        }

        private double Y0 => y0.ValueOr(TopMargin);

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

        public void Draw(XGraphics xGraphics, PdfDocument document)
        {
            var leftBorderFunc = LeftBorder();
            var rightBorderFunc = RightBorder();
            var topBorderFunc = TopBorder();
            var bottomBorderFunc = BottomBorder();
            var maxLeftBorder = Rows.Max(row => leftBorderFunc(new CellInfo(row.Index, 0)).ValueOr(0));
            var maxHeights = MaxHeights(xGraphics, rightBorderFunc, bottomBorderFunc);
            var firstPage = true;
            foreach (var rows in SplitByPages(topBorderFunc, bottomBorderFunc, maxHeights))
            {
                if (firstPage)
                    Draw(leftBorderFunc, rightBorderFunc, topBorderFunc, bottomBorderFunc,
                        maxLeftBorder, maxHeights, xGraphics, rows, Y0);
                else
                    using (var xGraphics2 = XGraphics.FromPdfPage(document.AddPage()))
                        Draw(leftBorderFunc, rightBorderFunc, topBorderFunc, bottomBorderFunc,
                            maxLeftBorder, maxHeights, xGraphics2, rows, TopMargin);
                firstPage = false;
            }
        }

        private IEnumerable<IEnumerable<int>> SplitByPages(Func<CellInfo, Option<double>> topBorderFunc, Func<CellInfo, Option<double>> bottomBorderFunc,
            Dictionary<int, double> maxHeights)
        {
            var mergedRows = MergedRows();
            var y = Y0 + Columns.Max(column => topBorderFunc(new CellInfo(0, column.Index)).ValueOr(0));
            var lastRowOnPreviousPage = new Option<int>();
            var row = 0;
            var fistPage = true;
            while (true)
                if (PageHeight - BottomMargin - y - maxHeights[row] < 0)
                {
                    var firstMergedRow = FirstMergedRow(mergedRows, row);
                    if (firstMergedRow == 0)
                    {
                        if (fistPage && !y0.HasValue)
                            yield return Empty<int>();
                        else
                        {
                            yield return new[] {0};
                            lastRowOnPreviousPage = 0;
                            row = 1;
                            if (row >= Rows.Count) break;
                        }
                    }
                    else
                    {
                        var start = lastRowOnPreviousPage.Match(_ => _ + 1, () => 0);
                        if (firstMergedRow - start > 0)
                        {
                            yield return Range(start, firstMergedRow - start);
                            lastRowOnPreviousPage = firstMergedRow - 1;
                            row = firstMergedRow;
                            if (row >= Rows.Count) break;
                        }
                        else
                        {
                            var endMergedRow = EndMergedRow(mergedRows, row);
                            yield return Range(start, endMergedRow - start);
                            lastRowOnPreviousPage = endMergedRow;
                            row = endMergedRow + 1;
                            if (row >= Rows.Count) break;
                        }
                    }
                    fistPage = false;
                    y = TopMargin + Columns.Max(column => bottomBorderFunc(new CellInfo(row - 1, column.Index)).ValueOr(0));
                }
                else
                {
                    y += maxHeights[row];
                    row++;
                    if (row >= Rows.Count) break;
                }
            {
                var start = lastRowOnPreviousPage.Match(_ => _ + 1, () => 0);
                if (start < Rows.Count)
                    yield return Range(start, Rows.Count - start);
            }
        }

        private void Draw(Func<CellInfo, Option<double>> leftBorderFunc, Func<CellInfo, Option<double>> rightBorderFunc,
            Func<CellInfo, Option<double>> topBorderFunc, Func<CellInfo, Option<double>> bottomBorderFunc, double maxLeftBorder,
            Dictionary<int, double> maxHeights, XGraphics xGraphics, IEnumerable<int> rows, double y0OnPage)
        {
            var firstRow = rows.FirstOrNone();
            if (firstRow.HasValue)
            {
                var maxTopBorder = firstRow.Value == 0
                    ? Columns.Max(column => topBorderFunc(new CellInfo(firstRow.Value, column.Index)).ValueOr(0))
                    : Columns.Max(column => bottomBorderFunc(new CellInfo(firstRow.Value - 1, column.Index)).ValueOr(0));
                {
                    var x = x0 + maxLeftBorder;
                    foreach (var column in Columns)
                    {
                        var topBorder = firstRow.Value == 0
                            ? topBorderFunc(new CellInfo(firstRow.Value, column.Index))
                            : bottomBorderFunc(new CellInfo(firstRow.Value - 1, column.Index));
                        if (topBorder.HasValue)
                        {
                            var borderY = y0OnPage + maxTopBorder - topBorder.Value/2;
                            var leftBorder = column.Index == 0
                                ? leftBorderFunc(new CellInfo(firstRow.Value, 0)).ValueOr(0)
                                : rightBorderFunc(new CellInfo(firstRow.Value, column.Index - 1)).ValueOr(0);
                            xGraphics.DrawLine(new XPen(XColors.Black, topBorder.Value),
                                x - leftBorder, borderY,
                                x + column.Width, borderY);
                        }
                        x += column.Width;
                    }
                }
                var y = y0OnPage + maxTopBorder;
                foreach (var row in rows)
                {
                    var leftBorder = leftBorderFunc(new CellInfo(row, 0));
                    if (leftBorder.HasValue)
                    {
                        var borderX = x0 + maxLeftBorder - leftBorder.Value/2;
                        xGraphics.DrawLine(new XPen(XColors.Black, leftBorder.Value),
                            borderX, y, borderX, y + maxHeights[row]);
                    }
                    var x = x0 + maxLeftBorder;
                    foreach (var column in Columns)
                    {
                        string text;
                        if (texts.TryGetValue(new CellInfo(row, column.Index), out text))
                            Util.DrawTextBox(xGraphics, text, x, y, ContentWidth(row, column, rightBorderFunc), ParagraphAlignment.Left);
                        var rightBorder = rightBorderFunc(new CellInfo(row, column.Index));
                        if (rightBorder.HasValue)
                        {
                            var borderX = x + column.Width - rightBorder.Value/2;
                            xGraphics.DrawLine(new XPen(XColors.Black, rightBorder.Value),
                                borderX, y, borderX, y + maxHeights[row]);
                        }
                        var bottomBorder = bottomBorderFunc(new CellInfo(row, column.Index));
                        if (bottomBorder.HasValue)
                        {
                            var borderY = y + maxHeights[row] - bottomBorder.Value/2;
                            xGraphics.DrawLine(new XPen(XColors.Black, bottomBorder.Value),
                                x, borderY, x + column.Width, borderY);
                        }
                        if (highlightCells)
                            xGraphics.DrawRectangle(HighlightBrush(row, column), new XRect(x, y,
                                column.Width - rightBorderFunc(new CellInfo(row, column.Index)).ValueOr(0),
                                maxHeights[row] - bottomBorder.ValueOr(0)));
                        x += column.Width;
                    }
                    y += maxHeights[row];
                }
            }
        }

        private int EndMergedRow(HashSet<int> mergedRows, int row)
        {
            if (row + 1 >= Rows.Count) return row;
            var i = row + 1;
            while (true)
            {
                if (!mergedRows.Contains(i))
                    return i - 1;
                i++;
            }
        }

        private static int FirstMergedRow(HashSet<int> mergedRows, int row)
        {
            if (row == 0) return 0;
            var i = row - 1;
            while (true)
            {
                if (!mergedRows.Contains(i))
                    return i;
                i--;
            }
        }

        private HashSet<int> MergedRows()
        {
            var set = new HashSet<int>();
            foreach (var row in Rows)
                foreach (var column in Columns)
                {
                    var rowspan = rowspans.Get(new CellInfo(row, column));
                    if (rowspan.HasValue)
                        for (var i = row.Index + 1; i < row.Index + rowspan.Value; i++)
                            set.Add(i);
                }
            return set;
        }

        private double ContentWidth(int row, Column column, Func<CellInfo, Option<double>> rightBorderFunc)
            => colspans.Get(new CellInfo(row, column.Index)).Match(
                colspan => column.Width
                    + Range(column.Index + 1, colspan - 1).Sum(i => Columns[i].Width)
                    - BorderWidth(row, column, column.Index + colspan - 1, rightBorderFunc),
                () => column.Width - BorderWidth(row, column, column.Index, rightBorderFunc));

        private double BorderWidth(int row, Column column, int columnIndex, Func<CellInfo, Option<double>> rightBorderFunc)
            => rowspans.Get(new CellInfo(row, column.Index)).Match(
                rowspan => Range(row, rowspan)
                    .Max(i => rightBorderFunc(new CellInfo(i, columnIndex)).ValueOr(0)),
                () => rightBorderFunc(new CellInfo(row, columnIndex)).ValueOr(0));

        private Dictionary<int, double> MaxHeights(XGraphics graphics, Func<CellInfo, Option<double>> rightBorderFunc,
            Func<CellInfo, Option<double>> bottomBorderFunc)
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
                            ContentWidth(tuple.Item3.Index, column, rightBorderFunc));
                        var rowHeightByContent = tuple.Item2.Match(
                            value => Math.Max(textHeight - Range(1, value - 1).Sum(i => result[row.Index - i]), 0),
                            () => textHeight);
                        var height = rowHeights.Get(row.Index).Match(
                            _ => Math.Max(rowHeightByContent, _), () => rowHeightByContent);
                        var heightWithBorder = height
                            + colspans.Get(new CellInfo(row, column)).Match(
                                colspan => Range(column.Index, colspan)
                                    .Max(i => bottomBorderFunc(new CellInfo(row.Index, i)).ValueOr(0)),
                                () => bottomBorderFunc(new CellInfo(row, column)).ValueOr(0));
                        if (maxHeight < heightWithBorder)
                            maxHeight = heightWithBorder;
                    }
                }
                result.Add(row.Index, maxHeight);
            }
            return result;
        }

        private Func<CellInfo, Option<double>> RightBorder()
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in Rows)
                foreach (var column in Columns)
                {
                    var rightBorder = rightBorders.Get(new CellInfo(row, column));
                    if (rightBorder.HasValue)
                    {
                        var mergeRight = colspans.Get(new CellInfo(row, column)).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index, column.Index + mergeRight),
                            Tuple.Create(rightBorder.Value, new CellInfo(row, column)));
                        var rowspan = rowspans.Get(new CellInfo(row, column));
                        if (rowspan.HasValue)
                            for (var i = 1; i <= rowspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + i, column.Index + mergeRight),
                                    Tuple.Create(rightBorder.Value, new CellInfo(row, column)));
                    }
                    var leftBorder = leftBorders.Get(new CellInfo(row.Index, column.Index + 1));
                    if (leftBorder.HasValue)
                    {
                        var mergeRight = colspans.Get(new CellInfo(row, column)).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index, column.Index + mergeRight),
                            Tuple.Create(leftBorder.Value, new CellInfo(row.Index, column.Index + 1)));
                        var rowspan = rowspans.Get(new CellInfo(row.Index, column.Index + 1));
                        if (rowspan.HasValue)
                            for (var i = 1; i <= rowspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + i, column.Index + mergeRight),
                                    Tuple.Create(leftBorder.Value, new CellInfo(row.Index, column.Index + 1)));
                    }
                }
            return cell => result.Get(cell).Select(list => {
                if (list.Count > 1)
                    throw new Exception($"The right border is ambiguous Cells={CellsToSttring(list.Select(_ => _.Item2))}");
                else
                    return list[0].Item1;
            });
        }

        private Func<CellInfo, Option<double>> BottomBorder()
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in Rows)
                foreach (var column in Columns)
                {
                    var bottomBorder = bottomBorders.Get(new CellInfo(row, column));
                    if (bottomBorder.HasValue)
                    {
                        var mergeDown = rowspans.Get(new CellInfo(row, column)).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index + mergeDown, column.Index),
                            Tuple.Create(bottomBorder.Value, new CellInfo(row, column)));
                        var colspan = colspans.Get(new CellInfo(row, column));
                        if (colspan.HasValue)
                            for (var i = 1; i <= colspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + mergeDown, column.Index + i),
                                    Tuple.Create(bottomBorder.Value, new CellInfo(row, column)));
                    }
                    var topBorder = topBorders.Get(new CellInfo(row.Index + 1, column.Index));
                    if (topBorder.HasValue)
                    {
                        var mergeDown = rowspans.Get(new CellInfo(row, column)).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index + mergeDown, column.Index),
                            Tuple.Create(topBorder.Value, new CellInfo(row.Index + 1, column.Index)));
                        var colspan = colspans.Get(new CellInfo(row.Index + 1, column.Index));
                        if (colspan.HasValue)
                            for (var i = 1; i <= colspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + mergeDown, column.Index + i),
                                    Tuple.Create(topBorder.Value, new CellInfo(row.Index + 1, column.Index)));
                    }
                }
            return cell => result.Get(cell).Select(list => {
                if (list.Count > 1)
                    throw new Exception($"The bottom border is ambiguous Cells={CellsToSttring(list.Select(_ => _.Item2))}");
                else
                    return list[0].Item1;
            });
        }

        private Func<CellInfo, Option<double>> LeftBorder()
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in Rows)
            {
                var leftBorder = leftBorders.Get(new CellInfo(row.Index, 0));
                if (leftBorder.HasValue)
                {
                    result.Add(new CellInfo(row.Index, 0), Tuple.Create(leftBorder.Value, new CellInfo(row.Index, 0)));
                    var rowspan = rowspans.Get(new CellInfo(row.Index, 0));
                    if (rowspan.HasValue)
                        for (var i = 1; i <= rowspan.Value - 1; i++)
                            result.Add(new CellInfo(row.Index + i, 0),
                                Tuple.Create(leftBorder.Value, new CellInfo(row.Index, 0)));
                }
            }
            return cell => result.Get(cell).Select(list => {
                if (list.Count > 1)
                    throw new Exception($"The left border is ambiguous Cells={CellsToSttring(list.Select(_ => _.Item2))}");
                else
                    return list[0].Item1;
            });
        }

        private Func<CellInfo, Option<double>> TopBorder()
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var column in Columns)
            {
                var bottomBorder = topBorders.Get(new CellInfo(0, column.Index));
                if (bottomBorder.HasValue)
                {
                    result.Add(new CellInfo(0, column.Index),
                        Tuple.Create(bottomBorder.Value, new CellInfo(0, column.Index)));
                    var colspan = colspans.Get(new CellInfo(0, column.Index));
                    if (colspan.HasValue)
                        for (var i = 1; i <= colspan.Value - 1; i++)
                            result.Add(new CellInfo(0, column.Index + i),
                                Tuple.Create(bottomBorder.Value, new CellInfo(0, column.Index)));
                }
            }
            return cell => result.Get(cell).Select(list => {
                if (list.Count > 1)
                    throw new Exception($"The top border is ambiguous Cells={CellsToSttring(list.Select(_ => _.Item2))}");
                else
                    return list[0].Item1;
            });
        }

        private static string CellsToSttring(IEnumerable<CellInfo> cells) => string.Join(",", cells.Select(_ => $"({_.RowIndex},{_.ColumnIndex})"));

        private static XSolidBrush HighlightBrush(int row, Column column)
        {
            if ((row + column.Index)%2 == 1)
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

        private readonly Dictionary<int, double> rowHeights = new Dictionary<int, double>();

        public void RowHeight(Row row, double value) => rowHeights.Add(row.Index, value);
    }
}