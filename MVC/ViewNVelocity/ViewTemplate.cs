using Commons.Collections;
using CY_MVC;
using CY_MVC.ViewTemplate;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace ViewNVelocity
{
    internal class ViewTemplate : BaseViewTemplate
    {
        internal static readonly VelocityEngine TemplateEngine;

        static ViewTemplate()
        {
            TemplateEngine = new VelocityEngine();
            //使用设置初始化VelocityEngine       
            var props = new ExtendedProperties();

            //全局的模板库设置为空,即可防止程序报 "unable to find resource 'VM_global_library.vm'" 的错误
            props.AddProperty(RuntimeConstants.VM_LIBRARY, "");

            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "utf-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "utf-8");
            
            //props.AddProperty(RuntimeConstants.RUNTIME_LOG_LOGSYSTEM_CLASS, "null");

            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, StaticConfig.RootPath);
            //props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_CACHE, true); //是否缓存

            TemplateEngine.Init(props);
        }

        public ViewTemplate()
        {
            Extension = ".vm";
        }

        private Template Template { get; set; }

        public override string TemplateText
        {
            get { return File.ReadAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile); }
            set
            {
                File.WriteAllText(StaticConfig.RootPath.TrimEnd('/') + TemplateFile, value);
                SetTemplateFile(TemplateFile);
            }
        }

        public override void SetTemplateFile(string p_TemplateFile)
        {
            base.SetTemplateFile(p_TemplateFile);
            Template = TemplateEngine.GetTemplate(TemplateFile);
            if (Template == null)
            {
                throw new Exception(TemplateFile + @"未能正确加载模版文件" + TemplateFile);
            }
        }

        public override string BuildString(IDictionary<string, dynamic> p_ViewData)
        {
            if (p_ViewData == null)
                return null;

            var context = new VelocityContext();
            foreach (var item in p_ViewData)
            {
                context.Put(item.Key, (object)item.Value);
            }
            var writer = new StringWriter();

            Template.Merge(context, writer);

            return writer.ToString();
        }
    }
}