using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Webshop.Data;
using Microsoft.EntityFrameworkCore;
using Webshop.Services.Interfaces;
using Webshop.Models.DTO;
namespace Webshop.Services;
internal class CategoryService : ICategoryService
{
    private readonly MyDbContext _dbContext;
    public CategoryService(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task<(bool success, string message)> AddProductCategoryAsync(ProductCategory category) //Lägg till ny kategori
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Category name is required");

            _dbContext.ProductCategories.AddAsync(category);
            await _dbContext.SaveChangesAsync();
            return (true, "Category added successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding category: {ex.Message}");
        }
    }
    public async Task<(bool success, string message)> UpdateProductCategoryAsync(ProductCategory category) //Uppdatera kategori
    {
        try
        {
            var existingcategory = await _dbContext.ProductCategories.FindAsync(category.Id);
            if (existingcategory == null)
                return (false, "Category not found");

            existingcategory.CategoryName = category.CategoryName;
            existingcategory.Description = category.Description;

            //_dbContext.ProductCategories.Update(existingcategory);
            await _dbContext.SaveChangesAsync();
            return (true, "Category updated successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occured" + e.Message);
        }
    }


    public async Task<List<ProductCategoryDTO>> GetAllProductCategoriesDtosAsync()
    //Hämta alla kategorier med DTO
    {
        return await _dbContext.ProductCategories
            .Select(c => new ProductCategoryDTO
            {
                Id = c.Id,
                CategoryName = c.CategoryName ?? "",
                Description = c.Description ?? ""
            })
            .ToListAsync();
    }
    public async Task<(bool success, string message)> DeleteCategory(int categoryId)
    {
        try
        {
            var category = await _dbContext.ProductCategories.FindAsync(categoryId);
            if (category != null)
            {
                _dbContext.ProductCategories.Remove(category);
                await _dbContext.SaveChangesAsync();

            }
            return (true, "Category deleted successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occurred while deleting the category: " + e.Message);
        }
    }
}
