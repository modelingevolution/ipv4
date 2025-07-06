using System;
using System.Linq;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class Ipv4NetworkTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesCorrectNetwork()
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);

        // Act
        var network = new Ipv4Network(address, 24);

        // Assert
        Assert.Equal("192.168.1.0", network.NetworkAddress.ToString());
        Assert.Equal(24, network.PrefixLength);
        Assert.Equal("192.168.1.0/24", network.ToString());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    public void Constructor_WithInvalidPrefixLength_ThrowsArgumentOutOfRangeException(int prefixLength)
    {
        // Arrange
        var address = Ipv4Address.Parse("192.168.1.100", null);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ipv4Network(address, prefixLength));
    }

    [Fact]
    public void BroadcastAddress_ReturnsCorrectValue()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act
        var broadcast = network.BroadcastAddress;

        // Assert
        Assert.Equal("192.168.1.255", broadcast.ToString());
    }

    [Fact]
    public void SubnetMask_ReturnsCorrectValue()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act
        var mask = network.SubnetMask;

        // Assert
        Assert.Equal("255.255.255.0", mask.ToString());
    }

    [Theory]
    [InlineData(24, 256)]
    [InlineData(25, 128)]
    [InlineData(30, 4)]
    [InlineData(32, 1)]
    [InlineData(0, 4294967296L)]
    public void TotalAddressCount_ReturnsCorrectValue(int prefixLength, long expectedCount)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), prefixLength);

        // Act
        var count = network.TotalAddressCount;

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData(24, 254)]
    [InlineData(25, 126)]
    [InlineData(30, 2)]
    [InlineData(31, 2)]
    [InlineData(32, 1)]
    public void HostCount_ReturnsCorrectValue(int prefixLength, long expectedCount)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), prefixLength);

        // Act
        var count = network.HostCount;

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void FirstHostAddress_ReturnsCorrectValue()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act
        var firstHost = network.FirstHostAddress;

        // Assert
        Assert.Equal("192.168.1.1", firstHost.ToString());
    }

    [Fact]
    public void LastHostAddress_ReturnsCorrectValue()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act
        var lastHost = network.LastHostAddress;

        // Assert
        Assert.Equal("192.168.1.254", lastHost.ToString());
    }

    [Fact]
    public void FirstHostAddress_ForSlash31_ReturnsNetworkAddress()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 31);

        // Act
        var firstHost = network.FirstHostAddress;

        // Assert
        Assert.Equal("192.168.1.0", firstHost.ToString());
    }

    [Fact]
    public void LastHostAddress_ForSlash31_ReturnsBroadcastAddress()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 31);

        // Act
        var lastHost = network.LastHostAddress;

        // Assert
        Assert.Equal("192.168.1.1", lastHost.ToString());
    }

    [Theory]
    [InlineData("192.168.1.100", true)]
    [InlineData("192.168.1.0", true)]
    [InlineData("192.168.1.255", true)]
    [InlineData("192.168.2.1", false)]
    [InlineData("10.0.0.1", false)]
    public void Contains_WithVariousAddresses_ReturnsExpectedResult(string addressString, bool expected)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);
        var address = Ipv4Address.Parse(addressString, null);

        // Act
        bool result = network.Contains(address);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.0/25", true)]   // Overlaps
    [InlineData("192.168.1.128/25", true)] // Overlaps
    [InlineData("192.168.0.0/23", true)]   // Contains this network
    [InlineData("192.168.2.0/24", false)]  // No overlap
    [InlineData("10.0.0.0/8", false)]      // No overlap
    public void Overlaps_WithVariousNetworks_ReturnsExpectedResult(string otherNetworkString, bool expected)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);
        var otherNetwork = Ipv4Network.Parse(otherNetworkString, null);

        // Act
        bool result = network.Overlaps(otherNetwork);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.0/25", true)]   // This contains other
    [InlineData("192.168.1.128/25", true)] // This contains other
    [InlineData("192.168.0.0/23", false)]  // Other contains this
    [InlineData("192.168.2.0/24", false)]  // No overlap
    public void ContainsNetwork_WithVariousNetworks_ReturnsExpectedResult(string otherNetworkString, bool expected)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);
        var otherNetwork = Ipv4Network.Parse(otherNetworkString, null);

        // Act
        bool result = network.ContainsNetwork(otherNetwork);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAddresses_ReturnsAllAddressesInNetwork()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 30);

        // Act
        var addresses = network.GetAddresses().ToList();

        // Assert
        Assert.Equal(4, addresses.Count);
        Assert.Equal("192.168.1.0", addresses[0].ToString());
        Assert.Equal("192.168.1.1", addresses[1].ToString());
        Assert.Equal("192.168.1.2", addresses[2].ToString());
        Assert.Equal("192.168.1.3", addresses[3].ToString());
    }

    [Fact]
    public void GetHostAddresses_ReturnsOnlyHostAddresses()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 30);

        // Act
        var addresses = network.GetHostAddresses().ToList();

        // Assert
        Assert.Equal(2, addresses.Count);
        Assert.Equal("192.168.1.1", addresses[0].ToString());
        Assert.Equal("192.168.1.2", addresses[1].ToString());
    }

    [Fact]
    public void GetHostAddresses_ForSlash31_ReturnsAllAddresses()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 31);

        // Act
        var addresses = network.GetHostAddresses().ToList();

        // Assert
        Assert.Equal(2, addresses.Count);
        Assert.Equal("192.168.1.0", addresses[0].ToString());
        Assert.Equal("192.168.1.1", addresses[1].ToString());
    }

    [Fact]
    public void Subnet_WithValidNewPrefix_ReturnsCorrectSubnets()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act
        var subnets = network.Subnet(26).ToList();

        // Assert
        Assert.Equal(4, subnets.Count);
        Assert.Equal("192.168.1.0/26", subnets[0].ToString());
        Assert.Equal("192.168.1.64/26", subnets[1].ToString());
        Assert.Equal("192.168.1.128/26", subnets[2].ToString());
        Assert.Equal("192.168.1.192/26", subnets[3].ToString());
    }

    [Theory]
    [InlineData(24)]
    [InlineData(23)]
    public void Subnet_WithInvalidNewPrefix_ThrowsArgumentOutOfRangeException(int newPrefixLength)
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 24);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => network.Subnet(newPrefixLength).ToList());
    }

    [Theory]
    [InlineData("192.168.1.0/24")]
    [InlineData("10.0.0.0/8")]
    [InlineData("172.16.0.0/12")]
    [InlineData("0.0.0.0/0")]
    public void Parse_WithValidString_ReturnsCorrectNetwork(string input)
    {
        // Act
        var network = Ipv4Network.Parse(input, null);

        // Assert
        Assert.Equal(input, network.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("192.168.1.0")]
    [InlineData("192.168.1.0/")]
    [InlineData("192.168.1.0/33")]
    [InlineData("192.168.1.0/-1")]
    [InlineData("invalid/24")]
    public void Parse_WithInvalidString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Ipv4Network.Parse(input, null));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Ipv4Network.Parse(null!, null));
    }

    [Theory]
    [InlineData("192.168.1.0/24", true)]
    [InlineData("10.0.0.0/8", true)]
    [InlineData("", false)]
    [InlineData("192.168.1.0", false)]
    [InlineData("192.168.1.0/33", false)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string input, bool expectedResult)
    {
        // Act
        bool result = Ipv4Network.TryParse(input, null, out var network);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.NotEqual(default(Ipv4Network), network);
        }
        else
        {
            Assert.Equal(default(Ipv4Network), network);
        }
    }

    [Fact]
    public void CompareTo_WithDifferentNetworks_ReturnsCorrectOrder()
    {
        // Arrange
        var network1 = Ipv4Network.Parse("192.168.1.0/24", null);
        var network2 = Ipv4Network.Parse("192.168.2.0/24", null);

        // Act
        int result = network1.CompareTo(network2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_WithSameNetworkDifferentPrefix_ReturnsCorrectOrder()
    {
        // Arrange
        var network1 = Ipv4Network.Parse("192.168.1.0/24", null);
        var network2 = Ipv4Network.Parse("192.168.1.0/25", null);

        // Act
        int result = network1.CompareTo(network2);

        // Assert
        Assert.True(result < 0); // /24 should come before /25
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        Ipv4Network network = "192.168.1.0/24";

        // Assert
        Assert.Equal("192.168.1.0/24", network.ToString());
    }

    [Fact]
    public void GetEnumerator_ReturnsAllAddresses()
    {
        // Arrange
        var network = new Ipv4Network(Ipv4Address.Parse("192.168.1.0", null), 30);

        // Act
        var addresses = network.ToList();

        // Assert
        Assert.Equal(4, addresses.Count);
        Assert.Equal("192.168.1.0", addresses[0].ToString());
        Assert.Equal("192.168.1.3", addresses[3].ToString());
    }

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = Ipv4Network.Parse("192.168.1.0/24", null);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Ipv4Network>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"192.168.1.0/24\"", json);
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var network1 = Ipv4Network.Parse("192.168.1.0/24", null);
        var network2 = Ipv4Network.Parse("192.168.1.0/24", null);

        // Act & Assert
        Assert.Equal(network1, network2);
        Assert.True(network1 == network2);
        Assert.False(network1 != network2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var network1 = Ipv4Network.Parse("192.168.1.0/24", null);
        var network2 = Ipv4Network.Parse("192.168.2.0/24", null);

        // Act & Assert
        Assert.NotEqual(network1, network2);
        Assert.False(network1 == network2);
        Assert.True(network1 != network2);
    }
}