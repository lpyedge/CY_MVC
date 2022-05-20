using CY_MVC.HttpHandlers;
using CY_MVC.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;

namespace CY_MVC.ViewTemplate
{
    public static class ViewTemplateHelper
    {
        private static readonly string ViewTemplateKey = typeof(ViewTemplateHelper).FullName;

        private static readonly object locker = new object();

        private static Type ViewTemplateType;

        public static void ClearCache()
        {
            MemoryCacher.RemoveByKeyStartWith(ViewTemplateKey);
        }

        public static string Extension
        {
            get { return ViewTemplate.Extension; }
        }

        private static BaseViewTemplate ViewTemplate
        {
            get
            {
                if (ViewTemplateType == null)
                {
                    lock (locker)
                    {
                        if (ViewTemplateType == null)
                        {
                            if (!string.IsNullOrWhiteSpace(StaticConfig.ViewTemplateLibraryName) && !string.Equals("ViewDefault", StaticConfig.ViewTemplateLibraryName, StringComparison.OrdinalIgnoreCase))
                            {
                                var AssemblyViewTemplateLibrary =
                                    SimpleTypeHelper.AssemblyLoad(StaticConfig.ViewTemplateLibraryName);

                                if (AssemblyViewTemplateLibrary == null)
                                {
                                    throw new Exception("未能加载模版引擎" + StaticConfig.ViewTemplateLibraryName + "！");
                                }
                                ViewTemplateType =
                                    AssemblyViewTemplateLibrary.GetType(StaticConfig.ViewTemplateLibraryName +
                                                                        ".ViewTemplate");
                            }
                            else
                            {
                                ViewTemplateType = typeof(Default.ViewTemplate);
                            }
                            if (ViewTemplateType == null)
                            {
                                throw new Exception(StaticConfig.ViewTemplateLibraryName + "模版引擎不存在");
                            }
                        }
                    }
                }
                return (BaseViewTemplate)Activator.CreateInstance(ViewTemplateType);
            }
        }

        public static BaseViewTemplate GetView(PageHandler p_BaseController)
        {
            return GetView(p_BaseController.GetType());
        }

        public static BaseViewTemplate GetView(Type p_Type)
        {
            return
                GetView(StaticConfig.TemplatePath +
                        p_Type.FullName.ToLowerInvariant()
                            .Replace(StaticConfig.LibraryName.ToLowerInvariant() + ".", string.Empty)
                            .Replace(".", "\\")
                    );
        }

        public static BaseViewTemplate GetView(string p_TemplatePath)
        {
            p_TemplatePath = p_TemplatePath.Replace("\\", "/");
            BaseViewTemplate viewTemplate;
            if (!MemoryCacher.TryGet(ViewTemplateKey + p_TemplatePath, out viewTemplate))
            {
                viewTemplate = ViewTemplate;
                var templatepath = p_TemplatePath;
                if (templatepath.StartsWith(StaticConfig.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    templatepath = templatepath.Substring(StaticConfig.RootPath.Length - 1, templatepath.Length - StaticConfig.RootPath.Length + 1);
                }
                if (!templatepath.EndsWith(ViewTemplate.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    templatepath = templatepath + ViewTemplate.Extension;
                }

                var templdatefile = new FileInfo(StaticConfig.RootPath.TrimEnd('/') + templatepath);
                if (templdatefile.Exists)
                {
                    viewTemplate.SetTemplateFile(templatepath);
                    MemoryCacher.Set(ViewTemplateKey + p_TemplatePath, viewTemplate,
                        CacheItemPriority.NotRemovable, null, TimeSpan.FromHours(1),
                       new List<ChangeMonitor>() { MemoryCacher.DependencyOnFiles(templdatefile.FullName) });
                }
                else
                {
                    throw new Exception(p_TemplatePath + "模版文件不存在");
                }
            }
            return viewTemplate;
        }

        /// <summary>
        /// 加载模版生成内容(一般用于加载子模版)
        /// </summary>
        /// <param name="p_TemplatePath">默认基于Template目录的相对路径,或者输入位于网站根目录下的绝对路径 </param>
        /// <param name="viewdata"></param>
        /// <returns></returns>
        public static string TemplateRender(string p_TemplatePath, IDictionary<string, dynamic> viewdata)
        {
            p_TemplatePath = p_TemplatePath.Replace("\\", "/");
            var view = GetView(p_TemplatePath.StartsWith(StaticConfig.RootPath, StringComparison.OrdinalIgnoreCase) ? p_TemplatePath : StaticConfig.TemplatePath + p_TemplatePath);
            if (view != null)
                return view.BuildString(viewdata);
            return string.Empty;
        }
    }
}