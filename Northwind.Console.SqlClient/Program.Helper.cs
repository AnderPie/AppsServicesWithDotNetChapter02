/*
 * Configures console and assists with printing colored text
 */

using System.Globalization; // To use CultureInfo
partial class Program
{
    private static void ConfigureConsole(string culture = "en-US", bool useComputerCulture = true){
        OutputEncoding = System.Text.Encoding.UTF8; // To use Unicode characters like Euro symbol
        if(!useComputerCulture){
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture); // Look up culture with culture string
        }
        WriteLine($"CurrentCulture: {CultureInfo.CurrentCulture.DisplayName}");
    }

    private static void WriteLineInColor(string value, ConsoleColor color = ConsoleColor.DarkYellow){
        ConsoleColor previousColor = ForegroundColor;
        ForegroundColor = color;
        WriteLine(value);
        ForegroundColor = previousColor;
    }
}