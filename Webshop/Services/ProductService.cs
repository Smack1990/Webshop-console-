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
using Webshop.Models.DTO;
using Webshop.Services.Interfaces;
using Webshop.UI;

namespace Webshop.Services;
internal class ProductService : IProductService
{
    private readonly MyDbContext _dbContext;

  

    public ProductService(MyDbContext context)
    {
        _dbContext = context;
    }
    #region Products
    public async Task<ProductDTO?> GetProductAsync(int id) //hämta produkter
    {
        return await _dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                Stock = p.Stock,
                SKU = p.SKU!,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                QuantitySold = p.QuantitySold,
                CategoryName = p.Category!.CategoryName!,
                SupplierName = p.Supplier!.CompanyName!,
                ProductCategoryId = p.ProductCategoryId
            })
        .FirstOrDefaultAsync();
    }
    public async Task<List<ProductDTO>> GetProductByName(string input)
    {
        var likeInput = $"%{input}%";
        return await _dbContext.Products
            .Where(p =>
                EF.Functions.Like(p.Name!, likeInput) ||
                EF.Functions.Like(p.Supplier!.CompanyName, likeInput))
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                Stock = p.Stock,
                SKU = p.SKU!,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                QuantitySold = p.QuantitySold,
                CategoryName = p.Category!.CategoryName!,
                SupplierName = p.Supplier!.CompanyName!,
                ProductCategoryId = p.ProductCategoryId
            })
            .ToListAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync() //hämta alla produkter
    {

        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddProductAsync(CreateProductDTO dto) // lägga till produkt
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            ProductCategoryId = dto.ProductCategoryId,
            SupplierId = dto.SupplierId,
            SKU = dto.SKU.Trim(),
            CreatedDate = DateTime.UtcNow,
            QuantitySold = 0
        };

       
        if (string.IsNullOrWhiteSpace(product.Name))
            return (false, "Product name is required");
        if (product.Price <= 0)
            return (false, "Price must be greater than 0");
        if (product.Stock < 0)
            return (false, "Stock quantity cannot be negative");

       

       
        try
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return (true, $"Product '{product.Name}' added (ID = {product.Id})");
        }
        catch (DbUpdateException dbEx)
        {
            
            var sqlError = dbEx.InnerException?.Message;
            var fullMsg = dbEx.Message + (sqlError != null ? $"\n→ Inner: {sqlError}" : "");
            return (false, $"Error adding product: {fullMsg}");
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error: {ex.Message}");
        }
    }


    public async Task<(bool Success, string Message)> DeleteProductAsync(int productId) // ta bort produkt
    {
        
        var hasOrderItems = await _dbContext.OrderItems
            .AnyAsync(oi => oi.ProductId == productId);
        if (hasOrderItems)
            return (false, "Cannot delete: product is in existing orders");

       
        var inCart = await _dbContext.CartItems
            .AnyAsync(ci => ci.ProductId == productId);
        if (inCart)
            return (false, "Cannot delete: product is in someone's cart");

        
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null)
            return (false, "Product not found");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return (true, "Product removed successfully");
    }

    public async Task SwitchIsActive(int prodId) // aktivera/deaktivera produkt från featured product list
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
    public async Task<List<ProductDTO>> GetAllProductDtosAsync() //hämta alla produkter asynkront, skriv om till DTO'n. 
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Select (p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                Stock = p.Stock,
                SKU = p.SKU!,
                CreatedDate = p.CreatedDate,
                IsActive = p.IsActive,
                QuantitySold = p.QuantitySold,
                CategoryName = p.Category!.CategoryName!,
                SupplierName = p.Supplier!.CompanyName!,
                ProductCategoryId = p.ProductCategoryId

            })
            .ToListAsync();


    }

    public async Task<UpdateProductDTO?> GetProductForUpdateAsync(int id) //Hämmta produkt för uppdatering, skriv om till DTO
    {
        return await _dbContext.Products
            .Where(p => p.Id == id)
            .Select(p => new UpdateProductDTO
            {
                Id = p.Id,
                Name = p.Name!,
                Description = p.Description!,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                ProductCategoryId = p.ProductCategoryId,
                SupplierId = p.SupplierId,
                SKU = p.SKU!
            })
            .FirstOrDefaultAsync();
    }
    public async Task<(bool Success, string Message)> UpdateProductAsync(UpdateProductDTO dto) //Uppdatera produkt via dto
    {
        var p = await _dbContext.Products.FindAsync(dto.Id);
        if (p == null) return (false, "Product not found");

       
        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.Stock = dto.Stock;
        p.IsActive = dto.IsActive;
        p.ProductCategoryId = dto.ProductCategoryId;
        p.SupplierId = dto.SupplierId;
        p.SKU = dto.SKU;

        await _dbContext.SaveChangesAsync();
        return (true, "Product updated");
    }
    #endregion


}
