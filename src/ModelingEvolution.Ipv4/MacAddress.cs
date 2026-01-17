using System;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents a MAC (Media Access Control) hardware address.
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a strongly-typed representation of 48-bit MAC addresses with
/// built-in parsing, formatting, and comparison capabilities. It implements
/// <see cref="IParsable{TSelf}"/> for seamless integration with JSON serialization.
/// </para>
/// <para>
/// Supports multiple input formats: colon-separated (AA:BB:CC:DD:EE:FF),
/// dash-separated (AA-BB-CC-DD-EE-FF), and continuous (AABBCCDDEEFF).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create MAC addresses
/// var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
/// var mac2 = MacAddress.Parse("AA-BB-CC-DD-EE-FF");
/// var mac3 = MacAddress.Parse("AABBCCDDEEFF");
/// var mac4 = MacAddress.From(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
///
/// // Properties
/// bool isBroadcast = mac1.IsBroadcast;
/// bool isMulticast = mac1.IsMulticast;
/// bool isUnicast = mac1.IsUnicast;
/// byte[] bytes = mac1.GetBytes();
///
/// // String output (canonical colon format)
/// string str = mac1.ToString(); // "AA:BB:CC:DD:EE:FF"
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<MacAddress>))]
public readonly record struct MacAddress : IParsable<MacAddress>, IComparable<MacAddress>
{
    private readonly ulong _value; // Lower 48 bits used

    /// <summary>
    /// Gets the broadcast MAC address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public static MacAddress Broadcast { get; } = new(0xFFFFFFFFFFFFUL);

    /// <summary>
    /// Gets the zero/empty MAC address (00:00:00:00:00:00).
    /// </summary>
    public static MacAddress None { get; } = new(0UL);

    /// <summary>
    /// Initializes a new instance of the <see cref="MacAddress"/> struct from a 48-bit value.
    /// </summary>
    /// <param name="value">The MAC address as a 64-bit unsigned integer (lower 48 bits used).</param>
    public MacAddress(ulong value)
    {
        _value = value & 0xFFFFFFFFFFFFUL; // Mask to 48 bits
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacAddress"/> struct from six bytes.
    /// </summary>
    public MacAddress(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
    {
        _value = ((ulong)b0 << 40) | ((ulong)b1 << 32) | ((ulong)b2 << 24) |
                 ((ulong)b3 << 16) | ((ulong)b4 << 8) | b5;
    }

    /// <summary>
    /// Gets the raw 48-bit value of this MAC address.
    /// </summary>
    public ulong Value => _value;

    /// <summary>
    /// Gets the first byte of the MAC address.
    /// </summary>
    public byte B0 => (byte)((_value >> 40) & 0xFF);

    /// <summary>
    /// Gets the second byte of the MAC address.
    /// </summary>
    public byte B1 => (byte)((_value >> 32) & 0xFF);

    /// <summary>
    /// Gets the third byte of the MAC address.
    /// </summary>
    public byte B2 => (byte)((_value >> 24) & 0xFF);

    /// <summary>
    /// Gets the fourth byte of the MAC address.
    /// </summary>
    public byte B3 => (byte)((_value >> 16) & 0xFF);

    /// <summary>
    /// Gets the fifth byte of the MAC address.
    /// </summary>
    public byte B4 => (byte)((_value >> 8) & 0xFF);

    /// <summary>
    /// Gets the sixth byte of the MAC address.
    /// </summary>
    public byte B5 => (byte)(_value & 0xFF);

    /// <summary>
    /// Gets whether this is the broadcast address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public bool IsBroadcast => _value == 0xFFFFFFFFFFFFUL;

    /// <summary>
    /// Gets whether this is a multicast address (least significant bit of first byte is 1).
    /// </summary>
    public bool IsMulticast => (B0 & 0x01) == 0x01;

    /// <summary>
    /// Gets whether this is a unicast address (least significant bit of first byte is 0).
    /// </summary>
    public bool IsUnicast => (B0 & 0x01) == 0x00;

    /// <summary>
    /// Gets whether this is a locally administered address (second least significant bit of first byte is 1).
    /// </summary>
    public bool IsLocallyAdministered => (B0 & 0x02) == 0x02;

    /// <summary>
    /// Gets whether this is a universally administered (OUI-based) address.
    /// </summary>
    public bool IsUniversallyAdministered => (B0 & 0x02) == 0x00;

    /// <summary>
    /// Gets the OUI (Organizationally Unique Identifier) - the upper 24 bits.
    /// </summary>
    public uint Oui => (uint)(_value >> 24);

    /// <summary>
    /// Gets the bytes of this MAC address.
    /// </summary>
    /// <returns>A 6-byte array containing the MAC address.</returns>
    public byte[] GetBytes() => new[] { B0, B1, B2, B3, B4, B5 };

    /// <summary>
    /// Creates a <see cref="MacAddress"/> from a 48-bit unsigned integer.
    /// </summary>
    /// <param name="value">The MAC address as a 64-bit unsigned integer (lower 48 bits used).</param>
    /// <returns>A new <see cref="MacAddress"/> instance.</returns>
    public static MacAddress From(ulong value) => new(value);

    /// <summary>
    /// Creates a <see cref="MacAddress"/> from a byte array.
    /// </summary>
    /// <param name="bytes">A 6-byte array containing the MAC address.</param>
    /// <returns>A new <see cref="MacAddress"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bytes is null.</exception>
    /// <exception cref="ArgumentException">Thrown when bytes is not exactly 6 bytes.</exception>
    public static MacAddress From(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length != 6)
            throw new ArgumentException("MAC address must be exactly 6 bytes.", nameof(bytes));

        return new MacAddress(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
    }

    /// <summary>
    /// Creates a <see cref="MacAddress"/> from a span of bytes.
    /// </summary>
    /// <param name="bytes">A span containing exactly 6 bytes.</param>
    /// <returns>A new <see cref="MacAddress"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when span is not exactly 6 bytes.</exception>
    public static MacAddress From(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 6)
            throw new ArgumentException("MAC address must be exactly 6 bytes.", nameof(bytes));

        return new MacAddress(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
    }

    /// <summary>
    /// Returns the canonical string representation (colon-separated uppercase).
    /// </summary>
    /// <returns>A string in the format "AA:BB:CC:DD:EE:FF".</returns>
    public override string ToString()
    {
        return $"{B0:X2}:{B1:X2}:{B2:X2}:{B3:X2}:{B4:X2}:{B5:X2}";
    }

    /// <summary>
    /// Returns the string representation in the specified format.
    /// </summary>
    /// <param name="separator">The separator character (':' or '-'), or null for no separator.</param>
    /// <param name="uppercase">Whether to use uppercase hex digits.</param>
    /// <returns>The formatted MAC address string.</returns>
    public string ToString(char? separator, bool uppercase = true)
    {
        var format = uppercase ? "X2" : "x2";
        if (separator == null)
        {
            return $"{B0.ToString(format)}{B1.ToString(format)}{B2.ToString(format)}{B3.ToString(format)}{B4.ToString(format)}{B5.ToString(format)}";
        }
        return $"{B0.ToString(format)}{separator}{B1.ToString(format)}{separator}{B2.ToString(format)}{separator}{B3.ToString(format)}{separator}{B4.ToString(format)}{separator}{B5.ToString(format)}";
    }

    /// <summary>
    /// Parses a string representation of a MAC address.
    /// </summary>
    /// <param name="s">A string containing a MAC address.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>A <see cref="MacAddress"/> equivalent to the address contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid MAC address.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static MacAddress Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);

        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid MAC address format: '{s}'");
    }

    /// <summary>
    /// Attempts to parse a string representation of a MAC address.
    /// </summary>
    /// <param name="s">A string containing a MAC address.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">When this method returns, contains the parsed MAC address if successful.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Supports formats:
    /// - Colon-separated: "AA:BB:CC:DD:EE:FF"
    /// - Dash-separated: "AA-BB-CC-DD-EE-FF"
    /// - Continuous: "AABBCCDDEEFF"
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, out MacAddress result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        s = s.Trim();

        // Try colon-separated format
        if (s.Length == 17 && s[2] == ':')
        {
            return TryParseSeparated(s, ':', out result);
        }

        // Try dash-separated format
        if (s.Length == 17 && s[2] == '-')
        {
            return TryParseSeparated(s, '-', out result);
        }

        // Try continuous format
        if (s.Length == 12)
        {
            return TryParseContinuous(s, out result);
        }

        return false;
    }

    private static bool TryParseSeparated(string s, char separator, out MacAddress result)
    {
        result = default;
        var parts = s.Split(separator);
        if (parts.Length != 6)
            return false;

        Span<byte> bytes = stackalloc byte[6];
        for (int i = 0; i < 6; i++)
        {
            if (parts[i].Length != 2 || !byte.TryParse(parts[i], System.Globalization.NumberStyles.HexNumber, null, out bytes[i]))
                return false;
        }

        result = new MacAddress(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
        return true;
    }

    private static bool TryParseContinuous(string s, out MacAddress result)
    {
        result = default;
        Span<byte> bytes = stackalloc byte[6];

        for (int i = 0; i < 6; i++)
        {
            if (!byte.TryParse(s.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[i]))
                return false;
        }

        result = new MacAddress(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
        return true;
    }

    /// <summary>
    /// Compares this instance with another <see cref="MacAddress"/>.
    /// </summary>
    public int CompareTo(MacAddress other) => _value.CompareTo(other._value);

    /// <summary>
    /// Implicitly converts a string to a <see cref="MacAddress"/>.
    /// </summary>
    public static implicit operator MacAddress(string value) => Parse(value, null);

    /// <summary>
    /// Implicitly converts a byte array to a <see cref="MacAddress"/>.
    /// </summary>
    public static implicit operator MacAddress(byte[] bytes) => From(bytes);

    /// <summary>
    /// Implicitly converts a <see cref="ulong"/> to a <see cref="MacAddress"/>.
    /// </summary>
    public static implicit operator MacAddress(ulong value) => new(value);

    /// <summary>
    /// Explicitly converts a <see cref="MacAddress"/> to a <see cref="ulong"/>.
    /// </summary>
    public static explicit operator ulong(MacAddress mac) => mac._value;
}
