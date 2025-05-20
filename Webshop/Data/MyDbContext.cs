using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Data;
public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) :base(options)
    {
        
    }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
           
        optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;
  Initial Catalog=WebShop1;
  Integrated Security=True;
  MultipleActiveResultSets=True;
  Connect Timeout=30;
  Encrypt=False;
  Trust Server Certificate=False;
  Application Intent=ReadWrite;
  Multi Subnet Failover=False");
    }
        }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderItem>()
           .Property(p => p.UnitPrice)
           .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>()
           .Property(p => p.TotalAmount)
           .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Cart)
            .WithOne(c => c.Customer)
            .HasForeignKey<Cart>(c => c.CustomerId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
