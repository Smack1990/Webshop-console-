using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface ILoginService
{
    Task<(Customer customer, string message)> AuthenticateAsync(string email, string password);
}
