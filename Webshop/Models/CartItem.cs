using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.Now;
    public int CartId { get; set; }
    public virtual Cart Cart { get; set; }
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
}
