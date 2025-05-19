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
    public int Id { get; set; }


    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string SKU { get; set; }
    
    public DateTime CreatedDate { get; set; } 

    public bool IsActive { get; set; } = true;
    public int QuantitySold { get; set; }

    public int ProductCategoryId { get; set; } 
    public ProductCategory Category { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }
   
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> Orders { get; set; } = new List<OrderItem>();
}
