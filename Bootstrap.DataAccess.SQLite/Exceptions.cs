﻿using Longbow;
using Longbow.Cache;
using Longbow.Configuration;
using Longbow.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;

namespace Bootstrap.DataAccess.SQLite
{
    /// <summary>
    /// 
    /// </summary>
    public class Exceptions : DataAccess.Exceptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="additionalInfo"></param>
        /// <returns></returns>
        public override void Log(Exception ex, NameValueCollection additionalInfo)
        {
            if (additionalInfo == null)
            {
                additionalInfo = new NameValueCollection
                {
                    ["UserId"] = null,
                    ["UserIp"] = null,
                    ["ErrorPage"] = null
                };
            }
            var errorPage = additionalInfo["ErrorPage"] ?? (nameof(ex).Length > 50 ? nameof(ex).Substring(0, 50) : nameof(ex));
            var sql = "insert into Exceptions (ID, AppDomainName, ErrorPage, UserID, UserIp, ExceptionType, Message, StackTrace, LogTime) values (NULL, @AppDomainName, @ErrorPage, @UserID, @UserIp, @ExceptionType, @Message, @StackTrace, datetime('now', 'localtime'))";
            using (DbCommand cmd = DBAccessManager.DBAccess.CreateCommand(CommandType.Text, sql))
            {
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@AppDomainName", AppDomain.CurrentDomain.FriendlyName));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@ErrorPage", errorPage));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@UserID", DbAccessFactory.ToDBValue(additionalInfo["UserId"])));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@UserIp", DbAccessFactory.ToDBValue(additionalInfo["UserIp"])));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@ExceptionType", ex.GetType().FullName));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@Message", ex.Message));
                cmd.Parameters.Add(DBAccessManager.DBAccess.CreateParameter("@StackTrace", DbAccessFactory.ToDBValue(ex.StackTrace)));
                DBAccessManager.DBAccess.ExecuteNonQuery(cmd);
                CacheManager.Clear(RetrieveExceptionsDataKey);
                ClearExceptions();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static void ClearExceptions()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                string sql = $"delete from Exceptions where LogTime < datetime('now', 'localtime', '-{ConfigurationManager.AppSettings["KeepExceptionsPeriod"]} month')";
                DbCommand cmd = DBAccessManager.DBAccess.CreateCommand(CommandType.Text, sql);
                DBAccessManager.DBAccess.ExecuteNonQuery(cmd);
            });
        }
        /// <summary>
        /// 查询一周内所有异常
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<DataAccess.Exceptions> RetrieveExceptions()
        {
            return CacheManager.GetOrAdd(RetrieveExceptionsDataKey, key =>
            {
                string sql = "select * from Exceptions where LogTime > datetime('now', 'localtime', '-7 day') order by LogTime desc";
                List<Exceptions> exceptions = new List<Exceptions>();
                DbCommand cmd = DBAccessManager.DBAccess.CreateCommand(CommandType.Text, sql);
                using (DbDataReader reader = DBAccessManager.DBAccess.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        exceptions.Add(new Exceptions()
                        {
                            Id = LgbConvert.ReadValue(reader[0], 0),
                            AppDomainName = (string)reader[1],
                            ErrorPage = reader.IsDBNull(2) ? string.Empty : (string)reader[2],
                            UserId = reader.IsDBNull(3) ? string.Empty : (string)reader[3],
                            UserIp = reader.IsDBNull(4) ? string.Empty : (string)reader[4],
                            ExceptionType = (string)reader[5],
                            Message = (string)reader[6],
                            StackTrace = (string)reader[7],
                            LogTime = LgbConvert.ReadValue(reader[8], DateTime.MinValue)
                        });
                    }
                }
                return exceptions;
            });
        }
    }
}
