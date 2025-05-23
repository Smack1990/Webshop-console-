using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;
using Webshop.Services.Interfaces;

namespace Webshop.Services;
internal class LoginService : ILoginService
{
    private readonly MyDbContext _dbContext;

    public LoginService(MyDbContext context)
    {
        _dbContext = context;
    }
    public async Task<(Customer, string)> AuthenticateAsync(string email, string password) //Hanterar inloggning
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == email);

        if (customer == null)
            return (null, "No user found with that email.");

        if (!BCrypt.Net.BCrypt.Verify(password, customer.Password)) //Hanterar läsning/verifiering av hashat lösenord
            return (null, "Incorrect password.");

        await _dbContext.SaveChangesAsync();

        return (customer, "Login successful.");
    }
}
