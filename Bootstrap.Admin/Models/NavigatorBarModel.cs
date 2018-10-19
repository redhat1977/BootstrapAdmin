﻿using Bootstrap.DataAccess;
using Bootstrap.Security;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Bootstrap.Admin.Models
{
    public class NavigatorBarModel : HeaderBarModel
    {
        public NavigatorBarModel(ControllerBase controller) : base(controller.User.Identity)
        {
            Navigations = MenuHelper.RetrieveSystemMenus(UserName, $"~{controller.HttpContext.Request.Path}");
            Applications = DictHelper.RetrieveApps();
        }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<BootstrapMenu> Navigations { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Applications { get; private set; }
    }
}