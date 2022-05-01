using IronOcr;
using PdfProcessing.Entities.Document;
using Serilog;
using System;

namespace PdfProcessing.Extensions
{
    public static class WordExtensions
    {
        public static Word ToLocalWord(this OcrResult.Word word)
        {
            try
            {
                var _word = new Word
                {
                    Location = word.Location,
                    WordNumber = word.WordNumber,
                    Text = word.Text,
                    X = word.X,
                    Y = word.Y,
                    Width = word.Width,
                    Height = word.Height,
                    FontInfos = new OcrFontInfo
                    {
                        FontIsUnderlined = word.Font != null ? word.Font.IsUnderlined : false,
                        FontIsBold = word.Font != null ? word.Font.IsBold : false,
                        FontIsItalic = word.Font != null ? word.Font.IsItalic : false,
                        FontName = word.Font != null ? !string.IsNullOrEmpty(word.Font.FontName) ? word.Font.FontName.Replace("_Bold", "").Replace("_Italic", "") : "" : null,
                        FontSize = word.Font != null ? word.Font.FontSize : 13
                    }
                };

                return _word;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());                
            }
            return new Word();
        }
    }
}
