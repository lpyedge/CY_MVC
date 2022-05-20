using System;
using System.Collections.Generic;
using System.Data;

namespace TespLib
{
    public class Dao
    {
        public static DataTable GetTable()
        {
            DataTable dt = new DataTable("temp");
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("sex", typeof(bool));
            dt.Columns.Add("regdate", typeof(DateTime));
            dt.Columns.Add("money", typeof(double));
            DataRow dr = dt.NewRow();
            for (int i = 0; i < 5; i++)
            {
                dr = dt.NewRow();
                dr["name"] = "name" + i.ToString();
                dr["sex"] = i > 3;
                dr["regdate"] = DateTime.Now.AddDays(i);
                dr["money"] = i;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static List<Users> GetList()
        {
            List<Users> list = new List<Users>();
            for (int i = 0; i < 5; i++)
            {
                Users user = new Users();
                user.name = "name" + i.ToString();
                user.sex = i > 2;
                user.regdate = DateTime.Now.AddDays(i);
                user.money = i;
                list.Add(user);
            }
            return list;
        }
    }

    [Serializable]
    public class Users
    {
        private string m_name;

        public string name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private bool m_sex;

        public bool sex
        {
            get { return m_sex; }
            set { m_sex = value; }
        }

        private DateTime m_regdate;

        public DateTime regdate
        {
            get { return m_regdate; }
            set { m_regdate = value; }
        }

        private double m_money;

        public double money
        {
            get { return m_money; }
            set { m_money = value; }
        }
    }

    [Serializable]
    public class AAA
    {
        public string name { get; set; }
        public bool sex { get; set; }
        public List<Users> users { get; set; }
    }
}