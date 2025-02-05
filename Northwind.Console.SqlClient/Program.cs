using Microsoft.Data.SqlClient; // To use SQLConnection etc.

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
    connection.Open();
    WriteLine($"SQL Server Version: {connection.ServerVersion}");
}
catch(SqlException ex){
WriteLineInColor($"SQL exception: {ex.Message}", ConsoleColor.Red);
return;
}
#endregion
connection.Close();
