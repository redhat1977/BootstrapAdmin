﻿using System.ComponentModel.DataAnnotations;

namespace BootstrapAdmin.DataAccess.Models
{
    /// <summary>
    /// Group 实体类
    /// </summary>
    public class Group
    {
        /// <summary>
        /// 获得/设置 主键 ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 获得/设置 群组名称
        /// </summary>
        [Display(Name = "群组名称")]
        [NotNull]
        public string? GroupName { get; set; }

        /// <summary>
        /// 获得/设置 群组编码
        /// </summary>
        [Display(Name = "群组编码")]
        [NotNull]
        public string? GroupCode { get; set; }

        /// <summary>
        /// 获得/设置 群组描述
        /// </summary>
        [Display(Name = "群组描述")]
        public string? Description { get; set; }

        public override string ToString() => $"{GroupName}({GroupCode})";
    }
}
