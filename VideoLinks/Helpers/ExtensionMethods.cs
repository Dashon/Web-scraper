using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

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

        public static MvcHtmlString ImageLink(this HtmlHelper htmlHelper,
          string source, string alternativeText, string href)
        {
            //declare the html helper 
            var image = new TagBuilder("image");
            var link = new TagBuilder("a");
            link.MergeAttribute("href", href);
            //hook the properties and add any required logic
            image.MergeAttribute("src", source);
            image.MergeAttribute("alt", alternativeText);
            //create the helper with a self closing capability
            link.InnerHtml += image;
            return MvcHtmlString.Create(link.ToString(TagRenderMode.SelfClosing));
        }
    }
}