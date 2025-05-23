using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Cart
{
    public int Id { get; set; } // Primary Key

    public DateTime CreatedAt { get; set; } = DateTime.Now; //Datum för kundkorg
    public DateTime? UpdatedAt { get; set; } //Datum för senaste uppdatering av kundkorg
    public int CustomerId { get; set; } // Foreign Key till Customer
    public virtual Customer Customer { get; set; } // Navigation property till Customer

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>(); // Lista med CartItems

    //public const decimal Tax = 1.25m;

    //public decimal TotalPrice => Items?.Sum(item => item.Quantity * item.Product.Price * Tax ) ?? 0;
}
