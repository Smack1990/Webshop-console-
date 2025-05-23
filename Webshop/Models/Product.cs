using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Product
{
    public int Id { get; set; } // Primary Key


    public string? Name { get; set; } // Namn på produkten
    public string? Description { get; set; } // Beskrivning av produkten


    public decimal Price { get; set; } // Enhetspris för produkten
    public int Stock { get; set; } // Antal i lager
    public string SKU { get; set; } // SKU (Stock Keeping Unit) för produkten

    public DateTime CreatedDate { get; set; } // Datum för när produkten skapades

    public bool IsActive { get; set; } = true; // Om produkten är aktiv eller inte, sätts i adminpanelen, visas i featured produkter
    public int QuantitySold { get; set; }// Antal sålda produkter

    public int ProductCategoryId { get; set; }  // Foreign Key till ProductCategory
    public virtual ProductCategory Category { get; set; } // Navigation property till ProductCategory
    public int SupplierId { get; set; } // Foreign Key till Supplier
    public virtual Supplier Supplier { get; set; } // Navigation property till Supplier

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>(); // Lista med CartItems
    public virtual ICollection<OrderItem> Orders { get; set; } = new List<OrderItem>(); // Lista med OrderItems
}
