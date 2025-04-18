﻿using Diamond.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diamond.Database.Configurations
{
    public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
    {
        public void Configure(EntityTypeBuilder<Factory> builder)
        {
            builder.HasKey(f => f.Id);
            builder.HasIndex(f => f.Name).IsUnique();

            #region Связи
            // С маршрутами
            builder.HasMany(f => f.Routes).WithOne(r => r.Factory)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С участками
            builder.HasMany(f => f.Regions).WithOne(r => r.Factory)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С заказами
            builder.HasMany(f => f.Orders).WithOne(r => r.Factory)
                .HasForeignKey(r => r.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // С планом
            builder.HasMany(f => f.Plan).WithOne(p => p.Factory)
                .HasPrincipalKey(f => f.Id)
                .HasForeignKey(p => p.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            // Склад
            builder.HasOne(f => f.Warehouse).WithOne(w => w.Factory)
                .HasPrincipalKey<Factory>(f => f.Id)
                .HasForeignKey<Warehouse>(w => w.FactoryId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}