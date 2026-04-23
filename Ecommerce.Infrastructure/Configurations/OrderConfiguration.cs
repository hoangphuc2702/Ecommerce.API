using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.UserId)
                .IsRequired();

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.SubTotal)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.PromotionDiscount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.CouponDiscount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(o => o.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(Ecommerce.Domain.Enums.PaymentStatus.Pending);

            builder.Property(o => o.OrderDate)
                .IsRequired();

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            builder.Property(o => o.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("COD");

            builder.Property(o => o.Latitude)
                .IsRequired();

            builder.Property(o => o.Longitude)
                .IsRequired();


            builder.HasOne(o => o.Shipment)
                .WithOne(s => s.Order)
                .HasForeignKey<Shipment>(s => s.OrderId);
        }
    }
}