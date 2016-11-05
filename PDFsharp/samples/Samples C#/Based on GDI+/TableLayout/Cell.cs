namespace TableLayout
{
    public class Cell
    {
        public Table Table { get; }
        public int RowIndex { get; }
        public int ColumnIndex { get; }

        public int Rowspan
        {
            set { Table.Rowspan(this, value); }
        }

        public int Colspan
        {
            set { Table.Colspan(this, value); }
        }

        public double LeftBorder
        {
            set { Table.LeftBorder(this, value); }
        }

        public double RightBorder
        {
            set { Table.RightBorder(this, value); }
        }

        public double TopBorder
        {
            set { Table.TopBorder(this, value); }
        }

        public double BottomBorder
        {
            set { Table.BottomBorder(this, value); }
        }

        public int MergeRight
        {
            set { Colspan = value + 1; }
        }

        public int MergeDown
        {
            set { Rowspan = value + 1; }
        }

        public Cell(Table table, int rowIndex, int columnIndex)
        {
            Table = table;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }

        public void Add(string text) => Table.Add(this, text);
    }
}