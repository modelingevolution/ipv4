using System;
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

namespace ModelingEvolution.Ipv4;

/// <summary>
/// Represents a TCP/UDP port number (0-65535).
/// </summary>
/// <remarks>
/// <para>
/// This struct provides a strongly-typed representation of network ports with
/// built-in validation, well-known port constants, and range classification.
/// It implements <see cref="IParsable{TSelf}"/> for seamless JSON serialization.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create ports
/// var port1 = new Port(8080);
/// var port2 = Port.Parse("443");
/// var port3 = Port.Http;
///
/// // Properties
/// bool isWellKnown = port1.IsWellKnown;     // false (8080 > 1023)
/// bool isRegistered = port1.IsRegistered;   // true (1024-49151)
/// bool isDynamic = port1.IsDynamic;         // false
///
/// // Implicit conversions
/// ushort value = port1;  // 8080
/// Port port4 = 3000;     // implicit from int
///
/// // Well-known ports
/// var http = Port.Http;        // 80
/// var https = Port.Https;      // 443
/// var ssh = Port.Ssh;          // 22
/// </code>
/// </example>
[JsonConverter(typeof(JsonParsableConverter<Port>))]
public readonly record struct Port : IParsable<Port>, IComparable<Port>
{
    private readonly ushort _value;

    #region Well-Known Ports (0-1023)

    /// <summary>FTP Data (20)</summary>
    public static Port FtpData { get; } = new(20);

    /// <summary>FTP Control (21)</summary>
    public static Port Ftp { get; } = new(21);

    /// <summary>SSH (22)</summary>
    public static Port Ssh { get; } = new(22);

    /// <summary>Telnet (23)</summary>
    public static Port Telnet { get; } = new(23);

    /// <summary>SMTP (25)</summary>
    public static Port Smtp { get; } = new(25);

    /// <summary>DNS (53)</summary>
    public static Port Dns { get; } = new(53);

    /// <summary>DHCP Server (67)</summary>
    public static Port DhcpServer { get; } = new(67);

    /// <summary>DHCP Client (68)</summary>
    public static Port DhcpClient { get; } = new(68);

    /// <summary>TFTP (69)</summary>
    public static Port Tftp { get; } = new(69);

    /// <summary>HTTP (80)</summary>
    public static Port Http { get; } = new(80);

    /// <summary>Kerberos (88)</summary>
    public static Port Kerberos { get; } = new(88);

    /// <summary>POP3 (110)</summary>
    public static Port Pop3 { get; } = new(110);

    /// <summary>NTP (123)</summary>
    public static Port Ntp { get; } = new(123);

    /// <summary>NetBIOS Name Service (137)</summary>
    public static Port NetBiosNs { get; } = new(137);

    /// <summary>NetBIOS Session Service (139)</summary>
    public static Port NetBiosSs { get; } = new(139);

    /// <summary>IMAP (143)</summary>
    public static Port Imap { get; } = new(143);

    /// <summary>SNMP (161)</summary>
    public static Port Snmp { get; } = new(161);

    /// <summary>LDAP (389)</summary>
    public static Port Ldap { get; } = new(389);

    /// <summary>HTTPS (443)</summary>
    public static Port Https { get; } = new(443);

    /// <summary>SMB (445)</summary>
    public static Port Smb { get; } = new(445);

    /// <summary>SMTP over TLS (465)</summary>
    public static Port SmtpTls { get; } = new(465);

    /// <summary>LDAPS (636)</summary>
    public static Port Ldaps { get; } = new(636);

    /// <summary>SMTP Submission (587)</summary>
    public static Port SmtpSubmission { get; } = new(587);

    /// <summary>IMAPS (993)</summary>
    public static Port Imaps { get; } = new(993);

    /// <summary>POP3S (995)</summary>
    public static Port Pop3s { get; } = new(995);

    #endregion

    #region Common Registered Ports (1024-49151)

    /// <summary>SOCKS Proxy (1080)</summary>
    public static Port Socks { get; } = new(1080);

    /// <summary>OpenVPN (1194)</summary>
    public static Port OpenVpn { get; } = new(1194);

    /// <summary>MS SQL Server (1433)</summary>
    public static Port MsSql { get; } = new(1433);

    /// <summary>Oracle Database (1521)</summary>
    public static Port Oracle { get; } = new(1521);

    /// <summary>MySQL (3306)</summary>
    public static Port MySql { get; } = new(3306);

    /// <summary>RDP (3389)</summary>
    public static Port Rdp { get; } = new(3389);

    /// <summary>PostgreSQL (5432)</summary>
    public static Port PostgreSql { get; } = new(5432);

    /// <summary>VNC (5900)</summary>
    public static Port Vnc { get; } = new(5900);

    /// <summary>Redis (6379)</summary>
    public static Port Redis { get; } = new(6379);

    /// <summary>HTTP Proxy (8080)</summary>
    public static Port HttpProxy { get; } = new(8080);

    /// <summary>HTTPS Alternate (8443)</summary>
    public static Port HttpsAlt { get; } = new(8443);

    /// <summary>Elasticsearch HTTP (9200)</summary>
    public static Port Elasticsearch { get; } = new(9200);

    /// <summary>MongoDB (27017)</summary>
    public static Port MongoDb { get; } = new(27017);

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Port"/> struct.
    /// </summary>
    /// <param name="value">The port number (0-65535).</param>
    public Port(ushort value)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Port"/> struct.
    /// </summary>
    /// <param name="value">The port number (must be 0-65535).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside 0-65535 range.</exception>
    public Port(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, ushort.MaxValue);
        _value = (ushort)value;
    }

    /// <summary>
    /// Gets the port number value.
    /// </summary>
    public ushort Value => _value;

    /// <summary>
    /// Gets whether this is a well-known port (0-1023).
    /// </summary>
    /// <remarks>
    /// Well-known ports are assigned by IANA and typically require root/admin privileges to bind.
    /// </remarks>
    public bool IsWellKnown => _value <= 1023;

    /// <summary>
    /// Gets whether this is a registered port (1024-49151).
    /// </summary>
    /// <remarks>
    /// Registered ports are assigned by IANA for specific services but don't require special privileges.
    /// </remarks>
    public bool IsRegistered => _value >= 1024 && _value <= 49151;

    /// <summary>
    /// Gets whether this is a dynamic/ephemeral port (49152-65535).
    /// </summary>
    /// <remarks>
    /// Dynamic ports are used for temporary connections and are typically assigned by the OS.
    /// </remarks>
    public bool IsDynamic => _value >= 49152;

    /// <summary>
    /// Returns the string representation of this port.
    /// </summary>
    public override string ToString() => _value.ToString();

    /// <summary>
    /// Parses a string representation of a port number.
    /// </summary>
    /// <param name="s">A string containing a port number.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <returns>A <see cref="Port"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="s"/> is not a valid port number.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
    public static Port Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"Invalid port number: '{s}'");
    }

    /// <summary>
    /// Attempts to parse a string representation of a port number.
    /// </summary>
    /// <param name="s">A string containing a port number.</param>
    /// <param name="provider">An object that provides culture-specific formatting information (not used).</param>
    /// <param name="result">When this method returns, contains the parsed port if successful.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out Port result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        if (!ushort.TryParse(s.Trim(), out var value))
            return false;

        result = new Port(value);
        return true;
    }

    /// <summary>
    /// Compares this instance with another <see cref="Port"/>.
    /// </summary>
    public int CompareTo(Port other) => _value.CompareTo(other._value);

    /// <summary>
    /// Implicitly converts a <see cref="Port"/> to a <see cref="ushort"/>.
    /// </summary>
    public static implicit operator ushort(Port port) => port._value;

    /// <summary>
    /// Implicitly converts a <see cref="ushort"/> to a <see cref="Port"/>.
    /// </summary>
    public static implicit operator Port(ushort value) => new(value);

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="Port"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside 0-65535 range.</exception>
    public static implicit operator Port(int value) => new(value);

    /// <summary>
    /// Implicitly converts a string to a <see cref="Port"/>.
    /// </summary>
    public static implicit operator Port(string value) => Parse(value, null);
}
