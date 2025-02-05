/*
 * Defines functions for handling events related to changes in database connection state
 * And for when database sends an InfoMessage 
 */

using Microsoft.Data.SqlClient; // To use SqlInfoMessageEventArgs
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Data; 

partial class Program{
    private static void Connection_StateChange(object sender, StateChangeEventArgs e){
        WriteLineInColor($"State change from {e.OriginalState} to {e.CurrentState}.");
    }

    private static void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e){
        WriteLineInColor($"Info: {e.Message}.", ConsoleColor.DarkBlue);
    }
}