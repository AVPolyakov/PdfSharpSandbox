using System.Collections.Generic;

namespace TableLayout
{
    public class Table
    {
        internal readonly double X0;
        public readonly List<Column> Columns = new List<Column>();
        internal readonly List<Row> Rows = new List<Row>();

        public Table(double x0)
        {
            X0 = x0;
        }

        public Column AddColumn(double width)
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

        internal readonly Dictionary<CellInfo, string> texts = new Dictionary<CellInfo, string>();

        public void SetText(CellInfo cell, string text) => texts.Add(cell, text);

        internal readonly Dictionary<CellInfo, int> rowspans = new Dictionary<CellInfo, int>();

        public void SetRowspan(Cell cell, int value) => rowspans.Add(cell, value);

        internal readonly Dictionary<CellInfo, int> colspans = new Dictionary<CellInfo, int>();

        public void SetColspan(Cell cell, int value) => colspans.Add(cell, value);

        internal readonly Dictionary<CellInfo, double> leftBorders = new Dictionary<CellInfo, double>();

        public void SetLeftBorder(Cell cell, double value) => leftBorders.Add(cell, value);

        internal readonly Dictionary<CellInfo, double> rightBorders = new Dictionary<CellInfo, double>();

        public void SetRightBorder(Cell cell, double value) => rightBorders.Add(cell, value);

        internal readonly Dictionary<CellInfo, double> topBorders = new Dictionary<CellInfo, double>();

        public void SetTopBorder(Cell cell, double value) => topBorders.Add(cell, value);

        internal readonly Dictionary<CellInfo, double> bottomBorders = new Dictionary<CellInfo, double>();

        public void SetBottomBorder(Cell cell, double value) => bottomBorders.Add(cell, value);

        internal readonly Dictionary<int, double> rowHeights = new Dictionary<int, double>();

        public void SetRowHeight(Row row, double value) => rowHeights.Add(row.Index, value);
    }
}