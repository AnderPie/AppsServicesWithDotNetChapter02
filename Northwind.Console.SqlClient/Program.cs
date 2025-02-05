using Microsoft.Data.SqlClient; // To use SQLConnection etc.
using System.Data; // To use CommandType for working with ADO.NET command types so that we can run commands and receive results from Northwind tables
using System.Text.Json; // To support serializing to JSON with Utf8JsonWriter and JsonSerializer
using static System.Environment; // So that the serialized date we produce can be given a file to live in
using static System.IO.Path; // So that the serialized date we produce can be given a file to live in
using Northwind.Models; // To use our .NET classes based on the Northwind objects such as product, order etc.

/*
 * Take user input to connect to database
 * user enters password currently, in future implement keypass functionality
 */

 #region Set up connection builder
 SqlConnectionStringBuilder builder = new(){
    InitialCatalog = "Northwind",
    MultipleActiveResultSets = true, 
    Encrypt = true,
    TrustServerCertificate = true,
    ConnectTimeout = 10 // Default is 30 seconds
 };

 WriteLine("Connect to:");
 WriteLine("  1 - SQL Server on local machine");
 WriteLine("  2 - Azure SQL Database");
 WriteLine("  3 - Azure SQL Edge");
 WriteLine();
 Write("Press a key: ");
 WriteLine(); WriteLine();
 ConsoleKey key = ReadKey().Key;
 switch(key){
    case ConsoleKey.D1 or ConsoleKey.NumPad1:
    builder.DataSource = "."; // . refers to the local machine
    break;
    default:
      WriteLine("Sorry, only connection to local machine is implemented (I haven't set uo Azure SQL Database or SQL Edge!)");
      break;
    /*
    case ConsoleKey.D2 or ConsoleKey.NumPad2:
    builder.DataSource = "tcp:etc."
    */
 }

 WriteLine("Authenticate using:");
 WriteLine("  1 - Windows Integrated Security");
 WriteLine("  2 - SQL Login, for example, sa");
 WriteLine();
 Write("Press a key: ");

 key = ReadKey().Key;
 WriteLine(); WriteLine();

 if(key is ConsoleKey.D1 or ConsoleKey.NumPad1){
    builder.IntegratedSecurity = true;
 }
 else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2){
    Write("Enter your SQL Server user ID: ");
    string? userID = ReadLine();
    if(string.IsNullOrWhiteSpace(userID)){
        WriteLine("User ID cannot be empty or null");
        return;
    }

    builder.UserID = userID;
    Write("Enter your SQL Server password: ");
    string? password = ReadLine();
    if(string.IsNullOrWhiteSpace(password)){
        WriteLine("Password cannot be empty or null.");
        return;
    }

    builder.Password = password;
    builder.PersistSecurityInfo = false;
 }
 else{
    WriteLine("No authentication selected");
    return;
 }

 #endregion

#region Create and open the connection
SqlConnection connection = new(builder.ConnectionString);
WriteLine(connection.ConnectionString);
WriteLine();

connection.StateChange += Connection_StateChange;
connection.InfoMessage += Connection_InfoMessage;

try{
    WriteLine("Opening connection. Please wait up to {0} seconds.", builder.ConnectTimeout);
    WriteLine();
    // connection.Open(); 
    await connection.OpenAsync(); // Asynchronous open
    WriteLine($"SQL Server Version: {connection.ServerVersion}");
    connection.StatisticsEnabled = true;
}
catch(SqlException ex){
WriteLineInColor($"SQL exception: {ex.Message}", ConsoleColor.Red);
return;
}
#endregion

#region Run some commands


// Create a command
SqlCommand command = connection.CreateCommand();

#region Solicit a unit price
Write("Enter a unit price: ");
string? priceText = ReadLine();

if(!decimal.TryParse(priceText, out decimal price)){
    WriteLine("You must enter a valid unit price");
    return;
}
#endregion

#region Choose between running the stored procedure or the console defined method
WriteLineInColor("Would you like to run the stored procedure in the database or the method in the console app?");
WriteLine("  1 - Stored Procedure");
WriteLine("  2 - Console App Method");
key = ReadKey().Key;
SqlParameter p1, p2 = new(), p3 = new();

if(key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
{
    command.CommandType = CommandType.StoredProcedure;
    command.CommandText = "GetExpensiveProducts";

    p1 = new(){
        ParameterName = "price",
        SqlDbType = SqlDbType.Money,
        SqlValue = price
    };

    p2 = new(){
        Direction = ParameterDirection.Output,
        ParameterName = "count",
        SqlDbType = SqlDbType.Int
    };

    p3 = new(){
        Direction = ParameterDirection.ReturnValue,
        ParameterName = "rv",
        SqlDbType = SqlDbType.Int
    };

    command.Parameters.AddRange(new[] {p1, p2, p3});
}
else if(key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
{
    GetExpensiveProduct(command, price);
}
else
{
    WriteLine("You did not enter a valid option.");
    return;
}
#endregion

// Create data reader to write results of command to Console
SqlDataReader reader = await command.ExecuteReaderAsync();

string horizontalLine = new('-', 60);
WriteLineInColor(horizontalLine, ConsoleColor.Magenta);
WriteLine("| {0,5} | {1, -35} | {2,10} |", arg0: "Id", arg1: "Name", arg2: "Price");
WriteLine(horizontalLine);

#region Define JSON Serialization path and instantiate serializer
string jsonPath = Combine(CurrentDirectory, "products.json");
List<Product> products = new(capacity: 77); // Define a new list to store our products
await using(FileStream jsonStream = File.Create(jsonPath))
{
    Utf8JsonWriter jsonWriter = new(jsonStream);
    jsonWriter.WriteStartArray();
    while(await reader.ReadAsync()){

        
        Product product = new()
        {
            ProductId = await reader.GetFieldValueAsync<int>("ProductId"),
            ProductName = await reader.GetFieldValueAsync<string>("ProductName"),
            UnitPrice = await reader.GetFieldValueAsync<decimal>("UnitPrice")
        };

        products.Add(product);
        
        WriteLine("| {0,5} | {1, -35} | {2,10:C} |", 
        await reader.GetFieldValueAsync<int>("ProductId"), 
        await reader.GetFieldValueAsync<string>("ProductName"), 
        await reader.GetFieldValueAsync<decimal>("UnitPrice"));

        jsonWriter.WriteStartObject();
        jsonWriter.WriteNumber("productId", await reader.GetFieldValueAsync<int>("ProductId"));
        jsonWriter.WriteString("productName", await reader.GetFieldValueAsync<string>("ProductName"));
        jsonWriter.WriteNumber("unitPrice", await reader.GetFieldValueAsync<decimal>("UnitPrice"));
        jsonWriter.WriteEndObject();
    }
    jsonWriter.WriteEndArray();
    jsonWriter.Flush();
    jsonStream.Close();
    WriteLine(horizontalLine);
}
WriteLineInColor($"Written to: {jsonPath}", ConsoleColor.DarkGreen);
#endregion

#endregion

#region  Output statistics
WriteLineInColor(JsonSerializer.Serialize(products), ConsoleColor.Magenta);
OutPutStatistics(connection);
#endregion
await reader.CloseAsync();
if(key is ConsoleKey.D2 or ConsoleKey.NumPad2){
    WriteLine($"Output count: {p2.Value}");
    WriteLine($"Output count: {p3.Value}");
}
await connection.CloseAsync();
