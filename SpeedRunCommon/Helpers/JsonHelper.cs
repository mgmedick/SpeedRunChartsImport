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
            var response = uri.GetResponse(parameters, userAgent, timeout);

            var jsonString = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                jsonString = reader.ReadToEnd();
            }

            return JObject.Parse(jsonString);
        }

        public static dynamic FromUriPost(Uri uri, string postBody = null, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var request = uri.PostRequest(postBody, parameters, "application/json", userAgent, timeout);
            var response = request.GetResponse();

            var jsonString = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                jsonString = reader.ReadToEnd();
            }

            return JObject.Parse(jsonString);
        }
    }
}
