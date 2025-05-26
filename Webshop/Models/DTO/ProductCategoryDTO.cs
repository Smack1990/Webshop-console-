using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models.DTO;
public record ProductCategoryDTO
{
    public int Id { get; set; } // Primary Key
    public string? CategoryName { get; init; } // Namn på produktkategori
    public string? Description { get; init; } // Beskrivning av produktkategori
}
