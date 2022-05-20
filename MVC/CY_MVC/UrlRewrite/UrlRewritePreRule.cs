using CY_MVC.Utility;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CY_MVC.UrlRewrite
{
    public abstract class UrlRewritePreRule
    {
        private MatchCollection m_UrlMatchs;

        public UrlRewritePreRule()
        {
            RequiredProperties = new Dictionary<string, string>();
            Properties = new Dictionary<string, string>();
            Info = string.Empty;
            ReDirect = false;
            Stop = true;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Dictionary<string, string> RequiredProperties { get; protected set; }
        public Dictionary<string, string> Properties { get; protected set; }

        public string Text { get; protected set; }

        public string Info { get; protected set; }

        public string ShowUrl { get; protected set; }

        public string RealUrl { get; protected set; }

        public bool ReDirect { get; protected set; }

        public bool Stop { get; protected set; }

        public virtual string RewriteShowUrl
        {
            get { return ShowUrl; }
        }

        public virtual void SetRewrite(string p_Url)
        {
            if (string.IsNullOrWhiteSpace(p_Url)) throw new ArgumentException("p_Url不得为空", "p_Url");
            foreach (var property in RequiredProperties)
            {
                if (!p_Url.Contains(property.Key))
                {
                    throw new ArgumentException(
                        GetType().FullName + "类的方法SetRewrite所传参数p_Url必须包含" + property.Key + " - " + property.Value,
                        "p_Url");
                }
            }
            ShowUrl = p_Url;
        }

        public MatchCollection UrlMatchs()
        {
            if (m_UrlMatchs == null)
            {
                m_UrlMatchs = ModelProperty.RegexPropertyName.Matches(ShowUrl);
            }
            return m_UrlMatchs;
        }
    }
}