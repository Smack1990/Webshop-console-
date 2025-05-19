using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Order
{
   public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    
    public string ShippingAddress { get; set; } = string.Empty;
    public int zipCode { get; set; }
    public string City { get; set; }
    public string invoiceAddress { get; set; } = string.Empty;
    public int invoicezipCode { get; set; }
    public string invoiceCity { get; set; }


    public string PaymentInfo { get; set; }
    
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public string? ShipmentMethod { get; set; }
    public int CustomerId { get; set; }
    public decimal FreightPrice { get; set; }
    public string PhoneNumber { get; set; }

    public Customer Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();



}
