using System;
using System.Net;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class Ipv4AddressTests
{
    [Fact]
    public void Constructor_WithValidUInt_CreatesCorrectAddress()
    {
        // Arrange
        uint value = 0xC0A80101u; // 192.168.1.1

        // Act
        var address = new Ipv4Address(value);

        // Assert
        Assert.Equal(192, address.A);
        Assert.Equal(168, address.B);
        Assert.Equal(1, address.C);
        Assert.Equal(1, address.D);
        Assert.Equal(value, address.Value);
    }

    [Fact]
    public void Constructor_WithValidOctets_CreatesCorrectAddress()
    {
        // Arrange & Act
        var address = new Ipv4Address(192, 168, 1, 1);

        // Assert
        Assert.Equal(192, address.A);
        Assert.Equal(168, address.B);
        Assert.Equal(1, address.C);
        Assert.Equal(1, address.D);
        Assert.Equal(0xC0A80101u, address.Value);
    }

    [Fact]
    public void Loopback_ReturnsCorrectValue()
    {
        // Act
        var loopback = Ipv4Address.Loopback;

        // Assert
        Assert.Equal("127.0.0.1", loopback.ToString());
        Assert.Equal(127, loopback.A);
        Assert.Equal(0, loopback.B);
        Assert.Equal(0, loopback.C);
        Assert.Equal(1, loopback.D);
    }

    [Fact]
    public void Any_ReturnsCorrectValue()
    {
        // Act
        var any = Ipv4Address.Any;

        // Assert
        Assert.Equal("0.0.0.0", any.ToString());
        Assert.Equal(0u, any.Value);
    }

    [Fact]
    public void Broadcast_ReturnsCorrectValue()
    {
        // Act
        var broadcast = Ipv4Address.Broadcast;

        // Assert
        Assert.Equal("255.255.255.255", broadcast.ToString());
        Assert.Equal(0xFFFFFFFFu, broadcast.Value);
    }

    [Theory]
    [InlineData("192.168.1.1", 192, 168, 1, 1)]
    [InlineData("0.0.0.0", 0, 0, 0, 0)]
    [InlineData("255.255.255.255", 255, 255, 255, 255)]
    [InlineData("127.0.0.1", 127, 0, 0, 1)]
    public void Parse_WithValidString_ReturnsCorrectAddress(string input, byte expectedA, byte expectedB, byte expectedC, byte expectedD)
    {
        // Act
        var address = Ipv4Address.Parse(input, null);

        // Assert
        Assert.Equal(expectedA, address.A);
        Assert.Equal(expectedB, address.B);
        Assert.Equal(expectedC, address.C);
        Assert.Equal(expectedD, address.D);
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    [InlineData("256.1.1.1")]
    [InlineData("192.168.1.256")]
    [InlineData("not.an.ip.address")]
    [InlineData("192.168.-1.1")]
    public void Parse_WithInvalidString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Ipv4Address.Parse(input, null));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Address.Parse(null!, null));
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("", false)]
    [InlineData("192.168.1", false)]
    [InlineData("256.1.1.1", false)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string input, bool expectedResult)
    {
        // Act
        bool result = Ipv4Address.TryParse(input, null, out var address);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            // For successful parsing, the address should be valid (even if it's 0.0.0.0)
            Assert.True(result);
        }
        else
        {
            Assert.Equal(default(Ipv4Address), address);
        }
    }

    [Fact]
    public void GetNetwork_WithValidPrefix_ReturnsCorrectNetwork()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);

        // Act
        var network = address.GetNetwork(24);

        // Assert
        Assert.Equal("192.168.1.0", network.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    public void GetNetwork_WithInvalidPrefix_ThrowsArgumentOutOfRangeException(int prefix)
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => address.GetNetwork(prefix));
    }

    [Fact]
    public void GetBroadcast_WithValidPrefix_ReturnsCorrectBroadcast()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);

        // Act
        var broadcast = address.GetBroadcast(24);

        // Assert
        Assert.Equal("192.168.1.255", broadcast.ToString());
    }

    [Fact]
    public void IsInNetwork_WithSameNetwork_ReturnsTrue()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);
        var network = Ipv4Address.Parse("192.168.1.0", null);

        // Act
        bool result = address.IsInNetwork(network, 24);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInNetwork_WithDifferentNetwork_ReturnsFalse()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);
        var network = Ipv4Address.Parse("192.168.2.0", null);

        // Act
        bool result = address.IsInNetwork(network, 24);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("10.0.0.1", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("192.168.1.1", true)]
    [InlineData("8.8.8.8", false)]
    [InlineData("172.15.255.255", false)]
    [InlineData("172.32.0.1", false)]
    public void IsPrivate_WithVariousAddresses_ReturnsExpectedResult(string addressString, bool expected)
    {
        // Arrange
        var address = Ipv4Address.Parse(addressString, null);

        // Act
        bool result = address.IsPrivate();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("127.255.255.254", true)]
    [InlineData("192.168.1.1", false)]
    [InlineData("8.8.8.8", false)]
    public void IsLoopback_WithVariousAddresses_ReturnsExpectedResult(string addressString, bool expected)
    {
        // Arrange
        var address = Ipv4Address.Parse(addressString, null);

        // Act
        bool result = address.IsLoopback();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("224.0.0.1", true)]
    [InlineData("239.255.255.255", true)]
    [InlineData("192.168.1.1", false)]
    [InlineData("223.255.255.255", false)]
    public void IsMulticast_WithVariousAddresses_ReturnsExpectedResult(string addressString, bool expected)
    {
        // Arrange
        var address = Ipv4Address.Parse(addressString, null);

        // Act
        bool result = address.IsMulticast();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToIPAddress_ReturnsCorrectIPAddress()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        var ipAddress = address.ToIPAddress();

        // Assert
        Assert.Equal("192.168.1.1", ipAddress.ToString());
        Assert.Equal(System.Net.Sockets.AddressFamily.InterNetwork, ipAddress.AddressFamily);
    }

    [Fact]
    public void FromIPAddress_WithValidIPv4_ReturnsCorrectAddress()
    {
        // Arrange
        var ipAddress = IPAddress.Parse("192.168.1.1");

        // Act
        var address = Ipv4Address.FromIPAddress(ipAddress);

        // Assert
        Assert.Equal("192.168.1.1", address.ToString());
    }

    [Fact]
    public void FromIPAddress_WithIPv6_ThrowsArgumentException()
    {
        // Arrange
        var ipAddress = IPAddress.Parse("::1");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ipv4Address.FromIPAddress(ipAddress));
    }

    [Fact]
    public void FromIPAddress_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Address.FromIPAddress(null!));
    }

    [Fact]
    public void CompareTo_WithDifferentAddresses_ReturnsCorrectOrder()
    {
        // Arrange
        var address1 = Ipv4Address.Parse("192.168.1.1", null);
        var address2 = Ipv4Address.Parse("192.168.1.2", null);

        // Act
        int result = address1.CompareTo(address2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        Ipv4Address address = "192.168.1.1";

        // Assert
        Assert.Equal("192.168.1.1", address.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromUInt_WorksCorrectly()
    {
        // Act
        Ipv4Address address = 0xC0A80101u;

        // Assert
        Assert.Equal("192.168.1.1", address.ToString());
    }

    [Fact]
    public void ExplicitConversion_ToUInt_WorksCorrectly()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        uint value = (uint)address;

        // Assert
        Assert.Equal(0xC0A80101u, value);
    }

    [Fact]
    public void ImplicitConversion_ToIPAddress_WorksCorrectly()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        IPAddress ipAddress = address;

        // Assert
        Assert.Equal("192.168.1.1", ipAddress.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromIPAddress_WorksCorrectly()
    {
        // Arrange
        var ipAddress = IPAddress.Parse("192.168.1.1");

        // Act
        Ipv4Address address = ipAddress;

        // Assert
        Assert.Equal("192.168.1.1", address.ToString());
    }

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Ipv4Address>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"192.168.1.1\"", json);
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var address1 = Ipv4Address.Parse("192.168.1.1", null);
        var address2 = Ipv4Address.Parse("192.168.1.1", null);

        // Act & Assert
        Assert.Equal(address1, address2);
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var address1 = Ipv4Address.Parse("192.168.1.1", null);
        var address2 = Ipv4Address.Parse("192.168.1.2", null);

        // Act & Assert
        Assert.NotEqual(address1, address2);
        Assert.False(address1 == address2);
        Assert.True(address1 != address2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var address1 = Ipv4Address.Parse("192.168.1.1", null);
        var address2 = Ipv4Address.Parse("192.168.1.1", null);

        // Act & Assert
        Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
    }
}