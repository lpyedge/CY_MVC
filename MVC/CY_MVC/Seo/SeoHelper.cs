using CY_MVC.HttpHandlers;
using CY_MVC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace CY_MVC.Seo
{
    #region SeoHelper

    /// <summary>
    ///     UrlRewrite工具类
    ///     要在web.config的AppSettings里面设置字段SEORulesPath为UrlRewrite规则的路径
    /// </summary>
    public static class SeoHelper
    {
        private static readonly string SeoKey = typeof(SeoHelper).FullName;

        internal static readonly string TitleKey = SeoKey + "_Title_";
        internal static readonly string KeywordsKey = SeoKey + "_Keywords_";
        internal static readonly string DescriptionKey = SeoKey + "_Description_";
        public static void ClearCache()
        {
            MemoryCacher.RemoveByKeyStartWith(SeoKey);
        }

        public static List<SeoRule> SeoRules
        {
            get
            {
                List<SeoRule> seoRules;
                if (!MemoryCacher.TryGet(SeoKey, out seoRules))
                {
                    seoRules = XmlDataHelper.LoadFile<List<SeoRule>>(StaticConfig.SeoRulesPath) ?? new List<SeoRule>();

                    if (seoRules.Count == 0)
                    {
                        seoRules.AddRange(from item in HandlerHelper.Handlers
                                          where item.Key != null && item.Value != null && item.Value.BaseType == typeof(PageHandler)
                                          let page = HandlerHelper.GetHandler(item.Key) as PageHandler
                                          where page != null
                                          select new SeoRule
                                          {
                                              PageName = item.Key,
                                              Name = page.Name,
                                              Title = page.Name,
                                              Keywords = page.Name,
                                              Description = page.Name,
                                              Info = page.Info
                                          });
                        Save(seoRules);
                    }
                    else
                    {
                        foreach (var seoRule in seoRules)
                        {
                            var controller =
                                HandlerHelper.Handlers.FirstOrDefault(
                                    p => string.Equals(seoRule.PageName, p.Key, StringComparison.OrdinalIgnoreCase));
                            if (controller.Key != null && controller.Value != null &&
                                controller.Value.BaseType == typeof(PageHandler))
                            {
                                var page = HandlerHelper.GetHandler(controller.Key) as PageHandler;
                                if (page != null)
                                {
                                    seoRule.Info = page.Info ?? string.Empty;
                                    seoRule.Name = page.Name ?? string.Empty;
                                }
                            }
                        }
                    }

                    seoRules = seoRules.OrderBy(p => p.PageName).ToList();

                    MemoryCacher.Set(SeoKey, seoRules, CacheItemPriority.NotRemovable, null,
                        TimeSpan.FromDays(1),
                       new List<ChangeMonitor>() { MemoryCacher.DependencyOnFiles(StaticConfig.SeoRulesPath) });
                }
                return seoRules;
            }
            set
            {
                if (null != value)
                {
                    var seoRules = value.OrderBy(p => p.PageName).ToList();

                    MemoryCacher.Set(SeoKey, seoRules, CacheItemPriority.NotRemovable, null,
                        TimeSpan.FromDays(1),
                       new List<ChangeMonitor>() { MemoryCacher.DependencyOnFiles(StaticConfig.SeoRulesPath) });

                    Save(seoRules);
                }
            }
        }

        public static void Save()
        {
            Save(SeoRules);
        }

        public static void Save(List<SeoRule> p_SeoRules)
        {
            if (p_SeoRules == null)
            {
                throw new Exception("SEO优化列表为空!");
            }
            XmlDataHelper.SaveFile(p_SeoRules, StaticConfig.SeoRulesPath);
        }

        internal static string Execute(string p_ViewStr)
        {
            var seoStr = string.Empty;

            if (HttpContext.Current.Items.Contains(TitleKey))
            {
                seoStr += "<title>" + HttpContext.Current.Items[TitleKey] + "</title>" + Environment.NewLine;
            }

            if (HttpContext.Current.Items.Contains(KeywordsKey))
            {
                seoStr += "<meta name=\"keywords\" content=\"" + HttpContext.Current.Items[KeywordsKey] + "\" />" +
                          Environment.NewLine;
            }

            if (HttpContext.Current.Items.Contains(DescriptionKey))
            {
                seoStr += "<meta name=\"description\" content=\"" + HttpContext.Current.Items[DescriptionKey] + "\" />" +
                          Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(seoStr))
            {
                p_ViewStr = p_ViewStr.Replace("<head>", "<head>" + seoStr);
            }

            return p_ViewStr;
        }

        internal static void Seo(PageHandler p_Page, string pagename, dynamic[] p_Objs)
        {
            var seorule =
                SeoRules.FirstOrDefault(p => string.Equals(p.PageName, pagename, StringComparison.OrdinalIgnoreCase));
            if (seorule == null)
            {
                SeoRules.Add(new SeoRule
                {
                    PageName = pagename,
                    Title = pagename,
                    Keywords = pagename,
                    Description = pagename
                });
                Save();
            }
            else
            {
                string Title = seorule.Title,
                    Keywords = seorule.Keywords,
                    Description = seorule.Description;
                if (p_Objs != null && p_Objs.Length > 0)
                {
                    Title = ModelProperty.RegexReplace(Title, seorule.TitleMatchs(), p_Objs);
                    Keywords = ModelProperty.RegexReplace(Keywords, seorule.KeywordsMatchs(), p_Objs);
                    Description = ModelProperty.RegexReplace(Description, seorule.DescriptionMatchs(), p_Objs);
                }
                p_Page.Title = Title;
                p_Page.Keywords = Keywords;
                p_Page.Description = Description;
            }
        }
    }

    #endregion SeoHelper
}