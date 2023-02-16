using CY_MVC.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;

namespace CY_MVC
{
    public static class StaticConfig
    {
        static StaticConfig()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase;
            if (path != null)
            {
                if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(8);
                }
                else if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(5);
                }

                var config = ConfigurationManager.OpenExeConfiguration(path);

                RootPath = config.AppSettings.Settings["RootPath"] == null ||
                           string.IsNullOrWhiteSpace(config.AppSettings.Settings["RootPath"].Value)
                    ? AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/")
                    : (AppDomain.CurrentDomain.BaseDirectory +
                       config.AppSettings.Settings["RootPath"].Value.TrimStart('/').TrimStart('\\'))
                        .Replace(
                            "\\", "/");

                TemplatePath = config.AppSettings.Settings["TemplatePath"] == null ||
                               string.IsNullOrWhiteSpace(config.AppSettings.Settings["TemplatePath"].Value)
                    ? string.Empty : config.AppSettings.Settings["TemplatePath"].Value.Replace("\\", "/");

                BundleConfigPath = config.AppSettings.Settings["BundleConfigPath"] == null ||
                               string.IsNullOrWhiteSpace(config.AppSettings.Settings["BundleConfigPath"].Value)
                    ? "bundleconfig.json"
                    : config.AppSettings.Settings["BundleConfigPath"].Value.Replace("\\", "/");

                LibraryName = config.AppSettings.Settings["LibraryName"] == null ||
                              string.IsNullOrWhiteSpace(config.AppSettings.Settings["LibraryName"].Value)
                    ? string.Empty : config.AppSettings.Settings["LibraryName"].Value;

                Debug = config.AppSettings.Settings["Debug"] != null &&
                             !string.IsNullOrWhiteSpace(config.AppSettings.Settings["Debug"].Value) ?
                             bool.Parse(config.AppSettings.Settings["Debug"].Value)
                             : false;

                OnlineClient = config.AppSettings.Settings["OnlineClient"] != null &&
                             !string.IsNullOrWhiteSpace(config.AppSettings.Settings["OnlineClient"].Value) ?
                             bool.Parse(config.AppSettings.Settings["OnlineClient"].Value)
                             : false;

                ForceHttps = config.AppSettings.Settings["ForceHttps"] != null &&
                             !string.IsNullOrWhiteSpace(config.AppSettings.Settings["ForceHttps"].Value) &&
                             bool.Parse(config.AppSettings.Settings["ForceHttps"].Value);

                IgnoreHandler = config.AppSettings.Settings["IgnoreHandler"] != null &&
                                !string.IsNullOrWhiteSpace(config.AppSettings.Settings["IgnoreHandler"].Value) &&
                                bool.Parse(config.AppSettings.Settings["IgnoreHandler"].Value);

                WhiteSpaceClear = config.AppSettings.Settings["WhiteSpaceClear"] != null &&
                                  !string.IsNullOrWhiteSpace(config.AppSettings.Settings["WhiteSpaceClear"].Value) &&
                                  bool.Parse(config.AppSettings.Settings["WhiteSpaceClear"].Value);

                GzipLength = config.AppSettings.Settings["GzipLength"] == null ||
                             string.IsNullOrWhiteSpace(config.AppSettings.Settings["GzipLength"].Value)
                    ? 0
                    : int.Parse(config.AppSettings.Settings["GzipLength"].Value) * 1024;

                UrlRewriteRulesPath = config.AppSettings.Settings["UrlRewriteRulesPath"] == null ||
                                      string.IsNullOrWhiteSpace(
                                          config.AppSettings.Settings["UrlRewriteRulesPath"].Value)
                    ? string.Empty
                    : RootPath +
                      config.AppSettings.Settings["UrlRewriteRulesPath"].Value.TrimStart('/')
                          .TrimStart('\\')
                          .Replace("\\", "/");

                UrlRewritePreRuleLibraryName = config.AppSettings.Settings["UrlRewritePreRuleLibraryName"] == null ||
                                               string.IsNullOrWhiteSpace(
                                                   config.AppSettings.Settings["UrlRewritePreRuleLibraryName"].Value)
                    ? string.Empty : config.AppSettings.Settings["UrlRewritePreRuleLibraryName"].Value;

