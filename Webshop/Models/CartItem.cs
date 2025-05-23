using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class CartItem
{
    public int Id { get; set; } // Primary Key
    public int Quantity { get; set; } // Antal av produkten i kundkorgen
    public DateTime AddedAt { get; set; } = DateTime.Now; // Datum för när produkten lades till i kundkorgen
    public int CartId { get; set; } // Foreign Key till Cart
    public virtual Cart Cart { get; set; } // Navigation property till Cart
    public int ProductId { get; set; } // Foreign Key till Product

    public virtual Product Product { get; set; } // Navigation property till Product
}
