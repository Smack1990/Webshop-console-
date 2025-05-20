using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models;
public class Customer
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Password { get; set; }
    public bool IsAdmin { get; set; } = false;
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public int Sitevisit { get; set; }

    public virtual Cart Cart { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
