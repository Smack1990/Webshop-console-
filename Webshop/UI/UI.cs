using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;
using Webshop.Models;
using Webshop.Services;
using Spectre.Console;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;


namespace Webshop.UI;
internal class UI
{
    private readonly MyDbContext _dbContext;
    private readonly Registration _registration;
    private readonly Logic _logic;
    private readonly Login _login;
    private readonly AdminHandler _adminHandler;
    private readonly StatisticService _statsService;
    private readonly GUI _gui;

    private Customer _currentCustomer;

    public UI(MyDbContext context)
    {
        _dbContext = context;
        _registration = new Registration(context);
        _login = new Login(context);
        _logic = new Logic(context);
        _adminHandler = new AdminHandler(context);
        _statsService = new StatisticService(context);
        _gui = new GUI(context);
    }
    #region start/login
    public async Task Start() //start av program
    {
        Console.CursorVisible = false;
   
        TestDataSeeder.SeedTestDataAsync(_dbContext).Wait(); //Kör dataseeder för att populera databasen



        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
            var choice = await _gui.PromptMenu("Select a alterative", "1.Register, 2.Login, 3.Exit");
            switch (choice)
            {
                case '1':
                    RegistrationAsync().Wait();
                    if (_currentCustomer != null)
                        await RouteAfterLogin();
                    break;
                case '2':
                    HandleLogin().Wait(); ;
                    if (_currentCustomer != null)
                        await RouteAfterLogin();
                    break;
                case '3':
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private async Task RouteAfterLogin() //Hanterar det som sker efter inlogg eller registrering
    {
        while (true)
        {
            if (_currentCustomer.IsAdmin)
            {
                AnsiConsole.Clear();

                await _gui.PrintBanner();
                Console.WriteLine("");
                var choice = await _gui.PromptMenu("Select a alterative", "1.Admin Menu, 2.Webshop, 3.Logout");

                switch (choice)
                {
                    case '1':
                        Console.Clear();
                        await AdminMenu();
                        break;
                    case '2':
                        Console.Clear();
                        await CustomerMenuAsync();
                        break;
                    case '3':
                        _currentCustomer = null;
                        await Start();
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        await Task.Delay(1000);

                        break;
                }

            }
            else
                await CustomerMenuAsync();
        }
    }

    private async Task HandleLogin() //Hanterar inloggningen och 
    {
        AnsiConsole.Clear();
        await _gui.PrintBanner();
        Console.WriteLine("");

        AnsiConsole.MarkupLine("[underline][bold yellow]Login Portal[/][/]");

        var email = AnsiConsole.Prompt(
            new TextPrompt<string>("Email:")
                .PromptStyle("green")
                .Validate(e =>
                    e.Contains("@") //Kontrollerar att emailen innehåller "@"
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Ogiltig e-postadress[/]"))
        );

        //Skriver ut lösenordet dolt med "*"
        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Password:")
                .PromptStyle("yellow")
                .Secret()
        );

        var (customer, message) = await _login.AuthenticateAsync(email, password); //Login autentisering
        if (customer != null)
        {
            _currentCustomer = customer;
            _currentCustomer.Sitevisit++;
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Welcome, {customer.FirstName}!\n");
        }
        else
        {
            Console.WriteLine(message + "\n");
            Task.Delay(1500);
        }
    }

    private async Task RegistrationAsync() //Hanterar registreringen av användare med checkar mot varje inmatning och promptar för att visa att något är fel. 
    {
        AnsiConsole.Clear();
        await _gui.PrintBanner();
        Console.WriteLine("");

        AnsiConsole.MarkupLine("[underline][bold yellow]Registration Portal[/][/]");


        var firstName = AnsiConsole.Prompt(
            new TextPrompt<string>("First Name:")
                .PromptStyle("green")
                .Validate(firstName =>
                    !string.IsNullOrWhiteSpace(firstName)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]First name may not be empty[/]"))
        );


        var lastName = AnsiConsole.Prompt(
            new TextPrompt<string>("Last Name:")
                .PromptStyle("green")
                .Validate(lastname =>
                    !string.IsNullOrWhiteSpace(lastname)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Last name may not be empty[/]"))
        );


        var email = AnsiConsole.Prompt(
    new TextPrompt<string>("Email:")
        .PromptStyle("green")
        .Validate(mail =>
        {

            if (!mail.Contains("@"))
                return ValidationResult.Error("[red]Invalid email address[/]");


            var taken = _registration.CheckIfEmailExistAsync(mail)
                              .GetAwaiter().GetResult();
            if (taken)
                return ValidationResult.Error("[red]That email is already registered[/]");

            return ValidationResult.Success();
        })
        );


        var address = AnsiConsole.Prompt(
            new TextPrompt<string>("Address:")
                .PromptStyle("green")
                .Validate(a =>
                    !string.IsNullOrWhiteSpace(a)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Address may not be empty[/]"))
        );
        var city = AnsiConsole.Prompt(
      new TextPrompt<string>("City:")
          .PromptStyle("green")
          .Validate(a =>
              !string.IsNullOrWhiteSpace(a)
                  ? ValidationResult.Success()
                  : ValidationResult.Error("[red]City may not be empty[/]"))
  );
        var zipcode = AnsiConsole.Prompt(
      new TextPrompt<string>("Zipcode:")
          .PromptStyle("green")
          .Validate(a =>
              !string.IsNullOrWhiteSpace(a)
                  ? ValidationResult.Success()
                  : ValidationResult.Error("[red]Zipcode may not be empty[/]"))
  );


        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Password (min 8 chars):")
                .PromptStyle("yellow")
                .Secret()
                .Validate(pw =>
                    pw.Length >= 8
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Password must be at least 8 characters[/]"))
        );
        var phone = AnsiConsole.Prompt(
            new TextPrompt<string>("Phone number:")
                .PromptStyle("green")
                .Validate(p =>
                    !string.IsNullOrWhiteSpace(p)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Phone number may not be empty[/]"))
        );


        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Address = address,
            City = city,
            ZipCode = zipcode,
            Phone = phone
        };

        var (success, message) = await _registration.RegisterCustomerAsync(customer, password);
        if (success)
        {
            _currentCustomer = customer;
            AnsiConsole.MarkupLine("\n[green]Registration successful! Welcome, [/]" +
                                   $"[bold]{customer.FirstName}[/]!");
            await Task.Delay(800);
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[red]{message}[/]");
            await Task.Delay(1500);


            await RegistrationAsync();
        }
    }
    public async Task ShowLogins() //skriver ut antalet gånger användaren varit inloggad
    {
        Console.SetCursorPosition(98, 6); Console.Write($"Amount of Logins: {_currentCustomer.Sitevisit}");

    }
    #endregion
    #region Customer   
    private async Task CustomerMenuAsync() //huvudmeny för shopen
    {
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            var cartQty = await _logic.NumberOfCartItemsAsync(_currentCustomer.Id);
            Console.SetCursorPosition(98, 4); Console.Write($"Cart: {cartQty} ");
            Console.SetCursorPosition(98, 5); Console.Write($"Welcome, {_currentCustomer.FirstName}!");
            await ShowLogins();
            await DrawFeaturedProducts();

            var key = await _gui.PromptMenu("Select an option", "1. View Categories and products, 2. View Cart, 3. View Orders, 4. Checkout, 5. Logout, 6.Back");

            Console.WriteLine();

            if (_featuredKeyMap.TryGetValue(char.ToLower(key), out var prodId))
            {
                var (ok, msg) = _logic.AddToCartAsync(_currentCustomer.Id, prodId, 1).GetAwaiter().GetResult();
                Console.WriteLine(ok ? $"Added to cart: {msg}" : $"Error: {msg}");
                Console.WriteLine("Press any key to continue…");
                Console.ReadKey(intercept: true);
                continue;
            }



            switch (key)
            {
                case '1':
                    Console.Clear();
                    await ViewCategoriesAsync();
                    break;

                case '2':
                    Console.Clear();

                    await ViewCartAsync();

                    break;

                case '3':
                    Console.Clear();
                    await ViewOrdersAsync();
                    Console.WriteLine("Press any key to return to menu...");
                    Console.ReadKey(intercept: true);
                    break;

                case '4':
                    await Checkout();
                    break;

                case '5':
                    _currentCustomer = null;

                    return;
                case '6' when _currentCustomer.IsAdmin:

                    return;

                default:
                    Console.WriteLine("Invalid choice. Press any key to retry…");
                    Console.ReadKey(intercept: true);
                    break;
            }
        }
    }
    private Dictionary<char, int> _featuredKeyMap;

    private async Task DrawFeaturedProducts()
    {
        var products = _adminHandler.GetAllProducts().Result
            .Where(p => p.IsActive)
            .Take(3)
            .ToList();

        const int colWidth = 40;
        string fp = "Featured Products";
        int width = (Console.WindowWidth - fp.Length) / 2;
        Console.WriteLine("\n" + new string('-', colWidth * 3));
        Console.SetCursorPosition(width, Console.CursorTop); Console.ForegroundColor = ConsoleColor.DarkYellow; Console.WriteLine(fp); Console.ResetColor();

        Console.WriteLine(new string('-', colWidth * 3));

        foreach (var p in products) Console.Write(p.Name.PadRight(colWidth));
        Console.WriteLine();

        foreach (var p in products)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            var desc = p.Description.Length > colWidth - 3
                ? p.Description.Substring(0, colWidth - 3) + "..."
                : p.Description.PadRight(colWidth);
            Console.Write(desc);
            Console.ResetColor();
        }
        Console.WriteLine();

        foreach (var p in products) Console.Write(p.Price.ToString("C").PadRight(colWidth));
        Console.WriteLine();

        _featuredKeyMap = new Dictionary<char, int>();
        char key = 'a';
        foreach (var p in products)
        {
            _featuredKeyMap[key] = p.Id;
            Console.Write($"[{key}] Add to cart  ".PadRight(colWidth));
            key++;
        }
        Console.WriteLine();
       
        string bs = "  Best Selling   ";
        int width2 = (Console.WindowWidth - bs.Length) / 2;
        Console.WriteLine("\n" + new string('-', colWidth * 3));
        Console.SetCursorPosition(width2, Console.CursorTop); Console.ForegroundColor = ConsoleColor.DarkYellow; Console.WriteLine(bs); Console.ResetColor();
        Console.WriteLine(new string('-', colWidth * 3));

        var bestSelling = await _statsService.GetallProductsOrderdBybestsellingAsync(3);

        foreach (var p in bestSelling) Console.Write(p.Name.PadRight(colWidth));
        Console.WriteLine();
        foreach (var p in bestSelling)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            var desc = p.Description.Length > colWidth - 3
                ? p.Description.Substring(0, colWidth - 3) + "..."
                : p.Description.PadRight(colWidth);
            Console.Write(desc);
            Console.ResetColor();
        }
        Console.WriteLine();
        foreach (var p in bestSelling) Console.Write(p.Price.ToString("C").PadRight(colWidth));
        Console.WriteLine();
        _featuredKeyMap = new Dictionary<char, int>();
        char key2 = 'd';
        foreach (var p in bestSelling)
        {
            _featuredKeyMap[key2] = p.Id;
            Console.Write($"[{key2}] Add to cart  ".PadRight(colWidth));
            key2++;
        }

        
    }
    public async Task ViewCategoriesAsync()
    {
        
        Console.CursorVisible = false;

        var categories = _dbContext.ProductCategories.ToList();
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[underline][bold yellow]Product Categories[/][/]");
            Console.WriteLine("");
            for (int i = 0; i < categories.Count; i++)
            {
                Console.Write($"{i + 1}. {categories[i].CategoryName} - {categories[i].Description} ");
                Console.WriteLine("");
            }
            AnsiConsole.MarkupLine("[underline][bold yellow]Choose a category by number or[/][/]");
            Console.WriteLine(new string('-', 75));
            AnsiConsole.MarkupLine("[underline][cyan]b) back[/][/]");
            AnsiConsole.MarkupLine("[underline][cyan]S) Search for product(free text)[/][/]");

           
            var key = Console.ReadKey(intercept: true).KeyChar;

            if (char.ToLower(key) == 's')
            {
                await SearchForProductsByName();
                continue;
            }
            if (char.ToLower(key) == 'b')
                break;

            if (char.IsDigit(key) && (key - '0') is int idx && idx >= 1 && idx <= categories.Count)
            {
                var cat = categories[idx - 1];
                var products = await _adminHandler.GetAllProducts();

                var prod = products
                   .Where(p => p.ProductCategoryId == cat.Id)
                   .ToList();


                while (true)
                {
                    Console.Clear();
                    AnsiConsole.MarkupLine($"[bold yellow]Products in {cat.CategoryName}:[/]\n");
                    AnsiConsole.MarkupLine("[bold yellow]ID   Name               \t    Supplier        \t\t\t      Price            Stock[/]");
                    Console.WriteLine(new string('-', 98));
                    foreach (var p in prod)
                    {
                        string stockDisp = p.Stock >= 1 ? p.Stock.ToString() : "Out of stock";


                        Console.WriteLine(
                                          $"{p.Id,-4} " +
                                          $"{p.Name,-30} " +
                                          $"{p.Supplier.CompanyName,-30} " +
                                          $"{p.Price,20:C} " +
                                          $"{stockDisp,10}"

                                      );
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"  {p.Description}");
                        Console.ResetColor();

                    }
                    AnsiConsole.MarkupLine("[underline][cyan]b) Back[/][/]");
                    AnsiConsole.MarkupLine("[underline][cyan]a) Add to cart[/][/]");

                    var cmd = Console.ReadKey(intercept: true).KeyChar;
                    Console.WriteLine();

                    if (char.ToLower(cmd) == 'b')
                        break;
                    if (char.ToLower(cmd) == 'a')
                    {
                        Console.Write("Enter product ID: ");
                        if (int.TryParse(Console.ReadLine(), out int pid))
                        {
                            Console.Write("Quantity: ");
                            if (int.TryParse(Console.ReadLine(), out int qty))
                            {
                                var (ok, msg) = _logic.AddToCartAsync(_currentCustomer.Id, pid, qty).Result;
                                Console.WriteLine(msg);
                            }
                            else Console.WriteLine("Invalid quantity.");
                        }
                        else Console.WriteLine("Invalid product ID.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(intercept: true);
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice, press any key to retry...");
                        Console.ReadKey(intercept: true);
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid category selection, press any key to retry...");
                Console.ReadKey(intercept: true);
            }
        }


        Console.CursorVisible = true;
    }
    public async Task<decimal> GetCartDetailsAsync()
    {
        var customer = await _dbContext.Customers
                   .Include(c => c.Cart)
                   .ThenInclude(c => c.Items)
                   .ThenInclude(i => i.Product)
                   .FirstOrDefaultAsync(c => c.Id == _currentCustomer.Id);



        if (customer?.Cart?.Items == null || !customer.Cart.Items.Any())
        {
            Console.WriteLine("Your cart is empty.\n");
            Console.WriteLine("Press any key to return to the menu...");
            Console.ReadKey(true);
            return 0m;
        }





        Console.Clear();
        AnsiConsole.MarkupLine("[underline][bold yellow]Your Cart[/][/]\n");
        Console.WriteLine("ID\t Product     Qty \t\tUnit Price \t   Total");
        Console.WriteLine(new string('-', 98));

        decimal total = 0;
        foreach (var item in customer.Cart.Items)
        {
            var name = item.Product?.Name ?? "Unknown";
            var unitPrice = item.Product?.Price ?? 0;
            var lineTotal = item.Quantity * unitPrice;
            total += lineTotal;

            Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"{item.Product.Id,-10}{name,-10} {item.Quantity,-15}   {unitPrice,10:C}   {lineTotal,14:C}"); Console.ResetColor();
        }
        decimal tax = total * 0.25m;

        Console.WriteLine(new string('-', 98));
        AnsiConsole.MarkupLine($"[bold yellow]{total:C}\tTax: {tax:C}[/]");
        Console.WriteLine(new string('-', 98));
        Console.WriteLine("Press d, to delete you shopping cart. Enter to continue checkout");
        return total;

    }
    public async Task<Customer?> GetCustomerCartAsync(int customerId)
    {
        return await _dbContext.Customers
            .Include(c => c.Cart)
                .ThenInclude(cart => cart.Items)
                    .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }
    private async Task ViewOrdersAsync()// Lägg till search for order by date. 
    {

        var orders = await _logic.GetCustomerOrdersAsync(_currentCustomer.Id);
        Console.WriteLine("Your Orders");
        Console.WriteLine(new string('-', 12));
        foreach (var order in orders)
        {
            Console.WriteLine(new string('-', 98));
            decimal netAmount = order.TotalAmount - order.FreightPrice;
            decimal taxPrice = Math.Round(netAmount * 0.25m, 2);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nOrder ID  : {order.Id}");
            Console.WriteLine($"Order date: {order.OrderDate}");
            Console.WriteLine($"Shipment    : {order.ShipmentMethod}");
            Console.WriteLine($"Ship adress : {order.ShippingAddress}");
            Console.WriteLine($"Ship Zip    : {order.zipCode}");
            Console.WriteLine($"Ship City   : {order.City}");
            Console.WriteLine($"inv address : {order.invoiceAddress}");
            Console.WriteLine($"inv Zip     : {order.invoicezipCode}");
            Console.WriteLine($"inv City    : {order.invoiceCity}");
            Console.WriteLine($"Payment     : {order.PaymentMethod} ({order.PaymentInfo})");
            Console.WriteLine($"Freight     : {order.FreightPrice:C}");
            Console.WriteLine($"Phone       : {order.PhoneNumber}");
            Console.WriteLine($"TotalPrice  : {order.TotalAmount:C}");
            Console.WriteLine($"VAT         : {taxPrice:C}");
            
            Console.ResetColor();
            

            foreach (var item in order.OrderItems)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Product ID: {item.ProductId}, Product Name: {item.Product.Name}, Quantity: {item.Quantity}, Unit Price: {item.UnitPrice}, ");
                Console.ResetColor();

            }
            Console.WriteLine(new string('-', 75));



        }
    }


