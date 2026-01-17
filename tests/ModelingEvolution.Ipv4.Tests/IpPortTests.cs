using System;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class IpPortTests
{
    [Fact]
    public void Constructor_WithUShort_CreatesCorrectPort()
    {
        // Arrange & Act
        var port = new IpPort((ushort)8080);

        // Assert
        Assert.Equal(8080, port.Value);
    }

    [Fact]
    public void Constructor_WithInt_CreatesCorrectPort()
    {
        // Arrange & Act
        var port = new IpPort(8080);

        // Assert
        Assert.Equal(8080, port.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(70000)]
    public void Constructor_WithInvalidInt_ThrowsArgumentOutOfRangeException(int value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new IpPort(value));
    }

    [Fact]
    public void Constructor_WithZero_IsValid()
    {
        // Arrange & Act
        var port = new IpPort(0);

        // Assert
        Assert.Equal(0, port.Value);
    }

    [Fact]
    public void Constructor_WithMaxPort_IsValid()
    {
        // Arrange & Act
        var port = new IpPort(65535);

        // Assert
        Assert.Equal(65535, port.Value);
    }

    #region Well-Known Ports Tests

    [Fact]
    public void Http_ReturnsPort80()
    {
        Assert.Equal(80, IpPort.Http.Value);
    }

    [Fact]
    public void Https_ReturnsPort443()
    {
        Assert.Equal(443, IpPort.Https.Value);
    }

    [Fact]
    public void Ssh_ReturnsPort22()
    {
        Assert.Equal(22, IpPort.Ssh.Value);
    }

    [Fact]
    public void Ftp_ReturnsPort21()
    {
        Assert.Equal(21, IpPort.Ftp.Value);
    }

    [Fact]
    public void FtpData_ReturnsPort20()
    {
        Assert.Equal(20, IpPort.FtpData.Value);
    }

    [Fact]
    public void Dns_ReturnsPort53()
    {
        Assert.Equal(53, IpPort.Dns.Value);
    }

    [Fact]
    public void MySql_ReturnsPort3306()
    {
        Assert.Equal(3306, IpPort.MySql.Value);
    }

    [Fact]
    public void PostgreSql_ReturnsPort5432()
    {
        Assert.Equal(5432, IpPort.PostgreSql.Value);
    }

    [Fact]
    public void Redis_ReturnsPort6379()
    {
        Assert.Equal(6379, IpPort.Redis.Value);
    }

    [Fact]
    public void MongoDb_ReturnsPort27017()
    {
        Assert.Equal(27017, IpPort.MongoDb.Value);
    }

    [Fact]
    public void Rdp_ReturnsPort3389()
    {
        Assert.Equal(3389, IpPort.Rdp.Value);
    }

    [Fact]
    public void HttpProxy_ReturnsPort8080()
    {
        Assert.Equal(8080, IpPort.HttpProxy.Value);
    }

    #endregion

    #region Port Range Classification Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(22, true)]
    [InlineData(80, true)]
    [InlineData(443, true)]
    [InlineData(1023, true)]
    [InlineData(1024, false)]
    [InlineData(8080, false)]
    public void IsWellKnown_WithVariousPorts_ReturnsExpectedResult(int portValue, bool expected)
    {
        // Arrange
        var port = new IpPort(portValue);

        // Act & Assert
        Assert.Equal(expected, port.IsWellKnown);
    }

    [Theory]
    [InlineData(1024, true)]
    [InlineData(3306, true)]
    [InlineData(8080, true)]
    [InlineData(49151, true)]
    [InlineData(1023, false)]
    [InlineData(49152, false)]
    public void IsRegistered_WithVariousPorts_ReturnsExpectedResult(int portValue, bool expected)
    {
        // Arrange
        var port = new IpPort(portValue);

        // Act & Assert
        Assert.Equal(expected, port.IsRegistered);
    }

    [Theory]
    [InlineData(49152, true)]
    [InlineData(50000, true)]
    [InlineData(65535, true)]
    [InlineData(49151, false)]
    [InlineData(8080, false)]
    public void IsDynamic_WithVariousPorts_ReturnsExpectedResult(int portValue, bool expected)
    {
        // Arrange
        var port = new IpPort(portValue);

        // Act & Assert
        Assert.Equal(expected, port.IsDynamic);
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("80", 80)]
    [InlineData("443", 443)]
    [InlineData("8080", 8080)]
    [InlineData("0", 0)]
    [InlineData("65535", 65535)]
    [InlineData("  8080  ", 8080)]
    public void Parse_WithValidString_ReturnsCorrectPort(string input, int expected)
    {
        // Act
        var port = IpPort.Parse(input);

        // Assert
        Assert.Equal(expected, port.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("65536")]
    [InlineData("not-a-number")]
    public void Parse_WithInvalidString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => IpPort.Parse(input));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => IpPort.Parse(null!));
    }

    [Theory]
    [InlineData("80", true, 80)]
    [InlineData("443", true, 443)]
    [InlineData("", false, 0)]
    [InlineData("abc", false, 0)]
    [InlineData("-1", false, 0)]
    [InlineData(null, false, 0)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string? input, bool expectedResult, int expectedValue)
    {
        // Act
        bool result = IpPort.TryParse(input, null, out var port);

        // Assert
        Assert.Equal(expectedResult, result);
        if (expectedResult)
        {
            Assert.Equal(expectedValue, port.Value);
        }
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsPortAsString()
    {
        // Arrange
        var port = new IpPort(8080);

        // Act
        var result = port.ToString();

        // Assert
        Assert.Equal("8080", result);
    }

    #endregion

    #region CompareTo Tests

    [Fact]
    public void CompareTo_WithSmallerPort_ReturnsPositive()
    {
        // Arrange
        var port1 = new IpPort(443);
        var port2 = new IpPort(80);

        // Act
        int result = port1.CompareTo(port2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_WithLargerPort_ReturnsNegative()
    {
        // Arrange
        var port1 = new IpPort(80);
        var port2 = new IpPort(443);

        // Act
        int result = port1.CompareTo(port2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_WithEqualPort_ReturnsZero()
    {
        // Arrange
        var port1 = new IpPort(8080);
        var port2 = new IpPort(8080);

        // Act
        int result = port1.CompareTo(port2);

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_ToUShort_WorksCorrectly()
    {
        // Arrange
        var port = new IpPort(8080);

        // Act
        ushort value = port;

        // Assert
        Assert.Equal(8080, value);
    }

    [Fact]
    public void ImplicitConversion_FromUShort_WorksCorrectly()
    {
        // Act
        IpPort port = (ushort)8080;

        // Assert
        Assert.Equal(8080, port.Value);
    }

    [Fact]
    public void ImplicitConversion_FromInt_WorksCorrectly()
    {
        // Act
        IpPort port = 8080;

        // Assert
        Assert.Equal(8080, port.Value);
    }

    [Fact]
    public void ImplicitConversion_FromInt_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => { IpPort port = 70000; });
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        IpPort port = "8080";

        // Assert
        Assert.Equal(8080, port.Value);
    }

    #endregion

    #region JSON Serialization Tests

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = new IpPort(8080);

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<IpPort>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"8080\"", json);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var port1 = new IpPort(8080);
        var port2 = new IpPort(8080);

        // Act & Assert
        Assert.Equal(port1, port2);
        Assert.True(port1 == port2);
        Assert.False(port1 != port2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var port1 = new IpPort(80);
        var port2 = new IpPort(443);

        // Act & Assert
        Assert.NotEqual(port1, port2);
        Assert.False(port1 == port2);
        Assert.True(port1 != port2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var port1 = new IpPort(8080);
        var port2 = new IpPort(8080);

        // Act & Assert
        Assert.Equal(port1.GetHashCode(), port2.GetHashCode());
    }

    #endregion
}
