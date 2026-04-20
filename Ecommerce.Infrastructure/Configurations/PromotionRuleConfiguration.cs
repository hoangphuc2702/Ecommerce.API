using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Configurations
{
    public class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
    {
        public void Configure(EntityTypeBuilder<PromotionRule> builder)
        {
            builder.ToTable("PromotionRules");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(p => p.Priority).HasDefaultValue(0);

            builder.Property(p => p.MinQuantity).HasDefaultValue(1);

            builder.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");

            builder.Property(c => c.StartDate).IsRequired();

            builder.Property(c => c.EndDate).IsRequired();

            builder.Property(c => c.IsActive).HasDefaultValue(true);

            builder.HasIndex(p => new { p.IsActive, p.StartDate, p.EndDate });

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.TargetCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(p => p.BuyProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(p => p.GiftProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
