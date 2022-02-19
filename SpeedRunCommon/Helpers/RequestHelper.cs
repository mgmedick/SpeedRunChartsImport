using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;

namespace SpeedRunCommon
{
    public static class RequestHelper
    {
        public static WebResponse GetResponse(this Uri uri, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var request = CreateRequest(uri, parameters, userAgent, timeout);

            return request.GetResponse();
        }

        public static HttpWebRequest PostRequest(this Uri uri, string postBody = null, Dictionary<string, string> parameters = null, string contentType = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var request = CreateRequest(uri, parameters, userAgent, timeout);
            request.Method = "POST";
            request.ContentType = contentType;

            if (!string.IsNullOrWhiteSpace(postBody))
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postBody);
                }
            }

            return request;
        }

        private static HttpWebRequest CreateRequest(Uri uri, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : (int)DefaultTimeout.TotalMilliseconds;
            request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    request.Headers.Add(param.Key, param.Value);
                }
            }

            return request;
        }

        public static string DefaultUserAgent
        {
            get
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36";
            }
        }

        public static TimeSpan DefaultTimeout
        {
            get
            {
                return TimeSpan.FromSeconds(120);
            }
        }
    }
}
