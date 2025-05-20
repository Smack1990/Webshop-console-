using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Order
{
   public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    
    public string ShippingAddress { get; set; } = string.Empty;
    public int ZipCode { get; set; }
    public string City { get; set; }
    public string InvoiceAddress { get; set; } = string.Empty;
    public int InvoiceZipCode { get; set; }
    public string InvoiceCity { get; set; }


    public string PaymentInfo { get; set; }
    
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public string? ShipmentMethod { get; set; }
    public int CustomerId { get; set; }
    public decimal FreightPrice { get; set; }
    public string PhoneNumber { get; set; }


    public virtual Customer Customer { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();



}
