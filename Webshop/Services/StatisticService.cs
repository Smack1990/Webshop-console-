﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;
using Webshop.Services.Interfaces;

namespace Webshop.Services;
public class StatisticService : IStatisticService
{
    private readonly MyDbContext _dbContext;
    public StatisticService(MyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<List<(Product Product, int TotalSold)>> GetBestSellingProductsAsync(int topNumber) //Hämtade bästsäljande produkter with total sold amount
    {
        var stats = await _dbContext.OrderItems
            .GroupBy(o => o.Product)
            .Select(g => new
            {
                Product = g.Key,
                Total = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Total)
            .Take(topNumber)
            .ToListAsync();

        return stats
            .Where(x => x.Product != null)
            .Select(x => (x.Product!, x.Total))
            .OrderByDescending (x => x.Total)
            .ToList();
    }
    public async Task<List<Product>> GetallProductsOrderdBybestsellingAsync(int number) //hämta bästsäljande produkter
    {
        var info = await _dbContext.OrderItems
            .GroupBy(o => o.Product)
            .Select(g => new
            {
                Product = g.Key,
                Total = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Total)
            .Take(number)   
            .ToListAsync();
        return info
            .Select(x => x.Product!)
            .ToList();
    }

    public async Task<List<(string ProductCategory, int OrderCount)>> MostPopularCategoryAsync() //mest populär kategori
    {
       
        var stats = await _dbContext.OrderItems
            .Include(oi => oi.Product).ThenInclude(p => p!.Category)
            .GroupBy(oi => oi.Product!.Category!.CategoryName)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

   
        return stats.Select(x => (x.Category, x.Total)).ToList()!;
    }

    public async Task<List<(string? PaymentMethod, int OrderCount)>> MostPopularPaymentMethodAsync() //mest populär kategori
    {
        var stats = await _dbContext.Orders
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new
            {
                PaymentMethod = g.Key,
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .ToListAsync();
        return stats
            .Select(x => (x.PaymentMethod, x.OrderCount))
            .ToList();
    }
    public async Task<List<(Supplier Supplier, decimal TotalSales)>> GetSalesBySupplierAsync() // hämta försäljning per leverantör
    {
        var stats = await _dbContext.Orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.Product!.Supplier)
            .Select(g => new
            {
                Supplier = g.Key,
                TotalSales = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync();
        return stats
            .Select(x => (x.Supplier!, x.TotalSales))
            .ToList();
    }

    public async Task<List<(Customer Customer, int Visits)>> GetTopSiteVisitorsAsync(int top = 5) //hämta topp 5 besökare
    {
      
        var stats = await _dbContext.Customers
            .OrderByDescending(c => c.Sitevisit)
            .Take(top)
            .Select(c => new { Customer = c, Visits = c.Sitevisit })
            .ToListAsync();

      
        return stats.Select(x => (x.Customer, x.Visits)).ToList();
    }
}
