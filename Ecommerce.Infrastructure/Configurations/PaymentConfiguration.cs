using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayOS.Models.V2.PaymentRequests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentProvider)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ReferenceId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ProviderTransactionId)
                .HasMaxLength(100);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Message)
                .HasMaxLength(500);

            builder.HasOne(pt => pt.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
