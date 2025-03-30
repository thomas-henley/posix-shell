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
            TypeCommand(parts[1]);
            break;
        
        default:
            InvalidCommand(parts[0]);
            break;
    }
}

void TypeCommand(string command)
{
    if (builtins.Contains(command))
    {
        Console.WriteLine($"{command} is a shell builtin");
        return;
    }

    var path = SearchInPath(command);

    if (path is not "")
    {
        Console.WriteLine($"{command} is {path}");
        return;
    }
    
    Console.WriteLine($"{command}: not found");
}

string SearchInPath(string command)
{
    // Get path
    var pathEnvVar = Environment.GetEnvironmentVariable("PATH");
    if (pathEnvVar is null)
    {
        return "";
    }
    
    var paths = pathEnvVar.Split(Path.PathSeparator);

    // Search path
    foreach (var path in paths)
    {
        if (path is null or "") continue;
        
        string[] files;
        try
        {
            files = Directory.GetFileSystemEntries(path, "*.*", SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException)
        {
            continue;
        }
        
        foreach (var file in files)
        {
            if (Path.GetFileName(file) == command)
            {
                // Found it!
                return file;
            }
        }
    }

    // Return blank if not found
    return "";
}

void InvalidCommand(string command)
{
    Console.WriteLine($"{command}: command not found");
}
