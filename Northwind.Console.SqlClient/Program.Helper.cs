/*
 * Configures console and assists with printing colored text
 */

using System.Globalization;
using Microsoft.Data.SqlClient; // To use CultureInfo
using System.Collections; // To use IDictionary
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

    // Outputs various statistics that are tracked by ADO.NET connection
    private static void OutPutStatistics(SqlConnection connection){
        // Remove all the string values to see all the statistics
        string[] includeKeys = {"BytesSent", "BytesReceived", "ConnectionTime","SelectRows"};
        IDictionary statistics = connection.RetrieveStatistics();
        foreach(object? key in statistics.Keys){
            if(!includeKeys.Any() || includeKeys.Contains(key)){
                if(int.TryParse(statistics[key]?.ToString(), out int value)){
                    WriteLineInColor($"{key}: {value:N0}", ConsoleColor.Cyan);
                }
            }
        }
    }

    private static void GetExpensiveProduct(SqlCommand command, decimal price){
        command.CommandText = "SELECT ProductID, ProductName, UnitPrice FROM Products WHERE UnitPrice >= @minimumPrice";
        command.Parameters.AddWithValue("minimumPrice", price);
    }
}