using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface ICustomerService
{
    Task<(bool success, string message)> DeleteCustomerAsync(int customerId, int cid); // tar bort en kund
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<(bool success, string message)> UpdateCustomerAsync(Customer customer);
    Task<(bool success, string message)> AdminRightsHandelingAsync(int userId, int cid);
    Task<Customer?> GetCustomerCartAsync(int customerId);

}
