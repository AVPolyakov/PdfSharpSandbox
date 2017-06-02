using PdfSharp.Drawing;

namespace TableLayout
{
    public class Chunk
    {
        public string Text { get; }
        public XFont Font { get; }

        public Chunk(string text, XFont font)
        {
            Text = text;
            Font = font;
        }
    }
}