using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Cart
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    //public const decimal Tax = 1.25m;
  
    //public decimal TotalPrice => Items?.Sum(item => item.Quantity * item.Product.Price * Tax ) ?? 0;
}
