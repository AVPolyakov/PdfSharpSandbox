using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static TableLayout.Program;

namespace TableLayout
{
    public static class Renderer
    {
        public static void Draw(PdfDocument document, IEnumerable<Table> tables)
        {
            using (var xGraphics = XGraphics.FromPdfPage(document.AddPage()))
                Draw(document, xGraphics, tables);
        }

        public static void Draw(PdfDocument document, XGraphics xGraphics, IEnumerable<Table> tables)
        {
            var firstOnPage = true;
            var y = TopMargin;
            var list = tables.Select(table => {
                var rightBorderFunc = table.RightBorder();
                var bottomBorderFunc = table.BottomBorder();
                var tableInfo = new TableInfo(table, table.TopBorder(), bottomBorderFunc,
                    table.MaxHeights(xGraphics, rightBorderFunc, bottomBorderFunc), y, table.LeftBorder(),
                    rightBorderFunc);
                double endY;
                var splitByPages = SplitToPages(tableInfo, firstOnPage, out endY);
                if (splitByPages.Count > 0)
                    firstOnPage = false;
                y = endY;
                return new {tableInfo, splitByPages};
            }).ToList();
            if (list.Count == 0) return;
            var pages = new List<List<Tuple<TableInfo, IEnumerable<int>, double>>>();
            foreach (var item in list.SelectMany(item => item.splitByPages
                .Select((tablePage, tablePageIndex) => new {tablePage, item.tableInfo, tablePageIndex})))
                if (pages.Count > 0)
                    if (item.tablePageIndex == 0)
                        pages[pages.Count - 1].Add(
                            Tuple.Create(item.tableInfo, item.tablePage, item.tableInfo.Y));
                    else
                        pages.Add(new List<Tuple<TableInfo, IEnumerable<int>, double>> {
                            Tuple.Create(item.tableInfo, item.tablePage, TopMargin)
                        });
                else
                    pages.Add(new List<Tuple<TableInfo, IEnumerable<int>, double>> {
                        Tuple.Create(item.tableInfo, item.tablePage, TopMargin)
                    });
            for (var index = 0; index < pages.Count; index++)
                if (index == 0)
                    foreach (var tuple in pages[index])
                        Draw(tuple.Item1, tuple.Item2, tuple.Item3, xGraphics);
                else
                    using (var xGraphics2 = XGraphics.FromPdfPage(document.AddPage()))
                        foreach (var tuple in pages[index])
                            Draw(tuple.Item1, tuple.Item2, tuple.Item3, xGraphics2);
        }

        private static List<IEnumerable<int>> SplitToPages(TableInfo tableInfo, bool firstOnPage, out double endY)
        {
            if (tableInfo.Table.Rows.Count == 0)
            {
                endY = tableInfo.Y;
                return new List<IEnumerable<int>>();
            }
            var mergedRows = MergedRows(tableInfo.Table);
            var y = tableInfo.Y + tableInfo.Table.Columns.Max(column => tableInfo.TopBorderFunc(new CellInfo(0, column.Index)).ValueOr(0));
            var lastRowOnPreviousPage = new Option<int>();
            var row = 0;
            var tableFirstPage = true;
            var result = new List<IEnumerable<int>>();
            while (true)
                if (PageHeight - BottomMargin - y - tableInfo.MaxHeights[row] < 0)
                {
                    var firstMergedRow = FirstMergedRow(mergedRows, row);
                    if (firstMergedRow == 0)
                    {
                        if (tableFirstPage && !firstOnPage)
                        {
                            result.Add(Enumerable.Empty<int>());
                            lastRowOnPreviousPage = new Option<int>();
                            row = 0;
                        }
                        else
                        {
                            result.Add(new[] {0});
                            lastRowOnPreviousPage = 0;
                            row = 1;
                            if (row >= tableInfo.Table.Rows.Count) break;
                        }
                    }
                    else
                    {
                        var start = lastRowOnPreviousPage.Match(_ => _ + 1, () => 0);
                        if (firstMergedRow - start > 0)
                        {
                            result.Add(Enumerable.Range(start, firstMergedRow - start));
                            lastRowOnPreviousPage = firstMergedRow - 1;
                            row = firstMergedRow;
                            if (row >= tableInfo.Table.Rows.Count) break;
                        }
                        else
                        {
                            var endMergedRow = EndMergedRow(tableInfo.Table, mergedRows, row);
                            result.Add(Enumerable.Range(start, endMergedRow - start));
                            lastRowOnPreviousPage = endMergedRow;
                            row = endMergedRow + 1;
                            if (row >= tableInfo.Table.Rows.Count) break;
                        }
                    }
                    tableFirstPage = false;
                    y = TopMargin + (row == 0
                        ? tableInfo.Table.Columns.Max(column => tableInfo.TopBorderFunc(new CellInfo(row, column.Index)).ValueOr(0))
                        : tableInfo.Table.Columns.Max(column => tableInfo.BottomBorderFunc(new CellInfo(row - 1, column.Index)).ValueOr(0)));
                }
                else
                {
                    y += tableInfo.MaxHeights[row];
                    row++;
                    if (row >= tableInfo.Table.Rows.Count) break;
                }
            {
                var start = lastRowOnPreviousPage.Match(_ => _ + 1, () => 0);
                if (start < tableInfo.Table.Rows.Count)
                    result.Add(Enumerable.Range(start, tableInfo.Table.Rows.Count - start));
            }
            endY = y;
            return result;
        }

