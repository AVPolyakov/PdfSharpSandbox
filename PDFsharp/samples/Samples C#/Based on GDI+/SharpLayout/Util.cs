using PdfSharp.Drawing;

namespace SharpLayout
{
    public static class Util
    {
        public static void MergeRight(this Cell cell, Column dateColumn) => cell.MergeRight = dateColumn.Index - cell.ColumnIndex;

        public static double Px(double value) => XUnit.FromCentimeter(value / 100d);
    }
}