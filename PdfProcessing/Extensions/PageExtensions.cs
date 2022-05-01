using IronOcr;
using PdfProcessing.Entities.Document;
using System;
using System.Linq;

namespace PdfProcessing.Extensions
{
    public static class PageExtensions
    {
        public static Page ToLocalPage(this OcrResult.Page page, int numberInParentDocument)
        {
            try
            {
                var pageLocal = new Page()
                {
                    PageNumber = numberInParentDocument,
                    Text = page.Text,
                    Height = page.Height,
                    Width = page.Width,
                    Barcodes = page.Barcodes.Where(t => t.Format == OcrResult.BarcodeEncoding.QRCode).Select(t => new Barcode
                    {
                        Location = t.Location,
                        BarcodeNumber = t.BarcodeNumber,
                        Height = t.Height,
                        Width = t.Width,
                        X = t.X,
                        Y = t.Y,
                        Value = t.Value
                    }).ToList(),
                    Paragraphs = page.Paragraphs.Select(t => new Paragraph
                    {
                        ParagraphNumber = t.ParagraphNumber,
                        Text = t.Text,
                        X = t.X,
                        Y = t.Y,
                        Lines = t.Lines.Select(x => new Line
                        {
                            LineNumber = x.LineNumber,
                            Text = x.Text,
                            X = x.X,
                            Y = x.Y,
                            Words = x.Words.Select(y => y.ToLocalWord()).ToList()
                        }).ToList()
                    }).ToList(),
                    Words = page.Words.Select(t => t.ToLocalWord()).ToList(),
                    Lines = page.Lines.Select(t => new LineOfText
                    {
                        Location = t.Location,
                        Words = t.Words.Select(x => x.ToLocalWord()).ToList(),
                        LineNumber = t.LineNumber,
                        PageNumber = page.PageNumber,
                        Text = t.Text,
                        Height = t.Height,
                        Width = t.Width,
                        WordCount = t.Words.Count(),
                        X = t.X,
                        Y = t.Y
                    }).ToList(),
                };
                return pageLocal;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new Page();
        }
    }
}
