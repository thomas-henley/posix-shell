using System.Net;
using System.Net.Sockets;

while (true)
{
    Console.Write("$ ");

    // Wait for user input
    var input = Console.ReadLine() ?? string.Empty;
    InvalidCommand(input);
}

return;

void InvalidCommand(string command)
{
    Console.WriteLine($"{command}: command not found");
}
