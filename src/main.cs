using System.Net;
using System.Net.Sockets;

// Uncomment this line to pass the first stage
Console.Write("$ ");

// Wait for user input
var input = Console.ReadLine() ?? string.Empty;
InvalidCommand(input);
return;

void InvalidCommand(string command)
{
    Console.WriteLine($"{input}: command not found");
}
