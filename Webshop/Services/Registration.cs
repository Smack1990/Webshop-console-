using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;

namespace Webshop.Services;
internal class Registration
{
    private readonly MyDbContext _dbContext;
    public Registration(MyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<(bool Success, string Message)> RegisterCustomerAsync(Customer newCustomer, string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return (false, "Password must be at least 8 characters long.");

        try
        {
            var exists = await _dbContext.Customers
                .AsNoTracking()
                .AnyAsync(c => c.Email == newCustomer.Email);
            if (exists)
                return (false, "Email already exists.");

            newCustomer.Password = BCrypt.Net.BCrypt.HashPassword(password);
            newCustomer.Cart = new Cart { Customer = newCustomer };

            await _dbContext.Customers.AddAsync(newCustomer);
            await _dbContext.SaveChangesAsync();
            return (true, "Registration successful.\n");
        }
        catch (Exception e)
        {
            return (false, "An error occurred during registration: " + e.Message);
        }
    }
    public async Task<bool> CheckIfEmailExistAsync(string email)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email == email);
    }
}
