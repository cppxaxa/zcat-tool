#!/bin/bash
# Test script for zcat tool

set -e

echo "🧪 Testing zcat tool..."
echo

# Test 1: Help command
echo "✅ Test 1: Help command"
dotnet run -- --help | grep -q "zcat - ZeroMQ CLI Tool" && echo "   PASS" || echo "   FAIL"
echo

# Test 2: Build succeeds
echo "✅ Test 2: Build verification"
dotnet build -c Release > /dev/null 2>&1 && echo "   PASS" || echo "   FAIL"
echo

# Test 3: Pack succeeds
echo "✅ Test 3: Package creation"
dotnet pack -c Release > /dev/null 2>&1 && echo "   PASS" || echo "   FAIL"
echo

# Test 4: Package exists
echo "✅ Test 4: Package file exists"
[ -f "bin/Release/Zcat.Tool.1.0.0.nupkg" ] && echo "   PASS" || echo "   FAIL"
echo

# Test 5: All commands show in help
echo "✅ Test 5: All commands documented"
HELP_OUTPUT=$(dotnet run -- --help)
echo "$HELP_OUTPUT" | grep -q "sub" && \
echo "$HELP_OUTPUT" | grep -q "pub" && \
echo "$HELP_OUTPUT" | grep -q "req" && \
echo "$HELP_OUTPUT" | grep -q "rep" && \
echo "$HELP_OUTPUT" | grep -q "push" && \
echo "$HELP_OUTPUT" | grep -q "pull" && \
echo "   PASS" || echo "   FAIL"
echo

# Test 6: All options documented
echo "✅ Test 6: All options documented"
echo "$HELP_OUTPUT" | grep -q "timeout" && \
echo "$HELP_OUTPUT" | grep -q "count" && \
echo "$HELP_OUTPUT" | grep -q "topic" && \
echo "$HELP_OUTPUT" | grep -q "bind" && \
echo "$HELP_OUTPUT" | grep -q "verbose" && \
echo "$HELP_OUTPUT" | grep -q "quiet" && \
echo "   PASS" || echo "   FAIL"
echo

echo "🎉 All tests completed!"
echo
echo "Next steps:"
echo "1. Install locally: dotnet tool install --global --add-source ./bin/Release Zcat.Tool"
echo "2. Test: zcat --help"
echo "3. Publish: See STEPS.md for NuGet publishing"
