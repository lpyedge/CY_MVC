using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace CY_MVC.ViewTemplate.Default
{
    //模板页面
    //<%@ Control Language="C#" AutoEventWireup="false" Inherits="CY_MVC.ViewTemplate.Default.TemplateBody" %>
    /// <summary>
    ///
    /// </summary>
    public class ViewTemplate : BaseViewTemplate
    {
        private static readonly Regex RexControl = new Regex(@"<%@\sControl[\s\w#="".]+%>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private TemplateBody m_Template;

        public ViewTemplate()
        {
            Extension = ".ascx";
        }

        public override string TemplateText
        {
            get { return RexControl.Replace(File.ReadAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile), string.Empty); }
            set
            {
                File.WriteAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile,
                    @"<%@ Control Language=""C#"" AutoEventWireup=""false"" Inherits=""CY_MVC.ViewTemplate.Default.TemplateBody"" %>" +
                    value);
                SetTemplateFile(TemplateFile);
            }
        }

        public override void SetTemplateFile(string p_TemplateFile)
        {
            base.SetTemplateFile(p_TemplateFile);

            m_Template = new UserControl().LoadControl(TemplateFile.Replace("\\", "/")) as TemplateBody;
            if (m_Template == null)
            {
                throw new Exception(TemplateFile + @"页面未设置<%@ Control Language=""C#"" AutoEventWireup=""false"" Inherits=""CY_MVC.ViewTemplate.Default.TemplateBody"" %>");
            }
        }

        public override string BuildString(IDictionary<string, dynamic> p_ViewData)
        {
            if (m_Template != null)
            {
                m_Template.ViewData.Clear();
                if (p_ViewData != null)
                    foreach (var item in p_ViewData)
                    {
                        m_Template.ViewData[item.Key] = item.Value;
                    }
                using (TextWriter tw = new StringWriter())
                {
                    var htw = new HtmlTextWriter(tw);
                    m_Template.RenderControl(htw);
                    return tw.ToString();
                }
            }
            throw new Exception(TemplateFile +
                                @"页面未设置<%@ Control Language=""C#"" AutoEventWireup=""false"" Inherits=""CY_MVC.ViewTemplate.Default.TemplateBody"" %>");
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TemplateBody : UserControl
    {
        /// <summary>
        ///
        /// </summary>
        public TemplateBody()
        {
            ViewData = new Dictionary<string, dynamic>();
            ViewStateMode = ViewStateMode.Disabled;
            EnableTheming = false;
            EnableViewState = false;
        }

        /// <summary>
        ///
        /// </summary>
        public Dictionary<string, dynamic> ViewData { get; set; }
    }
}