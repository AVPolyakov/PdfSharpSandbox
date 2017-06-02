using System.Collections.Generic;
using PdfSharp.Drawing;

namespace TableLayout
{
    public class Document
    {
        public double PageWidth { get; set; } = XUnit.FromMillimeter(210);
        public double PageHeight { get; set; } = XUnit.FromMillimeter(297);

        public double LeftMargin { get; set; } = XUnit.FromCentimeter(1);
        public double RightMargin { get; set; } = XUnit.FromCentimeter(1);
        public double TopMargin { get; set; } = XUnit.FromCentimeter(1);
        public double BottomMargin { get; set; } = XUnit.FromCentimeter(1);

        public List<Table> Tables { get; set; } = new List<Table>();
    }
}