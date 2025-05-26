using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Webshop.Models.DTO;

namespace Webshop.Services.Interfaces;
internal interface ISupplierService
{
    Task<(bool success, string message)> UpdateSuppliers(Supplier Supplier);
    Task<(bool Success, string Message)> AddSupplier(Supplier supplier);
    Task<(bool success, string message)> DeleteSupplier(int supplierId);
    //Task<List<Supplier>> GetAllSuppliersAsync();
    Task<List<SupplierDTO>> GetAllSupplierDTOAsync();
}
