using CY_MVC;
using CY_MVC.ViewTemplate;
using System.Collections.Generic;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;
using RazorEngine.Configuration;
using System.Web;
using System.Web.Razor;

namespace ViewRazor
{
    internal class ViewTemplate : BaseViewTemplate
    {
        //internal static readonly RazorEngine.Templating TemplateEngine = new RazorMachine(includeGeneratedSourceCode: false, htmlEncode: true, rootOperatorPath: null);
        internal static readonly IRazorEngineService EngineService;

        static ViewTemplate()
        {
            var config = new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(MyClassImplementingTemplateBase<>),
                //EncodedStringFactory = new RawStringFactory(),
            };
            EngineService = RazorEngineService.Create(config);
        }
        public ViewTemplate()
        {
            Extension = ".cshtml";
        }

        private string Template { get; set; }
        private ITemplateKey TemplateKey { get; set; }

        public override string TemplateText
        {
            get { return Template; }
            set
            {
                Template = value;
                File.WriteAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile, Template);
            }
        }

        public override void SetTemplateFile(string p_TemplateFile)
        {
            base.SetTemplateFile(p_TemplateFile);
            TemplateKey = ViewTemplate.EngineService.GetKey(TemplateFile);
            if (TemplateKey.Name == null)
            {
                Template = File.ReadAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile);
                EngineService.AddTemplate(TemplateFile, Template);
                TemplateKey = EngineService.GetKey(TemplateFile);
                EngineService.Compile(TemplateKey);
            }
        }

        public override string BuildString(IDictionary<string, dynamic> p_ViewData)
        {
            if (p_ViewData == null)
                return null;
            return EngineService.Run(TemplateKey, model: p_ViewData);
        }
    }
    public class MyHtmlHelper
    {
        dynamic m_templateBase;
        public MyHtmlHelper(dynamic templateBase)
        {
            m_templateBase = templateBase;
        }

        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public IEncodedString Raw(string rawString)
        {
            return new RawString(rawString);
        }

        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public IEncodedString Raw(IHtmlString rawHtmlString)
        {
            return new RawString(rawHtmlString.ToString());
        }

        public PartialParseResult Partial(string pathString)
        {
            return m_templateBase.Include(pathString);
        }

    }

    /// <summary>
    /// A simple helper demonstrating the @Html.Raw
    /// </summary>
    public abstract class MyClassImplementingTemplateBase<T> : TemplateBase<T>
    {
        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public MyClassImplementingTemplateBase()
        {
            Html = new MyHtmlHelper(this);
        }

        /// <summary>
        /// A simple helper demonstrating the @Html.Raw
        /// </summary>
        public MyHtmlHelper Html { get; set; }

        //public HtmlString RenderSection(string path, bool required = false)
        //{
        //    return Include(path);
        //}

        public TemplateWriter RenderPage(string pathString)
        {
            var templatekey = ViewTemplate.EngineService.GetKey(pathString);
            if (templatekey.Name == null)
            {
                ViewTemplate.EngineService.AddTemplate(pathString, File.ReadAllText(StaticConfig.RootPath.TrimEnd('/') + pathString.TrimStart('~', '/')));

                templatekey = ViewTemplate.EngineService.GetKey(pathString);
                ViewTemplate.EngineService.Compile(templatekey);
            }
            return Include(pathString);
        }
    }
}


