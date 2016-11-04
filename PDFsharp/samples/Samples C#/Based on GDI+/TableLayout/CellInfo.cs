using System;

namespace TableLayout
{
    public class CellInfo
    {
        private readonly Tuple<int, int> tuple;
        public int RowIndex => tuple.Item1;
        public int ColumnIndex => tuple.Item2;

        public CellInfo(Tuple<int, int> tuple)
        {
            this.tuple = tuple;
        }

        public CellInfo(int rowIndex, int columnIndex) : this(Tuple.Create(rowIndex, columnIndex))
        {
        }

        public CellInfo(Row row, Column column) : this(row.Index, column.Index)
        {
        }

        public CellInfo(Cell cell) : this(cell.RowIndex, cell.ColumnIndex)
        {
        }

        public static implicit operator CellInfo(Cell cell) => new CellInfo(cell);

        public override bool Equals(object obj)
        {
            var tuple2 = obj as CellInfo;
            if (tuple2 == null) return false;
            return tuple.Equals(tuple2.tuple);
        }

        public override int GetHashCode() => tuple.GetHashCode();
    }
}