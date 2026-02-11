using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace Zcat.Tool;

/// <summary>
/// Handlers for different ZeroMQ messaging patterns.
/// </summary>
public static class ZeroMqHandlers
{
    private static CancellationTokenSource? _cts;

    static ZeroMqHandlers()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _cts?.Cancel();
        };
    }

    /// <summary>
    /// Run a ZeroMQ subscriber.
    /// </summary>
    public static void RunSubscriber(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var subscriber = new SubscriberSocket();

        try
        {
            if (options.Bind)
                subscriber.Bind(options.Address);
            else
                subscriber.Connect(options.Address);

            subscriber.Subscribe(options.Topic);

            LogInfo(options, $"Subscribed to {options.Address} (topic='{options.Topic}', mode={(options.Bind ? "bind" : "connect")})");

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                if (subscriber.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out string? message))
                {
                    if (message != null)
                    {
                        Console.WriteLine(message);
                        messageCount++;
                    }
                }
            }

            LogInfo(options, $"Received {messageCount} messages");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run a ZeroMQ publisher.
    /// </summary>
    public static void RunPublisher(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var publisher = new PublisherSocket();

        try
        {
            if (options.Bind)
                publisher.Bind(options.Address);
            else
                publisher.Connect(options.Address);

            LogInfo(options, $"Publishing on {options.Address} (mode={(options.Bind ? "bind" : "connect")})");

            // Give subscribers time to connect
            Thread.Sleep(100);

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                string? line = Console.ReadLine();

                if (line == null) // EOF
                    break;

                string message = string.IsNullOrEmpty(options.Topic) ? line : $"{options.Topic} {line}";
                publisher.SendFrame(message);
                messageCount++;

                if (options.Verbose)
                    Console.Error.WriteLine($"Sent: {message}");
            }

            LogInfo(options, $"Published {messageCount} messages");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run a ZeroMQ requester (REQ socket - client side).
    /// </summary>
    public static void RunRequester(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var requester = new RequestSocket();

        try
        {
            if (options.Bind)
                requester.Bind(options.Address);
            else
                requester.Connect(options.Address);

            LogInfo(options, $"Requester connected to {options.Address} (mode={(options.Bind ? "bind" : "connect")})");

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                string? line = Console.ReadLine();

                if (line == null) // EOF
                    break;

                requester.SendFrame(line);

                if (requester.TryReceiveFrameString(TimeSpan.FromSeconds(5), out string? reply))
                {
                    if (reply != null)
                    {
                        Console.WriteLine(reply);
                        messageCount++;
                    }
                }
                else
                {
                    Console.Error.WriteLine("Timeout waiting for reply");
                }
            }

            LogInfo(options, $"Sent/received {messageCount} request-reply pairs");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run a ZeroMQ replier (REP socket - server side).
    /// </summary>
    public static void RunReplier(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var replier = new ResponseSocket();

        try
        {
            if (options.Bind)
                replier.Bind(options.Address);
            else
                replier.Connect(options.Address);

            LogInfo(options, $"Replier listening on {options.Address} (mode={(options.Bind ? "bind" : "connect")})");

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                if (replier.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out string? request))
                {
                    if (request != null)
                    {
                        Console.WriteLine($"Received: {request}");

                        // Echo the request back, or read from stdin for custom reply
                        string reply = $"Echo: {request}";
                        replier.SendFrame(reply);
                        messageCount++;

                        if (options.Verbose)
                            Console.Error.WriteLine($"Replied: {reply}");
                    }
                }
            }

            LogInfo(options, $"Handled {messageCount} requests");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run a ZeroMQ pusher (PUSH socket - pipeline source).
    /// </summary>
    public static void RunPusher(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var pusher = new PushSocket();

        try
        {
            if (options.Bind)
                pusher.Bind(options.Address);
            else
                pusher.Connect(options.Address);

            LogInfo(options, $"Pushing to {options.Address} (mode={(options.Bind ? "bind" : "connect")})");

            // Give pullers time to connect
            Thread.Sleep(100);

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                string? line = Console.ReadLine();

                if (line == null) // EOF
                    break;

                pusher.SendFrame(line);
                messageCount++;

                if (options.Verbose)
                    Console.Error.WriteLine($"Pushed: {line}");
            }

            LogInfo(options, $"Pushed {messageCount} messages");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Run a ZeroMQ puller (PULL socket - pipeline worker).
    /// </summary>
    public static void RunPuller(ZmqOptions options)
    {
        _cts = new CancellationTokenSource();
        SetupTimeout(options.TimeoutSeconds);

        using var puller = new PullSocket();

        try
        {
            if (options.Bind)
                puller.Bind(options.Address);
            else
                puller.Connect(options.Address);

            LogInfo(options, $"Pulling from {options.Address} (mode={(options.Bind ? "bind" : "connect")})");

            int messageCount = 0;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (options.MessageCount > 0 && messageCount >= options.MessageCount)
                    break;

                if (puller.TryReceiveFrameString(TimeSpan.FromMilliseconds(100), out string? message))
                {
                    if (message != null)
                    {
                        Console.WriteLine(message);
                        messageCount++;
                    }
                }
            }

            LogInfo(options, $"Pulled {messageCount} messages");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void SetupTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds > 0 && _cts != null)
        {
            _cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        }
    }

    private static void LogInfo(ZmqOptions options, string message)
    {
        if (!options.Quiet)
        {
            Console.Error.WriteLine(message);
        }
    }
}
