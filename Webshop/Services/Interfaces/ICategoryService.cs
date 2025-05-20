using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface ICategoryService
{

    Task<(bool success, string message)> AddProductCategoryAsync(ProductCategory category);
    Task<(bool success, string message)> UpdateProductCategoryAsync(ProductCategory category);
    Task<List<ProductCategory>> GetAllProductCategoriesAsync();
    Task<(bool success, string message)> DeleteCategory(int categoryId);

}
