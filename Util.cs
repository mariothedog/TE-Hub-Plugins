using System.Globalization;

namespace TEHub
{
    class Util
    {
        public static string CapitalizeEachWord(string str)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            return textInfo.ToTitleCase(str);
        }
    }
}
