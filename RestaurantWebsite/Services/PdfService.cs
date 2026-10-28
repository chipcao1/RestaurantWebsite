using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
namespace RestaurantWebsite.Services
{
    public static class PdfService
    {
        // PDFsharp GDI uses Windows Arial and embeds a Unicode subset; no external renderer.
        public static byte[] Create(string title, IEnumerable<string> lines)
        {
            using (var document = new PdfDocument())
            {
                document.Info.Title = title; document.Info.Author = "Nhà hàng Feane";
                var font = new XFont("Arial", 11, XFontStyle.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
                XGraphics graphics = null; PdfPage page = null; double y = 0;
                try
                {
                    foreach (var raw in new[] { title, "" }.ConcatLines(lines))
                    {
                        var remaining = raw ?? "";
                        do
                        {
                            if (page == null || y > page.Height.Point - 55)
                            {
                                if (graphics != null) graphics.Dispose(); page = document.AddPage(); page.Size = PdfSharp.PageSize.A4; graphics = XGraphics.FromPdfPage(page); y = 50;
                            }
                            int count = remaining.Length;
                            while (count > 1 && graphics.MeasureString(remaining.Substring(0, count), font).Width > page.Width.Point - 100) count--;
                            if (count < remaining.Length) { int space = remaining.LastIndexOf(' ', Math.Max(0, count - 1), count); if (space > 0) count = space; }
                            graphics.DrawString(remaining.Substring(0, count), font, XBrushes.Black, new XPoint(50, y)); y += 18;
                            remaining = remaining.Substring(count).TrimStart();
                        } while (remaining.Length > 0);
                    }
                }
                finally { if (graphics != null) graphics.Dispose(); }
                using (var memory = new MemoryStream()) { document.Save(memory, false); return memory.ToArray(); }
            }
        }
        private static IEnumerable<string> ConcatLines(this IEnumerable<string> head, IEnumerable<string> tail)
        { foreach (var s in head) yield return s; foreach (var s in tail) yield return s; }
    }
}
