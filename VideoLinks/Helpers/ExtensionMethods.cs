using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace VideoLinks.Helpers
{
    public static class ExtensionMethods
    {
        public static int ToIntIgnoreStrings(this String str)
        {
            var result = 0;
            Int32.TryParse(Regex.Replace(str, "[^0-9]+", string.Empty), out result);
            return result;
        }


        public static string InnerHtmlValue(this HtmlNode node)
        {
            var value = string.Empty;
            if (null != node)
            {
                value = node.InnerHtml;
            }
            return value;
        }

        public static string InnerTextValue(this HtmlNode node)
        {
            var value = string.Empty;
            if (null != node)
            {
                value = node.InnerText;
            }
            return value;
        }

        public static string AttributeValue(this HtmlNode node, string attributeName)
        {
            var value = string.Empty;
            if (null != node)
            {
                var attr = node.Attributes[attributeName];
                if (null != attr)
                {
                    value = attr.Value;
                }
            }
            return value;
        }
    }
}