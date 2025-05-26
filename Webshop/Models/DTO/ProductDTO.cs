using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models.DTO;

public record ProductDTO //dto för produkter. DTO (data transer objekt) är ett objekt som bär data mellan olika lager. 
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public int Stock { get; init; }

    public string SKU { get; init; } = "";
    public DateTime CreatedDate { get; init; }
    public bool IsActive { get; init; }
    public int QuantitySold { get; init; }
    public string CategoryName { get; init; } = "";
    public string SupplierName { get; init; } = "";

    public int ProductCategoryId { get; init; }
}

