using CY_MVC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CY_MVC.WordFilter
{
    public static class WordFilterHelper
    {
        private static List<WordFilterRule> m_WordFilterRules;
        private static readonly object locker = new object();
        public static void ClearCache()
        {
            m_WordFilterRules = null;
        }

        public static List<WordFilterRule> WordFilterRules
        {
            get
            {
                if (m_WordFilterRules == null)
                {
                    lock (locker)
                    {
                        if (m_WordFilterRules == null)
                        {
                            m_WordFilterRules = XmlDataHelper.LoadFile<List<WordFilterRule>>(StaticConfig.WordFilterRulesPath) ??
                                      new List<WordFilterRule>();
                        }
                    }
                }
                return m_WordFilterRules;
            }
            set
            {
                if (null != value)
                {
                    m_WordFilterRules = value;
                }
            }
        }

        public static void Save()
        {
            Save(WordFilterRules);
        }
        public static void Save(List<WordFilterRule> p_WordFilterRules)
        {
            if (p_WordFilterRules == null)
            {
                throw new Exception("过滤关键词列表为空!");
            }
            XmlDataHelper.SaveFile(p_WordFilterRules, StaticConfig.WordFilterRulesPath);
        }

        public static string Execute(string p_ViewStr)
        {
            if (!string.IsNullOrEmpty(StaticConfig.WordFilterRulesPath))
            {
                p_ViewStr = WordFilterRules.Aggregate(p_ViewStr,
                    (current, rule) => rule.MaskWordRegex.Replace(current, rule.ShowWord));
            }
            return p_ViewStr;
        }
    }
}