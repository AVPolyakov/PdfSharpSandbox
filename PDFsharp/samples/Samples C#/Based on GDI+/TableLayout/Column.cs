using PdfSharp.Drawing;

namespace TableLayout
{
    public class Column
    {
        public double Width { get; }
        public int Index { get; }

        public Column(XUnit width, int index)
        {
            Width = width;
            Index = index;
        }
    }
}