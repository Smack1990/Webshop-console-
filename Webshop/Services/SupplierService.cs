using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Webshop.Data;
using Microsoft.EntityFrameworkCore;
using Webshop.Services.Interfaces;

namespace Webshop.Services;
internal class SupplierService : ISupplierService
{
    private readonly MyDbContext _dbContext;
    public SupplierService(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task<(bool success, string message)> UpdateSuppliers(Supplier Supplier)
    {
        try
        {
            var supplier = await _dbContext.Suppliers.FindAsync(Supplier.Id);
            if (supplier == null)
                return (false, "Supplier not found");

            supplier.CompanyName = Supplier.CompanyName;
            supplier.Email = Supplier.Email;
            supplier.PhoneNumber = Supplier.PhoneNumber;
            supplier.Address = Supplier.Address;
            _dbContext.Suppliers.Update(supplier);
            await _dbContext.SaveChangesAsync();
            return (true, "Supplier updated successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occured" + e.Message);
        }

    }
    public async Task<(bool Success, string Message)> AddSupplier(Supplier supplier)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(supplier.CompanyName))
                return (false, "Product name is required");
            if (string.IsNullOrWhiteSpace(supplier.Email))
                return (false, "Email must be provided");
            if (string.IsNullOrWhiteSpace(supplier.PhoneNumber))
                return (false, "Phonenumer must be provided");
            if (string.IsNullOrWhiteSpace(supplier.Address))
                return (false, "Address must be provided");

            await _dbContext.Suppliers.AddAsync(supplier);
            await _dbContext.SaveChangesAsync();
            return (true, "Product added successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding product: {ex.Message}");
        }
    }
    public async Task<(bool success, string message)> DeleteSupplier(int supplierId)
    {
        try
        {

            var supplier = await _dbContext.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == supplierId);

            if (supplier == null)
                return (false, "\nSupplier not found");
            if (supplier.Products.Any())
                return (false, "\nCannot delete supplier with associated products");

            _dbContext.Suppliers.Remove(supplier);
            await _dbContext.SaveChangesAsync();

            return (true, "\nSupplier deleted successfully");
        }
        catch (Exception e)
        {
            return (false, $"An error occurred while deleting the supplier: {e.Message}");
        }
    }

    public async Task<List<Supplier>> GetAllSuppliersAsync()
    {
        return _dbContext.Suppliers.ToList();
    }

    public async Task DeleteSuppliers(int id)
    {
        var supplier = await _dbContext.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            _dbContext.Suppliers.Remove(supplier);
            await _dbContext.SaveChangesAsync();
        }

    }
}
