using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");
            builder.HasKey(p => p.Id);

            builder.HasIndex(v => v.Sku).IsUnique();
            builder.Property(v => v.Sku).IsRequired().HasMaxLength(50);

            builder.Property(p => p.Size).IsRequired().HasMaxLength(20);
            builder.Property(p => p.Color).IsRequired().HasMaxLength(20);

            builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

            builder.Property(p => p.Stock)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint("CK_ProductVariant_Stock", "[Stock] >= 0"));

            builder.ToTable(t => t.HasCheckConstraint("CK_ProductVariant_Price", "[Price] > 0"));

            builder.HasOne(v => v.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