        private static void Draw(TableInfo info, IEnumerable<int> rows, double y0, XGraphics xGraphics)
        {
            var firstRow = rows.FirstOrNone();
            if (!firstRow.HasValue) return;
            var maxTopBorder = firstRow.Value == 0
                ? info.Table.Columns.Max(column => info.TopBorderFunc(new CellInfo(firstRow.Value, column.Index)).ValueOr(0))
                : info.Table.Columns.Max(column => info.BottomBorderFunc(new CellInfo(firstRow.Value - 1, column.Index)).ValueOr(0));
            {
                var x = info.Table.X0 + info.MaxLeftBorder;
                foreach (var column in info.Table.Columns)
                {
                    var topBorder = firstRow.Value == 0
                        ? info.TopBorderFunc(new CellInfo(firstRow.Value, column.Index))
                        : info.BottomBorderFunc(new CellInfo(firstRow.Value - 1, column.Index));
                    if (topBorder.HasValue)
                    {
                        var borderY = y0 + maxTopBorder - topBorder.Value/2;
                        var leftBorder = column.Index == 0
                            ? info.LeftBorderFunc(new CellInfo(firstRow.Value, 0)).ValueOr(0)
                            : info.RightBorderFunc(new CellInfo(firstRow.Value, column.Index - 1)).ValueOr(0);
                        xGraphics.DrawLine(new XPen(XColors.Black, topBorder.Value),
                            x - leftBorder, borderY,
                            x + column.Width, borderY);
                    }
                    x += column.Width;
                }
            }
            var y = y0 + maxTopBorder;
            foreach (var row in rows)
            {
                var leftBorder = info.LeftBorderFunc(new CellInfo(row, 0));
                if (leftBorder.HasValue)
                {
                    var borderX = info.Table.X0 + info.MaxLeftBorder - leftBorder.Value/2;
                    xGraphics.DrawLine(new XPen(XColors.Black, leftBorder.Value),
                        borderX, y, borderX, y + info.MaxHeights[row]);
                }
                var x = info.Table.X0 + info.MaxLeftBorder;
                foreach (var column in info.Table.Columns)
                {
                    var text = info.Table.Find(new CellInfo(row, column.Index)).SelectMany(_ => _.Text);
                    if (text.HasValue)
                        Util.DrawTextBox(xGraphics, text.Value, x, y, info.Table.ContentWidth(row, column, info.RightBorderFunc), ParagraphAlignment.Left);
                    var rightBorder = info.RightBorderFunc(new CellInfo(row, column.Index));
                    if (rightBorder.HasValue)
                    {
                        var borderX = x + column.Width - rightBorder.Value/2;
                        xGraphics.DrawLine(new XPen(XColors.Black, rightBorder.Value),
                            borderX, y, borderX, y + info.MaxHeights[row]);
                    }
                    var bottomBorder = info.BottomBorderFunc(new CellInfo(row, column.Index));
                    if (bottomBorder.HasValue)
                    {
                        var borderY = y + info.MaxHeights[row] - bottomBorder.Value/2;
                        xGraphics.DrawLine(new XPen(XColors.Black, bottomBorder.Value),
                            x, borderY, x + column.Width, borderY);
                    }
                    HighlightCells(xGraphics, info, bottomBorder, row, column, x, y);
                    x += column.Width;
                }
                y += info.MaxHeights[row];
            }
        }

        private static int EndMergedRow(Table table, HashSet<int> mergedRows, int row)
        {
            if (row + 1 >= table.Rows.Count) return row;
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

        private static HashSet<int> MergedRows(Table table)
        {
            var set = new HashSet<int>();
            foreach (var row in table.Rows)
                foreach (var column in table.Columns)
                {
                    var rowspan = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Rowspan);
                    if (rowspan.HasValue)
                        for (var i = row.Index + 1; i < row.Index + rowspan.Value; i++)
                            set.Add(i);
                }
            return set;
        }

