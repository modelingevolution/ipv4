using System;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class Ipv4ConfigurationTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesCorrectConfiguration()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);
        var gateway = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        var config = new Ipv4Configuration(address, 24, gateway);

        // Assert
        Assert.Equal(address, config.Address);
        Assert.Equal(24, config.PrefixLength);
        Assert.Equal(gateway, config.Gateway);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    public void Constructor_WithInvalidPrefixLength_ThrowsArgumentOutOfRangeException(int prefixLength)
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);
        var gateway = Ipv4Address.Parse("192.168.1.1", null);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ipv4Configuration(address, prefixLength, gateway));
    }

    [Fact]
    public void Network_ReturnsCorrectNetwork()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        var network = config.Network;

        // Assert
        Assert.Equal("192.168.1.0/24", network.ToString());
    }

    [Fact]
    public void NetworkAddress_ReturnsCorrectAddress()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        var networkAddress = config.NetworkAddress;

        // Assert
        Assert.Equal("192.168.1.0", networkAddress.ToString());
    }

    [Fact]
    public void BroadcastAddress_ReturnsCorrectAddress()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        var broadcastAddress = config.BroadcastAddress;

        // Assert
        Assert.Equal("192.168.1.255", broadcastAddress.ToString());
    }

    [Fact]
    public void SubnetMask_ReturnsCorrectMask()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        var subnetMask = config.SubnetMask;

        // Assert
        Assert.Equal("255.255.255.0", subnetMask.ToString());
    }

    [Fact]
    public void IsValid_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        bool isValid = config.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithAddressAsNetworkAddress_ReturnsFalse()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.0", null), // Network address
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        bool isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithAddressAsBroadcastAddress_ReturnsFalse()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.255", null), // Broadcast address
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        bool isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithGatewayOutsideNetwork_ReturnsFalse()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.2.1", null) // Gateway in different network
        );

        // Act
        bool isValid = config.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithSlash31Network_ReturnsTrue()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.0", null),
            31,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        bool isValid = config.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void CanReachDirectly_WithAddressInSameNetwork_ReturnsTrue()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );
        var otherAddress = Ipv4Address.Parse("192.168.1.200", null);

        // Act
        bool canReach = config.CanReachDirectly(otherAddress);

        // Assert
        Assert.True(canReach);
    }

    [Fact]
    public void CanReachDirectly_WithAddressInDifferentNetwork_ReturnsFalse()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );
        var otherAddress = Ipv4Address.Parse("192.168.2.100", null);

        // Act
        bool canReach = config.CanReachDirectly(otherAddress);

        // Assert
        Assert.False(canReach);
    }

    [Fact]
    public void FromNetwork_WithValidParameters_CreatesCorrectConfiguration()
    {
        // Arrange
        var network = Ipv4Network.Parse("192.168.1.0/24", null);
        var hostAddress = Ipv4Address.Parse("192.168.1.100", null);
        var gateway = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        var config = Ipv4Configuration.FromNetwork(network, hostAddress, gateway);

        // Assert
        Assert.Equal(hostAddress, config.Address);
        Assert.Equal(24, config.PrefixLength);
        Assert.Equal(gateway, config.Gateway);
    }

    [Fact]
    public void FromNetwork_WithHostAddressOutsideNetwork_ThrowsArgumentException()
    {
        // Arrange
        var network = Ipv4Network.Parse("192.168.1.0/24", null);
        var hostAddress = Ipv4Address.Parse("192.168.2.100", null); // Outside network
        var gateway = Ipv4Address.Parse("192.168.1.1", null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ipv4Configuration.FromNetwork(network, hostAddress, gateway));
    }

    [Fact]
    public void FromNetworkFirstHost_CreatesConfigurationWithFirstHost()
    {
        // Arrange
        var network = Ipv4Network.Parse("192.168.1.0/24", null);
        var gateway = Ipv4Address.Parse("192.168.1.1", null);

        // Act
        var config = Ipv4Configuration.FromNetworkFirstHost(network, gateway);

        // Assert
        Assert.Equal("192.168.1.1", config.Address.ToString()); // First host address
        Assert.Equal(24, config.PrefixLength);
        Assert.Equal(gateway, config.Gateway);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var config = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        string result = config.ToString();

        // Assert
        Assert.Equal("192.168.1.100/24 gw 192.168.1.1", result);
    }

    [Theory]
    [InlineData("192.168.1.100/24 gw 192.168.1.1")]
    [InlineData("192.168.1.100/24 gateway 192.168.1.1")]
    [InlineData("192.168.1.100/24,192.168.1.1")]
    public void Parse_WithValidFormats_ReturnsCorrectConfiguration(string input)
    {
        // Act
        var config = Ipv4Configuration.Parse(input, null);

        // Assert
        Assert.Equal("192.168.1.100", config.Address.ToString());
        Assert.Equal(24, config.PrefixLength);
        Assert.Equal("192.168.1.1", config.Gateway.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.1.100/24")]
    [InlineData("192.168.1.100 gw 192.168.1.1")]
    [InlineData("invalid/24 gw 192.168.1.1")]
    [InlineData("192.168.1.100/24 gw invalid")]
    public void Parse_WithInvalidFormats_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Ipv4Configuration.Parse(input, null));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Configuration.Parse(null!, null));
    }

    [Theory]
    [InlineData("192.168.1.100/24 gw 192.168.1.1", true)]
    [InlineData("192.168.1.100/24,192.168.1.1", true)]
    [InlineData("", false)]
    [InlineData("invalid format", false)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string input, bool expectedResult)
    {
        // Act
        bool result = Ipv4Configuration.TryParse(input, null, out var config);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotEqual(default(Ipv4Configuration), config);
        }
        else
        {
            Assert.Equal(default(Ipv4Configuration), config);
        }
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        Ipv4Configuration config = "192.168.1.100/24 gw 192.168.1.1";

        // Assert
        Assert.Equal("192.168.1.100", config.Address.ToString());
        Assert.Equal(24, config.PrefixLength);
        Assert.Equal("192.168.1.1", config.Gateway.ToString());
    }

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Ipv4Configuration>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"192.168.1.100/24 gw 192.168.1.1\"", json);
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var config1 = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );
        var config2 = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act & Assert
        Assert.Equal(config1, config2);
        Assert.True(config1 == config2);
        Assert.False(config1 != config2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var config1 = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.100", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );
        var config2 = new Ipv4Configuration(
            Ipv4Address.Parse("192.168.1.101", null),
            24,
            Ipv4Address.Parse("192.168.1.1", null)
        );

        // Act & Assert
        Assert.NotEqual(config1, config2);
        Assert.False(config1 == config2);
        Assert.True(config1 != config2);
    }
}