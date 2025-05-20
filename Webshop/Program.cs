using Webshop.Data;
using Webshop.Models;
using Webshop.UI;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Webshop.Services.Interfaces;
using Webshop.Services;
namespace Webshop
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var services = new ServiceCollection();

          
            services.AddDbContext<MyDbContext>(opts =>
                opts.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;
  Initial Catalog=WebShop1;
  Integrated Security=True;
  MultipleActiveResultSets=True;
  Connect Timeout=30;
  Encrypt=False;
  Trust Server Certificate=False;
  Application Intent=ReadWrite;
  Multi Subnet Failover=False"));

           
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ILogicService, LogicService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IStatisticService, StatisticService>();
            services.AddScoped<GUI>();
            services.AddScoped<UI.UI>();

            var provider = services.BuildServiceProvider();

           
            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
               
                await db.Database.MigrateAsync();
                await DataSeeder.SeedTestDataAsync(db);
            }

         
            using (var scope = provider.CreateScope())
            {
                var ui = scope.ServiceProvider.GetRequiredService<UI.UI>();
                await ui.Start();
            }
        }
    }
}
 

