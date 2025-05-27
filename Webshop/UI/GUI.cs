using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webshop.Data;

namespace Webshop.UI;
internal class GUI
{
    private readonly MyDbContext _dbContext;
    public GUI(MyDbContext context)
    {
        _dbContext = context;
    }

    public  Task PrintBanner() //skriver ut loggan
    {
        const string banner = @"
  ____  _ _             _                 
 | __ )(_) | _____  ___| |__   ___  _ __  
 |  _ \| | |/ / _ \/ __| '_ \ / _ \| '_ \ 
 | |_) | |   <  __/\__ \ | | | (_) | |_) |
 |____/|_|_|\_\___||___/_| |_|\___/| .__/ 
 ----------------------------------|_|---";
        Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write(banner);Console.ResetColor();
        return Task.CompletedTask;
    }

    public Task<char> PromptMenu(string title, string commaSeparatedChoices) //metod för att prompta switchmenyer
    {

        var choices = commaSeparatedChoices
             .Split(',', StringSplitOptions.RemoveEmptyEntries)
             .Select(x => x.Trim())
             .ToArray();

        var prompt = new SelectionPrompt<string>()
            .Title($"[underline][bold yellow]{title}[/][/]")
            .PageSize(choices.Length)
            .HighlightStyle(Style.Parse("cyan"))
            .AddChoices(choices);


        var selected = AnsiConsole.Prompt(prompt);

        char result = string.IsNullOrEmpty(selected)
            ? '\0'
            : selected[0];

        return Task.FromResult(result);
    }
}

