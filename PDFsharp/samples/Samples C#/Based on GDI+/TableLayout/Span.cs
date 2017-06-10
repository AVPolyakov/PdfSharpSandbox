using PdfSharp.Drawing;

namespace TableLayout
{
    public class Span
    {
        public string Text { get; }
        
        public XFont Font { get; }
        
        public XBrush Brush { get; set; } = XBrushes.Black;

        public Span(string text, XFont font)
        {
            Text = text;
            Font = font;
        }
    }
}