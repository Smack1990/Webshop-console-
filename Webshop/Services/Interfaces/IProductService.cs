using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Webshop.Models.DTO;

namespace Webshop.Services.Interfaces;
internal interface IProductService
{
    Task<ProductDTO?> GetProductAsync(int id);
    Task<List<ProductDTO?>> GetProductByName(string input);
    //Task<List<Product>> GetAllProductsAsync();
    Task<(bool Success, string Message)> AddProductAsync(CreateProductDTO dto);
    Task<(bool Success, string Message)> UpdateProductAsync(UpdateProductDTO dto);
    Task<(bool Success, string Message)> DeleteProductAsync(int id);

    Task<List<ProductDTO>> GetAllProductDtosAsync();
    Task<UpdateProductDTO> GetProductForUpdateAsync(int id);
}
