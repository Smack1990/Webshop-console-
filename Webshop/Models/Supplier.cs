using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Supplier
{
    public int Id { get; set; } // Primary Key
    public string? CompanyName { get; set; } // Namn på leverantör
    public string? Email { get; set; } // Email till leverantör
    public string? PhoneNumber { get; set; } //e-post till leverantör
    public string? Address { get; set; } // Leverantörens adress

    public virtual ICollection<Product> Products { get; set; } = new List<Product>(); // Lista med produkter från leverantören
}
