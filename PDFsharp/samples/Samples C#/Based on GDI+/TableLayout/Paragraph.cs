using System.Collections.Generic;
using PdfSharp.Drawing;

namespace TableLayout
{
    public class Paragraph
    {
        public List<Chunk> Chunks { get; } = new List<Chunk>();

        public Paragraph Add(Chunk chunk)
        {
            Chunks.Add(chunk);
            return this;
        }
    }

    public class Chunk
    {
        public string Text { get; }
        public XFont Font { get; }
        public XBrush Brush { get; set; } = XBrushes.Black;

        public Chunk(string text, XFont font)
        {
            Text = text;
            Font = font;
        }
    }
}