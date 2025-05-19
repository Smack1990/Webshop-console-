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
using Webshop.UI;

namespace Webshop.Services;
internal class AdminHandler
{
    private readonly MyDbContext _dbContext;
    private readonly UI.UI _UI;
    private readonly Logic _logic;
    private Customer _currentCustomer;
    public AdminHandler(MyDbContext context)
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
    public async Task<(bool success, string message)> UpdateProductAsync(Product product)
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

    public async Task<(bool success, string message)> DeleteProduct(int productId)
    {
        try
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product == null)
                    return(false, "Product not found");

            var hasOrders =  await _dbContext.Products.AnyAsync(o => o.Id == productId);
            if(hasOrders)
                return (false, "Product cannot be deleted with existing orders");

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

    #region Supplier
    public async Task<(bool success, string message)> UpdateSuppliers(Supplier Supplier)
    {
        try
        {
            var supplier = await _dbContext.Suppliers.FindAsync(Supplier.Id); 
            if(supplier == null)
                return (false, "Supplier not found");

            supplier.CompanyName = Supplier.CompanyName;
            supplier.Email = Supplier.Email;
            supplier.PhoneNumber = Supplier.PhoneNumber;
            supplier.Address = Supplier.Address;
             _dbContext.Suppliers.Update(supplier);
           await _dbContext.SaveChangesAsync(); 
            return(true, "Supplier updated successfully");
        }
        catch(Exception e)
        {
            return (false, "An error occured" + e.Message);
        }
        
    }
    public async Task<(bool Success, string Message)> AddSupplier(Supplier supplier)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(supplier.CompanyName))
                return (false, "Product name is required");
            if (string.IsNullOrWhiteSpace(supplier.Email))
                return (false, "Email must be provided");
            if (string.IsNullOrWhiteSpace(supplier.PhoneNumber))
                return (false, "Phonenumer must be provided");
            if (string.IsNullOrWhiteSpace(supplier.Address))
                return (false, "Address must be provided"); 

           await _dbContext.Suppliers.AddAsync(supplier);
           await _dbContext.SaveChangesAsync();
            return (true, "Product added successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding product: {ex.Message}");
        }
    }
    public async Task<(bool success, string message)> DeleteSupplier(int supplierId)
    {
        try
        {
          
            var supplier =  await _dbContext.Suppliers
                .Include(s => s.Products) 
                .FirstOrDefaultAsync(s => s.Id == supplierId);

            if (supplier == null)
                return (false, "\nSupplier not found");
            if (supplier.Products.Any())
                return (false, "\nCannot delete supplier with associated products");
           
           _dbContext.Suppliers.Remove(supplier);
           await  _dbContext.SaveChangesAsync();

            return (true, "\nSupplier deleted successfully");
        }
        catch (Exception e)
        {
            return (false, $"An error occurred while deleting the supplier: {e.Message}");
        }
    }
#endregion
    #region Category   

    public (bool success, string message) AddProductCategory(ProductCategory category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Category name is required");

            _dbContext.ProductCategories.Add(category);
            _dbContext.SaveChanges();
            return (true, "Category added successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding category: {ex.Message}");
        }
    }
    public (bool success, string message) UpdateProductCategory(ProductCategory category)
    {
        try
        {
            var existingcategory = _dbContext.ProductCategories.Find(category.Id);
            if (existingcategory == null)
                return (false, "Category not found");

            existingcategory.CategoryName = category.CategoryName;
            existingcategory.Description = category.Description;

            //_dbContext.ProductCategories.Update(existingcategory);
            _dbContext.SaveChanges();
            return (true, "Category updated successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occured" + e.Message);
        }
    }
    public async Task<List<ProductCategory>> GetAllProductCategories()
    {
        return _dbContext.ProductCategories.ToList();
    }

    #endregion
    #region Supplier
    public async Task<List<Supplier>> GetAllSuppliers()
    {
        return  _dbContext.Suppliers.ToList();
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
    public async Task DeleteSuppliers(int id)
    {
        var supplier = await _dbContext.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            _dbContext.Suppliers.Remove(supplier);
            await _dbContext.SaveChangesAsync();
        }

    }

    #endregion
    #region customer
    public async Task DeleteCustomer(int customerId)
    {
    
            var customer = await _dbContext.Customers.FindAsync(customerId);
            if (customer != null)
            {
                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();
              
            }
    
    }

    public async Task<List<Customer>> GetAllCustomers()
    {
        return await _dbContext.Customers.ToListAsync();
    }
    public async Task<Customer?> GetCustomerById(int customerId)
    {
        return await _dbContext.Customers.FindAsync(customerId);
    }
    public async Task<Customer?> GetCustomerByEmail(string email)
    {
        return await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }
    public async Task<(bool success, string message)> UpdateCustomer(Customer customer)
    {
        try
        {
            var existingCustomer = await _dbContext.Customers.FindAsync(customer.Id);
            if (existingCustomer == null)
                return (false, "Customer not found");
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;
            await _dbContext.SaveChangesAsync();
            return (true, "Customer updated successfully");
        }
        catch (Exception e)
        {
            return (false, "An error occurred while updating the customer: " + e.Message);
        }
    }

    public async Task<(bool success, string message)> AdminHandelingOnID(int userId)
    {
        var customer = await _dbContext.Customers.FindAsync(userId);
        if(customer == null)
            return (false, "Customer not found");
        customer.IsAdmin = !customer.IsAdmin;
        await _dbContext.SaveChangesAsync();
        return (true, customer.IsAdmin? $"{customer.FirstName} with Id  {userId} har been updated with Admin rights." : $" {customer.FirstName} with Id {userId} admins righs has been revoked.");
    }

    public async Task GetOrdersFromId(int userId)
    {
        
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (customer == null)
        {
            Console.WriteLine($"Customer with ID {userId} not found.");
            return;
        }

       
        if (!customer.Orders.Any())
        {
            Console.WriteLine($"Customer with ID {userId} has no orders.");
            return;
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
            Console.WriteLine($"Ship Zip    : {order.zipCode}");
            Console.WriteLine($"Ship City   : {order.City}");
            Console.WriteLine($"inv address : {order.invoiceAddress}");
            Console.WriteLine($"inv Zip     : {order.invoicezipCode}");
            Console.WriteLine($"inv City    : {order.invoiceCity}");
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
    }
    #endregion
}
