# Project Summary

## What We Built

**zcat** - A professional .NET global tool for interacting with ZeroMQ messaging patterns from the command line.

## Features Implemented

✅ **6 ZeroMQ Patterns**:
- PUB/SUB (publisher/subscriber)
- REQ/REP (request/reply)
- PUSH/PULL (pipeline)

✅ **Timeout Support**: `--timeout` flag for auto-exit after N seconds

✅ **Message Count Limits**: `--count` flag to exit after N messages

✅ **Bind/Connect Modes**: Support for both server (`--bind`) and client (default) modes

✅ **Topic Filtering**: Subscribe to specific topics or publish with topic prefix

✅ **Verbose/Quiet Modes**: Control output verbosity

✅ **Graceful Shutdown**: Ctrl+C handling with proper cleanup

✅ **Professional CLI**: Clean argument parsing, help text, error handling

## Project Structure

```
zcat-tool/
├── Program.cs              # Entry point, CLI parsing, help text (includes embedded quick start)
├── ZeroMqHandlers.cs       # ZeroMQ pattern implementations
├── Zcat.Tool.csproj        # Project file with PackAsTool configuration
├── nuget.config            # NuGet sources configuration
├── README.md               # Complete user documentation
├── PROJECT_SUMMARY.md      # Project overview and technical details
├── PUBSUB_FIX.md          # Documentation of PUB/SUB slow joiner fix
├── test.sh                 # Automated test script
├── LICENSE                 # MIT license
├── .gitignore              # Git ignore rules
└── .github/
    └── workflows/
        ├── build.yml       # Build workflow (manual trigger only)
        └── publish.yml     # Publish to NuGet on release
```

## Technology Stack

- **.NET 8.0**: Target framework
- **NetMQ 4.0.1.13**: ZeroMQ implementation for .NET
- **Global Tool**: Packaged as dotnet global tool

## Key Design Decisions

### 1. Removed Bullseye Dependency
Initially planned to use Bullseye for CLI, but switched to simple argument parsing for:
- Reduced dependencies
- Better control over argument handling
- Simpler codebase
- Faster startup

### 2. Graceful Shutdown Architecture
- Static `CancellationTokenSource` shared across handlers
- Console.CancelKeyPress event handler
- Timeout support via `CancelAfter()`
- All socket operations use `Try*` methods with timeouts

### 3. Smart Defaults
- Default address: `tcp://localhost:5556`
- Default mode: Connect (client mode)
- Infinite timeout and message count by default
- Empty topic subscribes to all messages

### 4. Exit Codes
- 0: Success (normal exit or timeout)
- 1: Error (connection failed, invalid args, etc.)

## Usage Examples

### Basic Pub/Sub (IMPORTANT: Read the note!)

**Note**: Due to ZeroMQ's "slow joiner" problem, the publisher must bind first and you must wait 1-2 seconds after the subscriber connects before sending messages.

```bash
# Terminal 1 - Publisher (bind first!)
zcat pub tcp://*:5556 --bind

# Terminal 2 - Subscriber
zcat sub tcp://localhost:5556 --timeout 30

# Terminal 1 - Wait 1-2 seconds, then type messages
Hello!
```

See [PUBSUB_FIX.md](PUBSUB_FIX.md) for detailed explanation.

### With Timeout
```bash
# Monitor for 30 seconds
zcat sub tcp://prod-server:5556 --timeout 30
```

### With Message Limit
```bash
# Get first 100 messages
zcat sub tcp://localhost:5556 --count 100
```

### Request/Reply
```bash
# Terminal 1 - Server
zcat rep tcp://*:5557 --bind

# Terminal 2 - Client
echo "ping" | zcat req tcp://localhost:5557
```

### Pipeline
```bash
# Terminal 1 & 2 - Workers
zcat pull tcp://localhost:5558

# Terminal 3 - Distributor
seq 1 1000 | zcat push tcp://*:5558 --bind
```

## Installation

### Local Testing
```bash
cd zcat-tool
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release Zcat.Tool
```

### From NuGet (after publishing)
```bash
dotnet tool install --global Zcat.Tool
```

## Publishing to NuGet

### GitHub Actions Workflow

The project uses GitHub Actions with Trusted Publishing (OIDC):

1. **Setup**: Add `NUGET_USER` secret in GitHub repository settings
2. **Configure Trusted Publishing** on NuGet.org (after first manual publish)
3. **Create GitHub Release**: Automatically triggers publish workflow
4. **Manual publish**: Use workflow_dispatch if needed

### Manual Publishing

```bash
# Update version in Zcat.Tool.csproj
# Build and pack
dotnet pack -c Release

# Publish to NuGet
dotnet nuget push bin/Release/Zcat.Tool.*.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## What's Different from Original Design

### Changes Made:
1. ✅ **Simplified CLI parsing**: Used direct arg parsing instead of external library
2. ✅ **Added nuget.config**: To avoid private feed conflicts
3. ✅ **Better error handling**: Try-catch with proper exit codes
4. ✅ **Embedded quick start**: Built into CLI with `--quickstart` flag
5. ✅ **Enhanced documentation**: More examples and use cases
6. ✅ **Fixed PUB/SUB slow joiner**: Publisher binds first, added delays
7. ✅ **GitHub Actions**: CI/CD with Trusted Publishing (OIDC)

### Kept from Original:
- ✅ Timeout support
- ✅ Message count limits
- ✅ All 6 ZeroMQ patterns
- ✅ Bind/connect modes
- ✅ Topic filtering
- ✅ Verbose/quiet modes

## Next Steps (Future Enhancements)

**Potential additions**:
- DEALER/ROUTER patterns
- Multipart message support
- JSON formatting option
- Color-coded output
- Message timestamps
- Multiple address support
- Connection retry logic
- Config file support
- Shell completion

## Build & Test

### Build
```bash
dotnet build -c Release
```

### Test locally
```bash
dotnet run -- sub tcp://localhost:5556 --help
```

### Pack
```bash
dotnet pack -c Release
# Creates: bin/Release/Zcat.Tool.0.0.3.nupkg
```

### Run tests
```bash
./test.sh
```

## License

MIT License - See [LICENSE](LICENSE) file

## Credits

Built with:
- [NetMQ](https://github.com/zeromq/netmq) - ZeroMQ for .NET
- [.NET 8.0](https://dotnet.microsoft.com/) - Runtime

---

**Status**: ✅ Ready for testing and NuGet publishing (v0.0.3)

**Repository**: https://github.com/cppxaxa/zcat-tool

**Next Action**: Create GitHub Release to trigger automatic NuGet publish