                UrlRewriteRulesSuffixFilters = (config.AppSettings.Settings["UrlRewriteRulesSuffixFilters"] == null ||
                                                string.IsNullOrWhiteSpace(
                                                    config.AppSettings.Settings["UrlRewriteRulesSuffixFilters"].Value)
                    ? string.Empty : config.AppSettings.Settings["UrlRewriteRulesSuffixFilters"].Value).Split(new[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLowerInvariant()).ToArray();

                SeoRulesPath = config.AppSettings.Settings["SeoRulesPath"] == null ||
                               string.IsNullOrWhiteSpace(
                                   config.AppSettings.Settings["SeoRulesPath"].Value)
                    ? string.Empty
                    : RootPath +
                      config.AppSettings.Settings["SeoRulesPath"].Value.TrimStart('/')
                          .TrimStart('\\')
                          .Replace("\\", "/");

                WordFilterRulesPath = config.AppSettings.Settings["WordFilterRulesPath"] == null ||
                                      string.IsNullOrWhiteSpace(
                                          config.AppSettings.Settings["WordFilterRulesPath"].Value)
                    ? string.Empty
                    : RootPath +
                      config.AppSettings.Settings["WordFilterRulesPath"].Value.TrimStart('/')
                          .TrimStart('\\')
                          .Replace("\\", "/");

                ViewTemplateLibraryName = config.AppSettings.Settings["ViewTemplateLibraryName"] == null ||
                                          string.IsNullOrWhiteSpace(
                                              config.AppSettings.Settings["ViewTemplateLibraryName"].Value)
                    ? string.Empty : config.AppSettings.Settings["ViewTemplateLibraryName"].Value;

                Key = config.AppSettings.Settings["Key"] == null ||
                      string.IsNullOrWhiteSpace(config.AppSettings.Settings["Key"].Value)
                    ? string.Empty : config.AppSettings.Settings["Key"].Value;

                MainDomain = config.AppSettings.Settings["MainDomain"] == null ||
                             string.IsNullOrWhiteSpace(config.AppSettings.Settings["MainDomain"].Value)
                    ? string.Empty : config.AppSettings.Settings["MainDomain"].Value;

                Domains =
                    (config.AppSettings.Settings["Domains"] == null ||
                     string.IsNullOrWhiteSpace(config.AppSettings.Settings["Domains"].Value)
                        ? new HashSet<string>()
                        : new HashSet<string>((config.AppSettings.Settings["Domains"].Value).Split(new[] { '|' },
                            StringSplitOptions.RemoveEmptyEntries).Select(p => p.ToLowerInvariant())));

                //加载自定义Handlers
                XmlDocument xd = new XmlDocument();
                xd.Load(AppDomain.CurrentDomain.BaseDirectory + "web.config");
                if (xd != null)
                {
                    var r = XmlDocHelper.Find(xd, "configuration", "system.webServer", "handlers");
                    if (r != null)
                    {
                        CustomHandlers = r.ChildNodes.Cast<XmlNode>().Select(p => p.Attributes["path"].InnerXml).ToArray();
                    }
                }

                if (string.IsNullOrWhiteSpace(TemplatePath) || string.IsNullOrWhiteSpace(LibraryName))
                {
                    throw new Exception("必须设置模版路径和后台程序类库名称！");
                }

                FileInfo bundleconfigfile = new FileInfo(RootPath + BundleConfigPath);
                if (bundleconfigfile.Exists)
                {
                    var bundleconfigstr = File.ReadAllText(bundleconfigfile.FullName);
                    var bundleconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(bundleconfigstr);
                    foreach (dynamic bundlesubconfig in bundleconfig)
                    {
                        System.Web.Optimization.Bundle bundle = null;
                        var outputfilename = ((string)bundlesubconfig.outputFileName);
                        if (outputfilename.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                        {
                            bundle = new System.Web.Optimization.StyleBundle(outputfilename);
                        }
                        else if (outputfilename.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                        {
                            bundle = new System.Web.Optimization.ScriptBundle(outputfilename);
                        }
                        if (bundle != null)
                        {
                            List<string> inputfiles = new List<string>();
                            foreach (string inputfile in bundlesubconfig.inputFiles)
                            {
                                if (inputfile.StartsWith("~/"))
                                {
                                    inputfiles.Add(inputfile);
                                }
                            }
                            bundle.Include(inputfiles.ToArray());
                            System.Web.Optimization.BundleTable.Bundles.Add(bundle);
                        }
                    }

                    System.Web.Optimization.BundleTable.EnableOptimizations = !Debug;
                }

                if (string.IsNullOrWhiteSpace(Key) || Domains.Count < 1)
                {
                    throw new Exception("必须设置网站域名和密钥！");
                }
            }
            else
            {
                throw new Exception("加载MVC配置文件失败！");
            }
        }

        #region MVC设置属性

        /// <summary>
        ///     绑定域名（必填）
        /// </summary>
        public static HashSet<string> Domains { get; internal set; }

        /// <summary>
        ///     主域名
        /// </summary>
        public static string MainDomain { get; internal set; }

        /// <summary>
        ///     后台类库名称(必填)
        /// </summary>
        internal static string LibraryName { get; }

        /// <summary>
        ///     密钥（必填）
        /// </summary>
        public static string Key { get; set; }

        /// <summary>
        ///     模版路径（必填）
        /// </summary>
        public static string TemplatePath { get; internal set; }

        /// <summary>
        ///     资源打包配置文件路径
        /// </summary>
        public static string BundleConfigPath { get; internal set; }

        /// <summary>
        ///     程序根路径
        /// </summary>
        public static string RootPath { get; internal set; }

        /// <summary>
        ///     程序根路径
        /// </summary>
        public static bool ForceHttps { get; internal set; }

        /// <summary>
        ///     是否开启调试（页面执行效率跟踪 显示非合并打包的css和js文件）
        /// </summary>
        public static bool Debug { get; internal set; }

        /// <summary>
        ///     是否开启在线客户统计
        /// </summary>
        public static bool OnlineClient { get; internal set; }

        /// <summary>
        ///     是否忽略后台代码
        /// </summary>
        public static bool IgnoreHandler { get; internal set; }

        /// <summary>
        ///     是否开启清理空白字符
        /// </summary>
        internal static bool WhiteSpaceClear { get; set; }

        internal static readonly Regex WhiteSpaceHtmlRegex = new Regex(@">\s+<", RegexOptions.Compiled);
        internal static readonly Regex WhiteSpaceAllRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        ///     GZIP压缩最低大小，单位kb（推荐数值200）
        /// </summary>
        internal static int GzipLength { get; set; }

        /// <summary>
        ///     URL重写规则文件路径
        /// </summary>
        internal static string UrlRewriteRulesPath { get; set; }

        /// <summary>
        ///     URL重写预生成规则类库名称
        /// </summary>
        internal static string UrlRewritePreRuleLibraryName { get; private set; }

        /// <summary>
        ///     URL重写规则后缀名过滤
        /// </summary>
        internal static string[] UrlRewriteRulesSuffixFilters { get; set; }

        /// <summary>
        ///     SEO规则文件路径
        /// </summary>
        internal static string SeoRulesPath { get; set; }

        /// <summary>
        ///     关键词过滤规则文件路径
        /// </summary>
        internal static string WordFilterRulesPath { get; set; }

        /// <summary>
        ///     模版引擎类库名称
        /// </summary>
        internal static string ViewTemplateLibraryName { get; private set; }

        /// <summary>
        ///     模版引擎类库名称
        /// </summary>
        internal static string[] CustomHandlers { get; private set; }

        #endregion MVC设置属性

        #region SecretKey

        /// <summary>
        ///     系统计算出来的密钥
        /// </summary>
        internal static SecureString SecretKey
        {
            get
            {
                SecureString secretKey;
                if (!MemoryCacher.TryGet<SecureString>("CY_MVC.SecretKey", out secretKey))
                {
                    secretKey = GenerateKey();
                    MemoryCacher.Set("CY_MVC.SecretKey", secretKey, CacheItemPriority.NotRemovable, DateTime.Now.AddHours(2));
                }
                return secretKey;
            }
        }

        private static SecureString GenerateKey()
        {
            var key = new SecureString();
            var DomainsCount = Domains.Count > 20 ? 20 : 40 - Domains.Count;
            using (HashAlgorithm ha = HASHCrypto.Generate(HASHCrypto.DEncryptEnum.SHA1))
            {
                var tempkey = ha.Encrypt(Domains.OrderBy(p => p.ToLowerInvariant())
                                .Aggregate(string.Empty, (current, item) => current + item));
#if NoKey
                tempkey = ha.Encrypt(LibraryName + tempkey.ToUpperInvariant());
#else
                var LibraryFile = new FileInfo(RootPath + "Bin/" + LibraryName + ".dll");
                if (LibraryFile.Exists)
                {
                    tempkey = ha.Encrypt(ha.Encrypt(File.ReadAllBytes(LibraryFile.FullName)) + tempkey.ToUpperInvariant());
                }
                else
                {
                    tempkey = ha.Encrypt(LibraryFile.FullName.ToLowerInvariant() + DomainsCount);
                }
#endif
                tempkey = ha.Encrypt(tempkey.Substring(DomainsCount));
                tempkey = ha.Encrypt("CY_MVC" + tempkey.Remove(DomainsCount));

                foreach (var item in tempkey)
                {
                    key.AppendChar(item);
                }
            }
            return key;
        }

        #endregion SecretKey
    }
}