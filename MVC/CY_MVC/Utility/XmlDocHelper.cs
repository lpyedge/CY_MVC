using System;
using System.Linq;
using System.Xml;

namespace CY_MVC.Utility
{
    public static class XmlDocHelper
    {
        public static XmlNode Find(XmlNode xn, params string[] nodenames)
        {
            if (nodenames.Length == 0)
                return xn;
            foreach (XmlNode childNode in xn.ChildNodes)
            {
                if (string.Equals(childNode.Name, nodenames[0], StringComparison.OrdinalIgnoreCase))
                    return Find(childNode, nodenames.Skip(1).ToArray());
            }
            return null;
        }

        public static XmlNode Find(XmlDocument xn, params string[] nodenames)
        {
            if (nodenames.Length == 0)
                return xn;
            foreach (XmlNode childNode in xn.ChildNodes)
            {
                if (string.Equals(childNode.Name, nodenames[0], StringComparison.OrdinalIgnoreCase))
                    return Find(childNode, nodenames.Skip(1).ToArray());
            }
            return null;
        }
    }
}