﻿using BootstrapAdmin.Web.Core;
using BootstrapAdmin.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using BootstrapAdmin.Caching;
using BootstrapAdmin.DataAccess.EFCore.Models;

namespace BootstrapAdmin.DataAccess.EFCore.Services
{
    /// <summary>
    /// 
    /// </summary>
    class NavigationService : INavigation
    {
        private IDbContextFactory<BootstrapAdminContext> DbFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public NavigationService(IDbContextFactory<BootstrapAdminContext> factory) => DbFactory = factory;

        /// <summary>
        /// 获得指定用户名可访问的所有菜单集合
        /// </summary>
        /// <param name="userName">当前用户名</param>
        /// <returns>未层次化的菜单集合</returns>
        public List<Navigation> GetAllMenus(string userName)
        {
            return CacheManager.GetOrAdd($"{nameof(NavigationService)}-{nameof(GetAllMenus)}-{userName}", entry =>
            {
                using var context = DbFactory.CreateDbContext();
                return context.Set<Navigation>().FromSqlRaw("select n.ID, n.ParentId, n.Name, n.[order], n.Icon, n.Url, n.Category, n.Target, n.IsResource, n.Application from Navigations n inner join (select nr.NavigationID from Users u inner join UserRole ur on ur.UserID = u.ID inner join NavigationRole nr on nr.RoleID = ur.RoleID where u.UserName = {0} union select nr.NavigationID from Users u inner join UserGroup ug on u.ID = ug.UserID inner join RoleGroup rg on rg.GroupID = ug.GroupID inner join NavigationRole nr on nr.RoleID = rg.RoleID where u.UserName = {0} union select n.ID from Navigations n where EXISTS (select UserName from Users u inner join UserRole ur on u.ID = ur.UserID inner join Roles r on ur.RoleID = r.ID where u.UserName = {0} and r.RoleName = {1})) nav on n.ID = nav.NavigationID ORDER BY n.Application, n.[order]", new[] { userName, "Administrators" }).ToList();
            });
        }

        public List<string> GetMenusByRoleId(string? roleId)
        {
            using var context = DbFactory.CreateDbContext();

            return context.NavigationRole.Where(s => s.RoleId == roleId).Select(s => s.NavigationId!).ToList();
        }

        public bool SaveMenusByRoleId(string? roleId, List<string> menuIds)
        {
            using var dbcontext = DbFactory.CreateDbContext();
            var ret = false;
            try
            {
                dbcontext.Database.ExecuteSqlRaw("delete from NavigationRole where RoleID = {0}", roleId!);
                dbcontext.Set<NavigationRole>().AddRange(menuIds.Select(g => new NavigationRole { NavigationId = g, RoleId = roleId }));
                dbcontext.SaveChanges();
                ret = true;
                CacheManager.Clear($"{nameof(NavigationService)}-{nameof(GetAllMenus)}-*");
            }
            catch (Exception)
            {

                throw;
            }

            return ret;
        }
    }
}
