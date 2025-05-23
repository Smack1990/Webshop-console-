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
   public int Id { get; set; } // Primary Key
    public DateTime OrderDate { get; set; } = DateTime.Now; //beställningsdatum
    public decimal TotalAmount { get; set; } // totalt belopp för beställningen

    public string ShippingAddress { get; set; } = string.Empty; //leveransaddress
    public int ZipCode { get; set; } //  leveransaddress zip code
    public string City { get; set; } // Leveransaddress city
    public string InvoiceAddress { get; set; } = string.Empty; // faktureringsaddress
    public int InvoiceZipCode { get; set; } // faktureringsaddress zip code

    public string InvoiceCity { get; set; } // faktureringsaddress city


    public string PaymentInfo { get; set; } //betalningsinfo
    
    public string Status { get; set; } = "Pending"; //status för beställningen
    public string? PaymentMethod { get; set; } //betalningsmetod
    public string? ShipmentMethod { get; set; } // fraktmetod
    public int CustomerId { get; set; } //Foreign Key till Customer
    public decimal FreightPrice { get; set; } // fraktpris
    public string PhoneNumber { get; set; } // telefonnummer till kunden


    public virtual Customer Customer { get; set; } // Navigation property till Customer
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // lista med OrderItems



}
