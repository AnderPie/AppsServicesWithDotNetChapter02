# Exercise 2-1 Test your knowledge

## Which NuGet package should you reference in a .NET project to get the best performance when working with data in SQL Server? 
Microsoft.Data.SqlClient, it uses ADO.NET for extremely performant SQL server work

## What is the safest way to define a database connection string?
Using KeyVault, a similar password manager. environment variables are OK but usually unencrypted, so if your machine is compromised you are in big trouble.

## What must T-SQL parameters and variables be prefixed with?
@

## What must you do before reading an output parameter?
Declare the output parameter, execute the stored procedure where the output parameter is specified (this procedure should have an EXEC statement with the OUTPUT specified).

## What type does Dapper add its extension methods to
It adds its extension methods to any implementer of the IDbConnection class.

## What are the two most commonly used methods provided by Dapper?
Query<T>() for SELECT type statements and Execute() for INSERT, UPDATE, and DELETE methods