        private static double ContentWidth(this Table table, int row, Column column, Func<CellInfo, Option<double>> rightBorderFunc)
            => table.Find(new CellInfo(row, column.Index)).SelectMany(_ => _.Colspan).Match(
                colspan => column.Width
                    + Enumerable.Range(column.Index + 1, colspan - 1).Sum(i => table.Columns[i].Width)
                    - table.BorderWidth(row, column, column.Index + colspan - 1, rightBorderFunc),
                () => column.Width - table.BorderWidth(row, column, column.Index, rightBorderFunc));

        private static double BorderWidth(this Table table, int row, Column column, int columnIndex, Func<CellInfo, Option<double>> rightBorderFunc)
            => table.Find(new CellInfo(row, column.Index)).SelectMany(_ => _.Rowspan).Match(
                rowspan => Enumerable.Range(row, rowspan)
                    .Max(i => rightBorderFunc(new CellInfo(i, columnIndex)).ValueOr(0)),
                () => rightBorderFunc(new CellInfo(row, columnIndex)).ValueOr(0));

        private static Dictionary<int, double> MaxHeights(this Table table, XGraphics graphics, Func<CellInfo, Option<double>> rightBorderFunc,
            Func<CellInfo, Option<double>> bottomBorderFunc)
        {
            var cellContentsByBottomRow = new Dictionary<CellInfo, Tuple<string, Option<int>, Row>>();
            foreach (var row in table.Rows)
                foreach (var column in table.Columns)
                {
                    var text = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Text);
                    if (text.HasValue)
                    {
                        var rowspan = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Rowspan);
                        var rowIndex = rowspan.Match(value => row.Index + value - 1, () => row.Index);
                        cellContentsByBottomRow.Add(new CellInfo(rowIndex, column.Index),
                            Tuple.Create(text.Value, rowspan, row));
                    }
                }
            var result = new Dictionary<int, double>();
            foreach (var row in table.Rows)
            {
                var maxHeight = 0d;
                foreach (var column in table.Columns)
                {
                    Tuple<string, Option<int>, Row> tuple;
                    if (cellContentsByBottomRow.TryGetValue(new CellInfo(row, column), out tuple))
                    {
                        var textHeight = Util.GetTextBoxHeight(graphics, tuple.Item1, table.ContentWidth(tuple.Item3.Index, column, rightBorderFunc));
                        var rowHeightByContent = tuple.Item2.Match(
                            value => Math.Max(textHeight - Enumerable.Range(1, value - 1).Sum(i => result[row.Index - i]), 0),
                            () => textHeight);
                        var height = row.Height.Match(
                            _ => Math.Max(rowHeightByContent, _), () => rowHeightByContent);
                        var heightWithBorder = height
                            + table.Find(new CellInfo(row, column)).SelectMany(_ => _.Colspan).Match(
                                colspan => Enumerable.Range(column.Index, colspan)
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

        private static Func<CellInfo, Option<double>> RightBorder(this Table table)
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in table.Rows)
                foreach (var column in table.Columns)
                {
                    var rightBorder = table.Find(new CellInfo(row, column)).SelectMany(_ => _.RightBorder);
                    if (rightBorder.HasValue)
                    {
                        var mergeRight = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Colspan).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index, column.Index + mergeRight),
                            Tuple.Create(rightBorder.Value, new CellInfo(row, column)));
                        var rowspan = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Rowspan);
                        if (rowspan.HasValue)
                            for (var i = 1; i <= rowspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + i, column.Index + mergeRight),
                                    Tuple.Create(rightBorder.Value, new CellInfo(row, column)));
                    }
                    var leftBorder = table.Find(new CellInfo(row.Index, column.Index + 1)).SelectMany(_ => _.LeftBorder);
                    if (leftBorder.HasValue)
                    {
                        var mergeRight = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Colspan).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index, column.Index + mergeRight),
                            Tuple.Create(leftBorder.Value, new CellInfo(row.Index, column.Index + 1)));
                        var rowspan = table.Find(new CellInfo(row.Index, column.Index + 1)).SelectMany(_ => _.Rowspan);
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

        private static Func<CellInfo, Option<double>> BottomBorder(this Table table)
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in table.Rows)
                foreach (var column in table.Columns)
                {
                    var bottomBorder = row[column].BottomBorder;
                    if (bottomBorder.HasValue)
                    {
                        var mergeDown = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Rowspan).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index + mergeDown, column.Index),
                            Tuple.Create(bottomBorder.Value, new CellInfo(row, column)));
                        var colspan = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Colspan);
                        if (colspan.HasValue)
                            for (var i = 1; i <= colspan.Value - 1; i++)
                                result.Add(new CellInfo(row.Index + mergeDown, column.Index + i),
                                    Tuple.Create(bottomBorder.Value, new CellInfo(row, column)));
                    }
                    var topBorder = table.Find(new CellInfo(row.Index + 1, column.Index)).SelectMany(_ => _.TopBorder);
                    if (topBorder.HasValue)
                    {
                        var mergeDown = table.Find(new CellInfo(row, column)).SelectMany(_ => _.Rowspan).Match(_ => _ - 1, () => 0);
                        result.Add(new CellInfo(row.Index + mergeDown, column.Index),
                            Tuple.Create(topBorder.Value, new CellInfo(row.Index + 1, column.Index)));
                        var colspan = table.Find(new CellInfo(row.Index + 1, column.Index)).SelectMany(_ => _.Colspan);
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

        private static Func<CellInfo, Option<double>> LeftBorder(this Table table)
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var row in table.Rows)
            {
                var leftBorder = table.Find(new CellInfo(row.Index, 0)).SelectMany(_ => _.LeftBorder);
                if (leftBorder.HasValue)
                {
                    result.Add(new CellInfo(row.Index, 0), Tuple.Create(leftBorder.Value, new CellInfo(row.Index, 0)));
                    var rowspan = table.Find(new CellInfo(row.Index, 0)).SelectMany(_ => _.Rowspan);
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

        private static Func<CellInfo, Option<double>> TopBorder(this Table table)
        {
            var result = new Dictionary<CellInfo, List<Tuple<double, CellInfo>>>();
            foreach (var column in table.Columns)
            {
                var bottomBorder = table.Find(new CellInfo(0, column.Index)).SelectMany(_ => _.TopBorder);
                if (bottomBorder.HasValue)
                {
                    result.Add(new CellInfo(0, column.Index),
                        Tuple.Create(bottomBorder.Value, new CellInfo(0, column.Index)));
                    var colspan = table.Find(new CellInfo(0, column.Index)).SelectMany(_ => _.Colspan);
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

        private class TableInfo
        {
            public Table Table { get; }
            public Func<CellInfo, Option<double>> RightBorderFunc { get; }
            public Func<CellInfo, Option<double>> LeftBorderFunc { get; }
            public Func<CellInfo, Option<double>> TopBorderFunc { get; }
            public Func<CellInfo, Option<double>> BottomBorderFunc { get; }
            public Dictionary<int, double> MaxHeights { get; }
            public double MaxLeftBorder { get; }
            public double Y { get; }

            public TableInfo(Table table, Func<CellInfo, Option<double>> topBorderFunc, Func<CellInfo, Option<double>> bottomBorderFunc,
                Dictionary<int, double> maxHeights, double y, Func<CellInfo, Option<double>> leftBorderFunc, Func<CellInfo, Option<double>> rightBorderFunc)
            {
                Table = table;
                RightBorderFunc = rightBorderFunc;
                LeftBorderFunc = leftBorderFunc;
                TopBorderFunc = topBorderFunc;
                BottomBorderFunc = bottomBorderFunc;
                MaxHeights = maxHeights;
                MaxLeftBorder = table.Rows.Max(row => leftBorderFunc(new CellInfo(row.Index, 0)).ValueOr(0));
                Y = y;                
            }
        }

        private static void HighlightCells(XGraphics xGraphics, TableInfo info, Option<double> bottomBorder, int row, Column column, double x, double y)
        {
            if (!isHighlightCells) return;
            xGraphics.DrawRectangle(
                (row + column.Index)%2 == 1
                    ? new XSolidBrush(XColor.FromArgb(32, 127, 127, 127))
                    : new XSolidBrush(XColor.FromArgb(32, 0, 255, 0)), new XRect(x, y,
                        column.Width - info.RightBorderFunc(new CellInfo(row, column.Index)).ValueOr(0),
                        info.MaxHeights[row] - bottomBorder.ValueOr(0)));
            if (column.Index == 0)
                xGraphics.DrawString($"r{row}",
                    new XFont("Times New Roman", 10, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode)),
                    new XSolidBrush(XColor.FromArgb(128, 255, 0, 0)),
                    new XRect(x - 100 - 2, y, 100, 100),
                    new XStringFormat {
                        Alignment = XStringAlignment.Far,
                        LineAlignment = XLineAlignment.Near
                    });
            if (row == 0)
                xGraphics.DrawString($"c{column.Index}",
                    new XFont("Times New Roman", 10, XFontStyle.Regular,
                        new XPdfFontOptions(PdfFontEncoding.Unicode)),
                    new XSolidBrush(XColor.FromArgb(128, 255, 0, 0)),
                    new XRect(x, y - 100, 100, 100),
                    new XStringFormat {
                        Alignment = XStringAlignment.Near,
                        LineAlignment = XLineAlignment.Far
                    });
        }

        private static bool isHighlightCells => true;
    }
}