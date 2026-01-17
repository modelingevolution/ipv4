using System;
using System.Net;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class Ipv4EndpointTests
{
    [Fact]
    public void Constructor_WithAddressAndPort_CreatesCorrectEndpoint()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100");
        var port = new IpPort(8080);

        // Act
        var endpoint = new Ipv4Endpoint(address, port);

        // Assert
        Assert.Equal(address, endpoint.Address);
        Assert.Equal(port, endpoint.Port);
    }

    [Fact]
    public void Constructor_WithAddressAndIntPort_CreatesCorrectEndpoint()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100");

        // Act
        var endpoint = new Ipv4Endpoint(address, 8080);

        // Assert
        Assert.Equal(address, endpoint.Address);
        Assert.Equal(8080, endpoint.Port.Value);
    }

    [Fact]
    public void Constructor_WithStringAddressAndIntPort_CreatesCorrectEndpoint()
    {
        // Act
        var endpoint = new Ipv4Endpoint("192.168.1.100", 8080);

        // Assert
        Assert.Equal("192.168.1.100", endpoint.Address.ToString());
        Assert.Equal(8080, endpoint.Port.Value);
    }

    #region Parse Tests

    [Theory]
    [InlineData("192.168.1.100:8080", "192.168.1.100", 8080)]
    [InlineData("127.0.0.1:80", "127.0.0.1", 80)]
    [InlineData("0.0.0.0:443", "0.0.0.0", 443)]
    [InlineData("255.255.255.255:65535", "255.255.255.255", 65535)]
    [InlineData("10.0.0.1:0", "10.0.0.1", 0)]
    public void Parse_WithValidString_ReturnsCorrectEndpoint(string input, string expectedAddress, int expectedPort)
    {
        // Act
        var endpoint = Ipv4Endpoint.Parse(input);

        // Assert
        Assert.Equal(expectedAddress, endpoint.Address.ToString());
        Assert.Equal(expectedPort, endpoint.Port.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.1.100")]
    [InlineData(":8080")]
    [InlineData("192.168.1.100:")]
    [InlineData("192.168.1.100:abc")]
    [InlineData("not.an.ip:8080")]
    [InlineData("192.168.1.256:8080")]
    [InlineData("192.168.1.100:70000")]
    public void Parse_WithInvalidString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Ipv4Endpoint.Parse(input));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Endpoint.Parse(null!));
    }

    [Theory]
    [InlineData("192.168.1.100:8080", true)]
    [InlineData("127.0.0.1:80", true)]
    [InlineData("", false)]
    [InlineData("invalid:8080", false)]
    [InlineData("192.168.1.100", false)]
    [InlineData(null, false)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string? input, bool expectedResult)
    {
        // Act
        bool result = Ipv4Endpoint.TryParse(input, null, out var endpoint);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var endpoint = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        var result = endpoint.ToString();

        // Assert
        Assert.Equal("192.168.1.100:8080", result);
    }

    #endregion

    #region IPEndPoint Conversion Tests

    [Fact]
    public void ToIPEndPoint_ReturnsCorrectIPEndPoint()
    {
        // Arrange
        var endpoint = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        IPEndPoint ipEndPoint = endpoint.ToIPEndPoint();

        // Assert
        Assert.Equal("192.168.1.100", ipEndPoint.Address.ToString());
        Assert.Equal(8080, ipEndPoint.Port);
        Assert.Equal(System.Net.Sockets.AddressFamily.InterNetwork, ipEndPoint.AddressFamily);
    }

    [Fact]
    public void FromIPEndPoint_WithValidIPv4_ReturnsCorrectEndpoint()
    {
        // Arrange
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 8080);

        // Act
        var endpoint = Ipv4Endpoint.FromIPEndPoint(ipEndPoint);

        // Assert
        Assert.Equal("192.168.1.100", endpoint.Address.ToString());
        Assert.Equal(8080, endpoint.Port.Value);
    }

    [Fact]
    public void FromIPEndPoint_WithIPv6_ThrowsArgumentException()
    {
        // Arrange
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("::1"), 8080);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ipv4Endpoint.FromIPEndPoint(ipEndPoint));
    }

    [Fact]
    public void FromIPEndPoint_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Endpoint.FromIPEndPoint(null!));
    }

    #endregion

    #region CompareTo Tests

    [Fact]
    public void CompareTo_SameAddress_ComparesPort()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 80);
        var endpoint2 = new Ipv4Endpoint("192.168.1.100", 443);

        // Act
        int result = endpoint1.CompareTo(endpoint2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_DifferentAddress_ComparesAddress()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.101", 80);

        // Act
        int result = endpoint1.CompareTo(endpoint2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_EqualEndpoints_ReturnsZero()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        int result = endpoint1.CompareTo(endpoint2);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        Ipv4Endpoint endpoint = "192.168.1.100:8080";

        // Assert
        Assert.Equal("192.168.1.100", endpoint.Address.ToString());
        Assert.Equal(8080, endpoint.Port.Value);
    }

    [Fact]
    public void ImplicitConversion_ToIPEndPoint_WorksCorrectly()
    {
        // Arrange
        var endpoint = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        IPEndPoint ipEndPoint = endpoint;

        // Assert
        Assert.Equal("192.168.1.100", ipEndPoint.Address.ToString());
        Assert.Equal(8080, ipEndPoint.Port);
    }

    [Fact]
    public void ImplicitConversion_FromIPEndPoint_WorksCorrectly()
    {
        // Arrange
        var ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 8080);

        // Act
        Ipv4Endpoint endpoint = ipEndPoint;

        // Assert
        Assert.Equal("192.168.1.100", endpoint.Address.ToString());
        Assert.Equal(8080, endpoint.Port.Value);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void AddressAndPort_Operator_CreatesEndpoint()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100");
        var port = new IpPort(8080);

        // Act
        var endpoint = address + port;

        // Assert
        Assert.Equal(address, endpoint.Address);
        Assert.Equal(port, endpoint.Port);
    }

    #endregion

    #region Deconstruct Tests

    [Fact]
    public void Deconstruct_ReturnsCorrectComponents()
    {
        // Arrange
        var endpoint = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        var (address, port) = endpoint;

        // Assert
        Assert.Equal("192.168.1.100", address.ToString());
        Assert.Equal(8080, port.Value);
    }

    #endregion

    #region JSON Serialization Tests

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Ipv4Endpoint>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"192.168.1.100:8080\"", json);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act & Assert
        Assert.Equal(endpoint1, endpoint2);
        Assert.True(endpoint1 == endpoint2);
        Assert.False(endpoint1 != endpoint2);
    }

    [Fact]
    public void Equality_WithDifferentAddress_ReturnsFalse()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.101", 8080);

        // Act & Assert
        Assert.NotEqual(endpoint1, endpoint2);
        Assert.False(endpoint1 == endpoint2);
        Assert.True(endpoint1 != endpoint2);
    }

    [Fact]
    public void Equality_WithDifferentPort_ReturnsFalse()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.100", 8081);

        // Act & Assert
        Assert.NotEqual(endpoint1, endpoint2);
        Assert.False(endpoint1 == endpoint2);
        Assert.True(endpoint1 != endpoint2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var endpoint1 = new Ipv4Endpoint("192.168.1.100", 8080);
        var endpoint2 = new Ipv4Endpoint("192.168.1.100", 8080);

        // Act & Assert
        Assert.Equal(endpoint1.GetHashCode(), endpoint2.GetHashCode());
    }

    #endregion
}
