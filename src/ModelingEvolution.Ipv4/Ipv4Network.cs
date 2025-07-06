using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents an IPv4 network with CIDR notation support.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a lightweight representation of IPv4 networks using CIDR notation
/// (e.g., "192.168.1.0/24"). It supports network calculations, address enumeration,
/// and contains/overlaps operations.
/// </para>
/// <para>
/// The struct implements <see cref="IParsable{TSelf}"/> for seamless JSON serialization
/// and integration with .NET APIs.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create networks
/// var network = Ipv4Network.Parse("192.168.1.0/24");
/// var network2 = new Ipv4Network(Ipv4Address.Parse("10.0.0.0"), 8);
/// 
/// // Network properties
/// var networkAddr = network.NetworkAddress;    // 192.168.1.0
/// var broadcast = network.BroadcastAddress;    // 192.168.1.255
/// var hostCount = network.HostCount;           // 254
/// var totalCount = network.TotalAddressCount;  // 256
/// 
/// // Check if address is in network
/// bool contains = network.Contains(Ipv4Address.Parse("192.168.1.100")); // true
/// 
/// // Check network overlap
/// var other = Ipv4Network.Parse("192.168.1.128/25");
/// bool overlaps = network.Overlaps(other); // true
/// 
/// // Enumerate addresses
/// foreach (var addr in network.GetAddresses())
/// {
///     Console.WriteLine(addr);
/// }
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<Ipv4Network>))]
public readonly record struct Ipv4Network : IParsable<Ipv4Network>, IComparable<Ipv4Network>, IEnumerable<Ipv4Address>
{
    /// <summary>
    /// Gets the network address (base address with host bits set to zero).
    /// </summary>
    public Ipv4Address NetworkAddress { get; }

    /// <summary>
    /// Gets the subnet mask prefix length (0-32).
    /// </summary>
    public int PrefixLength { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ipv4Network"/> struct.
    /// </summary>
    /// <param name="address">Any address within the network.</param>
    /// <param name="prefixLength">The subnet mask prefix length (0-32).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when prefixLength is outside the range 0-32.</exception>
    public Ipv4Network(Ipv4Address address, int prefixLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(prefixLength, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(prefixLength, 32);

        PrefixLength = prefixLength;
        NetworkAddress = address.GetNetwork(prefixLength);
    }

    /// <summary>
    /// Gets the broadcast address for this network.
    /// </summary>
    public Ipv4Address BroadcastAddress => NetworkAddress.GetBroadcast(PrefixLength);

    /// <summary>
    /// Gets the subnet mask for this network.
    /// </summary>
    public Ipv4Address SubnetMask => PrefixLength == 0 
        ? Ipv4Address.Any 
        : new Ipv4Address(~((1u << (32 - PrefixLength)) - 1));

    /// <summary>
    /// Gets the total number of addresses in this network (including network and broadcast addresses).
    /// </summary>
    public long TotalAddressCount => PrefixLength == 32 ? 1 : 1L << (32 - PrefixLength);

    /// <summary>
    /// Gets the number of host addresses in this network (excluding network and broadcast addresses).
    /// </summary>
    /// <remarks>
    /// For /31 and /32 networks, this returns the total address count as these are special cases
    /// where all addresses can be used for hosts.
    /// </remarks>
    public long HostCount => PrefixLength >= 31 ? TotalAddressCount : Math.Max(0, TotalAddressCount - 2);

    /// <summary>
    /// Gets the first host address in this network.
    /// </summary>
    /// <remarks>
    /// For /31 and /32 networks, this returns the network address.
    /// For other networks, this returns the address after the network address.
    /// </remarks>
    public Ipv4Address FirstHostAddress => PrefixLength >= 31 
        ? NetworkAddress 
        : new Ipv4Address(NetworkAddress.Value + 1);

    /// <summary>
    /// Gets the last host address in this network.
    /// </summary>
    /// <remarks>
    /// For /31 and /32 networks, this returns the broadcast address.
    /// For other networks, this returns the address before the broadcast address.
    /// </remarks>
    public Ipv4Address LastHostAddress => PrefixLength >= 31 
        ? BroadcastAddress 
        : new Ipv4Address(BroadcastAddress.Value - 1);

    /// <summary>
    /// Determines whether the specified address is contained within this network.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns><c>true</c> if the address is within this network; otherwise, <c>false</c>.</returns>
    public bool Contains(Ipv4Address address)
    {
        return address.IsInNetwork(NetworkAddress, PrefixLength);
    }

    /// <summary>
    /// Determines whether this network overlaps with another network.
    /// </summary>
    /// <param name="other">The other network to check for overlap.</param>
    /// <returns><c>true</c> if the networks overlap; otherwise, <c>false</c>.</returns>
    public bool Overlaps(Ipv4Network other)
    {
        return Contains(other.NetworkAddress) || 
               Contains(other.BroadcastAddress) ||
               other.Contains(NetworkAddress) ||
               other.Contains(BroadcastAddress);
    }

    /// <summary>
    /// Determines whether this network completely contains another network.
    /// </summary>
    /// <param name="other">The other network to check.</param>
    /// <returns><c>true</c> if this network completely contains the other network; otherwise, <c>false</c>.</returns>
    public bool ContainsNetwork(Ipv4Network other)
    {
        return PrefixLength <= other.PrefixLength && 
               Contains(other.NetworkAddress) && 
               Contains(other.BroadcastAddress);
    }

    /// <summary>
    /// Gets all addresses in this network.
    /// </summary>
    /// <returns>An enumerable of all addresses in the network.</returns>
    /// <remarks>
    /// This method returns all addresses including the network and broadcast addresses.
    /// Use <see cref="GetHostAddresses"/> to get only usable host addresses.
    /// </remarks>
    public IEnumerable<Ipv4Address> GetAddresses()
    {
        uint start = NetworkAddress.Value;
        uint count = (uint)Math.Min(TotalAddressCount, uint.MaxValue);
        
        for (uint i = 0; i < count; i++)
        {
            yield return new Ipv4Address(start + i);
        }
    }

    /// <summary>
    /// Gets all host addresses in this network (excluding network and broadcast addresses).
    /// </summary>
    /// <returns>An enumerable of usable host addresses in the network.</returns>
    /// <remarks>
    /// For /31 and /32 networks, this returns all addresses as they are all usable for hosts.
    /// </remarks>
    public IEnumerable<Ipv4Address> GetHostAddresses()
    {
        if (PrefixLength >= 31)
        {
            // For /31 and /32, all addresses are host addresses
            foreach (var address in GetAddresses())
            {
                yield return address;
            }
            yield break;
        }

        uint start = FirstHostAddress.Value;
        uint end = LastHostAddress.Value;
        
        for (uint addr = start; addr <= end; addr++)
        {
            yield return new Ipv4Address(addr);
        }
    }

    /// <summary>
    /// Splits this network into smaller subnets with the specified prefix length.
    /// </summary>
    /// <param name="newPrefixLength">The prefix length for the new subnets (must be greater than current prefix length).</param>
    /// <returns>An enumerable of subnet networks.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when newPrefixLength is not greater than the current prefix length or is outside the range 0-32.</exception>
    /// <example>
    /// <code>
    /// var network = Ipv4Network.Parse("192.168.1.0/24");
    /// var subnets = network.Subnet(26); // Split into /26 subnets
    /// // Returns: 192.168.1.0/26, 192.168.1.64/26, 192.168.1.128/26, 192.168.1.192/26
    /// </code>
    /// </example>
    public IEnumerable<Ipv4Network> Subnet(int newPrefixLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(newPrefixLength, PrefixLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(newPrefixLength, 32);

        int hostBits = 32 - newPrefixLength;
        uint subnetSize = 1u << hostBits;
        uint subnetCount = 1u << (newPrefixLength - PrefixLength);
        
        uint currentNetwork = NetworkAddress.Value;
        
        for (uint i = 0; i < subnetCount; i++)
        {
            yield return new Ipv4Network(new Ipv4Address(currentNetwork), newPrefixLength);
            currentNetwork += subnetSize;
        }
    }

    /// <summary>
    /// Returns the string representation of this network in CIDR notation.
    /// </summary>
    /// <returns>A string in the format "x.x.x.x/y" representing this network.</returns>
    public override string ToString()
    {
        return $"{NetworkAddress}/{PrefixLength}";
    }

    /// <summary>
    /// Parses a string representation of an IPv4 network in CIDR notation.
    /// </summary>
    /// <param name="s">A string containing an IPv4 network in CIDR notation (e.g., "192.168.1.0/24").</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>An <see cref="Ipv4Network"/> equivalent to the network contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid IPv4 network in CIDR notation.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static Ipv4Network Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        
        return TryParse(s, provider, out var result) 
            ? result 
            : throw new FormatException($"Invalid IPv4 network format: '{s}'. Expected format: 'x.x.x.x/y'");
    }

    /// <summary>
    /// Attempts to parse a string representation of an IPv4 network in CIDR notation.
    /// </summary>
    /// <param name="s">A string containing an IPv4 network in CIDR notation.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="Ipv4Network"/> equivalent of the network
    /// contained in <paramref name="s"/>, if the conversion succeeded, or default if the conversion failed.
    /// </param>
    /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Ipv4Network result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(s))
            return false;

        string[] parts = s.Split('/');
        if (parts.Length != 2)
            return false;

        if (!Ipv4Address.TryParse(parts[0], provider, out var address))
            return false;

        if (!int.TryParse(parts[1], out var prefixLength) || 
            prefixLength < 0 || prefixLength > 32)
            return false;

        result = new Ipv4Network(address, prefixLength);
        return true;
    }

    /// <summary>
    /// Compares this instance with another <see cref="Ipv4Network"/> and returns an integer that indicates
    /// whether this instance precedes, follows, or occurs in the same position in the sort order as the other instance.
    /// </summary>
    /// <param name="other">An <see cref="Ipv4Network"/> to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared.
    /// Networks are first compared by network address, then by prefix length.
    /// </returns>
    public int CompareTo(Ipv4Network other)
    {
        int networkComparison = NetworkAddress.CompareTo(other.NetworkAddress);
        return networkComparison != 0 ? networkComparison : PrefixLength.CompareTo(other.PrefixLength);
    }

    /// <summary>
    /// Returns an enumerator that iterates through all addresses in this network.
    /// </summary>
    /// <returns>An enumerator for the addresses in this network.</returns>
    public IEnumerator<Ipv4Address> GetEnumerator()
    {
        return GetAddresses().GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through all addresses in this network.
    /// </summary>
    /// <returns>An enumerator for the addresses in this network.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Implicitly converts a string to an <see cref="Ipv4Network"/>.
    /// </summary>
    /// <param name="value">The string representation of an IPv4 network in CIDR notation.</param>
    /// <returns>An <see cref="Ipv4Network"/> equivalent to the specified string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid IPv4 network.</exception>
    public static implicit operator Ipv4Network(string value)
    {
        return Parse(value, null);
    }
}