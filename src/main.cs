using System.Diagnostics;
using System.Text;

string[] builtins = ["exit", "echo", "type", "pwd", "cd"];

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
    var parts = input.SplitWithQuotes(' ');

    if (parts.Contains(">") || parts.Contains("1>"))
    {
        // TODO: do this stage on linux to figure out why the tests are failing
        Console.SetOut(new StreamWriter(parts.Last()) { AutoFlush = true });
        parts = parts.Take(parts.Length - 2).ToArray();
    }

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
        
        case "pwd":
            Console.WriteLine(Directory.GetCurrentDirectory());
            break;
        
        case "cd":
            ChangeDirectory(parts[1]);
            break;
        
        case "dir":
            foreach (var filename in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                Console.WriteLine(Path.GetFileName(filename));
            }
            break;
        
        case "wincat":
            Console.WriteLine(File.ReadAllText(parts[1]));
            break;
        
        default:
            var file = SearchInPath(parts[0]);

            if (file is not "")
            {
                if (!OperatingSystem.IsWindows()) file = Path.GetFileName(file);
                var proc = Process.Start(file, parts.Skip(1));
                proc.WaitForExit();
            }
            else
            {
                InvalidCommand(parts[0]);
            }
            break;
    }
    
    Console.Out.Dispose();
    Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
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
    
    var paths = pathEnvVar.SplitWithQuotes(Path.PathSeparator);

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
        catch (DirectoryNotFoundException)
        {
            continue;
        }
        
        foreach (var file in files)
        {
            if (Path.GetFileName(file) == command || Path.GetFileName(file) == command + ".exe")
            {
                // Found it!
                return file;
            }
        }
    }

    // Return blank if not found
    return "";
}

void ChangeDirectory(string path)
{
    if (path == "~")
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Directory.SetCurrentDirectory(homePath);
    }
    else if (Directory.Exists(path))
    {
        Directory.SetCurrentDirectory(path);
    }
    else
    {
        Console.WriteLine($"{path}: No such file or directory");
    }
}

void InvalidCommand(string command)
{
    Console.WriteLine($"{command}: command not found");
}

public static class Extensions
{
    public static string[] SplitWithQuotes(this string input, char separator = ' ')
    {
        var parts = new List<string>();

        StringBuilder token = new();
        var openQuote = false;
        var openDoubleQuote = false;
        var backslash = false;
        
        foreach (var c in input)
        {
            if (c == '\\' && !openQuote && !backslash)
            {
                backslash = true;
                continue;
            }
            
            if (backslash)
            {
                char[] special = ['$', '"', '\\'];
                if (openDoubleQuote && !special.Contains(c))
                {
                    token.Append('\\');
                }
                token.Append(c);
                backslash = false;
                continue;
            }
            
            if (c == '"' && !openQuote)
            {
                openDoubleQuote = !openDoubleQuote;
                continue;
            }
            
            if (c == '\'' && !openDoubleQuote)
            {
                openQuote = !openQuote;
                continue;
            }
            
            if (c == separator && token.ToString() != string.Empty && !openQuote && !openDoubleQuote)
            {
                parts.Add(token.ToString());
                token.Clear();
                continue;
            }

            if (c == separator && !openQuote && !openDoubleQuote)
            {
                continue;
            }

            token.Append(c);
        }

        if (token.ToString() != string.Empty)
        {
            parts.Add(token.ToString());
        }

        return parts.ToArray();
    }
}