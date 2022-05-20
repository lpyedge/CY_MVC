using System;

namespace MyControllers.Models
{
    /// <summary>
    ///HotGame 的摘要说明
    /// </summary>
    [Serializable]
    public class News
    {
        private string m_Title;

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        private int m_ID;

        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        private Guid m_Key;

        public Guid Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }

        private bool m_Hot;

        public bool Hot
        {
            get { return m_Hot; }
            set { m_Hot = value; }
        }

        private DateTime m_AddDate;

        public DateTime AddDate
        {
            get { return m_AddDate; }
            set { m_AddDate = value; }
        }


        public string URL
        {
            get { return "/newsdetail." + m_ID.ToString() + ".html"; }
        }
    }
}