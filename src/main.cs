using System.Net;
using System.Net.Sockets;

string[] builtins = ["exit", "echo", "type"];

while (true)
{
    Console.Write("$ ");

    // Wait for user input
    var input = Console.ReadLine() ?? string.Empty;
    if (input is null or "")
    {
        continue;
    }
    
    ProcessInput(input);
}

return;

void ProcessInput(string input)
{
    var parts = input.Split(' ');
    switch (parts[0])
    {
        case "exit":
            int.TryParse(parts[1], out var exitCode);
            Environment.Exit(exitCode);
            break;
        
        case "echo":
            Console.WriteLine(string.Join(" ", parts.Skip(1)));
            break;
        
        case "type":
            if (builtins.Contains(parts[1]))
            {
                Console.WriteLine($"{parts[1]} is a shell builtin");
            }
            else
            {
                InvalidCommand(parts[1]);
            }
            break;
        
        default:
            InvalidCommand(parts[0]);
            break;
    }
}

void InvalidCommand(string command)
{
    Console.WriteLine($"{command}: command not found");
}
