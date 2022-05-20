using CY_MVC.Utility;
using System.Diagnostics;
using System.Web;

namespace CY_MVC.HttpHandlers
{
    public abstract class CommandHandler : BaseHandler
    {
        protected byte[] ResultBytes;

        private const string gl = "gziplength";

        protected void Invoke(int gziplength = 0)
        {
            if (gziplength > 0)
                Items[gl] = gziplength;
            ResultBytes = AjaxMethod.Invoke(this.GetType());
        }

        protected void Invoke<T>(int gziplength = 0)
        {
            if (gziplength > 0)
                Items[gl] = gziplength;
            ResultBytes = AjaxMethod.Invoke(typeof(T));
        }

        protected void InvokeJsonP(int gziplength = 0)
        {
            if (gziplength > 0)
                Items[gl] = gziplength;
            ResultBytes = AjaxMethod.InvokeJsonP(this.GetType());
        }

        protected void InvokeJsonP<T>(int gziplength = 0)
        {
            if (gziplength > 0)
                Items[gl] = gziplength;
            ResultBytes = AjaxMethod.InvokeJsonP(typeof(T));
        }

        public override void ProcessRequest(HttpContext p_Context)
        {
            Stopwatch sw = null;

            if (StaticConfig.Debug)
            {
                sw = new Stopwatch();
                sw.Start();
            }

            Init(p_Context);

            Response.ContentType = "application/json;charset=UTF-8";

            Page_Load();

            if (ResultBytes != null)
            {
                var gziplength = Items.Contains(gl) ? (int)Items[gl] : StaticConfig.GzipLength;
                if (gziplength != 0 && HttpGzip.CanGZip() && ResultBytes.Length > gziplength)
                {
                    Response.AppendHeader("Content-Encoding", "gzip");
                    ResultBytes = HttpGzip.GzipStr(ResultBytes);
                }
                Response.BinaryWrite(ResultBytes);
            }

            if (StaticConfig.Debug)
            {
                sw.Stop();
                Response.Headers["Debug_ProcessTotal"] = sw.ElapsedMilliseconds.ToString();
                //sw.Restart();
            }

            if (!HttpRuntime.UsingIntegratedPipeline) return;
            //判断是否处于集成管道模式下，否则的话无法执行下列方法
            foreach (var Key in Response.Headers.AllKeys)
            {
                switch (Key.ToLowerInvariant())
                {
                    case "x-powered-by":
                        Response.Headers.Remove(Key);
                        break;

                    case "x-aspnet-version":
                        Response.Headers.Remove(Key);
                        break;

                    case "server":
                        Response.Headers.Remove(Key);
                        break;

                    case "etag":
                        Response.Headers.Remove(Key);
                        break;
                }
            }
        }
    }
}