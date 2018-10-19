﻿using Bootstrap.Security;
using Longbow.Data;
using System.Collections.Generic;

namespace Bootstrap.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class DictHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<BootstrapDict> RetrieveDicts() => DbAdapterManager.Create<Dict>().RetrieveDicts();
        /// <summary>
        /// 删除字典中的数据
        /// </summary>
        /// <param name="value">需要删除的IDs</param>
        /// <returns></returns>
        public static bool DeleteDict(IEnumerable<int> value) => DbAdapterManager.Create<Dict>().DeleteDict(value);
        /// <summary>
        /// 保存新建/更新的字典信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool SaveDict(BootstrapDict p) => DbAdapterManager.Create<Dict>().SaveDict(p);
        /// <summary>
        /// 保存网站个性化设置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static bool SaveSettings(BootstrapDict dict) => DbAdapterManager.Create<Dict>().SaveSettings(dict);
        /// <summary>
        /// 获取字典分类名称
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> RetrieveCategories() => DbAdapterManager.Create<Dict>().RetrieveCategories();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string RetrieveWebTitle() => DbAdapterManager.Create<Dict>().RetrieveWebTitle();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string RetrieveWebFooter() => DbAdapterManager.Create<Dict>().RetrieveWebFooter();
        /// <summary>
        /// 获得系统中配置的可以使用的网站样式
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<BootstrapDict> RetrieveThemes() => DbAdapterManager.Create<Dict>().RetrieveThemes();
        /// <summary>
        /// 获得网站设置中的当前样式
        /// </summary>
        /// <returns></returns>
        public static string RetrieveActiveTheme() => DbAdapterManager.Create<Dict>().RetrieveActiveTheme();
        /// <summary>
        /// 获取头像路径
        /// </summary>
        /// <returns></returns>
        public static BootstrapDict RetrieveIconFolderPath() => DbAdapterManager.Create<Dict>().RetrieveIconFolderPath();
        /// <summary>
        /// 获得默认的前台首页地址，默认为~/Home/Index
        /// </summary>
        /// <returns></returns>
        public static string RetrieveHomeUrl() => DbAdapterManager.Create<Dict>().RetrieveHomeUrl();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> RetrieveApps() => DbAdapterManager.Create<Dict>().RetrieveApps();
    }
}
