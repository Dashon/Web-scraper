using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace VideoLinks.Helpers
{

    public class MyHtmlDocument : HtmlDocument
    {
        /// <summary>
        /// May be diffrent from requested Url if the page was re-directed
        /// </summary>
        public string ResponseUri { get; set; }
    }

    public class MyWebClient : WebClient
    {
        private Uri _responseUri;

        /// <summary>
        /// May be diffrent from requested Url if the page was re-directed
        /// </summary>
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
        
        /// <summary>
        /// Converts ImageUrl to Image byte array 
        /// </summary>
        /// <param name="imageUrl">Must be a image Url</param>
        /// <returns>Byte array or Null is returned if error </returns>
        public byte[] UrlToImageByteArray(string imageUrl)
        {
            byte[] byteArray = null;
            try
            {
                var webRequest = WebRequest.Create(imageUrl);
                var webResponse = webRequest.GetResponse();
                var responseStream = webResponse.GetResponseStream();
                if (responseStream != null)
                {
                    var image = Image.FromStream(responseStream);
                    var memoryStream = new MemoryStream();
                    image.Save(memoryStream, ImageFormat.Png);
                    byteArray = memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return byteArray;
        }

        /// <summary>
        /// override to export responceUri
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                var response = base.GetWebResponse(request);
                if (response != null)
                {
                    _responseUri = response.ResponseUri;
                }
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!!URL FAILED--{0}----because {1}", request.RequestUri, e);
                return null;
            }
        }
    }

}