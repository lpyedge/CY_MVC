using CY_MVC.Utility;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace CY_MVC.Seo
{
    #region SeoRule

    //Seo规则
    [Serializable]
    [XmlRoot]
    public class SeoRule
    {
        private MatchCollection m_DescriptionMatchs;

        private MatchCollection m_KeywordsMatchs;

        private MatchCollection m_TitleMatchs;

        [XmlAttribute]
        public string PageName { get; set; }

        [XmlAttribute]
        public string Title { get; set; }

        [XmlAttribute]
        public string Keywords { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlIgnore]
        public string Name { get; internal set; }

        [XmlIgnore]
        public string Info { get; internal set; }

        public MatchCollection TitleMatchs()
        {
            if (m_TitleMatchs == null)
            {
                m_TitleMatchs = ModelProperty.RegexPropertyName.Matches(Title);
            }
            return m_TitleMatchs;
        }

        public MatchCollection KeywordsMatchs()
        {
            if (m_KeywordsMatchs == null)
            {
                m_KeywordsMatchs = ModelProperty.RegexPropertyName.Matches(Keywords);
            }
            return m_KeywordsMatchs;
        }

        public MatchCollection DescriptionMatchs()
        {
            if (m_DescriptionMatchs == null)
            {
                m_DescriptionMatchs = ModelProperty.RegexPropertyName.Matches(Description);
            }
            return m_DescriptionMatchs;
        }
    }

    #endregion SeoRule
}