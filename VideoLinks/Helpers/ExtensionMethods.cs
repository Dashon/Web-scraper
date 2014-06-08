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
            Int32.TryParse(Regex.Replace(str, "[^0-9]+", string.Empty),out result);
            return result;
        }
    }
}