using System;
using System.Linq;
using System.Threading.Tasks;
using Webshop.Models;

namespace Webshop.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Webshop.Models;

    public static class TestDataSeeder
    {
        public static async Task SeedTestDataAsync(MyDbContext context)
        {
   
            if (!context.Customers.Any(c => c.Email == "1@test2.se"))
            {
                var admin = new Customer
                {
                    FirstName = "Admin2",
                    LastName = "test",
                    Email = "1@test2.se",
                    Address = "123 Admin St",
                    Phone = "1234567890",
                    IsAdmin = true,
                    Cart = new Cart()
                };
                // Set a default password ("admin123")
                admin.Password = BCrypt.Net.BCrypt.HashPassword("admin");
                context.Customers.Add(admin);
                await context.SaveChangesAsync();
            }
            // Seed Categories individually
            var categoriesToSeed = new[]
            {
                new ProductCategory { CategoryName = "Bikes", Description = "All kinds of bicycles" },
                new ProductCategory { CategoryName = "Tools", Description = "Hand and power tools" },
                new ProductCategory { CategoryName = "Components", Description = "Bicycle components and spare parts" }
            };
            foreach (var cat in categoriesToSeed)
            {
                if (!context.ProductCategories.Any(c => c.CategoryName == cat.CategoryName))
                {
                    context.ProductCategories.Add(cat);
                }
            }
            await context.SaveChangesAsync();

            // Seed Suppliers individually
            var suppliersToSeed = new[]
            {
                new Supplier { CompanyName = "Allebike",    Email = "contact@allebike.com",    PhoneNumber = "555-0001", Address = "1 Bike Lane" },
                new Supplier { CompanyName = "Specialized",  Email = "info@specialized.com",     PhoneNumber = "555-0002", Address = "2 Race Rd" },
                new Supplier { CompanyName = "Scott",        Email = "support@scott.com",       PhoneNumber = "555-0003", Address = "3 Trail St" },
                new Supplier { CompanyName = "Park Tools",   Email = "service@parktools.com",   PhoneNumber = "555-0004", Address = "4 Workshop Ave" },
                new Supplier { CompanyName = "Shimano",      Email = "sales@shimano.com",       PhoneNumber = "555-0005", Address = "5 Gear Blvd" },
                new Supplier { CompanyName = "Sram",         Email = "info@sram.com",           PhoneNumber = "555-0006", Address = "6 Shift Ct" },
                new Supplier { CompanyName = "Vittoria",     Email = "contact@vittoria.com",   PhoneNumber = "555-0007", Address = "7 Tire Way" }
            };
            foreach (var sup in suppliersToSeed)
            {
                if(!context.Suppliers.Any(s => s.CompanyName == sup.CompanyName))
                {

                    context.Suppliers.Add(sup);
                }
                
            }
            await context.SaveChangesAsync();

            // Seed Products individually
            var bikesCat = context.ProductCategories.First(c => c.CategoryName == "Bikes");
            var toolsCat = context.ProductCategories.First(c => c.CategoryName == "Tools");
            var compsCat = context.ProductCategories.First(c => c.CategoryName == "Components");

            var allebike = context.Suppliers.First(s => s.CompanyName == "Allebike");
            var specialized = context.Suppliers.First(s => s.CompanyName == "Specialized");
            var scott = context.Suppliers.First(s => s.CompanyName == "Scott");
            var park = context.Suppliers.First(s => s.CompanyName == "Park Tools");
            var shimano = context.Suppliers.First(s => s.CompanyName == "Shimano");
            var sram = context.Suppliers.First(s => s.CompanyName == "Sram");
            var vittoria = context.Suppliers.First(s => s.CompanyName == "Vittoria");
            var now = DateTime.UtcNow;

            var productsToSeed = new[]
            {
                new Product { Name = "Alpa XT", Description = "Extremely fast XC bike.", Price = 34000m, Stock = 10, SupplierId = allebike.Id,    ProductCategoryId = bikesCat.Id, SKU = "Alpha-25",      IsActive = true, CreatedDate = now },
                new Product { Name = "Specialized S-Works evo comp", Description = "Carbon fiber frame.", Price = 98000m, Stock = 2, SupplierId = specialized.Id, ProductCategoryId = bikesCat.Id, SKU = "S-Work-b",    IsActive = true, CreatedDate = now },
                new Product { Name = "Scott sparks RC WC", Description = "World Cup edition.", Price = 55000m, Stock = 8, SupplierId = scott.Id,      ProductCategoryId = bikesCat.Id, SKU = "Scott-24",     IsActive = true, CreatedDate = now },

                // Tools
                new Product { Name = "Hex wrenches", Description = "Hex wrenches for every part.", Price = 199.98m, Stock = 25, SupplierId = park.Id,       ProductCategoryId = toolsCat.Id, SKU = "hw-6",         IsActive = true, CreatedDate = now },
                new Product { Name = "Rim tuner", Description = "Professional tuner for rims.", Price = 1200.00m, Stock = 10, SupplierId = park.Id,       ProductCategoryId = toolsCat.Id, SKU = "R-1-kd",        IsActive = true, CreatedDate = now },
                new Product { Name = "Derailleur hanger adjuster", Description = "A Derailleur hanger adjuster to help you with tweek your DH.", Price = 869.00m, Stock = 12, SupplierId = park.Id,   ProductCategoryId = toolsCat.Id, SKU = "DH-1",          IsActive = true, CreatedDate = now },
                // Additional Tools
                new Product { Name = "Pedal wrench", Description = "15mm wrench specifically for pedals.", Price = 129.99m, Stock = 50, SupplierId = park.Id, ProductCategoryId = toolsCat.Id, SKU = "PW-15", IsActive = true, CreatedDate = now },
                new Product { Name = "Tire lever set", Description = "Set of 3 nylon tire levers.", Price = 19.99m, Stock = 100, SupplierId = park.Id, ProductCategoryId = toolsCat.Id, SKU = "TL-3", IsActive = true, CreatedDate = now },
                new Product { Name = "Multi-tool", Description = "10-in-1 bike multi-tool with hex keys and screwdrivers.", Price = 49.50m, Stock = 75, SupplierId = park.Id, ProductCategoryId = toolsCat.Id, SKU = "MT-10", IsActive = true, CreatedDate = now },
                new Product { Name = "Chain whip", Description = "Durable chain whip for cassette removal.", Price = 25.00m, Stock = 30, SupplierId = park.Id, ProductCategoryId = toolsCat.Id, SKU = "CW-01", IsActive = true, CreatedDate = now },
                new Product { Name = "Torque wrench", Description = "Adjustable torque wrench with preset values.", Price = 199.95m, Stock = 20, SupplierId = park.Id, ProductCategoryId = toolsCat.Id, SKU = "W-100", IsActive = true, CreatedDate = now },
            // Components
                new Product { Name = "Shimano m700 Chain Set 12s", Description = "Lightweight aluminum bicycle chain set.", Price = 440.50m, Stock = 40, SupplierId = shimano.Id, ProductCategoryId = compsCat.Id, SKU = "m700-12s",     IsActive = true, CreatedDate = now },
                new Product { Name = "Rear Derailleur m700 12s", Description = "A good Derailleur for a good price.", Price = 89.50m, Stock = 40, SupplierId = shimano.Id, ProductCategoryId = compsCat.Id, SKU = "m700-12s-rd",  IsActive = true, CreatedDate = now },
                new Product { Name = "Shimano XT Front derailleur", Description = "shimano XT", Price = 752.00m, Stock = 40, SupplierId = shimano.Id, ProductCategoryId = compsCat.Id, SKU = "XT-01-1",       IsActive = true, CreatedDate = now },
                new Product { Name = "Chain Set 6s", Description = "Lightweight aluminum bicycle chain set.", Price = 89.50m, Stock = 40, SupplierId = shimano.Id, ProductCategoryId = compsCat.Id, SKU = "CS-ALU",         IsActive = true, CreatedDate = now },
                new Product { Name = "SRAM Eagle NX", Description = "SRAM Eagle Nx, Top components.", Price = 2500.00m, Stock = 4, SupplierId = sram.Id,        ProductCategoryId = compsCat.Id, SKU = "Eagle-nx",     IsActive = true, CreatedDate = now },
                new Product { Name = "SRAM NX Front derailleur", Description = "Better shifting isnt out there", Price = 920.00m, Stock = 40, SupplierId = sram.Id, ProductCategoryId = compsCat.Id, SKU = "nx-fd",  IsActive = true, CreatedDate = now },
                new Product { Name = "SRAM housing and cables", Description = "price per meter", Price = 60.50m, Stock = 200, SupplierId = sram.Id, ProductCategoryId = compsCat.Id, SKU = "c-h-1", IsActive = true, CreatedDate = now },
                new Product { Name = "Vittoria MEZCAL RACE XC", Description = "Tubeless RacingTyres", Price = 700.00m, Stock = 40, SupplierId = vittoria.Id, ProductCategoryId = compsCat.Id, SKU = "Tyres-vittoria", IsActive = true, CreatedDate = now }
            };
            foreach (var prod in productsToSeed)
            {
                // Only add if a product with the same SKU doesn't already exist
                if (!context.Products.Any(p => p.SKU == prod.SKU))
                {
                    context.Products.Add(prod);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
