using System.Collections.Generic;
using PdfSharp.Drawing;

namespace TableLayout
{
    public class Paragraph
    {
        public List<Span> Spans { get; } = new List<Span>();

        public Paragraph Add(Span span)
        {
            Spans.Add(span);
            return this;
        }
    }

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