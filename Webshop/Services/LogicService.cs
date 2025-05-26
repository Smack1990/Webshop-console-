using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Identity.Client;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;
using Webshop.Services.Interfaces;
using Webshop.UI;

namespace Webshop.Services;
internal class LogicService : ILogicService
{
    private readonly MyDbContext _dbContext;
    //private Customer _currentCustomer;
    public LogicService(MyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<(bool success, string message)> AddToCartAsync(int customerId, int productId, int quantity) //addera till kundvagnen
    {
        if (quantity <= 0)
            return (false, "Quantity must be greater than 0");

        try
        {

            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c!.Items)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                return (false, "Customer not found");


            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
                return (false, "Product not found");


            if (product.Stock < quantity)
                return (false, "Not enough stock available");


            var existingItem = customer!.Cart!.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                };
                customer.Cart.Items.Add(cartItem);
            }

            await _dbContext.SaveChangesAsync();
            return (true, "Product added to cart successfully");
        }
        catch (Exception e)
        {
            return (false, $"An error occurred while adding to cart: {e.Message}");
        }
    }

    //checkoutmetod
    public async Task<Order?> CheckoutAsync(int customerId, string shippingAddress, int zipCode, string city, string invoiceAddress, int invoiceZipCode, string invoiceCity, string paymentMethod, string shipmentMethod, string phoneNumber, string paymentInfo, decimal totalprice, decimal freightprice)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress))
            return null;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c!.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null || !customer!.Cart!.Items.Any()) return null;

            var outOfStockItem = customer.Cart.Items
                   .FirstOrDefault(i => i.Quantity > i.Product!.Stock);
            if (outOfStockItem != null)
            {
                Console.WriteLine($"Cannot checkout: requested quantity ({outOfStockItem.Quantity}) for product '{outOfStockItem.Product!.Name}' exceeds stock ({outOfStockItem.Product.Stock}).");
                return null;
            }

            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                ShippingAddress = shippingAddress,
                ZipCode = zipCode,
                City = city,
                InvoiceAddress = invoiceAddress,
                InvoiceZipCode = invoiceZipCode,
                InvoiceCity = invoiceCity,
                Status = "Processing",
                PaymentMethod = paymentMethod,
                ShipmentMethod = shipmentMethod,
                PhoneNumber = phoneNumber,
                PaymentInfo = paymentInfo,
                FreightPrice = freightprice,
                TotalAmount = totalprice,
                OrderItems = customer.Cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product!.Price
                }).ToList()
            };

            foreach (var item in customer.Cart.Items)
            {
                item.Product!.Stock -= item.Quantity;
                item.Product.QuantitySold++;
            }


            customer!.Cart!.Items.Clear();

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return order;



        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("An error occurred during checkout: " + e.Message);
            return null;
        }
    }
    //hämta orderhistorik
    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        
        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }
    public async Task<Cart?> DeleteCartAsync(int customerid) //radera kundvagn
    {
        try
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c!.Items)
                .FirstOrDefaultAsync(c => c.Id == customerid);
            if (customer == null)
                return null;
            _dbContext.CartItems.RemoveRange(customer.Cart!.Items);
            
            await _dbContext.SaveChangesAsync();
            return customer.Cart;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while emptying the cart: " + e.Message);
            return null;
        }
    }

    public async Task<(bool Success, string Message)> ChangeCartProductQuantityAsync(int customerId, int productId, int newQuantity) // ändra kvantitet i kundvagnen
    {

        var cartItem = await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Cart!.CustomerId == customerId && ci.ProductId == productId);

        if (cartItem == null)
            return (false, "Cart item not found.");


        if (newQuantity <= 0)
        {
            _dbContext.CartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
            return (true, "Cart item removed.");
        }


        if (newQuantity > cartItem.Product!.Stock)
            return (false, $"Cannot set quantity to {newQuantity}. Only {cartItem.Product.Stock} in stock.");


        cartItem.Quantity = newQuantity;
        await _dbContext.SaveChangesAsync();
        return (true, "Cart item quantity updated.");
    }
   public async Task<int> NumberOfCartItemsAsync(int id)
    {
        var maybeSum = await _dbContext.CartItems
         .Where(ci => ci.Cart!.CustomerId == id)
         .Select(ci => (int?)ci.Quantity)   
         .SumAsync();

        return maybeSum ?? 0;
    }

   

public async Task<Customer> RefreshCartAsync(int id) //uppdatera kundvagnen
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Cart)
            .ThenInclude(c => c!.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            throw new InvalidOperationException($"Customer with id {id} not found.");

        return customer;
    }
}
