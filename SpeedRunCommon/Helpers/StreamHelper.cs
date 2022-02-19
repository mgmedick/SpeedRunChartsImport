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
    public static class StreamHelper
    {
        public static byte[] ReadAllBytesFromUri(this Uri uri, Dictionary<string, string> parameters = null, string userAgent = null, TimeSpan? timeout = null)
        {
            var response = RequestHelper.GetResponse(uri);
            var stream = response.GetResponseStream();

            return ReadAllBytes(stream);
        }

        public static byte[] ReadAllBytes(this Stream instream)
        {
            if (instream is MemoryStream)
                return ((MemoryStream)instream).ToArray();

            using (var memoryStream = new MemoryStream())
            {
                instream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
