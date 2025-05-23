using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Customer
{
    public int Id { get; set; } // Primary Key
    public string? FirstName { get; set; } // Förnamn på kunden
    public string? LastName { get; set; } // Efternamn på kunden
    public string? Email { get; set; } // Email till kunden
    public string? Phone { get; set; }  // Telefonnummer till kunden
    public string? Address { get; set; } // Adress till kunden
    public string? City { get; set; }// Stad till kunden
    public string? ZipCode { get; set; } // Postnummer till kunden
    public string? Password { get; set; } // Lösenord till kunden
    public bool IsAdmin { get; set; } = false; // Om kunden är admin, sätts till true via adminpanelen
    public DateTime RegistrationDate { get; set; } = DateTime.Now; // Registreringsdatum för kunden
    public int Sitevisit { get; set; } // Antal besök på sidan

    public virtual Cart Cart { get; set; } // Navigation property till Cart
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>(); // Lista med Orders
}
