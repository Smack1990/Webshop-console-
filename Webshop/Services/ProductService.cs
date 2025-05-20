using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Spectre.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;
using Webshop.Services.Interfaces;
using Webshop.UI;

namespace Webshop.Services;
internal class ProductService : IProductService
{
    private readonly MyDbContext _dbContext;
    private readonly UI.UI _UI;
    private readonly LogicService _logic;
    private Customer _currentCustomer;
    public ProductService(MyDbContext context)
    {
        _dbContext = context;
    }
    #region Products
    public async Task<Product?> GetProductAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<List<Product?>> GetProductByName(string input)
    {
        var inputLike =  $"%{input}%";
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p =>
            EF.Functions.Like(p.Name, inputLike)
         || EF.Functions.Like(p.Supplier.CompanyName, inputLike)).ToListAsync();
    }

    public async Task<List<Product>> GetAllProducts()
    {
         
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddProductAsync(Product product)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return (false, "Product name is required");

            if (product.Price <= 0)
                return (false, "Price must be greater than 0");

            if (product.Stock< 0)
                return (false, "Stock quantity cannot be negative");

           await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return (true, "Product added successfully");
        }
        catch (Exception ex)
        {
            var dbEx = ex as Microsoft.EntityFrameworkCore.DbUpdateException;
            var sqlError = dbEx?.InnerException?.Message;

            var fullMessage = ex.Message
                            + (sqlError != null
                                ? $"\n→ Inner: {sqlError}"
                                : "");

            return (false, $"Error adding product: {fullMessage}");
        }
    }
    public async Task<(bool Success, string Message)> UpdateProductAsync(Product product)
    {
        try
        {
            var existingProduct = await _dbContext.Products.FindAsync(product.Id);
            if (existingProduct == null)
                return (false, "Product not found");

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.ProductCategoryId = product.ProductCategoryId;
            existingProduct.SupplierId = product.SupplierId;
            existingProduct.IsActive = product.IsActive;

           await _dbContext.SaveChangesAsync();


            return (true, "Product updated successfully");

        }
        catch(Exception e)
        {
            return (false, "An error occurred while updating the product: " + e.Message);
        }

    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int productId)
    {
        try
        {
            var product = await _dbContext.Products.FindAsync(productId);
         

            var hasOrdersItems =  await _dbContext.OrderItems.AnyAsync(o => o.Id == productId);
            if(hasOrdersItems)
                return (false, "Product cannot be deleted with existing orders");
            
            var inCart = await _dbContext.CartItems
         .AnyAsync(ci => ci.ProductId == productId);
            if (inCart)
                return (false, "Product cannot be deleted because it is in someone’s cart");

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return (true, "Product removed successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occurred while deleting the product: " + e.Message);
        }
    }

    public async Task SwitchIsActive(int prodId)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == prodId);

        if (product != null)
        {
            product.IsActive = !product.IsActive;
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine($"Product with ID {prodId} not found.");
        }
    }
    #endregion


}
