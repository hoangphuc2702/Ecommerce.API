using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Configurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            builder.HasKey(x => x.OrderId);

            builder.Property(x => x.TrackingNumber)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.AhamoveOrderId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.PartnerCode)
                .HasMaxLength(50)
                .HasDefaultValue("AHAMOVE")
                .IsRequired();

            builder.Property(x => x.ServiceId)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(x => x.Fee)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.CodAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(s => s.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<Shipment>(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}