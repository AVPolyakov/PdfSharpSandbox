using PdfSharp.Drawing;

namespace TableLayout
{
    public class Column
    {
        public double Width { get; }
        public int Index { get; }

        internal Column(XUnit width, int index)
        {
            Width = width;
            Index = index;
        }
    }
}