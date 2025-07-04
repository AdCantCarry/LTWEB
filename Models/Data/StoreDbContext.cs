using Microsoft.EntityFrameworkCore;
using TechNova.Models.Auth;
using TechNova.Models.Core;

namespace TechNova.Models.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SpecificationGroup> SpecificationGroups { get; set; }
        public DbSet<SpecificationItem> SpecificationItems { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<News> News { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⚠ Tránh cascade path gây vòng lặp
            modelBuilder.Entity<SpecificationGroup>()
        .HasOne(g => g.Category)
        .WithMany(c => c.SpecificationGroups)
        .HasForeignKey(g => g.CategoryId);

            modelBuilder.Entity<SpecificationItem>()
                .HasOne(i => i.Group)
                .WithMany(g => g.Items)
                .HasForeignKey(i => i.GroupId);

            modelBuilder.Entity<ProductSpecification>()
                .HasOne(p => p.Product)
                .WithMany(p => p.Specifications)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Tắt cascade ở đây

            modelBuilder.Entity<ProductSpecification>()
                .HasOne(p => p.SpecificationItem)
                .WithMany(i => i.ProductSpecifications)
                .HasForeignKey(p => p.SpecificationItemId);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // tránh cascade

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // cái này được phép cascade

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // 1-1 được phép cascade
        }
    }
}
