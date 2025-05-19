using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Cart
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    public decimal Moms = 1.25m;
    public decimal TotalPrice => Items?.Sum(item => item.Quantity * item.Product.Price * Moms ) ?? 0;
}
