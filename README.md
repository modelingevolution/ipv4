# ModelingEvolution.Ipv4

[![NuGet](https://img.shields.io/nuget/v/ModelingEvolution.Ipv4.svg)](https://www.nuget.org/packages/ModelingEvolution.Ipv4/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)

A lightweight and high-performance IPv4 address library for .NET. Provides strongly-typed IPv4 addresses, network calculations, CIDR notation support, and seamless JSON serialization. Perfect for network programming, configuration management, and Domain-Driven Design applications.

## Features

- 🌐 **Strongly-Typed IPv4 Addresses**: Type-safe IPv4 address representation with validation
- 📊 **Network Calculations**: CIDR notation, subnet calculations, network/broadcast addresses
- 🏗️ **Network Enumeration**: Enumerate all addresses or host addresses in a network
- 🔧 **Configuration Support**: Complete IPv4 configuration with address, subnet, and gateway
- 🚀 **High Performance**: Efficient bit operations and minimal allocations
- 📦 **JSON Serialization**: Automatic JSON support using attribute-based conversion
- 🎯 **IParsable Support**: Standard .NET parsing interface implementation
- 🧩 **DDD Friendly**: Perfect for domain primitives and value objects
- ✅ **Comprehensive Testing**: 154 unit tests with 100% coverage

## Installation

```bash
dotnet add package ModelingEvolution.Ipv4
```

## Quick Start

### Basic IPv4 Address Usage

```csharp
using ModelingEvolution.Ipv4;

// Create IPv4 addresses
var address1 = new Ipv4Address(192, 168, 1, 1);
var address2 = Ipv4Address.Parse("10.0.0.1");
var address3 = (Ipv4Address)"172.16.0.1";  // Implicit conversion
var address4 = new Ipv4Address(0xC0A80101u); // From uint

// Access octets
Console.WriteLine($"Address: {address1.A}.{address1.B}.{address1.C}.{address1.D}");

// Network operations
var network = address1.GetNetwork(24);      // 192.168.1.0
var broadcast = address1.GetBroadcast(24);  // 192.168.1.255
bool isPrivate = address1.IsPrivate();      // true
bool isLoopback = address1.IsLoopback();    // false

// JSON serialization works automatically
string json = JsonSerializer.Serialize(address1);  // "192.168.1.1"
var deserialized = JsonSerializer.Deserialize<Ipv4Address>(json);
```

### Network Operations with CIDR

```csharp
// Create networks
var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0"), 24);
var network2 = Ipv4Network.Parse("10.0.0.0/8");
var network3 = (Ipv4Network)"172.16.0.0/12";  // Implicit conversion

// Network properties
Console.WriteLine($"Network: {network.NetworkAddress}");        // 192.168.1.0
Console.WriteLine($"Broadcast: {network.BroadcastAddress}");     // 192.168.1.255
Console.WriteLine($"Total addresses: {network.TotalAddressCount}"); // 256
Console.WriteLine($"Host addresses: {network.HostCount}");       // 254
Console.WriteLine($"Subnet mask: {network.SubnetMask}");        // 255.255.255.0

// Check containment
bool contains = network.Contains(Ipv4Address.Parse("192.168.1.100")); // true
bool overlaps = network.Overlaps(Ipv4Network.Parse("192.168.1.128/25")); // true

// Enumerate addresses
foreach (var addr in network.GetHostAddresses().Take(5))
{
    Console.WriteLine(addr); // 192.168.1.1, 192.168.1.2, ...
}

// Subnet operations
var subnets = network.Subnet(26); // Split into /26 subnets
foreach (var subnet in subnets)
{
    Console.WriteLine(subnet); // 192.168.1.0/26, 192.168.1.64/26, ...
}
```

### Complete IPv4 Configuration

```csharp
// Create configurations
var config = new Ipv4Configuration(
    address: "192.168.1.100",
    prefixLength: 24,
    gateway: "192.168.1.1"
);

// Alternative creation methods
var config2 = Ipv4Configuration.Parse("192.168.1.100/24 gw 192.168.1.1");
var config3 = Ipv4Configuration.FromNetwork(
    network: "192.168.1.0/24",
    hostAddress: "192.168.1.100",
    gateway: "192.168.1.1"
);

// Configuration properties
Console.WriteLine($"Configuration: {config}");  // 192.168.1.100/24 gw 192.168.1.1
Console.WriteLine($"Network: {config.Network}"); // 192.168.1.0/24
Console.WriteLine($"Valid: {config.IsValid()}"); // true

// Reachability checks
bool canReach = config.CanReachDirectly("192.168.1.200"); // true (same network)
bool cannotReach = config.CanReachDirectly("10.0.0.1");   // false (different network)
```

## Advanced Usage

### Working with System.Net.IPAddress

```csharp
// Seamless conversion with System.Net.IPAddress
var ipAddress = IPAddress.Parse("192.168.1.1");
Ipv4Address address = ipAddress;  // Implicit conversion
IPAddress converted = address;    // Implicit conversion back

// Network interface integration
var config = new Ipv4Configuration("192.168.1.100/24 gw 192.168.1.1");
IPAddress gatewayIP = config.Gateway;  // Use with existing networking APIs
```

### JSON Serialization in ASP.NET Core

All types automatically work with System.Text.Json:

```csharp
public class NetworkSettings
{
    public Ipv4Address ServerAddress { get; set; }
    public Ipv4Network AllowedNetwork { get; set; }
    public Ipv4Configuration[] Interfaces { get; set; }
}

// In Program.cs - no additional configuration needed!
builder.Services.AddControllers(); // JSON serialization works automatically

// API Controller
[ApiController]
public class NetworkController : ControllerBase
{
    [HttpPost]
    public IActionResult ConfigureNetwork([FromBody] NetworkSettings settings)
    {
        // Automatically deserializes from JSON like:
        // {
        //   "serverAddress": "192.168.1.10",
        //   "allowedNetwork": "192.168.0.0/16",
        //   "interfaces": ["192.168.1.100/24 gw 192.168.1.1"]
        // }
        
        return Ok();
    }
}
```

### Network Analysis

```csharp
// Analyze network ranges
var network = Ipv4Network.Parse("192.168.0.0/16");

Console.WriteLine($"This network can hold {network.HostCount:N0} hosts");
Console.WriteLine($"First host: {network.FirstHostAddress}");
Console.WriteLine($"Last host: {network.LastHostAddress}");

// Check for RFC 1918 private networks
var networks = new[]
{
    "10.0.0.0/8",
    "172.16.0.0/12", 
    "192.168.0.0/16",
    "8.8.8.8/32"
};

foreach (var net in networks.Select(n => Ipv4Network.Parse(n)))
{
    var sample = net.FirstHostAddress;
    Console.WriteLine($"{net}: Private = {sample.IsPrivate()}");
}
```

### Configuration Validation

```csharp
// Validate network configurations
var configs = new[]
{
    "192.168.1.100/24 gw 192.168.1.1",    // Valid
    "192.168.1.0/24 gw 192.168.1.1",      // Invalid (network address)
    "192.168.1.100/24 gw 10.0.0.1",       // Invalid (gateway not in network)
    "192.168.1.100/31 gw 192.168.1.101"   // Valid (/31 special case)
};

foreach (var configStr in configs)
{
    if (Ipv4Configuration.TryParse(configStr, null, out var config))
    {
        Console.WriteLine($"{config}: {(config.IsValid() ? "✓ Valid" : "✗ Invalid")}");
    }
}
```

## API Reference

### Ipv4Address

Core IPv4 address representation with validation, parsing, and network operations.

**Key Methods:**
- `GetNetwork(int prefixLength)` - Calculate network address
- `GetBroadcast(int prefixLength)` - Calculate broadcast address  
- `IsInNetwork(Ipv4Address network, int prefixLength)` - Check network membership
- `IsPrivate()` - RFC 1918 private address check
- `IsLoopback()` - Loopback address check (127.0.0.0/8)
- `IsMulticast()` - Multicast address check (224.0.0.0/4)

### Ipv4Network

CIDR network representation with enumeration and subnet operations.

**Key Methods:**
- `Contains(Ipv4Address address)` - Check if address is in network
- `Overlaps(Ipv4Network other)` - Check network overlap
- `ContainsNetwork(Ipv4Network other)` - Check if this network contains another
- `GetAddresses()` - Enumerate all addresses (including network/broadcast)
- `GetHostAddresses()` - Enumerate only host addresses
- `Subnet(int newPrefixLength)` - Split into smaller subnets

### Ipv4Configuration

Complete network configuration with address, subnet, and gateway.

**Key Methods:**
- `IsValid()` - Validate configuration consistency
- `CanReachDirectly(Ipv4Address address)` - Check direct reachability
- `FromNetwork(network, hostAddress, gateway)` - Create from network
- `FromNetworkFirstHost(network, gateway)` - Use first available host

## Performance

This library is designed for high performance:

- **Zero allocations** for most value type operations
- **Efficient bit operations** for network calculations  
- **Minimal overhead** compared to manual implementations
- **Struct-based design** for optimal memory usage
- **Optimized parsing** with span-based operations where possible

## Requirements

- **.NET 9.0** or higher
- **ModelingEvolution.JsonParsableConverter** (automatically included)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This library extracts and enhances IPv4 functionality from the [EventPi NetworkMonitor](https://github.com/modelingevolution/rocket-welder2) project, providing a focused, reusable package for IPv4 operations.