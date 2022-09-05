// See https://aka.ms/new-console-template for more information
using Serilog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

Console.WriteLine("Hello, World!");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
if (args.Length > 3)
{
    var processName = args[0];
    var maximumLifetimeInMinutes = float.Parse(args[1]);
    var monitoringFrequencyInMinutes = float.Parse(args[2]);

    var processes = Process.GetProcessesByName(processName);
    if (processes.Length == 0)
    {
        Console.WriteLine("No process found");
        return 0;
    }
    CancellationTokenSource cancellationToken = new CancellationTokenSource();
    char ch = Console.ReadKey().KeyChar;
    if (char.ToLower(ch) == 'q')
    {
        cancellationToken.Cancel();
    }
}
    return 1;
