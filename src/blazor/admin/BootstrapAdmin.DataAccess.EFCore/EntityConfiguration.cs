﻿using BootstrapAdmin.DataAccess.EFCore.Models;
using BootstrapAdmin.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace BootstrapAdmin.DataAccess.EFCore;

/// <summary>
/// 
/// </summary>
public static class EntityConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    public static void Configure(this ModelBuilder builder)
    {
        var converter = new ValueConverter<string?, int>(
            v => Convert.ToInt32(v),
            v => v.ToString(),
              new ConverterMappingHints(valueGeneratorFactory: (p, t) => new GuidStringGenerator()));

        builder.Entity<User>().ToTable("Users");
        builder.Entity<User>().Ignore(u => u.Period);
        builder.Entity<User>().Ignore(u => u.NewPassword);
        builder.Entity<User>().Ignore(u => u.ConfirmPassword);
        builder.Entity<User>().Ignore(u => u.IsReset);
        builder.Entity<User>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();

        builder.Entity<UserRole>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();

        builder.Entity<Role>().ToTable("Roles");
        builder.Entity<Role>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();

        builder.Entity<Navigation>().ToTable("Navigations");
        builder.Entity<Navigation>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();
        builder.Entity<Navigation>().Ignore(s => s.HasChildren);

        builder.Entity<Dict>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();

        builder.Entity<Group>().Property(s => s.Id).HasConversion(converter).ValueGeneratedOnAdd();
    }
}

internal class GuidStringGenerator : ValueGenerator
{

    public override bool GeneratesTemporaryValues => false;

    protected override object? NextValue(EntityEntry entry) => "0";

}
