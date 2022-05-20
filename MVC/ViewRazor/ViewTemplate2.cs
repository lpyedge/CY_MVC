//using CY_MVC;
//using CY_MVC.ViewTemplate;
//using System.Collections.Generic;
//using System.IO;
//using Xipton.Razor;

//namespace ViewRazor
//{
//    internal class ViewTemplate : BaseViewTemplate
//    {
//        internal static readonly RazorMachine TemplateEngine = new RazorMachine(
//            includeGeneratedSourceCode: false,
//            htmlEncode: true, 
//            defaultExtension: ".cshtml");

//        public ViewTemplate()
//        {
//            Extension = ".cshtml";
//        }

//        private string Template { get; set; }

//        public override string TemplateText
//        {
//            get { return Template; }
//            set
//            {
//                Template = value;
//                File.WriteAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile, Template);
//            }
//        }

//        public override void SetTemplateFile(string p_TemplateFile)
//        {
//            base.SetTemplateFile(p_TemplateFile);
//            Template = File.ReadAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile);
//        }

//        public override string BuildString(IDictionary<string, dynamic> p_ViewData)
//        {
//            if (p_ViewData == null)
//                return null;
//            return TemplateEngine.ExecuteContent(Template, p_ViewData).Result;
//        }
//    }

//}