using System;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace CY_MVC.Utility
{
    public static class HttpGzip
    {
        public static bool CanGZip()
        {
            if (HttpContext.Current != null)
            {
                return !string.IsNullOrEmpty(HttpContext.Current.Request.Headers["Accept-Encoding"]) &&
                       HttpContext.Current.Request.Headers["Accept-Encoding"].IndexOf("gzip",
                           StringComparison.OrdinalIgnoreCase) > -1;
            }
            return false;
        }

        public static byte[] GzipStr(byte[] p_Bytes)
        {
            var stream = new MemoryStream();
            using (var writer = new GZipStream(stream, CompressionMode.Compress))
            {
                writer.Write(p_Bytes, 0, p_Bytes.Length);
            }
            return stream.ToArray();
        }
    }
}