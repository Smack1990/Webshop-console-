using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface IProductService
{
    Task<Product?> GetProductAsync(int id);
    Task<List<Product?>> GetProductByName(string input);
    Task<List<Product>> GetAllProducts();
    Task<(bool Success, string Message)> AddProductAsync(Product product);
    Task<(bool Success, string Message)> UpdateProductAsync(Product product);
    Task<(bool Success, string Message)> DeleteProductAsync(int id);
}
