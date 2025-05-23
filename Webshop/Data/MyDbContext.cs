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
    protected override void OnModelCreating(ModelBuilder modelBuilder) //onmodelcreating för att sätta upp relationer och konfigurationer mer specifikt. Exempelvis isUnique.
    {
        //sätter kolumntyp och antal decimaler för pris. Jag vill bara ha ut 2 decimaler och max 18siffror på priset. 
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderItem>()
           .Property(p => p.UnitPrice)
           .HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>()
           .Property(p => p.TotalAmount)
           .HasColumnType("decimal(18,2)");
        // En kund kan ha en kundvagn och en kundvagn kan ha många varor.
        //En vara kan tillhöra en kategori och en kategori kan ha många varor.
        //En vara kan tillhöra en leverantör och en leverantör kan ha många varor.
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Cart)
            .WithOne(c => c.Customer)
            .HasForeignKey<Cart>(c => c.CustomerId);

        // och en kategori kan ha många varor.
        //deletebehavior restrict, så att vi inte kan ta bort en kategori som har varor kopplade till sig.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        //En vara kan tillhöra en leverantör och en leverantör kan ha många varor.
        //deletebehavior restrict, så att vi inte kan ta bort en leverantör som har varor kopplade till sig.
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        //Email är unique
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();
        //SKu är unique
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();
        //
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);
        //CartItem är en many to many relation mellan Cart och Product
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        //orderitem är en many to many relation mellan order och product
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
