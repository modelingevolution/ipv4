using System;
using System.Net;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents an IPv4 endpoint (address and port combination).
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a strongly-typed representation of network endpoints combining
/// an <see cref="Ipv4Address"/> with a <see cref="IpPort"/>. It implements
/// <see cref="IParsable{TSelf}"/> for seamless JSON serialization and provides
/// implicit conversions to/from <see cref="IPEndPoint"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create endpoints
/// var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
/// var endpoint2 = Ipv4Endpoint.Parse("192.168.1.100:443");
/// var endpoint3 = new Ipv4Endpoint(Ipv4Address.Loopback, IpPort.Http);
///
/// // From IPEndPoint
/// var ipEndPoint = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 3000);
/// Ipv4Endpoint endpoint4 = ipEndPoint;
///
/// // To IPEndPoint
/// IPEndPoint converted = endpoint1;
///
/// // Properties
/// var address = endpoint1.Address;  // 192.168.1.100
/// var port = endpoint1.Port;        // 8080
///
/// // String representation
/// string str = endpoint1.ToString(); // "192.168.1.100:8080"
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<Ipv4Endpoint>))]
public readonly record struct Ipv4Endpoint : IParsable<Ipv4Endpoint>, IComparable<Ipv4Endpoint>
{
    /// <summary>
    /// Gets the IPv4 address component.
    /// </summary>
    public Ipv4Address Address { get; }

    /// <summary>
    /// Gets the port component.
    /// </summary>
    public IpPort Port { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Endpoint"/> struct.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="port">The port number.</param>
    public Ipv4Endpoint(Ipv4Address address, IpPort port)
    {
        Address = address;
        Port = port;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Endpoint"/> struct.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="port">The port number (0-65535).</param>
    public Ipv4Endpoint(Ipv4Address address, int port)
    {
        Address = address;
        Port = new IpPort(port);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Endpoint"/> struct from string address.
    /// </summary>
    /// <param name="address">The IPv4 address as string.</param>
    /// <param name="port">The port number.</param>
    public Ipv4Endpoint(string address, int port)
    {
        Address = Ipv4Address.Parse(address, null);
        Port = new IpPort(port);
    }

    /// <summary>
    /// Creates an endpoint from an <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="endPoint">The IPEndPoint to convert.</param>
    /// <returns>A new <see cref="Ipv4Endpoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when endPoint is null.</exception>
    /// <exception cref="ArgumentException">Thrown when endPoint is not an IPv4 endpoint.</exception>
    public static Ipv4Endpoint FromIPEndPoint(IPEndPoint endPoint)
    {
        ArgumentNullException.ThrowIfNull(endPoint);

        if (endPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            throw new ArgumentException("IPEndPoint must be an IPv4 endpoint.", nameof(endPoint));

        return new Ipv4Endpoint(
            Ipv4Address.FromIPAddress(endPoint.Address),
            new IpPort(endPoint.Port)
        );
    }

    /// <summary>
    /// Converts this endpoint to an <see cref="IPEndPoint"/>.
    /// </summary>
    /// <returns>An equivalent <see cref="IPEndPoint"/>.</returns>
    public IPEndPoint ToIPEndPoint()
    {
        return new IPEndPoint(Address.ToIPAddress(), Port.Value);
    }

    /// <summary>
    /// Returns the string representation of this endpoint.
    /// </summary>
    /// <returns>A string in the format "address:port" (e.g., "192.168.1.100:8080").</returns>
    public override string ToString()
    {
        return $"{Address}:{Port}";
    }

    /// <summary>
    /// Parses a string representation of an IPv4 endpoint.
    /// </summary>
    /// <param name="s">A string containing an endpoint in the format "address:port".</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>An <see cref="Ipv4Endpoint"/> equivalent to the endpoint contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid endpoint.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static Ipv4Endpoint Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);

        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid IPv4 endpoint format: '{s}'. Expected format: 'address:port'");
    }

    /// <summary>
    /// Attempts to parse a string representation of an IPv4 endpoint.
    /// </summary>
    /// <param name="s">A string containing an endpoint in the format "address:port".</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">When this method returns, contains the parsed endpoint if successful.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Ipv4Endpoint result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        // Find the last colon (to handle potential future IPv6 support format)
        int colonIndex = s.LastIndexOf(':');
        if (colonIndex <= 0 || colonIndex >= s.Length - 1)
            return false;

        var addressPart = s.Substring(0, colonIndex);
        var portPart = s.Substring(colonIndex + 1);

        if (!Ipv4Address.TryParse(addressPart, provider, out var address))
            return false;

        if (!IpPort.TryParse(portPart, provider, out var port))
            return false;

        result = new Ipv4Endpoint(address, port);
        return true;
    }

    /// <summary>
    /// Compares this instance with another <see cref="Ipv4Endpoint"/>.
    /// </summary>
    /// <remarks>
    /// Endpoints are first compared by address, then by port.
    /// </remarks>
    public int CompareTo(Ipv4Endpoint other)
    {
        int addressComparison = Address.CompareTo(other.Address);
        return addressComparison != 0 ? addressComparison : Port.CompareTo(other.Port);
    }

    /// <summary>
    /// Implicitly converts a string to an <see cref="Ipv4Endpoint"/>.
    /// </summary>
    public static implicit operator Ipv4Endpoint(string value) => Parse(value, null);

    /// <summary>
    /// Implicitly converts an <see cref="Ipv4Endpoint"/> to an <see cref="IPEndPoint"/>.
    /// </summary>
    public static implicit operator IPEndPoint(Ipv4Endpoint endpoint) => endpoint.ToIPEndPoint();

    /// <summary>
    /// Implicitly converts an <see cref="IPEndPoint"/> to an <see cref="Ipv4Endpoint"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when IPEndPoint is not an IPv4 endpoint.</exception>
    public static implicit operator Ipv4Endpoint(IPEndPoint endPoint) => FromIPEndPoint(endPoint);

    /// <summary>
    /// Deconstructs this endpoint into its components.
    /// </summary>
    /// <param name="address">The IPv4 address.</param>
    /// <param name="port">The port number.</param>
    public void Deconstruct(out Ipv4Address address, out IpPort port)
    {
        address = Address;
        port = Port;
    }
}
