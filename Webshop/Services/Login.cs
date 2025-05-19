using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;

namespace Webshop.Services;
internal class Login
{
    private readonly MyDbContext _dbContext;

    public Login(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task<(Customer, string)> AuthenticateAsync(string email, string password)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email);

        if (customer == null)
            return (null, "No user found with that email.");

        if (!BCrypt.Net.BCrypt.Verify(password, customer.Password)) //Hanterar läsning hashat lösenord
            return (null, "Incorrect password.");
        
        return (customer, "Login successful.");
    }
}