    public async Task Checkout()
    {
        Console.CursorVisible = false;


        decimal price = await GetCartDetailsAsync();
        if (price == 0)
            return;
        await DeleteCart();


        var shipPick = await _gui.PromptMenu(
            "Select shipment method",
            "1. Collect in store, 2. Home delivery, 3. Back"
        );
        if (shipPick == '3')
            return;

        string shipmentMethod;
        int freightPrice;
        string shippingAddress;
        string phoneNumber;
        int shippingZipCode = 0;
        string shippingCity = "";

        if (shipPick == '1')
        {

            shipmentMethod = "Collect in store";
            freightPrice = 0;

            Console.Write("Enter phone number for notification: ");
            phoneNumber = Console.ReadLine()!;
            shippingAddress = "Store";
        }
        else
        {
            shipmentMethod = "Home delivery";


            var delPick = await _gui.PromptMenu(
                "Select a delivery option",
                "1. DHL (1-3 days) | Price: 99kr, " +
                "2. FedEx (2-4 days) | Price: 49kr, " +
                "3. UPS (1-2 days) | Price: 159kr, " +
                "4. Back"
            );
            if (delPick == '4')
                return;


            freightPrice = delPick switch
            {
                '1' => 99,
                '2' => 49,
                '3' => 159,
                _ => throw new InvalidOperationException($"Ogiltigt val: {delPick}")
            };

            var courierName = delPick switch
            {
                '1' => "DHL (1-3 days)",
                '2' => "FedEx (2-4 days)",
                '3' => "UPS (1-2 days)",
                _ => ""
            };
            shipmentMethod = courierName;


            Console.Write("Enter shipping address: ");
            shippingAddress = Console.ReadLine()!;
            Console.Write("Enter shipping zipCode: ");
            shippingZipCode = int.TryParse(Console.ReadLine(), out shippingZipCode) ? shippingZipCode : 0;
            Console.Write("Enter shipping City: ");
            shippingCity = Console.ReadLine()!;
            Console.Write("Enter phone number: ");
            phoneNumber = Console.ReadLine()!;
        }


        var payPick = await _gui.PromptMenu(
            "Select payment method",
            "1. Swish, 2. Credit card, 3. Invoice (30 days), 4. In store, 5. Back"
        );
        if (payPick == '4')
            return;

        var paymentMethod = payPick switch
        {
            '1' => "Swish",
            '2' => "Credit card",
            '3' => "Invoice (30 days)",
            '4' => "In store",
            _ => throw new InvalidOperationException($"Ogiltigt val: {payPick}")
        };


        string paymentInfo = "";
        int invoiceZipCode = 0;
        string invoiceAddress = "";
        string invoiceCity = "";
        if (payPick == '1')
        {
            Console.Write("Enter Swish number: ");
            paymentInfo = Console.ReadLine()!;
        }
        else if (payPick == '2')
        {
            Console.Write("Enter credit card number: ");
            paymentInfo = Console.ReadLine()!;
        }
        else
        {
            Console.WriteLine("Is your invoice address the same as shipping address? Y/N");
            var answer = Console.ReadKey(true).KeyChar;
            if (char.ToLower(answer) == 'y')
            {
                paymentInfo = shippingAddress;
                invoiceAddress = shippingAddress;
                invoiceZipCode = shippingZipCode;
                invoiceCity = shippingCity;

            }
            else
            {

                Console.WriteLine("Enter invoice address:");
                paymentInfo = Console.ReadLine()!;
            }

        }
        decimal total = price + freightPrice;
        Console.WriteLine();

        var order = await _logic.Checkout(
            _currentCustomer.Id,
            shippingAddress,
            shippingZipCode,
            shippingCity,
            invoiceAddress,
            invoiceZipCode,
            invoiceCity,
            paymentMethod,
            shipmentMethod,
            phoneNumber,
            paymentInfo,
            total,
            freightPrice
        );

        // 4) Visa resultatet
        Console.CursorVisible = true;
        if (order != null)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nOrder placed! ID: {order.Id}");
            Console.WriteLine($"Shipment    : {shipmentMethod}");
            Console.WriteLine($"Ship adress : {shippingAddress}");
            Console.WriteLine($"Ship Zip    : {shippingZipCode}");
            Console.WriteLine($"Ship City   : {shippingCity}");
            Console.WriteLine($"inv address : {invoiceAddress}");
            Console.WriteLine($"inv address : {invoiceZipCode}");
            Console.WriteLine($"inv address : {invoiceCity}");
            Console.WriteLine($"Payment     : {paymentMethod} ({paymentInfo})");
            Console.WriteLine($"Freight     : {freightPrice:C}");

            Console.WriteLine($"Phone       : {phoneNumber}");
            Console.WriteLine($"TotalPrice  : {total:C}");
            Console.ResetColor();
            Console.WriteLine("\nThank you for your order! Press any key to continue…");
        }
        else
        {
            Console.WriteLine("\nCheckout failed. Please try again. Press any key…");
        }
        Console.ReadKey(true);
    }

    private async Task DeleteCart()
    {

        var keyInfo = Console.ReadKey(intercept: true);

        if (char.ToLower(keyInfo.KeyChar) == 'd')
        {
            var result = await _logic.EmptyCart(_currentCustomer.Id);
            Console.WriteLine(result != null
                ? "Cart emptied successfully."
                : "Could not empty cart.");
            await Task.Delay(2000);
            return;
        }
    }

  

    private async Task ViewCartAsync()
    {
        var customer = await GetCustomerCartAsync(_currentCustomer.Id);
        

        if (customer?.Cart?.Items == null || !customer.Cart.Items.Any())
        {
            Console.WriteLine("Your cart is empty.\n");
            Console.WriteLine("Press any key to return to the menu...");
            Console.ReadKey(true);
            return;
        }

        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[underline][bold yellow]Your Cart[/][/]\n");
            Console.WriteLine("ID\t Product     Qty \t\tUnit Price \t   Total");
            Console.WriteLine(new string('-', 98));

            decimal total = 0;
            foreach (var item in customer.Cart.Items)
            {
                var name = item.Product?.Name ?? "Unknown";
                var unitPrice = item.Product?.Price ?? 0;
                var lineTotal = item.Quantity * unitPrice;
                total += lineTotal;

                Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"{item.Product.Id,-10}{name,-10} {item.Quantity,-15}   {unitPrice,10:C}   {lineTotal,14:C}"); Console.ResetColor();
            }

            Console.WriteLine(new string('-', 98));
            AnsiConsole.MarkupLine($"[bold yellow]{total:C}[/]");
            Console.WriteLine(new string('-', 98));
            AnsiConsole.MarkupLine("[underline][bold yellow]Options:[/][/]");
            AnsiConsole.MarkupLine("[cyan]c) Change quantity\nd) Empty cart \nb) back[/]");



            var key = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine();

            switch (char.ToLower(key))
            {
                case 'b':

                    return;

                case 'd':
                    var emptied = await _logic.EmptyCart(_currentCustomer.Id);
                    Console.WriteLine(emptied != null
                        ? "Cart emptied successfully.\n"
                        : "Could not empty cart.\n");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    return;

                case 'c':
                    Console.Write("Enter product ID to update: ");
                    if (!int.TryParse(Console.ReadLine(), out int pid))
                    {
                        Console.WriteLine("Invalid product ID.\n");
                        Console.ReadKey(true);
                        break;
                    }

                    Console.Write("Enter new quantity (0 to remove): ");
                    if (!int.TryParse(Console.ReadLine(), out int newQty))
                    {
                        Console.WriteLine("Invalid quantity.\n");
                        Console.ReadKey(true);
                        break;
                    }

                    var (success, message) = await _logic.ChangeCartProductQuantityAsync(
                        _currentCustomer.Id, pid, newQty);
                    Console.WriteLine(message + "\n");
                    Console.WriteLine("Press any key to refresh cart...");
                    Console.ReadKey(true);
                    break;

                default:
                    Console.WriteLine("Invalid choice.\n");
                    Console.WriteLine("Press any key to retry...");
                    Console.ReadKey(true);
                    break;
            }


            customer = await _dbContext.Customers
                .Include(c => c.Cart).ThenInclude(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == _currentCustomer.Id);
        }
    }


    public async Task SearchForProductsByName()
    {
        Console.CursorVisible = false;
        Console.Clear();

    
        Console.Write("Search: ");
        var id = Console.ReadLine();

      
        var results = await _adminHandler.GetProductByName(id);
        if (!results.Any())
        {
            AnsiConsole.MarkupLine("[red]No products found.[/]");
            Console.WriteLine("Press any key to go back...");
            Console.ReadKey(true);
            return;
        }

        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[underline][bold yellow]Search results for \"{id}\": [/][/]\n");
            AnsiConsole.MarkupLine("[bold yellow]ID   Name               \t    Supplier        \t\t\t      Price            Stock[/]");
            Console.WriteLine(new string('-', 98));
            foreach (var p in results)
            {
                Console.WriteLine(
                                  $"{p.Id,-4} " +
                                  $"{p.Name,-30} " +
                                  $"{p.Supplier.CompanyName,-30} " +
                                  $"{p.Price,20:C} " +
                                  $"{p.Stock,10}"
                              );
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"  {p.Description}");
                Console.ResetColor();
            }
            Console.WriteLine();

            
            AnsiConsole.MarkupLine("[underline][cyan]b) Back[/][/]");
            AnsiConsole.MarkupLine("[underline][cyan]a) Add to cart[/][/]");

            var cmd = Console.ReadKey(intercept: true).KeyChar;

            switch (char.ToLower(cmd))
            {
                case 'a':
                    
                    Console.Write("Enter product ID: ");
                    if (int.TryParse(Console.ReadLine(), out var pid) &&
                        results.Any(p => p.Id == pid))
                    {
                        Console.Write("Quantity: ");
                        if (int.TryParse(Console.ReadLine(), out var qty))
                        {
                            var (ok, msg) = await _logic.AddToCartAsync(
                                _currentCustomer.Id, pid, qty
                            );
                            AnsiConsole.MarkupLine(ok
                                ? $"[green]{msg}[/]"
                                : $"[red]{msg}[/]");
                            await Task.Delay(800);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]Invalid quantity[/]");
                            await Task.Delay(800);
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Invalid product ID[/]");
                        await Task.Delay(800);
                    }
                    break;

                case 'b':
                    
                    return;

                default:
                    AnsiConsole.MarkupLine("[red]Invalid action[/]");
                    await Task.Delay(800);
                    break;
            }
        }
    }

    #endregion
    #region Adminmenu
    private async Task AdminMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
            var key = await _gui.PromptMenu("Select a alterative", "1.Add Product, 2.Edit Product, 3.Delete Product, 4.Manage Categories, 5.Manage Suppliers, 6.Manage Custumers, 7.Statistics, 8.Back");
      
            switch (key)
            {
                case '1':
                    await AddProduct();
                    break;
                case '2':
                    await EditProduct();
                    break;
                case '3':
                    await DeleteProduct();
                    break;
                case '4':
                    await ManageCategories();
                    break;
                case '5':
                    await ManageSuppliers();
                    break;
                case '6':
                    await ManageCustomers();
                    break;
                case '7':

                    await ViewStatsticsMenu();
                    break;
                case '8':
                    if (_currentCustomer.IsAdmin)
                        await RouteAfterLogin();

                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }

    private async Task ManageCustomers()
    {
        Console.CursorVisible = false;
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
            var key = await _gui.PromptMenu("Select an alterative", "1.List all customers, 2.View customer by ID, 3.View customer by Email, 4.Update customer, 5.Delete customer, 6.Toggle admin rights, 7.View customer orders, 8.Back");
   

            
            Console.WriteLine();
            switch (char.ToLower(key))
            {
                case '1':
                    // List all
                    await GetAllCustomers();

                    break;

                case '2':
                    Console.Clear();
                    await GetAllCustomers();
                    Console.Write("Enter customer ID: ");
                    if (int.TryParse(Console.ReadLine(), out int idLookUp))
                    {
                        var c2 = _adminHandler.GetCustomerById(idLookUp).GetAwaiter().GetResult();
                        if (c2 != null)
                        {
                            Console.WriteLine($"{c2.Id}: {c2.FirstName} {c2.LastName} – {c2.Email} - {c2.Phone} - {c2.Address} (Admin: {c2.IsAdmin})");

                        }
                        else
                            Console.WriteLine("Not found.");

                    }
                    break;

                case '3':
                    Console.Clear();
                    Console.Write("Enter email: ");
                    var email = Console.ReadLine();
                    var c3 = _adminHandler.GetCustomerByEmail(email).GetAwaiter().GetResult();
                    if (c3 != null)
                    {
                        Console.WriteLine($"{c3.Id}: {c3.FirstName} {c3.LastName} – {c3.Email} - {c3.Phone} - {c3.Address} (Admin: {c3.IsAdmin})");

                    }
                    else
                        Console.WriteLine("Not found.");
                    break;

                case '4':
                    Console.Clear();
                    Console.WriteLine("Update customer");

                    await GetAllCustomers();
                    if (CancelMethod())
                        break;
                    Console.Write("Enter ID to update: ");
                    if (int.TryParse(Console.ReadLine(), out int userId))
                    {
                        var custId = _adminHandler.GetCustomerById(userId).GetAwaiter().GetResult();
                        if (custId != null)
                        {
                            Console.Write($"First Name ({custId.FirstName}): ");
                            var firstName = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(firstName)) custId.FirstName = firstName;

                            Console.Write($"Last Name ({custId.LastName}): ");
                            var lastName = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(lastName)) custId.LastName = lastName;

                            Console.Write($"Email ({custId.Email}): ");
                            var newEmail = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(newEmail)) custId.Email = newEmail;

                            Console.Write($"Address ({custId.Address}): ");
                            var address = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(address)) custId.Address = address;
                            Console.Write($"Phone ({custId.Phone}): ");
                            var phone = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(phone)) custId.Phone = phone;

                            var (Success, M) = _adminHandler.UpdateCustomer(custId).Result;
                            Console.WriteLine(M);
                        }
                        else Console.WriteLine("Not found.");
                    }
                    break;

                case '5':
                    Console.Clear();
                    Console.WriteLine("Delete customer");
                    await GetAllCustomers();
                    if (CancelMethod())
                        break;
                    Console.Write("Enter ID to delete: ");
                    if (int.TryParse(Console.ReadLine(), out int deleteId))
                    {
                        await _adminHandler.DeleteCustomer(deleteId);
                        Console.WriteLine("Customer deleted.");
                    }
                    break;

                case '6':
                    Console.Write("Enter ID to toggle admin: ");
                    if (int.TryParse(Console.ReadLine(), out int AdminId))
                    {
                        var (ok, m) = await _adminHandler.AdminHandelingOnID(AdminId);
                        Console.WriteLine(m);
                    }
                    break;

                case '7':
                    Console.Write("Enter ID to view orders: ");
                    if (int.TryParse(Console.ReadLine(), out int orderId))
                    {
                        await GetAllCustomers();
                        await _adminHandler.GetOrdersFromId(orderId);


                    }
                    break;


                case '8':
                    Console.CursorVisible = true;
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(intercept: true);
        }
    }

    private async Task ViewStatsticsMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
           var key = await _gui.PromptMenu("Select an alterative", "1.View best selling products, 2.View most popular categories, 3.View most popular payment method, 4.View sales by supplier, 5.View most sitevisits by customer, 6.Back");
   
            switch (key)
            {
                case '1':
                    Console.Clear();
                    Console.WriteLine("How many products do you want to view?");
                    int.TryParse(Console.ReadLine(), out int number);
                    var restult = await _statsService.GetBestSellingProductsAsync(number);
                    foreach (var res in restult)
                    {
                        Console.WriteLine($"Product: {res.Product.Name}, Quantity Sold: {res.Product.QuantitySold}");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    break;
                case '2':
                    Console.Clear();
                    var result = await _statsService.MostPopularCategoryAsync();
                    foreach (var res in result)
                    {
                        Console.WriteLine($"Category: {res.ProductCategory,-15} Number of Orders: {res.OrderCount,2}");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    break;
                case '3':
                    Console.Clear();
                    var result2 = await _statsService.MostPopularPaymentMethodAsync();
                    foreach (var res in result2)
                    {
                        Console.WriteLine($"Payment Method: {res.PaymentMethod,-20} Number of Orders: {res.OrderCount,2}");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    break;
                case '4':
                    Console.Clear();
                    var result3 = await _statsService.GetSalesBySupplierAsync();
                    foreach (var res in result3)
                    {
                        Console.WriteLine($"Supplier: {res.Supplier.CompanyName,-15} Total Sales: {res.TotalSales,2}");
                    }

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);

                    break;
                case '5':
                    Console.Clear();
                    var result4 = await _statsService.GetTopSiteVisitorsAsync();
                    foreach (var res in result4)
                    {
                        Console.WriteLine($"Customer: {res.Customer.FirstName} {res.Customer.LastName,-20}, Site Visits: {res.Visits,2}");
                    }
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    break;
                case '6':
                    return;
                default:
                    Console.WriteLine("Invalic choice. Press any key and retry.");
                    Console.ReadKey(intercept: true);
                    break;

            }
        }

    }

    private async Task GetAllCustomers()
    {
        Console.Clear();
        var allCo = await _adminHandler.GetAllCustomers();
        Console.WriteLine("\nAll Customers:");
        allCo.ForEach(c =>
            Console.WriteLine($"{c.Id}: {c.FirstName} {c.LastName} – {c.Email} (Admin: {c.IsAdmin})"));
    }

    private async Task ManageSuppliers()
    {
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
            var input  = await _gui.PromptMenu("Select an alterative", "1.Add Supplier, 2.Edit Supplier, 3.Delete Supplier, 4.List Suppliers, 6.Back");

           
            Console.WriteLine();
            switch (input)
            {
                case '1':
                    Console.Clear();
                    Console.WriteLine("Add a new supplier");
                    if (CancelMethod())
                        break;
                    var supplier = new Supplier();
                    Console.Write("Company Name: ");
                    supplier.CompanyName = Console.ReadLine();
                    Console.Write("Email: ");
                    supplier.Email = Console.ReadLine();
                    Console.Write("Phone Number: ");
                    supplier.PhoneNumber = Console.ReadLine();
                    Console.Write("Address: ");
                    supplier.Address = Console.ReadLine();
                    var addSupplier = await _adminHandler.AddSupplier(supplier);
                    Console.WriteLine(addSupplier.Message);

                    break;
                case '2':
                    Console.Clear();
                    Console.WriteLine("Edit a supplier");
                    if (CancelMethod())
                        break;
                    var result = _adminHandler.GetAllSuppliers().GetAwaiter().GetResult();
                    result.ForEach(s => Console.WriteLine($"  {s.Id}. {s.CompanyName}"));
                    Console.Write("ID to edit: ");
                    if (!int.TryParse(Console.ReadLine(), out int sid)) break;
                    var update = new Supplier { Id = sid };
                    Console.Write("New Company Name: ");
                    update.CompanyName = Console.ReadLine();
                    Console.Write("New Email: ");
                    update.Email = Console.ReadLine();
                    Console.Write("New Phone Number: ");
                    update.PhoneNumber = Console.ReadLine();
                    Console.Write("New Address: ");
                    update.Address = Console.ReadLine();
                    var updateSupplier = await _adminHandler.UpdateSuppliers(update);
                    Console.WriteLine(updateSupplier.message);

                    break;
                case '3':
                    Console.Clear();
                    Console.WriteLine("Delete a supplier");
                    if (CancelMethod())
                        break;
                    var res = _adminHandler.GetAllSuppliers().GetAwaiter().GetResult();
                    res.ForEach(s => Console.WriteLine($"  {s.Id}. {s.CompanyName}"));
                    Console.Write("ID to delete: ");
                    if (!int.TryParse(Console.ReadLine(), out int did)) break;
                    var dres = await _adminHandler.DeleteSupplier(did);
                    Console.WriteLine(dres.message);
                    break;
                case '4':
                    var suppliers = _adminHandler.GetAllSuppliers().GetAwaiter().GetResult();
                    suppliers.ForEach(s => Console.WriteLine($"  {s.Id}. {s.CompanyName} – {s.Email}, {s.PhoneNumber}"));
                    break;
                case '6':
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }
    }

    private bool CancelMethod()
    {
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("Press ESC to cancel or any key to proceed");
        var keyInfo = Console.ReadKey(intercept: true);
        Console.WriteLine();
        return keyInfo.Key == ConsoleKey.Escape;
    }

    private async Task ManageCategories()
    {
        while (true)
        {
            AnsiConsole.Clear();
            await _gui.PrintBanner();
            Console.WriteLine("");
            var choice = await _gui.PromptMenu("Select an alterative", "1.Add Category, 2.Edit Category, 3.List Categories, 4.Delete Category, 5.Back");
     

           
            Console.WriteLine();
            switch (choice)
            {
                case '1':
                    Console.Clear();
                    Console.WriteLine("Add a new category");
                    if (CancelMethod())
                        break;
                    Console.Write("Name: ");
                    var name = Console.ReadLine();
                    Console.Write("Description: ");
                    var description = Console.ReadLine();
                    var addCategory = _adminHandler.AddProductCategory(new ProductCategory { CategoryName = name, Description = description });
                    Console.WriteLine(addCategory.message);
                    break;
                case '2':
                    Console.Clear();
                    Console.WriteLine("Edit a category");
                    if (CancelMethod())
                        break;

                    var categories = await _adminHandler.GetAllProductCategories();
                    categories.ForEach(c => Console.WriteLine($"  {c.Id}. {c.CategoryName}"));
                    Console.Write("Category ID to edit: ");
                    if (!int.TryParse(Console.ReadLine(), out int cid)) break;
                    Console.Write("New Name: ");
                    name = Console.ReadLine();
                    Console.Write("New Description: ");
                    description = Console.ReadLine();
                    var updateCategory = _adminHandler.UpdateProductCategory(new ProductCategory { Id = cid, CategoryName = name, Description = description });
                    Console.WriteLine(updateCategory.message);
                    break;
                case '3':
                    var result = await _adminHandler.GetAllProductCategories();
                    foreach (var c in result)
                        Console.WriteLine($"  {c.Id}. {c.CategoryName} – {c.Description}");


                    break;
                case '4':
                    var categories1 = await _adminHandler.GetAllProductCategories();
                    categories1.ForEach(c => Console.WriteLine($"  {c.Id}. {c.CategoryName}"));
                    Console.Write("Category ID to Delete: ");
                    if (!int.TryParse(Console.ReadLine(), out int cid1)) break;
                    var delRes = await _adminHandler.DeleteCategory(cid1);
                    Console.WriteLine(delRes.message);
                    break;
                case '5':
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }

    private async Task DeleteProduct()
    {
        Console.Clear();
        Console.WriteLine("Delete Product:\n");
        Console.Write("Enter Product ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
            return;

        var (success, message) = await _adminHandler.DeleteProduct(id);
        Console.WriteLine(message);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }


    private async Task EditProduct()
    {
        Console.Clear();
        Console.WriteLine("Edit Product");

        if (CancelMethod())
            return;

        var products = await _adminHandler.GetAllProducts();
        products.ForEach(p => Console.WriteLine($"{p.Id} - {p.Name}"));
        Console.Write("Enter Product ID: ");
        if (!int.TryParse(Console.ReadLine(), out var productId))
        {
            Console.WriteLine("Invalid product ID.");
            Console.ReadKey(true);
            return;
        }

        var prod = await _adminHandler.GetProductAsync(productId);
        if (prod == null)
        {
            Console.WriteLine("Product not found.");
            Console.ReadKey(true);
            return;
        }

       var key = await _gui.PromptMenu("Select an alternative", "1. Name, 2. Description, 3. Price, 4. Stock, 5. Is Active, 6. Category, 7. Supplier, 8. SKU, 9. Update all fields, b. Back");

     
        Console.WriteLine();

        string input;
        switch (key)
        {
            case '1':
                if (CancelMethod())
                    break;
                Console.Write($"New Name (current: {prod.Name}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.Name = input;
                break;

            case '2':
                if (CancelMethod())
                    break;
                Console.Write($"New Description (current: {prod.Description}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.Description = input;
                break;

            case '3':
                if (CancelMethod())
                    break;
                Console.Write($"New Price (current: {prod.Price}): ");
                input = Console.ReadLine();
                if (decimal.TryParse(input, out var newPrice)) prod.Price = newPrice;
                break;

            case '4':
                if (CancelMethod())
                    break;
                Console.Write($"New Stock (current: {prod.Stock}): ");
                input = Console.ReadLine();
                if (int.TryParse(input, out var newStock)) prod.Stock = newStock;
                break;

            case '5':
                if (CancelMethod())
                    break;
                Console.Write($"Is Active? (current: {prod.IsActive}) [y/n]: ");
                var keyYorN = Console.ReadKey(intercept: true).KeyChar;
                prod.IsActive = (keyYorN == 'y' || keyYorN == 'Y');
                Console.WriteLine();
                break;
            case '6':
                var cat = await _adminHandler.GetAllProductCategories();
                Console.WriteLine("\nCategories");
                cat.ForEach(c => Console.WriteLine($"{c.Id} - {c.CategoryName}"));

                Console.Write("Choose category ID: ");
                prod.ProductCategoryId = int.Parse(Console.ReadLine());
                break;
            case '7':
                if (CancelMethod())
                    break;
                var currentName = prod.Supplier?.CompanyName ?? "[ingen leverantör]";
                var suppliers = await _adminHandler.GetAllSuppliers();
                Console.WriteLine("\nSuppliers");
                suppliers.ForEach(s => Console.WriteLine($"{s.Id} - {s.CompanyName}"));
                Console.Write("Choose supplier ID: ");
                prod.SupplierId = int.Parse(Console.ReadLine());
                break;
            case '8':
                if (CancelMethod())
                    break;
                Console.Write($"New SKU (Current: {prod.SKU}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.SKU = input;
                break;
            case '9':
                if (CancelMethod())
                    break;
                Console.Write($"Name ({prod.Name}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.Name = input;

                Console.Write($"Description ({prod.Description}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.Description = input;

                Console.Write($"Price ({prod.Price}): ");
                input = Console.ReadLine();
                if (decimal.TryParse(input, out newPrice)) prod.Price = newPrice;

                Console.Write($"Stock (Current: {prod.Stock}): ");
                input = Console.ReadLine();
                if (int.TryParse(input, out newStock)) prod.Stock = newStock;

                Console.Write($"Is Active? ({prod.IsActive}) [y/n]: ");
                key = Console.ReadKey(intercept: true).KeyChar;
                prod.IsActive = (key == 'y' || key == 'Y');


                Console.Write($"New Description (current: {prod.Supplier.CompanyName}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.Supplier.CompanyName = input;
                Console.Write($"New SKU (Current: {prod.SKU}): ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) prod.SKU = input;
                break;

            case 'b':

                return;

            default:
                Console.WriteLine("Invalid choice. Press any key to continue...");
                Console.ReadKey(true);
                return;
        }

        // 3) Save changes
        var (success, message) = await _adminHandler.UpdateProductAsync(prod);
        Console.WriteLine(message);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }

    private async Task AddProduct()
    {
        Console.Clear();
        Console.WriteLine("Add Product");
        if (CancelMethod())
            return;
        var prod = new Product();
        Console.WriteLine("Add a new product ");
        Console.WriteLine(new string('-', 25));
        Console.Write("Name: ");
        prod.Name = Console.ReadLine();
        if(string.IsNullOrWhiteSpace(prod.Name))
        {
            Console.WriteLine("Name cannot be empty.");
            return;
        }
        Console.Write("Description: ");
        prod.Description = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(prod.Description))
        {
            Console.WriteLine("Description cannot be empty.");
            return;
        }
        Console.Write("Price: ");
        prod.Price = decimal.Parse(Console.ReadLine());
        if (prod.Price <= 0)
        {
            Console.WriteLine("Price must be greater than 0.");
            return;
        }
        Console.Write("Stock: ");
        prod.Stock = int.Parse(Console.ReadLine());
        if (prod.Stock < 0)
        {
            Console.WriteLine("Stock cannot be negative.");
            return;
        }

        var cat = await _adminHandler.GetAllProductCategories();
        Console.WriteLine("\nCategories");
        cat.ForEach(c => Console.WriteLine($"{c.Id} - {c.CategoryName}"));

        Console.Write("Choose category ID: ");
        prod.ProductCategoryId = int.Parse(Console.ReadLine());

        var suppliers = await _adminHandler.GetAllSuppliers();
        Console.WriteLine("\nSuppliers");
        suppliers.ForEach(s => Console.WriteLine($"{s.Id} - {s.CompanyName}"));
        Console.Write("Choose supplier ID: ");
        prod.SupplierId = int.Parse(Console.ReadLine());

        Console.Write("Is Active? (y/n): ");
        prod.IsActive = Console.ReadKey(intercept: true).KeyChar is 'y' or 'Y';
        Console.WriteLine("\n");
        Console.Write("SKU:");
        prod.SKU = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(prod.SKU))
        {
            Console.WriteLine("SKU cannot be empty.");
            return;
        }


        var (success, message) = await _adminHandler.AddProductAsync(prod);
        Console.WriteLine(message);
        Console.WriteLine("Press any key to continue");
        Console.ReadKey(true);



    }
    #endregion

}