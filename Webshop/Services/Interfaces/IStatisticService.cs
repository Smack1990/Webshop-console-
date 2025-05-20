using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface IStatisticService
{

    Task<List<(Product Product, int TotalSold)>> GetBestSellingProductsAsync(int topNumber);
    Task<List<Product>> GetallProductsOrderdBybestsellingAsync(int number);
    Task<List<(string ProductCategory, int OrderCount)>> MostPopularCategoryAsync();
    Task<List<(string? PaymentMethod, int OrderCount)>> MostPopularPaymentMethodAsync();
    Task<List<(Supplier Supplier, decimal TotalSales)>> GetSalesBySupplierAsync();
    Task<List<(Customer Customer, int Visits)>> GetTopSiteVisitorsAsync(int top = 5);
}
