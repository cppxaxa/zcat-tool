# zcat - ZeroMQ CLI Tool

![Build](https://github.com/cppxaxa/zcat-tool/workflows/Build%20and%20Test/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/Zcat.Tool.svg)](https://www.nuget.org/packages/Zcat.Tool/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A powerful command-line tool for interacting with ZeroMQ messaging patterns. Send and receive messages using PUB/SUB, REQ/REP, PUSH/PULL patterns with timeout support, message limits, and graceful shutdown.

## Features

- ✅ **Multiple ZeroMQ Patterns**: PUB/SUB, REQ/REP, PUSH/PULL
- ⏱️ **Timeout Support**: Auto-exit after specified seconds
- 🔢 **Message Count Limits**: Exit after N messages
- 🎯 **Topic Filtering**: Subscribe to specific topics
- 🔌 **Bind or Connect**: Choose socket connection mode
- 🛑 **Graceful Shutdown**: Ctrl+C handling
- 📊 **Verbose/Quiet Modes**: Control output verbosity
- 🚀 **Zero Config**: Works out of the box with sensible defaults

## Installation

### From NuGet (once published)

```bash
dotnet tool install --global Zcat.Tool
```

### From Source

```bash
git clone https://github.com/cppxaxa/zcat-tool.git
cd zcat-tool
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release Zcat.Tool
```

## Usage

### Get Help

```bash
# Show basic help
zcat --help

# Show quick start guide with all patterns and examples
zcat --quickstart
# or
zcat --examples
```

### Basic Syntax

```bash
zcat <command> [address] [options]
```

### Commands

- `sub` - Subscribe to messages (SUB socket)
- `pub` - Publish messages (PUB socket)
- `req` - Send requests (REQ socket - client)
- `rep` - Reply to requests (REP socket - server)
- `push` - Push messages to pipeline (PUSH socket)
- `pull` - Pull messages from pipeline (PULL socket)

### Options

| Flag | Description | Default |
|------|-------------|---------|
| `-a, --address <addr>` | ZeroMQ address | `tcp://localhost:5556` |
| `-t, --timeout <sec>` | Exit after N seconds | `0` (infinite) |
| `-c, --count <num>` | Exit after N messages | `0` (unlimited) |
| `--topic <topic>` | Topic filter (SUB) or prefix (PUB) | `""` (all) |
| `-b, --bind` | Bind socket (server mode) | `false` |
| `--connect` | Connect socket (client mode) | `true` |
| `-v, --verbose` | Verbose output | `false` |
| `-q, --quiet` | Quiet mode (no info logs) | `false` |

## Examples

### Quick Test (PUB/SUB)

**Important:** Start the publisher FIRST with `--bind`, then start subscriber(s). Wait 1-2 seconds after subscribers connect before sending messages (this avoids the ZeroMQ "slow joiner" problem).

```bash
# Terminal 1 - Start publisher (bind first!)
zcat pub tcp://*:5556 --bind

# Terminal 2 - Start subscriber
zcat sub tcp://localhost:5556 --timeout 30

# Terminal 1 - Send messages (wait 1-2 sec after subscriber starts)
# Type messages and press Enter
Hello ZeroMQ!
Testing 123
```

### Publisher/Subscriber Pattern

**Terminal 1 - Start publisher (bind):**
```bash
# Bind and publish
zcat pub tcp://*:5556 --bind

# Publish with topic prefix
zcat pub tcp://*:5556 --bind --topic weather
```

**Terminal 2 - Start subscriber (connect):**
```bash
# Subscribe to all messages
zcat sub tcp://localhost:5556

# Subscribe with 30 second timeout
zcat sub tcp://localhost:5556 --timeout 30

# Subscribe to specific topic
zcat sub tcp://localhost:5556 --topic weather

# Exit after 100 messages
zcat sub tcp://localhost:5556 --count 100
```

### Request/Reply Pattern

**Terminal 1 - Start replier (server):**
```bash
# Bind and wait for requests
zcat rep tcp://*:5557 --bind
```

**Terminal 2 - Send requests (client):**
```bash
# Send request and get reply
echo "ping" | zcat req tcp://localhost:5557
```

### Push/Pull Pipeline Pattern

**Terminal 1 - Start pullers (workers):**
```bash
# Worker 1
zcat pull tcp://localhost:5558

# Worker 2 (in another terminal)
zcat pull tcp://localhost:5558
```

**Terminal 2 - Start pusher:**
```bash
# Push work to workers (they'll load balance)
zcat push tcp://*:5558 --bind
```

## Common Use Cases

### Monitoring Live Messages

```bash
# Watch messages for 10 seconds
zcat sub tcp://prod-server:5556 --timeout 10

# Sample 50 messages
zcat sub tcp://prod-server:5556 --count 50
```

### Testing & Debugging

```bash
# Test publisher is working
zcat sub tcp://localhost:5556 --timeout 5 --verbose

# Send test message
echo "test message" | zcat pub tcp://localhost:5556
```

### Load Testing

```bash
# Generate 1000 messages
seq 1 1000 | zcat push tcp://*:5559 --bind

# Pull and process
zcat pull tcp://localhost:5559 --count 1000
```

### Integration with Unix Tools

```bash
# Pipe to grep
zcat sub tcp://logs:5556 | grep ERROR

# Pipe from file
cat messages.txt | zcat pub tcp://*:5556 --bind

# Count messages in 60 seconds
zcat sub tcp://events:5556 --timeout 60 | wc -l
```

## Architecture

### Bind vs Connect

- **Bind** (`--bind`): Socket acts as server, waits for connections
- **Connect** (default): Socket acts as client, connects to server

**Rule of thumb:**
- SUB sockets usually **connect** to PUB
- PUB sockets usually **bind**
- REP sockets usually **bind** (server)
- REQ sockets usually **connect** (client)
- PULL sockets usually **bind**
- PUSH sockets usually **connect**

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success (normal exit or timeout) |
| 1 | Error (connection failed, invalid args, etc.) |

## Troubleshooting

### "Address already in use"

Multiple processes trying to bind to the same address. Use `--connect` or choose different port.

```bash
# Instead of binding
zcat pub tcp://*:5556 --bind

# Try connecting
zcat pub tcp://localhost:5556
```

### No messages received (PUB/SUB)

This is often due to the **ZeroMQ "slow joiner" problem**:

1. **Always start the publisher FIRST** with `--bind`
2. Then start subscriber(s) with `--connect` (default)
3. **Wait 1-2 seconds** after subscribers connect before sending messages
4. Verify topic filters match
5. Try `--verbose` mode to see connection status

```bash
# Correct order:
# Terminal 1
zcat pub tcp://*:5556 --bind

# Terminal 2 (wait for publisher to be ready)
zcat sub tcp://localhost:5556

# Terminal 1 (wait 1-2 seconds after subscriber starts, then type)
Hello!
```

Other checks:
- Check firewall settings
- Ensure addresses match (localhost vs 0.0.0.0 vs *)

### Messages not load balancing

In PUSH/PULL, workers must be connected before messages are sent. Add a small delay:

```bash
# Start workers first, then:
sleep 1 && cat work.txt | zcat push tcp://*:5559 --bind
```

## Development

### Build

```bash
dotnet build
```

### Run Locally

```bash
dotnet run -- sub tcp://localhost:5556 --timeout 5
```

### Test

```bash
# Terminal 1
dotnet run -- sub tcp://localhost:5556

# Terminal 2
echo "test" | dotnet run -- pub tcp://localhost:5556
```

## License

MIT

## Contributing

Pull requests welcome! Please ensure:
- Code follows existing style
- Add tests for new features
- Update documentation

## Links

- [ZeroMQ Guide](https://zguide.zeromq.org/)
- [NetMQ Documentation](https://netmq.readthedocs.io/)
- [Report Issues](https://github.com/cppxaxa/zcat-tool/issues)
