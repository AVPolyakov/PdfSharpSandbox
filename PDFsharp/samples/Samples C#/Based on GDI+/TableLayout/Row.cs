namespace TableLayout
{
    public class Row
    {
        public Table Table { get; }
        public int Index { get; }

        public Row(Table table, int index)
        {
            Table = table;
            Index = index;
        }

        public Cell Cell(Column column) => Table.Cell(Index, column.Index);
    }
}