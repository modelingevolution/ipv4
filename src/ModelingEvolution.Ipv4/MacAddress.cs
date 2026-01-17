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
    private readonly byte _b0, _b1, _b2, _b3, _b4, _b5;

    /// <summary>
    /// Gets the broadcast MAC address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public static MacAddress Broadcast { get; } = new(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);

    /// <summary>
    /// Gets the zero/empty MAC address (00:00:00:00:00:00).
    /// </summary>
    public static MacAddress None { get; } = new(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="MacAddress"/> struct from six bytes.
    /// </summary>
    public MacAddress(byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
    {
        _b0 = b0;
        _b1 = b1;
        _b2 = b2;
        _b3 = b3;
        _b4 = b4;
        _b5 = b5;
    }

    /// <summary>
    /// Gets the first byte of the MAC address.
    /// </summary>
    public byte B0 => _b0;

    /// <summary>
    /// Gets the second byte of the MAC address.
    /// </summary>
    public byte B1 => _b1;

    /// <summary>
    /// Gets the third byte of the MAC address.
    /// </summary>
    public byte B2 => _b2;

    /// <summary>
    /// Gets the fourth byte of the MAC address.
    /// </summary>
    public byte B3 => _b3;

    /// <summary>
    /// Gets the fifth byte of the MAC address.
    /// </summary>
    public byte B4 => _b4;

    /// <summary>
    /// Gets the sixth byte of the MAC address.
    /// </summary>
    public byte B5 => _b5;

    /// <summary>
    /// Gets whether this is the broadcast address (FF:FF:FF:FF:FF:FF).
    /// </summary>
    public bool IsBroadcast => _b0 == 0xFF && _b1 == 0xFF && _b2 == 0xFF &&
                               _b3 == 0xFF && _b4 == 0xFF && _b5 == 0xFF;

    /// <summary>
    /// Gets whether this is a multicast address (least significant bit of first byte is 1).
    /// </summary>
    public bool IsMulticast => (_b0 & 0x01) == 0x01;

    /// <summary>
    /// Gets whether this is a unicast address (least significant bit of first byte is 0).
    /// </summary>
    public bool IsUnicast => (_b0 & 0x01) == 0x00;

    /// <summary>
    /// Gets whether this is a locally administered address (second least significant bit of first byte is 1).
    /// </summary>
    public bool IsLocallyAdministered => (_b0 & 0x02) == 0x02;

    /// <summary>
    /// Gets whether this is a universally administered (OUI-based) address.
    /// </summary>
    public bool IsUniversallyAdministered => (_b0 & 0x02) == 0x00;

    /// <summary>
    /// Gets the OUI (Organizationally Unique Identifier) - the first three bytes.
    /// </summary>
    public (byte, byte, byte) Oui => (_b0, _b1, _b2);

    /// <summary>
    /// Gets the bytes of this MAC address.
    /// </summary>
    /// <returns>A 6-byte array containing the MAC address.</returns>
    public byte[] GetBytes() => new[] { _b0, _b1, _b2, _b3, _b4, _b5 };

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
        return $"{_b0:X2}:{_b1:X2}:{_b2:X2}:{_b3:X2}:{_b4:X2}:{_b5:X2}";
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
            return $"{_b0.ToString(format)}{_b1.ToString(format)}{_b2.ToString(format)}{_b3.ToString(format)}{_b4.ToString(format)}{_b5.ToString(format)}";
        }
        return $"{_b0.ToString(format)}{separator}{_b1.ToString(format)}{separator}{_b2.ToString(format)}{separator}{_b3.ToString(format)}{separator}{_b4.ToString(format)}{separator}{_b5.ToString(format)}";
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
    public int CompareTo(MacAddress other)
    {
        int cmp = _b0.CompareTo(other._b0);
        if (cmp != 0) return cmp;
        cmp = _b1.CompareTo(other._b1);
        if (cmp != 0) return cmp;
        cmp = _b2.CompareTo(other._b2);
        if (cmp != 0) return cmp;
        cmp = _b3.CompareTo(other._b3);
        if (cmp != 0) return cmp;
        cmp = _b4.CompareTo(other._b4);
        if (cmp != 0) return cmp;
        return _b5.CompareTo(other._b5);
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="MacAddress"/>.
    /// </summary>
    public static implicit operator MacAddress(string value) => Parse(value, null);

    /// <summary>
    /// Implicitly converts a byte array to a <see cref="MacAddress"/>.
    /// </summary>
    public static implicit operator MacAddress(byte[] bytes) => From(bytes);
}
