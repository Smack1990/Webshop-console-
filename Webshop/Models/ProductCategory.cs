using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class ProductCategory
{
    public int Id { get; set; } // Primary Key
    public string? CategoryName { get; set; } // Namn på produktkategori
    public string? Description { get; set; } // Beskrivning av produktkategori
    public virtual ICollection<Product> Products { get; set; }  = new List<Product>(); //  Lista med produkter i kategorin
}
