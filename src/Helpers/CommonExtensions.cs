using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoCreatorTool
{
    public static class CommonExtensions
    {
        public static string CleanString(this string value, bool allowSpace = true)
        {
            char[] text = value.RemoveDiacritics().ToCharArray();

            StringBuilder sb = new StringBuilder(text.Length);

            foreach (char letter in text)
            {
                if (!char.IsLetterOrDigit(letter) && letter != '_' && letter != '-' && letter != ' ')
                    continue;

                if (!allowSpace && letter == ' ')
                {
                    sb.Append('-');
                    continue;
                }

                sb.Append(letter);
            }

            return sb.ToString();
        }
        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
