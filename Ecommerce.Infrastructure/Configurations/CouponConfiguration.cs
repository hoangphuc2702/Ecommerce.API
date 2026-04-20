using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50); 

            builder.HasIndex(c => c.Code)
                .IsUnique();

            builder.Property(c => c.DiscountType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.Value)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(c => c.MinOrderValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(c => c.MaxDiscountAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(c => c.StartDate)
            .IsRequired();

            builder.Property(c => c.EndDate)
                .IsRequired();

            builder.Property(c => c.UsageLimit)
                .IsRequired();

            builder.Property(c => c.UsedCount)
                .HasDefaultValue(0);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(c => new { c.IsActive, c.StartDate, c.EndDate });
        }
    }
}
