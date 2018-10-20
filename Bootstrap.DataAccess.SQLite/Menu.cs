﻿using Bootstrap.Security;
using Longbow;
using Longbow.Cache;
using Longbow.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Bootstrap.DataAccess.SQLite
{
    /// <summary>
    /// 
    /// </summary>
    public class Menu : DataAccess.Menu
    {
        /// <summary>
        /// 删除菜单信息
        /// </summary>
        /// <param name="value"></param>
        public override bool DeleteMenu(IEnumerable<int> value)
        {
            bool ret = false;
            var ids = string.Join(",", value);
            using (DbCommand cmd = DbAccessManager.DBAccess.CreateCommand(CommandType.StoredProcedure, "Proc_DeleteMenus"))
            {
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@ids", ids));
                ret = DbAccessManager.DBAccess.ExecuteNonQuery(cmd) == -1;
            }
            CacheCleanUtility.ClearCache(menuIds: value);
            return ret;
        }
        /// <summary>
        /// 保存新建/更新的菜单信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool SaveMenu(BootstrapMenu p)
        {
            if (string.IsNullOrEmpty(p.Name)) return false;
            bool ret = false;
            if (p.Name.Length > 50) p.Name = p.Name.Substring(0, 50);
            if (p.Icon != null && p.Icon.Length > 50) p.Icon = p.Icon.Substring(0, 50);
            if (p.Url != null && p.Url.Length > 4000) p.Url = p.Url.Substring(0, 4000);
            string sql = p.Id == 0 ?
                "Insert Into Navigations (ParentId, Name, [Order], Icon, Url, Category, Target, IsResource, [Application]) Values (@ParentId, @Name, @Order, @Icon, @Url, @Category, @Target, @IsResource, @ApplicationCode)" :
                "Update Navigations set ParentId = @ParentId, Name = @Name, [Order] = @Order, Icon = @Icon, Url = @Url, Category = @Category, Target = @Target, IsResource = @IsResource, Application = @ApplicationCode where ID = @ID";
            using (DbCommand cmd = DbAccessManager.DBAccess.CreateCommand(CommandType.Text, sql))
            {
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@ID", p.Id));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@ParentId", p.ParentId));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Name", p.Name));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Order", p.Order));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Icon", DbAccessFactory.ToDBValue(p.Icon)));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Url", DbAccessFactory.ToDBValue(p.Url)));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Category", p.Category));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@Target", p.Target));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@IsResource", p.IsResource));
                cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@ApplicationCode", p.ApplicationCode));
                ret = DbAccessManager.DBAccess.ExecuteNonQuery(cmd) == 1;
            }
            CacheCleanUtility.ClearCache(menuIds: p.Id == 0 ? new List<int>() : new List<int>() { p.Id });
            return ret;
        }

        /// <summary>
        /// 查询某个角色所配置的菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public override IEnumerable<BootstrapMenu> RetrieveMenusByRoleId(int roleId)
        {
            string key = string.Format("{0}-{1}", RetrieveMenusByRoleIdDataKey, roleId);
            return CacheManager.GetOrAdd(key, k =>
            {
                var menus = new List<BootstrapMenu>();
                string sql = "select NavigationID from NavigationRole where RoleID = @RoleID";
                using (DbCommand cmd = DbAccessManager.DBAccess.CreateCommand(CommandType.Text, sql))
                {
                    cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@RoleID", roleId));
                    using (DbDataReader reader = DbAccessManager.DBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            menus.Add(new BootstrapMenu()
                            {
                                Id = LgbConvert.ReadValue(reader[0], 0)
                            });
                        }
                    }
                }
                return menus;
            }, RetrieveMenusByRoleIdDataKey);
        }
        /// <summary>
        /// 通过角色ID保存当前授权菜单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="menuIds"></param>
        /// <returns></returns>
        public override bool SaveMenusByRoleId(int id, IEnumerable<int> menuIds)
        {
            bool ret = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("RoleID", typeof(int));
            dt.Columns.Add("NavigationID", typeof(int));
            menuIds.ToList().ForEach(menuId => dt.Rows.Add(id, menuId));
            using (TransactionPackage transaction = DbAccessManager.DBAccess.BeginTransaction())
            {
                try
                {
                    //删除菜单角色表该角色所有的菜单
                    string sql = "delete from NavigationRole where RoleID=@RoleID";
                    using (DbCommand cmd = DbAccessManager.DBAccess.CreateCommand(CommandType.Text, sql))
                    {
                        cmd.Parameters.Add(DbAccessManager.DBAccess.CreateParameter("@RoleID", id));
                        DbAccessManager.DBAccess.ExecuteNonQuery(cmd, transaction);
                        //批插入菜单角色表
                        using (SqlBulkCopy bulk = new SqlBulkCopy((SqlConnection)transaction.Transaction.Connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction.Transaction))
                        {
                            bulk.DestinationTableName = "NavigationRole";
                            bulk.ColumnMappings.Add("RoleID", "RoleID");
                            bulk.ColumnMappings.Add("NavigationID", "NavigationID");
                            bulk.WriteToServer(dt);
                            transaction.CommitTransaction();
                        }
                    }
                    CacheCleanUtility.ClearCache(menuIds: menuIds, roleIds: new List<int>() { id });
                    ret = true;
                }
                catch (Exception ex)
                {
                    transaction.RollbackTransaction();
                    throw ex;
                }
            }
            return ret;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="activeUrl"></param>
        ///// <returns></returns>
        //public override IEnumerable<BootstrapMenu> RetrieveAllMenus(string userName, string activeUrl = null) => RetrieveAllMenus(DBAccessManager.DBAccess, userName, activeUrl);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="activeUrl"></param>
        ///// <returns></returns>
        //public override IEnumerable<BootstrapMenu> RetrieveAppMenus(string userName, string activeUrl = null) => RetrieveAppMenus(DBAccessManager.DBAccess, userName, activeUrl);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="activeUrl"></param>
        ///// <returns></returns>
        //public override IEnumerable<BootstrapMenu> RetrieveMenusByUserName(string userName, string activeUrl = null) => RetrieveMenusByUserName(DBAccessManager.DBAccess, userName, activeUrl);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="activeUrl"></param>
        ///// <returns></returns>
        //public override IEnumerable<BootstrapMenu> RetrieveSystemMenus(string userName, string activeUrl = null) => RetrieveSystemMenus(DBAccessManager.DBAccess, userName, activeUrl);
    }
}