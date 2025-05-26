using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models.DTO;
public record SupplierDTO
{
    public int Id { get; init; } // Primary Key
    public string? CompanyName { get; init; } = ""; // Namn på leverantör
    public string? Email { get; init; } = ""; // Email till leverantör
    public string? PhoneNumber { get; init; } //e-post till leverantör
    public string? Address { get; init; } = ""; // Leverantörens adress

    
}
