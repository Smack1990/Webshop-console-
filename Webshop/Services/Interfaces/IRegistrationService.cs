using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface IRegistrationService
{
    Task<(bool Success, string Message)> RegisterCustomerAsync(Customer newCustomer, string password);
    Task<bool> CheckIfEmailExistAsync(string email);

}
