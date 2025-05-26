using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;
using Webshop.Data;
using Microsoft.EntityFrameworkCore;
using Webshop.Services.Interfaces;

namespace Webshop.Services;
internal class CustomerService : ICustomerService

{
    private readonly MyDbContext _dbContext;
    public CustomerService(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task<(bool success, string message)> DeleteCustomerAsync(int customerId, int cid) // tar bort en kund
    {

        if (customerId == cid)
            return (false, "You cannot delete yourself");

       
        var customer = await _dbContext.Customers.FindAsync(customerId);
        if (customer == null)
            return (false, "Customer not found");

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
        return (true, "Customer deleted successfully");
    }
    public async Task<Customer?> GetCustomerCartAsync(int customerId) // hämtar kundens varukorg
    {
        return await _dbContext.Customers
            .Include(c => c.Cart)
                .ThenInclude(cart => cart!.Items)
                    .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _dbContext.Customers.ToListAsync();
    }
    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return await _dbContext.Customers.FindAsync(customerId);
    }
    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }
    public async Task<(bool success, string message)> UpdateCustomerAsync(Customer customer) //updatea en kund
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

    public async Task<(bool success, string message)> AdminRightsHandelingAsync(int userId, int cid) // hantera adminrättigheter 
    {
        if(userId == cid) //kontroll om admin försöker ändra sina egna rättigheter
            return (false, "You cannot change your own admin rights");

        var customer = await _dbContext.Customers.FindAsync(userId);
        if (customer == null)
            return (false, "Customer not found");
        customer.IsAdmin = !customer.IsAdmin;
        await _dbContext.SaveChangesAsync();
        return (true, customer.IsAdmin ? $"{customer.FirstName} with Id  {userId} har been updated with Admin rights." : $" {customer.FirstName} with Id {userId} admins righs has been revoked.");
    }

 
}
