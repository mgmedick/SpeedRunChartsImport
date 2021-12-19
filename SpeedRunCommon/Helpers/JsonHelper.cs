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
    public static class JsonHelper
    {
        public static dynamic FromUri(Uri uri, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
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

            var response = request.GetResponse();
            return FromResponse(response);
        }

        public static dynamic FromResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                return FromStream(stream);
            }
        }

        public static dynamic FromStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = "";
            try
            {
                json = reader.ReadToEnd();
            }
            catch { }
            return FromString(json);
        }

        public static dynamic FromString(string value)
        {
            JObject obj = JObject.Parse(value);

            return obj;
        }

        public static string Escape(string value)
        {
            return JavaScriptEncoder.Default.Encode(value);
        }

        public static dynamic FromUriPost(Uri uri, string postBody = null, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : (int)DefaultTimeout.TotalMilliseconds;
            request.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            request.Method = "POST";

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    request.Headers.Add(param.Key, param.Value);
                }
            }

            request.ContentType = "application/json";

            if (!string.IsNullOrWhiteSpace(postBody))
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postBody);
                }
            }

            var response = request.GetResponse();

            return FromResponse(response);
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
