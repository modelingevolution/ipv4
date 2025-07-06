using System;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents an IPv4 network configuration with address, subnet, and gateway information.
/// </summary>
/// <remarks>
/// <para>
/// This record provides a complete IPv4 configuration typically used for network interfaces,
/// including the IP address, subnet prefix length, and gateway address. It supports
/// JSON serialization and provides convenient string representations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create configurations
/// var config = new Ipv4Configuration(
///     address: "192.168.1.100",
///     prefixLength: 24,
///     gateway: "192.168.1.1"
/// );
/// 
/// // Alternative creation methods
/// var config2 = Ipv4Configuration.Parse("192.168.1.100/24 gw 192.168.1.1");
/// var config3 = Ipv4Configuration.FromNetwork(
///     network: "192.168.1.0/24",
///     hostAddress: "192.168.1.100",
///     gateway: "192.168.1.1"
/// );
/// 
/// // Access properties
/// var network = config.Network;           // 192.168.1.0/24
/// var broadcast = config.BroadcastAddress; // 192.168.1.255
/// bool isValid = config.IsValid();        // true if gateway is in network
/// 
/// // String representation
/// Console.WriteLine(config); // "192.168.1.100/24 gw 192.168.1.1"
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<Ipv4Configuration>))]
public readonly record struct Ipv4Configuration : IParsable<Ipv4Configuration>
{
    /// <summary>
    /// Gets the IPv4 address.
    /// </summary>
    public Ipv4Address Address { get; }

    /// <summary>
    /// Gets the subnet prefix length (0-32).
    /// </summary>
    public int PrefixLength { get; }

    /// <summary>
    /// Gets the gateway IPv4 address.
    /// </summary>
    public Ipv4Address Gateway { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Configuration"/> struct.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="prefixLength">The subnet prefix length (0-32).</param>
    /// <param name="gateway">The gateway IPv4 address.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when prefixLength is outside the range 0-32.</exception>
    public Ipv4Configuration(Ipv4Address address, int prefixLength, Ipv4Address gateway)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(prefixLength, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prefixLength, 32);

        Address = address;
        PrefixLength = prefixLength;
        Gateway = gateway;
    }

    /// <summary>
    /// Gets the network that this configuration belongs to.
    /// </summary>
    public Ipv4Network Network => new(Address, PrefixLength);

    /// <summary>
    /// Gets the network address for this configuration.
    /// </summary>
    public Ipv4Address NetworkAddress => Address.GetNetwork(PrefixLength);

    /// <summary>
    /// Gets the broadcast address for this configuration.
    /// </summary>
    public Ipv4Address BroadcastAddress => Address.GetBroadcast(PrefixLength);

    /// <summary>
    /// Gets the subnet mask for this configuration.
    /// </summary>
    public Ipv4Address SubnetMask => Network.SubnetMask;

    /// <summary>
    /// Determines whether this configuration is valid.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the configuration is valid (address and gateway are in the same network);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A configuration is considered valid if:
    /// - The address is not the network address (unless it's a /31 or /32 network)
    /// - The address is not the broadcast address (unless it's a /31 or /32 network)
    /// - The gateway is within the same network as the address
    /// </remarks>
    public bool IsValid()
    {
        var network = Network;
        
        // For /31 and /32 networks, any address is valid
        if (PrefixLength >= 31)
            return network.Contains(Gateway);

        // Address should not be network or broadcast address
        if (Address == network.NetworkAddress || Address == network.BroadcastAddress)
            return false;

        // Gateway should be in the same network and not be network/broadcast address
        if (!network.Contains(Gateway))
            return false;

        if (Gateway == network.NetworkAddress || Gateway == network.BroadcastAddress)
            return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified address can communicate directly with this configuration
    /// (i.e., it's in the same network).
    /// </summary>
    /// <param name="other">The address to check.</param>
    /// <returns><c>true</c> if the address is in the same network; otherwise, <c>false</c>.</returns>
    public bool CanReachDirectly(Ipv4Address other)
    {
        return Network.Contains(other);
    }

    /// <summary>
    /// Creates an IPv4 configuration from a network and specific host address.
    /// </summary>
    /// <param name="network">The network.</param>
    /// <param name="hostAddress">The host address within the network.</param>
    /// <param name="gateway">The gateway address.</param>
    /// <returns>A new <see cref="Ipv4Configuration"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the host address is not within the specified network.</exception>
    public static Ipv4Configuration FromNetwork(Ipv4Network network, Ipv4Address hostAddress, Ipv4Address gateway)
    {
        if (!network.Contains(hostAddress))
            throw new ArgumentException($"Host address {hostAddress} is not within network {network}.", nameof(hostAddress));

        return new Ipv4Configuration(hostAddress, network.PrefixLength, gateway);
    }

    /// <summary>
    /// Creates an IPv4 configuration using the first available host address in the specified network.
    /// </summary>
    /// <param name="network">The network.</param>
    /// <param name="gateway">The gateway address.</param>
    /// <returns>A new <see cref="Ipv4Configuration"/> instance using the first host address.</returns>
    public static Ipv4Configuration FromNetworkFirstHost(Ipv4Network network, Ipv4Address gateway)
    {
        return new Ipv4Configuration(network.FirstHostAddress, network.PrefixLength, gateway);
    }

    /// <summary>
    /// Returns the string representation of this IPv4 configuration.
    /// </summary>
    /// <returns>A string in the format "address/prefix gw gateway" (e.g., "192.168.1.100/24 gw 192.168.1.1").</returns>
    public override string ToString()
    {
        return $"{Address}/{PrefixLength} gw {Gateway}";
    }

    /// <summary>
    /// Parses a string representation of an IPv4 configuration.
    /// </summary>
    /// <param name="s">A string containing an IPv4 configuration in the format "address/prefix gw gateway".</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>An <see cref="Ipv4Configuration"/> equivalent to the configuration contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid IPv4 configuration.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    /// <example>
    /// <code>
    /// var config = Ipv4Configuration.Parse("192.168.1.100/24 gw 192.168.1.1");
    /// var config2 = Ipv4Configuration.Parse("10.0.0.5/8 gateway 10.0.0.1");
    /// </code>
    /// </example>
    public static Ipv4Configuration Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        
        return TryParse(s, provider, out var result) 
            ? result 
            : throw new FormatException($"Invalid IPv4 configuration format: '{s}'. Expected format: 'address/prefix gw gateway'");
    }

    /// <summary>
    /// Attempts to parse a string representation of an IPv4 configuration.
    /// </summary>
    /// <param name="s">A string containing an IPv4 configuration.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="Ipv4Configuration"/> equivalent of the configuration
    /// contained in <paramref name="s"/>, if the conversion succeeded, or default if the conversion failed.
    /// </param>
    /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Supported formats:
    /// - "192.168.1.100/24 gw 192.168.1.1"
    /// - "192.168.1.100/24 gateway 192.168.1.1"
    /// - "192.168.1.100/24,192.168.1.1" (comma-separated)
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, out Ipv4Configuration result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(s))
            return false;

        try
        {
            // Try format: "address/prefix gw gateway" or "address/prefix gateway gateway"
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && (parts[1].Equals("gw", StringComparison.OrdinalIgnoreCase) || 
                                      parts[1].Equals("gateway", StringComparison.OrdinalIgnoreCase)))
            {
                // Parse address/prefix manually to preserve the original address
                var addressPrefixParts = parts[0].Split('/');
                if (addressPrefixParts.Length == 2 &&
                    Ipv4Address.TryParse(addressPrefixParts[0], provider, out var address) &&
                    int.TryParse(addressPrefixParts[1], out var prefixLength) &&
                    prefixLength >= 0 && prefixLength <= 32 &&
                    Ipv4Address.TryParse(parts[2], provider, out var gateway))
                {
                    result = new Ipv4Configuration(address, prefixLength, gateway);
                    return true;
                }
            }

            // Try format: "address/prefix,gateway"
            var commaParts = s.Split(',');
            if (commaParts.Length == 2)
            {
                // Parse address/prefix manually to preserve the original address
                var addressPrefixParts = commaParts[0].Trim().Split('/');
                if (addressPrefixParts.Length == 2 &&
                    Ipv4Address.TryParse(addressPrefixParts[0], provider, out var address) &&
                    int.TryParse(addressPrefixParts[1], out var prefixLength) &&
                    prefixLength >= 0 && prefixLength <= 32 &&
                    Ipv4Address.TryParse(commaParts[1].Trim(), provider, out var gateway))
                {
                    result = new Ipv4Configuration(address, prefixLength, gateway);
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Implicitly converts a string to an <see cref="Ipv4Configuration"/>.
    /// </summary>
    /// <param name="value">The string representation of an IPv4 configuration.</param>
    /// <returns>An <see cref="Ipv4Configuration"/> equivalent to the specified string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid IPv4 configuration.</exception>
    public static implicit operator Ipv4Configuration(string value)
    {
        return Parse(value, null);
    }
}