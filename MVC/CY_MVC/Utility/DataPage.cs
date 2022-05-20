using System;
using System.Collections.Generic;

namespace CY_MVC.Utility
{
    [Serializable]
    public class DataPage
    {
        public ulong PageIndex { internal set; get; }
        public ulong PageSize { internal set; get; }
        public ulong TotalPageCount { internal set; get; }
        public ulong PageShowCount { internal set; get; }
        public ulong TotalDataCount { internal set; get; }
        public string UrlFormat { internal set; get; }
        public Dictionary<ulong, string> PageList { internal set; get; }
        public KeyValuePair<string, string> FirstPage { internal set; get; }
        public KeyValuePair<string, string> LastPage { internal set; get; }
        public KeyValuePair<string, string> PreviousPage { internal set; get; }
        public KeyValuePair<string, string> NextPage { internal set; get; }

        public static DataPage Generate(ulong p_Pageindex, ulong p_Pagesize, ulong p_Totaldatacount,
            uint p_Pageshowcount, string p_Urlformat)
        {
            long res;
            var totalpagecount = (ulong)Math.DivRem((long)p_Totaldatacount, (long)p_Pagesize, out res);
            if (res > 0)
            {
                totalpagecount++;
            }
            var model = new DataPage
            {
                PageIndex = p_Pageindex,
                PageSize = p_Pagesize,
                TotalPageCount = totalpagecount,
                PageShowCount = p_Pageshowcount,
                TotalDataCount = p_Totaldatacount,
                UrlFormat = p_Urlformat,
                FirstPage = new KeyValuePair<string, string>("首页", string.Format(p_Urlformat, 1)),
                LastPage =
                    new KeyValuePair<string, string>("末页",
                        string.Format(p_Urlformat, totalpagecount == 0 ? 1 : totalpagecount)),
                PreviousPage =
                    new KeyValuePair<string, string>("前页",
                        string.Format(p_Urlformat, p_Pageindex > 1 ? p_Pageindex - 1 : 1)),
                NextPage =
                    new KeyValuePair<string, string>("后页",
                        string.Format(p_Urlformat,
                            totalpagecount == 0 ? 1 : (p_Pageindex < totalpagecount ? p_Pageindex + 1 : totalpagecount))),
                PageList = new Dictionary<ulong, string>()
            };

            if (totalpagecount > p_Pageshowcount)
            {
                var halfshowpagecount = p_Pageshowcount / 2;
                if (p_Pageindex <= halfshowpagecount)
                {
                    for (ulong i = 1; i <= p_Pageshowcount; i++)
                    {
                        model.PageList[i] = string.Format(p_Urlformat, i);
                    }
                }
                else if (p_Pageindex >= totalpagecount - halfshowpagecount)
                {
                    for (var i = totalpagecount - p_Pageshowcount; i <= totalpagecount; i++)
                    {
                        model.PageList[i] = string.Format(p_Urlformat, i);
                    }
                }
                else
                {
                    for (var i = p_Pageindex - halfshowpagecount; i <= p_Pageindex + halfshowpagecount; i++)
                    {
                        model.PageList[i] = string.Format(p_Urlformat, i);
                    }
                }
            }
            else
            {
                for (ulong i = 1; i <= totalpagecount; i++)
                {
                    model.PageList[i] = string.Format(p_Urlformat, i);
                }
            }
            return model;
        }

        public static DataPage Generate(int p_Pageindex, int p_Pagesize, int p_Totaldatacount,
            int p_Pageshowcount, string p_Urlformat)
        {
            return Generate((ulong)p_Pageindex, (ulong)p_Pagesize, (ulong)p_Totaldatacount, (uint)p_Pageshowcount, p_Urlformat);
        }
    }
}