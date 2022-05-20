using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace CY_MVC.WordFilter
{

    //WordFilter规则
    [Serializable]
    [XmlRoot]
    public class WordFilterRule
    {
        private Regex m_MaskWordRegex;

        private string m_ShowWord;

        [XmlAttribute]
        public string MaskWord { get; set; }

        [XmlAttribute]
        public string ShowWord
        {
            get { return string.IsNullOrEmpty(m_ShowWord) ? "[***]" : m_ShowWord; }
            set { m_ShowWord = value; }
        }

        public Regex MaskWordRegex
        {
            get
            {
                return m_MaskWordRegex ??
                       (m_MaskWordRegex = new Regex(MaskWord, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }
        }
    }
}
