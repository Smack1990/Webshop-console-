using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class OrderItem
{
    public int Id { get; set; } // Primary Key


    public int OrderId { get; set; } // Foreign Key till Order
    public virtual Order Order { get; set; } // Navigation property till Order

    public int ProductId { get; set; } // Foreign Key till Product
    public virtual Product Product { get; set; } // Navigation property till Product

    public int Quantity { get; set; } // Antal av produkten i beställningen

    //[Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // Enhetspris för produkten vid beställningstillfället

}
