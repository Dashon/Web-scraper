using System;
using System.Net;
using HtmlAgilityPack;

namespace VideoLinks.Helpers
{
    public class MyWebClient : WebClient
    {
        Uri _responseUri;

        public string ResponseUri
        {
            get
            {
                var value = string.Empty;
                if (_responseUri != null)
                {
                    value = _responseUri.ToString();
                }
                return value;
            }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            if (response != null)
            {
                _responseUri = response.ResponseUri;
            }
            return response;
        }
    }

    public class MyHtmlDocument : HtmlDocument
    {
        public string ResponseUri { get; set; }
    }

}