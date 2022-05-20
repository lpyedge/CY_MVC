using System;
using System.Collections.Generic;

namespace CY_MVC.ViewTemplate
{
    /// <summary>
    ///     模版类基类
    /// </summary>
    public abstract class BaseViewTemplate : IDisposable
    {
        /// <summary>
        ///     模版文件
        /// </summary>
        public string TemplateFile;

        /// <summary>
        ///     模版后缀名
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        ///     模版内容
        /// </summary>
        public virtual string TemplateText { get; set; }

        /// <summary>
        ///     设置模版文件
        /// </summary>
        /// <param name="p_TemplateFile"></param>
        public virtual void SetTemplateFile(string p_TemplateFile)
        {
            TemplateFile = p_TemplateFile;
        }

        /// <summary>
        ///     模版数据合并获得结果
        /// </summary>
        /// <param name="p_ViewData"></param>
        /// <returns></returns>
        public abstract string BuildString(IDictionary<string, dynamic> p_ViewData);

        #region IDisposable 成员

        /// <summary>
        ///     m_Disposed
        /// </summary>
        protected bool m_Disposed; // 保证多次调用Dispose方式不会抛出异常

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            //必须以Dispose(true)方式调用,以true告诉Dispose(bool disposing)函数是被客户直接调用的
            Dispose(true); // MUST be true
            // This object will be cleaned up by the Dispose method. Therefore, you should call GC.SupressFinalize to take this object off the finalization queue and prevent finalization code for this object from executing a second time.
            GC.SuppressFinalize(this); // 告诉垃圾回收器从Finalization队列中清除自己,从而阻止垃圾回收器调用Finalize方法.
        }

        // 无法被客户直接调用
        // 如果 disposing 是 true, 那么这个方法是被客户直接调用的,那么托管的,和非托管的资源都可以释放
        // 如果 disposing 是 false, 那么函数是从垃圾回收器在调用Finalize时调用的,此时不应当引用其他托管对象所以,只能释放非托管资源

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="p_Disposing"></param>
        protected virtual void Dispose(bool p_Disposing)
        {
            // If you need thread safety, use a lock around these operations, as well as in your methods that use the resource.
            // Check to see if Dispose has already been called
            if (m_Disposed)
                return;
            if (p_Disposing) // 这个方法是被客户直接调用的,那么托管的,和非托管的资源都可以释放
            {
                // 在这里释放托管资源
            }
            // 在这里释放非托管资源
            m_Disposed = true; // Indicate that the instance has been disposed
        }

        // 析构函数自动生成 Finalize 方法和对基类的 Finalize 方法的调用.默认情况下,一个类是没有析构函数的,也就是说,对象被垃圾回收时不会被调用Finalize方法

        /// <summary>
        ///     Dispose
        /// </summary>
        ~BaseViewTemplate()
        {
            // 为了保持代码的可读性性和可维护性,千万不要在这里写释放非托管资源的代码
            // 必须以Dispose(false)方式调用,以false告诉Dispose(bool disposing)函数是从垃圾回收器在调用Finalize时调用的
            Dispose(false); // MUST be false
        }

        #endregion IDisposable 成员
    }
}