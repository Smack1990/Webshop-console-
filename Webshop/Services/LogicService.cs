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
    private Customer _currentCustomer;
    public LogicService(MyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<(bool success, string message)> AddToCartAsync(int customerId, int productId, int quantity)
    {
        if (quantity <= 0)
            return (false, "Quantity must be greater than 0");

        try
        {

            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                return (false, "Customer not found");


            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
                return (false, "Product not found");


            if (product.Stock < quantity)
                return (false, "Not enough stock available");


            var existingItem = customer.Cart.Items.FirstOrDefault(i => i.ProductId == productId);

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

    public async Task<Order?> CheckoutAsync(int customerId, string shippingAddress, int zipCode, string city, string invoiceAddress, int invoiceZipCode, string invoiceCity, string paymentMethod, string shipmentMethod, string phoneNumber, string paymentInfo, decimal totalprice, decimal freightprice)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress))
            return null;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .ThenInclude(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null || !customer.Cart.Items.Any()) return null;

            var outOfStockItem = customer.Cart.Items
                   .FirstOrDefault(i => i.Quantity > i.Product.Stock);
            if (outOfStockItem != null)
            {
                Console.WriteLine($"Cannot checkout: requested quantity ({outOfStockItem.Quantity}) for product '{outOfStockItem.Product.Name}' exceeds stock ({outOfStockItem.Product.Stock}).");
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
                    UnitPrice = i.Product.Price
                }).ToList()
            };

            foreach (var item in customer.Cart.Items)
            {
                item.Product.Stock -= item.Quantity;
                item.Product.QuantitySold++;
            }


            customer.Cart.Items.Clear();

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
    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }
    public async Task<Cart?> DeleteCartAsync(int customerid)
    {
        try
        {
            var customer = await _dbContext.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == customerid);
            if (customer == null)
                return null;
            _dbContext.CartItems.RemoveRange(customer.Cart.Items);
            Console.WriteLine($"Removing {customer.Cart.Items.Count} item(s) from cart.");
            await _dbContext.SaveChangesAsync();
            return customer.Cart;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while emptying the cart: " + e.Message);
            return null;
        }
    }

    public async Task<(bool Success, string Message)> ChangeCartProductQuantityAsync(int customerId, int productId, int newQuantity)
    {

        var cartItem = await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Cart.CustomerId == customerId && ci.ProductId == productId);

        if (cartItem == null)
            return (false, "Cart item not found.");


        if (newQuantity <= 0)
        {
            _dbContext.CartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
            return (true, "Cart item removed.");
        }


        if (newQuantity > cartItem.Product.Stock)
            return (false, $"Cannot set quantity to {newQuantity}. Only {cartItem.Product.Stock} in stock.");


        cartItem.Quantity = newQuantity;
        await _dbContext.SaveChangesAsync();
        return (true, "Cart item quantity updated.");
    }
   public async Task<int> NumberOfCartItemsAsync(int id)
    {
        var maybeSum = await _dbContext.CartItems
         .Where(ci => ci.Cart.CustomerId == id)
         .Select(ci => (int?)ci.Quantity)   
         .SumAsync();

        return maybeSum ?? 0;
    }

    public async Task<List<Order>> GetOrdersFromIdAsync(int userId)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (customer == null)
        {
            Console.WriteLine($"Customer with ID {userId} not found.");
            return new List<Order>();
        }

        if (!customer.Orders.Any())
        {
            Console.WriteLine($"Customer with ID {userId} has no orders.");
            return new List<Order>();
        }

        foreach (var order in customer.Orders)
        {
            Console.WriteLine(new string('-', 98));
            decimal netAmount = order.TotalAmount - order.FreightPrice;
            decimal taxPrice = Math.Round(netAmount * 0.25m, 2);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nOrder ID  : {order.Id}");
            Console.WriteLine($"Order date: {order.OrderDate}");
            Console.WriteLine($"Shipment    : {order.ShipmentMethod}");
            Console.WriteLine($"Ship adress : {order.ShippingAddress}");
            Console.WriteLine($"Ship Zip    : {order.ZipCode}");
            Console.WriteLine($"Ship City   : {order.City}");
            Console.WriteLine($"inv address : {order.InvoiceAddress}");
            Console.WriteLine($"inv Zip     : {order.InvoiceZipCode}");
            Console.WriteLine($"inv City    : {order.InvoiceCity}");
            Console.WriteLine($"Payment     : {order.PaymentMethod} ({order.PaymentInfo})");
            Console.WriteLine($"Freight     : {order.FreightPrice:C}");
            Console.WriteLine($"Phone       : {order.PhoneNumber}");
            Console.WriteLine($"TotalPrice  : {order.TotalAmount:C}");
            Console.WriteLine($"VAT         : {taxPrice:C}");

            Console.ResetColor();

            foreach (var item in order.OrderItems)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Product ID: {item.ProductId}, Product Name: {item.Product.Name}, Quantity: {item.Quantity}, Unit Price: {item.UnitPrice}, ");
                Console.ResetColor();
            }
            Console.WriteLine(new string('-', 75));
        }

        return customer.Orders.ToList();
    }

    public async Task<Customer> RefreshCartAsync(int id)
    {

        return await _dbContext.Customers
                .Include(c => c.Cart).ThenInclude(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == _currentCustomer.Id);
        
        

    }
}
