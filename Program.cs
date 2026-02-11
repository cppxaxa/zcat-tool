using System;

namespace Zcat.Tool;

/// <summary>
/// Entry point for the zcat CLI tool.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            PrintUsage();
            return;
        }

        if (args[0] == "--quickstart" || args[0] == "--examples")
        {
            PrintQuickStart();
            return;
        }

        try
        {
            string command = args[0].ToLowerInvariant();
            var options = ParseOptions(args);

            switch (command)
            {
                case "sub":
                    ZeroMqHandlers.RunSubscriber(options);
                    break;

                case "pub":
                    ZeroMqHandlers.RunPublisher(options);
                    break;

                case "req":
                    ZeroMqHandlers.RunRequester(options);
                    break;

                case "rep":
                    ZeroMqHandlers.RunReplier(options);
                    break;

                case "push":
                    ZeroMqHandlers.RunPusher(options);
                    break;

                case "pull":
                    ZeroMqHandlers.RunPuller(options);
                    break;

                default:
                    Console.Error.WriteLine($"Unknown command: {command}");
                    PrintUsage();
                    Environment.Exit(1);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("zcat - ZeroMQ CLI Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: zcat <command> [address] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  sub       Subscribe to messages (SUB socket)");
        Console.WriteLine("  pub       Publish messages (PUB socket)");
        Console.WriteLine("  req       Send requests (REQ socket - client)");
        Console.WriteLine("  rep       Reply to requests (REP socket - server)");
        Console.WriteLine("  push      Push messages to pipeline (PUSH socket)");
        Console.WriteLine("  pull      Pull messages from pipeline (PULL socket)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -a, --address <addr>    ZeroMQ address (default: tcp://localhost:5556)");
        Console.WriteLine("  -t, --timeout <sec>     Exit after N seconds (0 = infinite)");
        Console.WriteLine("  -c, --count <num>       Exit after N messages (0 = unlimited)");
        Console.WriteLine("  --topic <topic>         Topic filter (SUB) or prefix (PUB)");
        Console.WriteLine("  -b, --bind              Bind socket (server mode)");
        Console.WriteLine("  --connect               Connect socket (client mode, default)");
        Console.WriteLine("  -v, --verbose           Verbose output");
        Console.WriteLine("  -q, --quiet             Quiet mode (no info logs)");
        Console.WriteLine();
        Console.WriteLine("Quick Examples:");
        Console.WriteLine("  zcat sub tcp://localhost:5556 --timeout 30");
        Console.WriteLine("  zcat pub tcp://*:5556 --bind");
        Console.WriteLine("  echo \"test\" | zcat pub tcp://localhost:5556");
        Console.WriteLine("  zcat pull tcp://localhost:5558 --count 100");
        Console.WriteLine();
        Console.WriteLine("More Help:");
        Console.WriteLine("  zcat --quickstart       Show quick start guide with all patterns");
        Console.WriteLine("  zcat --examples         Alias for --quickstart");
    }

    private static void PrintQuickStart()
    {
        Console.WriteLine("=== ZCAT QUICK START GUIDE ===");
        Console.WriteLine();
        Console.WriteLine("Get started with zcat in 60 seconds!");
        Console.WriteLine();

        Console.WriteLine("## BASIC TEST");
        Console.WriteLine();
        Console.WriteLine("  Terminal 1 - Subscriber:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --timeout 10");
        Console.WriteLine();
        Console.WriteLine("  Terminal 2 - Publisher:");
        Console.WriteLine("    echo \"Hello ZeroMQ!\" | zcat pub tcp://localhost:5556");
        Console.WriteLine();
        Console.WriteLine("  You should see \"Hello ZeroMQ!\" in Terminal 1!");
        Console.WriteLine();

        Console.WriteLine("## ALL PATTERNS");
        Console.WriteLine();
        Console.WriteLine("### PUB/SUB (1-to-many broadcast)");
        Console.WriteLine("  Terminal 1:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --topic weather");
        Console.WriteLine();
        Console.WriteLine("  Terminal 2:");
        Console.WriteLine("    echo \"weather sunny 25C\" | zcat pub tcp://localhost:5556");
        Console.WriteLine();

        Console.WriteLine("### REQ/REP (request-reply)");
        Console.WriteLine("  Terminal 1 - Server:");
        Console.WriteLine("    zcat rep tcp://*:5557 --bind");
        Console.WriteLine();
        Console.WriteLine("  Terminal 2 - Client:");
        Console.WriteLine("    echo \"ping\" | zcat req tcp://localhost:5557");
        Console.WriteLine();

        Console.WriteLine("### PUSH/PULL (load-balanced pipeline)");
        Console.WriteLine("  Terminal 1 & 2 - Workers:");
        Console.WriteLine("    zcat pull tcp://localhost:5558");
        Console.WriteLine();
        Console.WriteLine("  Terminal 3 - Work distributor:");
        Console.WriteLine("    seq 1 10 | zcat push tcp://*:5558 --bind");
        Console.WriteLine();

        Console.WriteLine("## COMMON FLAGS");
        Console.WriteLine();
        Console.WriteLine("  Run for 30 seconds:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --timeout 30");
        Console.WriteLine();
        Console.WriteLine("  Get 100 messages then exit:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --count 100");
        Console.WriteLine();
        Console.WriteLine("  Verbose mode:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --verbose");
        Console.WriteLine();
        Console.WriteLine("  Quiet mode (no status messages):");
        Console.WriteLine("    zcat sub tcp://localhost:5556 --quiet");
        Console.WriteLine();

        Console.WriteLine("## ADVANCED USAGE");
        Console.WriteLine();
        Console.WriteLine("### Pipe integration");
        Console.WriteLine("  Grep messages:");
        Console.WriteLine("    zcat sub tcp://logs:5556 | grep ERROR");
        Console.WriteLine();
        Console.WriteLine("  Count messages in 60s:");
        Console.WriteLine("    zcat sub tcp://events:5556 --timeout 60 | wc -l");
        Console.WriteLine();
        Console.WriteLine("  Publish from file:");
        Console.WriteLine("    cat messages.txt | zcat pub tcp://*:5556 --bind");
        Console.WriteLine();

        Console.WriteLine("### Multiple publishers to single consumer");
        Console.WriteLine("  Use PUSH/PULL for multiple publishers:");
        Console.WriteLine("    Terminal 1 - First publisher (binds):");
        Console.WriteLine("      zcat push tcp://*:5558 --bind");
        Console.WriteLine();
        Console.WriteLine("    Terminal 2 - Second publisher (connects):");
        Console.WriteLine("      zcat push tcp://localhost:5558");
        Console.WriteLine();
        Console.WriteLine("    Terminal 3 - Consumer:");
        Console.WriteLine("      zcat pull tcp://localhost:5558");
        Console.WriteLine();
        Console.WriteLine("  Both publishers can send to the same puller!");
        Console.WriteLine();

        Console.WriteLine("### Chain patterns (relay messages)");
        Console.WriteLine("  Subscribe and republish to relay messages:");
        Console.WriteLine("    Terminal 1 - Original publisher:");
        Console.WriteLine("      zcat pub tcp://*:5556 --bind");
        Console.WriteLine();
        Console.WriteLine("    Terminal 2 - Relay (sub -> pub):");
        Console.WriteLine("      zcat sub tcp://localhost:5556 | zcat pub tcp://*:5557 --bind");
        Console.WriteLine();
        Console.WriteLine("    Terminal 3 - Final subscriber:");
        Console.WriteLine("      zcat sub tcp://localhost:5557");
        Console.WriteLine();

        Console.WriteLine("### Multiple subscribers");
        Console.WriteLine("  Each subscriber gets ALL messages:");
        Console.WriteLine("    zcat sub tcp://localhost:5556 &");
        Console.WriteLine("    zcat sub tcp://localhost:5556 &");
        Console.WriteLine("    echo \"broadcast\" | zcat pub tcp://localhost:5556");
        Console.WriteLine();

        Console.WriteLine("### Load balancing");
        Console.WriteLine("  Messages are distributed round-robin:");
        Console.WriteLine("    zcat pull tcp://localhost:5558 &");
        Console.WriteLine("    zcat pull tcp://localhost:5558 &");
        Console.WriteLine("    seq 1 10 | zcat push tcp://*:5558 --bind");
        Console.WriteLine();

        Console.WriteLine("## PATTERN CHEAT SHEET");
        Console.WriteLine();
        Console.WriteLine("  PUB/SUB:    1-to-many broadcast, all subscribers get all messages");
        Console.WriteLine("  REQ/REP:    Synchronous request-reply, 1-to-1");
        Console.WriteLine("  PUSH/PULL:  Load-balanced pipeline, round-robin distribution");
        Console.WriteLine();
        Console.WriteLine("For complete documentation, use: zcat --help");
        Console.WriteLine();
    }

    private static ZmqOptions ParseOptions(string[] args)
    {
        var options = new ZmqOptions();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--") || args[i].StartsWith("-"))
            {
                switch (args[i])
                {
                    case "--address":
                    case "-a":
                        if (i + 1 < args.Length)
                            options.Address = args[++i];
                        break;

                    case "--timeout":
                    case "-t":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int timeout))
                            options.TimeoutSeconds = timeout;
                        break;

                    case "--count":
                    case "-c":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int count))
                            options.MessageCount = count;
                        break;

                    case "--topic":
                        if (i + 1 < args.Length)
                            options.Topic = args[++i];
                        break;

                    case "--bind":
                    case "-b":
                        options.Bind = true;
                        break;

                    case "--connect":
                        options.Bind = false;
                        break;

                    case "--verbose":
                    case "-v":
                        options.Verbose = true;
                        break;

                    case "--quiet":
                    case "-q":
                        options.Quiet = true;
                        break;
                }
            }
            else if (!IsCommand(args[i]) && string.IsNullOrEmpty(options.Address))
            {
                // First non-flag argument is the address
                options.Address = args[i];
            }
        }

        return options;
    }

    private static bool IsCommand(string arg)
    {
        return arg is "sub" or "pub" or "req" or "rep" or "push" or "pull";
    }
}

/// <summary>
/// Configuration options for ZeroMQ operations.
/// </summary>
public class ZmqOptions
{
    public string Address { get; set; } = "tcp://localhost:5556";
    public int TimeoutSeconds { get; set; } = 0; // 0 = infinite
    public int MessageCount { get; set; } = 0; // 0 = unlimited
    public string Topic { get; set; } = "";
    public bool Bind { get; set; } = false; // false = connect (default for most)
    public bool Verbose { get; set; } = false;
    public bool Quiet { get; set; } = false;
}
