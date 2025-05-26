using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Services.Interfaces;
internal interface ILogicService
{
    Task<(bool success, string message)> AddToCartAsync(int customerId, int productId, int quantity);
    Task<Order?> CheckoutAsync(int customerId, string shippingAddress, int zipCode, string city, string invoiceAddress, int invoiceZipCode, string invoiceCity, string paymentMethod, string shipmentMethod, string phoneNumber, string paymentInfo, decimal totalprice, decimal freightprice);
    Task<List<Order>> GetCustomerOrdersAsync(int customerId);
    Task<Cart?> DeleteCartAsync(int customerid);
    Task<(bool Success, string Message)> ChangeCartProductQuantityAsync(int customerId, int productId, int newQuantity);
    Task<int> NumberOfCartItemsAsync(int id);

    Task<Customer> RefreshCartAsync(int id);
}
