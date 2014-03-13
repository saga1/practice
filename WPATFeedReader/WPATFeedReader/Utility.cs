using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;

namespace WPATFeedReader
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    // オプション Converter：RssテキストでHTMLタグが入ってしまう場合など
    // 
    // XAMLのテキストバインディングにConverterオプションをつける
    // TextBlock Text="{Binding Summary.Text}}"
    //  　　　　　　　　　　　　　↓
    // TextBlock Text="{Binding Summary.Text, Converter={StaticResource RssTextTrimmer}}"
    ///////////////////////////////////////////////////////////////////////////////////////////////

    public class RssTextTrimmer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            int strLength = 0;
            string fixedString = "";

            fixedString = System.Text.RegularExpressions.Regex.Replace(value.ToString(), "<[^>]+>", string.Empty);
            fixedString = fixedString.Replace("\r", "").Replace("\n", "");
            fixedString = HttpUtility.HtmlDecode(fixedString);
            strLength = fixedString.ToString().Length;

            if (strLength == 0) return null;
            else if (strLength >= 200) fixedString = fixedString.Substring(0, 200);
            return fixedString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
