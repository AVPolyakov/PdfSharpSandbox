namespace TableLayout
{
    public class Cell
    {
        public Table Table { get; }
        public int RowIndex { get; }
        public int ColumnIndex { get; }

        public int Rowspan
        {
            set { Table.SetRowspan(this, value); }
        }

        public int Colspan
        {
            set { Table.SetColspan(this, value); }
        }

        public double LeftBorder
        {
            set { Table.SetLeftBorder(this, value); }
        }

        public double RightBorder
        {
            set { Table.SetRightBorder(this, value); }
        }

        public double TopBorder
        {
            set { Table.SetTopBorder(this, value); }
        }

        public double BottomBorder
        {
            set { Table.SetBottomBorder(this, value); }
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

        public string Text
        {
            set { Table.SetText(this, value); }
        }
    }
}