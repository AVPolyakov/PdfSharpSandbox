namespace TableLayout
{
    public class Row
    {
        public Table Table { get; }
        public int Index { get; }

        public double Height
        {
            set { Table.RowHeight(this, value); }
        }

        public Row(Table table, int index)
        {
            Table = table;
            Index = index;
        }

        public Cell this[Column column] => this[column.Index];

        public Cell this[int columnIndex] => Table.Cell(Index, columnIndex);
    }
}