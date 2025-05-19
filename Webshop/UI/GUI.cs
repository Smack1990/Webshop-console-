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
    public async Task DrawFrame(IEnumerable<string> lines, int width)
    {
        // Top border
        Console.WriteLine("╔" + new string('═', width - 2) + "╗");

        foreach (var line in lines)
        {
            // Trim or pad
            var text = line.Length > width - 4
              ? line.Substring(0, width - 4)
              : line.PadRight(width - 4);
            Console.WriteLine("║ " + text + " ║");
        }

        // Bottom border
        Console.WriteLine("╚" + new string('═', width - 2) + "╝");
    }
    public async Task PrintBanner()
    {
        const string banner = @"
  ____  _ _             _                 
 | __ )(_) | _____  ___| |__   ___  _ __  
 |  _ \| | |/ / _ \/ __| '_ \ / _ \| '_ \ 
 | |_) | |   <  __/\__ \ | | | (_) | |_) |
 |____/|_|_|\_\___||___/_| |_|\___/| .__/ 
 ----------------------------------|_|---";
        Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write(banner);Console.ResetColor();
    }

    public async Task<char> PromptMenu(string title, string commaSeparatedChoices)
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

        
        return string.IsNullOrEmpty(selected)
            ? '\0'
            : selected[0];
    }}

