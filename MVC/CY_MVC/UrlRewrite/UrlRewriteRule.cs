using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace CY_MVC.UrlRewrite
{
    #region UrlRewriteRule

    //UrlRewrite规则
    [Serializable]
    [XmlRoot]
    public class UrlRewriteRule
    {
        public UrlRewriteRule()
        {
            ReDirect = false;
            Stop = true;
        }

        private Regex m_RegexShowUrl;
        private static readonly object locker = new object();

        public Regex RegexShowUrl(string ApplicationPath = "")
        {
            if (m_RegexShowUrl == null)
            {
                lock (locker)
                {
                    if (m_RegexShowUrl == null)
                    {
                        string RegexStr;
                        if (!string.IsNullOrWhiteSpace(ApplicationPath))
                        {
                            RegexStr = "^" + UrlRewriteHelper.ResolveUrl(ApplicationPath, ShowUrl.Replace(".", "\u002E")) + "$";
                        }
                        else
                        {
                            RegexStr = "^" + ShowUrl.Replace(".", "\u002E") + "$";
                        }
                        m_RegexShowUrl = new Regex(RegexStr, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                    }
                }
            }
            return m_RegexShowUrl;
        }

        [XmlAttribute]
        public string ShowUrl { get; set; }

        [XmlAttribute]
        public string RealUrl { get; set; }

        [XmlAttribute]
        public bool ReDirect { get; set; }

        [XmlAttribute]
        public bool Stop { get; set; }
    }

    #endregion UrlRewriteRule
}