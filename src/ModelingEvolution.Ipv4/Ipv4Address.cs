using System;
using System.Net;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents an IPv4 address with high-performance operations and JSON serialization support.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a lightweight, strongly-typed representation of IPv4 addresses with
/// built-in parsing, formatting, and network calculation capabilities. It implements
/// <see cref="IParsable{TSelf}"/> for seamless integration with JSON serialization and
/// various .NET APIs.
/// </para>
/// <para>
/// The internal representation uses a 32-bit unsigned integer in network byte order
/// for efficient storage and calculations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create IPv4 addresses
/// var localhost = Ipv4Address.Loopback;
/// var address = Ipv4Address.Parse("192.168.1.1");
/// var fromUint = new Ipv4Address(0xC0A80101u);
/// 
/// // Network calculations
/// var network = address.GetNetwork(24); // 192.168.1.0
/// var broadcast = address.GetBroadcast(24); // 192.168.1.255
/// bool isInNetwork = address.IsInNetwork(network, 24);
/// 
/// // JSON serialization works automatically
/// string json = JsonSerializer.Serialize(address);
/// Ipv4Address deserialized = JsonSerializer.Deserialize&lt;Ipv4Address&gt;(json);
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<Ipv4Address>))]
public readonly record struct Ipv4Address : IParsable<Ipv4Address>, IComparable<Ipv4Address>
{
    private readonly uint _value;

    /// <summary>
    /// Gets the loopback address (127.0.0.1).
    /// </summary>
    public static Ipv4Address Loopback { get; } = new(0x7F000001u);

    /// <summary>
    /// Gets the "any" address (0.0.0.0).
    /// </summary>
    public static Ipv4Address Any { get; } = new(0u);

    /// <summary>
    /// Gets the broadcast address (255.255.255.255).
    /// </summary>
    public static Ipv4Address Broadcast { get; } = new(0xFFFFFFFFu);

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Address"/> struct from a 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The IPv4 address as a 32-bit unsigned integer in network byte order.</param>
    public Ipv4Address(uint value)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Address"/> struct from four octets.
    /// </summary>
    /// <param name="a">The first octet (0-255).</param>
    /// <param name="b">The second octet (0-255).</param>
    /// <param name="c">The third octet (0-255).</param>
    /// <param name="d">The fourth octet (0-255).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any octet is outside the range 0-255.</exception>
    public Ipv4Address(byte a, byte b, byte c, byte d)
    {
        _value = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d;
    }

    /// <summary>
    /// Gets the 32-bit unsigned integer representation of this IPv4 address in network byte order.
    /// </summary>
    public uint Value => _value;

    /// <summary>
    /// Gets the first octet of the IPv4 address.
    /// </summary>
    public byte A => (byte)((_value >> 24) & 0xFF);

    /// <summary>
    /// Gets the second octet of the IPv4 address.
    /// </summary>
    public byte B => (byte)((_value >> 16) & 0xFF);

    /// <summary>
    /// Gets the third octet of the IPv4 address.
    /// </summary>
    public byte C => (byte)((_value >> 8) & 0xFF);

    /// <summary>
    /// Gets the fourth octet of the IPv4 address.
    /// </summary>
    public byte D => (byte)(_value & 0xFF);

    /// <summary>
    /// Calculates the network address for this IP address with the specified subnet mask prefix length.
    /// </summary>
    /// <param name="prefixLength">The subnet mask prefix length (0-32).</param>
    /// <returns>The network address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when prefixLength is outside the range 0-32.</exception>
    /// <example>
    /// <code>
    /// var address = Ipv4Address.Parse("192.168.1.100");
    /// var network = address.GetNetwork(24); // Returns 192.168.1.0
    /// </code>
    /// </example>
    public Ipv4Address GetNetwork(int prefixLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(prefixLength, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prefixLength, 32);

        if (prefixLength == 0) return Any;
        
        uint mask = ~((1u << (32 - prefixLength)) - 1);
        return new Ipv4Address(mask & _value);
    }

    /// <summary>
    /// Calculates the broadcast address for this IP address with the specified subnet mask prefix length.
    /// </summary>
    /// <param name="prefixLength">The subnet mask prefix length (0-32).</param>
    /// <returns>The broadcast address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when prefixLength is outside the range 0-32.</exception>
    /// <example>
    /// <code>
    /// var address = Ipv4Address.Parse("192.168.1.100");
    /// var broadcast = address.GetBroadcast(24); // Returns 192.168.1.255
    /// </code>
    /// </example>
    public Ipv4Address GetBroadcast(int prefixLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(prefixLength, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prefixLength, 32);

        if (prefixLength == 0) return Broadcast;
        
        uint hostMask = (1u << (32 - prefixLength)) - 1;
        uint networkAddress = GetNetwork(prefixLength)._value;
        return new Ipv4Address(networkAddress | hostMask);
    }

    /// <summary>
    /// Determines whether this IP address is within the specified network.
    /// </summary>
    /// <param name="networkAddress">The network address.</param>
    /// <param name="prefixLength">The subnet mask prefix length (0-32).</param>
    /// <returns><c>true</c> if this address is within the specified network; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when prefixLength is outside the range 0-32.</exception>
    /// <example>
    /// <code>
    /// var address = Ipv4Address.Parse("192.168.1.100");
    /// var network = Ipv4Address.Parse("192.168.1.0");
    /// bool isInNetwork = address.IsInNetwork(network, 24); // Returns true
    /// </code>
    /// </example>
    public bool IsInNetwork(Ipv4Address networkAddress, int prefixLength)
    {
        return GetNetwork(prefixLength) == networkAddress.GetNetwork(prefixLength);
    }

    /// <summary>
    /// Determines whether this IP address represents a private address according to RFC 1918.
    /// </summary>
    /// <returns><c>true</c> if this is a private address; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Private address ranges are:
    /// - 10.0.0.0/8 (10.0.0.0 to 10.255.255.255)
    /// - 172.16.0.0/12 (172.16.0.0 to 172.31.255.255)
    /// - 192.168.0.0/16 (192.168.0.0 to 192.168.255.255)
    /// </remarks>
    public bool IsPrivate()
    {
        // 10.0.0.0/8
        if (IsInNetwork(new Ipv4Address(10, 0, 0, 0), 8)) return true;
        
        // 172.16.0.0/12
        if (IsInNetwork(new Ipv4Address(172, 16, 0, 0), 12)) return true;
        
        // 192.168.0.0/16
        if (IsInNetwork(new Ipv4Address(192, 168, 0, 0), 16)) return true;
        
        return false;
    }

    /// <summary>
    /// Determines whether this IP address represents a loopback address (127.0.0.0/8).
    /// </summary>
    /// <returns><c>true</c> if this is a loopback address; otherwise, <c>false</c>.</returns>
    public bool IsLoopback()
    {
        return IsInNetwork(new Ipv4Address(127, 0, 0, 0), 8);
    }

    /// <summary>
    /// Determines whether this IP address represents a multicast address (224.0.0.0/4).
    /// </summary>
    /// <returns><c>true</c> if this is a multicast address; otherwise, <c>false</c>.</returns>
    public bool IsMulticast()
    {
        return IsInNetwork(new Ipv4Address(224, 0, 0, 0), 4);
    }

    /// <summary>
    /// Converts this IPv4 address to its standard dotted decimal string representation.
    /// </summary>
    /// <returns>A string representation of this IPv4 address (e.g., "192.168.1.1").</returns>
    public override string ToString()
    {
        return $"{A}.{B}.{C}.{D}";
    }

    /// <summary>
    /// Converts this IPv4 address to a <see cref="System.Net.IPAddress"/> instance.
    /// </summary>
    /// <returns>An <see cref="IPAddress"/> representing this IPv4 address.</returns>
    public IPAddress ToIPAddress()
    {
        return new IPAddress(new[] { A, B, C, D });
    }

    /// <summary>
    /// Parses a string representation of an IPv4 address.
    /// </summary>
    /// <param name="s">A string containing an IPv4 address in dotted decimal notation.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>An <see cref="Ipv4Address"/> equivalent to the IPv4 address contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid IPv4 address.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static Ipv4Address Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        
        return TryParse(s, provider, out var result) 
            ? result 
            : throw new FormatException($"Invalid IPv4 address format: '{s}'");
    }

    /// <summary>
    /// Attempts to parse a string representation of an IPv4 address.
    /// </summary>
    /// <param name="s">A string containing an IPv4 address in dotted decimal notation.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="Ipv4Address"/> equivalent of the IPv4 address
    /// contained in <paramref name="s"/>, if the conversion succeeded, or default if the conversion failed.
    /// </param>
    /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Ipv4Address result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(s))
            return false;

        string[] parts = s.Split('.');
        if (parts.Length != 4)
            return false;

        Span<byte> octets = stackalloc byte[4];
        for (int i = 0; i < 4; i++)
        {
            if (!byte.TryParse(parts[i], out octets[i]))
                return false;
        }

        result = new Ipv4Address(octets[0], octets[1], octets[2], octets[3]);
        return true;
    }

    /// <summary>
    /// Creates an <see cref="Ipv4Address"/> from a <see cref="System.Net.IPAddress"/>.
    /// </summary>
    /// <param name="ipAddress">The <see cref="IPAddress"/> to convert.</param>
    /// <returns>An <see cref="Ipv4Address"/> equivalent to the specified <see cref="IPAddress"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ipAddress"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ipAddress"/> is not an IPv4 address.</exception>
    public static Ipv4Address FromIPAddress(IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        
        if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            throw new ArgumentException("IPAddress must be an IPv4 address.", nameof(ipAddress));

        var bytes = ipAddress.GetAddressBytes();
        return new Ipv4Address(bytes[0], bytes[1], bytes[2], bytes[3]);
    }

    /// <summary>
    /// Compares this instance with another <see cref="Ipv4Address"/> and returns an integer that indicates
    /// whether this instance precedes, follows, or occurs in the same position in the sort order as the other instance.
    /// </summary>
    /// <param name="other">An <see cref="Ipv4Address"/> to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared:
    /// - Less than zero: This instance precedes <paramref name="other"/> in the sort order.
    /// - Zero: This instance occurs in the same position in the sort order as <paramref name="other"/>.
    /// - Greater than zero: This instance follows <paramref name="other"/> in the sort order.
    /// </returns>
    public int CompareTo(Ipv4Address other)
    {
        return _value.CompareTo(other._value);
    }

    /// <summary>
    /// Implicitly converts a string to an <see cref="Ipv4Address"/>.
    /// </summary>
    /// <param name="value">The string representation of an IPv4 address.</param>
    /// <returns>An <see cref="Ipv4Address"/> equivalent to the specified string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid IPv4 address.</exception>
    public static implicit operator Ipv4Address(string value)
    {
        return Parse(value, null);
    }

    /// <summary>
    /// Implicitly converts a 32-bit unsigned integer to an <see cref="Ipv4Address"/>.
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer representation of an IPv4 address in network byte order.</param>
    /// <returns>An <see cref="Ipv4Address"/> equivalent to the specified integer.</returns>
    public static implicit operator Ipv4Address(uint value)
    {
        return new Ipv4Address(value);
    }

    /// <summary>
    /// Explicitly converts an <see cref="Ipv4Address"/> to a 32-bit unsigned integer.
    /// </summary>
    /// <param name="address">The IPv4 address to convert.</param>
    /// <returns>The 32-bit unsigned integer representation in network byte order.</returns>
    public static explicit operator uint(Ipv4Address address)
    {
        return address._value;
    }

    /// <summary>
    /// Implicitly converts an <see cref="Ipv4Address"/> to a <see cref="System.Net.IPAddress"/>.
    /// </summary>
    /// <param name="address">The IPv4 address to convert.</param>
    /// <returns>An <see cref="IPAddress"/> equivalent to the specified <see cref="Ipv4Address"/>.</returns>
    public static implicit operator IPAddress(Ipv4Address address)
    {
        return address.ToIPAddress();
    }

    /// <summary>
    /// Implicitly converts a <see cref="System.Net.IPAddress"/> to an <see cref="Ipv4Address"/>.
    /// </summary>
    /// <param name="ipAddress">The <see cref="IPAddress"/> to convert.</param>
    /// <returns>An <see cref="Ipv4Address"/> equivalent to the specified <see cref="IPAddress"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the <see cref="IPAddress"/> is not an IPv4 address.</exception>
    public static implicit operator Ipv4Address(IPAddress ipAddress)
    {
        return FromIPAddress(ipAddress);
    }
}