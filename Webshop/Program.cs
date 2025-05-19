using Webshop.Data;
using Webshop.Models;
using Webshop.UI;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
namespace Webshop
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            using var context = new MyDbContext();

            
            context.Database.Migrate();

          
            await TestDataSeeder.SeedTestDataAsync(context);

            var ui = new UI.UI(context);
            await ui.Start();
        }
    }
}
