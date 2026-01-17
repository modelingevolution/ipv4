using System;
using System.Text.Json;
using ModelingEvolution.Ipv4;
using Xunit;

namespace ModelingEvolution.Ipv4.Tests;

public class MacAddressTests
{
    [Fact]
    public void Constructor_WithULong_CreatesCorrectAddress()
    {
        // Arrange
        ulong value = 0xAABBCCDDEEFFUL;

        // Act
        var mac = new MacAddress(value);

        // Assert
        Assert.Equal(0xAA, mac.B0);
        Assert.Equal(0xBB, mac.B1);
        Assert.Equal(0xCC, mac.B2);
        Assert.Equal(0xDD, mac.B3);
        Assert.Equal(0xEE, mac.B4);
        Assert.Equal(0xFF, mac.B5);
        Assert.Equal(value, mac.Value);
    }

    [Fact]
    public void Constructor_WithULong_MasksTo48Bits()
    {
        // Arrange - upper 16 bits should be ignored
        ulong value = 0xFFFFAABBCCDDEEFFUL;

        // Act
        var mac = new MacAddress(value);

        // Assert
        Assert.Equal(0xAABBCCDDEEFFUL, mac.Value);
    }

    [Fact]
    public void Constructor_WithBytes_CreatesCorrectAddress()
    {
        // Arrange & Act
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Assert
        Assert.Equal(0xAA, mac.B0);
        Assert.Equal(0xBB, mac.B1);
        Assert.Equal(0xCC, mac.B2);
        Assert.Equal(0xDD, mac.B3);
        Assert.Equal(0xEE, mac.B4);
        Assert.Equal(0xFF, mac.B5);
        Assert.Equal(0xAABBCCDDEEFFUL, mac.Value);
    }

    [Fact]
    public void Broadcast_ReturnsCorrectValue()
    {
        // Act
        var broadcast = MacAddress.Broadcast;

        // Assert
        Assert.Equal("FF:FF:FF:FF:FF:FF", broadcast.ToString());
        Assert.Equal(0xFFFFFFFFFFFFUL, broadcast.Value);
        Assert.True(broadcast.IsBroadcast);
    }

    [Fact]
    public void None_ReturnsCorrectValue()
    {
        // Act
        var none = MacAddress.None;

        // Assert
        Assert.Equal("00:00:00:00:00:00", none.ToString());
        Assert.Equal(0UL, none.Value);
    }

    [Fact]
    public void Oui_ReturnsUpper24Bits()
    {
        // Arrange
        var mac = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act
        var oui = mac.Oui;

        // Assert
        Assert.Equal(0xAABBCCu, oui);
    }

    [Theory]
    [InlineData("01:00:00:00:00:00", true)]  // LSB of first byte = 1
    [InlineData("03:00:00:00:00:00", true)]  // LSB of first byte = 1
    [InlineData("FF:FF:FF:FF:FF:FF", true)]  // Broadcast is also multicast
    [InlineData("00:00:00:00:00:00", false)] // LSB of first byte = 0
    [InlineData("02:00:00:00:00:00", false)] // LSB of first byte = 0
    public void IsMulticast_WithVariousAddresses_ReturnsExpectedResult(string macString, bool expected)
    {
        // Arrange
        var mac = MacAddress.Parse(macString);

        // Act & Assert
        Assert.Equal(expected, mac.IsMulticast);
    }

    [Theory]
    [InlineData("00:00:00:00:00:00", true)]  // LSB of first byte = 0
    [InlineData("02:00:00:00:00:00", true)]  // LSB of first byte = 0
    [InlineData("01:00:00:00:00:00", false)] // LSB of first byte = 1
    [InlineData("FF:FF:FF:FF:FF:FF", false)] // Broadcast
    public void IsUnicast_WithVariousAddresses_ReturnsExpectedResult(string macString, bool expected)
    {
        // Arrange
        var mac = MacAddress.Parse(macString);

        // Act & Assert
        Assert.Equal(expected, mac.IsUnicast);
    }

    [Theory]
    [InlineData("02:00:00:00:00:00", true)]  // Second LSB of first byte = 1
    [InlineData("03:00:00:00:00:00", true)]  // Second LSB of first byte = 1
    [InlineData("00:00:00:00:00:00", false)] // Second LSB of first byte = 0
    [InlineData("01:00:00:00:00:00", false)] // Second LSB of first byte = 0
    public void IsLocallyAdministered_WithVariousAddresses_ReturnsExpectedResult(string macString, bool expected)
    {
        // Arrange
        var mac = MacAddress.Parse(macString);

        // Act & Assert
        Assert.Equal(expected, mac.IsLocallyAdministered);
    }

    [Theory]
    [InlineData("00:00:00:00:00:00", true)]  // Second LSB of first byte = 0
    [InlineData("01:00:00:00:00:00", true)]  // Second LSB of first byte = 0
    [InlineData("02:00:00:00:00:00", false)] // Second LSB of first byte = 1
    [InlineData("03:00:00:00:00:00", false)] // Second LSB of first byte = 1
    public void IsUniversallyAdministered_WithVariousAddresses_ReturnsExpectedResult(string macString, bool expected)
    {
        // Arrange
        var mac = MacAddress.Parse(macString);

        // Act & Assert
        Assert.Equal(expected, mac.IsUniversallyAdministered);
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF", 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF)]
    [InlineData("00:00:00:00:00:00", 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)]
    [InlineData("ff:ff:ff:ff:ff:ff", 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)]
    public void Parse_ColonSeparated_ReturnsCorrectAddress(string input, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
    {
        // Act
        var mac = MacAddress.Parse(input);

        // Assert
        Assert.Equal(b0, mac.B0);
        Assert.Equal(b1, mac.B1);
        Assert.Equal(b2, mac.B2);
        Assert.Equal(b3, mac.B3);
        Assert.Equal(b4, mac.B4);
        Assert.Equal(b5, mac.B5);
    }

    [Theory]
    [InlineData("AA-BB-CC-DD-EE-FF", 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF)]
    [InlineData("00-00-00-00-00-00", 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)]
    public void Parse_DashSeparated_ReturnsCorrectAddress(string input, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
    {
        // Act
        var mac = MacAddress.Parse(input);

        // Assert
        Assert.Equal(b0, mac.B0);
        Assert.Equal(b1, mac.B1);
        Assert.Equal(b2, mac.B2);
        Assert.Equal(b3, mac.B3);
        Assert.Equal(b4, mac.B4);
        Assert.Equal(b5, mac.B5);
    }

    [Theory]
    [InlineData("AABBCCDDEEFF", 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF)]
    [InlineData("000000000000", 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)]
    [InlineData("aabbccddeeff", 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF)]
    public void Parse_Continuous_ReturnsCorrectAddress(string input, byte b0, byte b1, byte b2, byte b3, byte b4, byte b5)
    {
        // Act
        var mac = MacAddress.Parse(input);

        // Assert
        Assert.Equal(b0, mac.B0);
        Assert.Equal(b1, mac.B1);
        Assert.Equal(b2, mac.B2);
        Assert.Equal(b3, mac.B3);
        Assert.Equal(b4, mac.B4);
        Assert.Equal(b5, mac.B5);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AA:BB:CC:DD:EE")]
    [InlineData("AA:BB:CC:DD:EE:FF:00")]
    [InlineData("GG:HH:II:JJ:KK:LL")]
    [InlineData("not-a-mac")]
    [InlineData("AABBCCDDEE")]
    [InlineData("AABBCCDDEEFFGG")]
    public void Parse_WithInvalidString_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => MacAddress.Parse(input));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MacAddress.Parse(null!));
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF", true)]
    [InlineData("AA-BB-CC-DD-EE-FF", true)]
    [InlineData("AABBCCDDEEFF", true)]
    [InlineData("", false)]
    [InlineData("invalid", false)]
    public void TryParse_WithVariousInputs_ReturnsExpectedResult(string input, bool expectedResult)
    {
        // Act
        bool result = MacAddress.TryParse(input, null, out var mac);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetBytes_ReturnsCorrectArray()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var bytes = mac.GetBytes();

        // Assert
        Assert.Equal(6, bytes.Length);
        Assert.Equal(0xAA, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xCC, bytes[2]);
        Assert.Equal(0xDD, bytes[3]);
        Assert.Equal(0xEE, bytes[4]);
        Assert.Equal(0xFF, bytes[5]);
    }

    [Fact]
    public void CopyTo_ByteArray_CopiesToCorrectPosition()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);
        var destination = new byte[10];

        // Act
        mac.CopyTo(destination, 2);

        // Assert
        Assert.Equal(0x00, destination[0]);
        Assert.Equal(0x00, destination[1]);
        Assert.Equal(0xAA, destination[2]);
        Assert.Equal(0xBB, destination[3]);
        Assert.Equal(0xCC, destination[4]);
        Assert.Equal(0xDD, destination[5]);
        Assert.Equal(0xEE, destination[6]);
        Assert.Equal(0xFF, destination[7]);
        Assert.Equal(0x00, destination[8]);
        Assert.Equal(0x00, destination[9]);
    }

    [Fact]
    public void CopyTo_ByteArray_DefaultOffset_CopiesToStart()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);
        var destination = new byte[6];

        // Act
        mac.CopyTo(destination);

        // Assert
        Assert.Equal(0xAA, destination[0]);
        Assert.Equal(0xBB, destination[1]);
        Assert.Equal(0xCC, destination[2]);
        Assert.Equal(0xDD, destination[3]);
        Assert.Equal(0xEE, destination[4]);
        Assert.Equal(0xFF, destination[5]);
    }

    [Fact]
    public void CopyTo_ByteArray_NullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var mac = MacAddress.Broadcast;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mac.CopyTo(null!));
    }

    [Fact]
    public void CopyTo_ByteArray_NegativeOffset_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var mac = MacAddress.Broadcast;
        var destination = new byte[10];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mac.CopyTo(destination, -1));
    }

    [Fact]
    public void CopyTo_ByteArray_DestinationTooSmall_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var mac = MacAddress.Broadcast;
        var destination = new byte[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mac.CopyTo(destination, 0));
    }

    [Fact]
    public void CopyTo_ByteArray_OffsetTooLarge_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var mac = MacAddress.Broadcast;
        var destination = new byte[10];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mac.CopyTo(destination, 5));
    }

    [Fact]
    public void CopyTo_Span_CopiesToSpan()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);
        Span<byte> destination = stackalloc byte[6];

        // Act
        mac.CopyTo(destination);

        // Assert
        Assert.Equal(0xAA, destination[0]);
        Assert.Equal(0xBB, destination[1]);
        Assert.Equal(0xCC, destination[2]);
        Assert.Equal(0xDD, destination[3]);
        Assert.Equal(0xEE, destination[4]);
        Assert.Equal(0xFF, destination[5]);
    }

    [Fact]
    public void CopyTo_Span_SpanTooSmall_ThrowsArgumentException()
    {
        // Arrange
        var mac = MacAddress.Broadcast;
        var destination = new byte[5];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => mac.CopyTo(destination.AsSpan()));
    }

    [Fact]
    public void From_ULong_ReturnsCorrectAddress()
    {
        // Arrange
        ulong value = 0xAABBCCDDEEFFUL;

        // Act
        var mac = MacAddress.From(value);

        // Assert
        Assert.Equal(value, mac.Value);
    }

    [Fact]
    public void From_ByteArray_ReturnsCorrectAddress()
    {
        // Arrange
        var bytes = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        // Act
        var mac = MacAddress.From(bytes);

        // Assert
        Assert.Equal(0xAA, mac.B0);
        Assert.Equal(0xFF, mac.B5);
    }

    [Fact]
    public void From_ByteArray_NullArray_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MacAddress.From((byte[])null!));
    }

    [Fact]
    public void From_ByteArray_WrongLength_ThrowsArgumentException()
    {
        // Arrange
        var bytes = new byte[] { 0xAA, 0xBB, 0xCC };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MacAddress.From(bytes));
    }

    [Fact]
    public void From_ReadOnlySpan_ReturnsCorrectAddress()
    {
        // Arrange
        ReadOnlySpan<byte> bytes = stackalloc byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        // Act
        var mac = MacAddress.From(bytes);

        // Assert
        Assert.Equal(0xAA, mac.B0);
        Assert.Equal(0xFF, mac.B5);
    }

    [Fact]
    public void From_ReadOnlySpan_WrongLength_ThrowsArgumentException()
    {
        // Arrange
        var bytes = new byte[] { 0xAA, 0xBB, 0xCC };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => MacAddress.From(bytes.AsSpan()));
    }

    [Fact]
    public void ToString_ReturnsColonSeparatedUppercase()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var result = mac.ToString();

        // Assert
        Assert.Equal("AA:BB:CC:DD:EE:FF", result);
    }

    [Fact]
    public void ToString_WithColonSeparator_ReturnsColonSeparated()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var result = mac.ToString(':');

        // Assert
        Assert.Equal("AA:BB:CC:DD:EE:FF", result);
    }

    [Fact]
    public void ToString_WithDashSeparator_ReturnsDashSeparated()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var result = mac.ToString('-');

        // Assert
        Assert.Equal("AA-BB-CC-DD-EE-FF", result);
    }

    [Fact]
    public void ToString_WithNullSeparator_ReturnsContinuous()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var result = mac.ToString(null);

        // Assert
        Assert.Equal("AABBCCDDEEFF", result);
    }

    [Fact]
    public void ToString_WithLowercase_ReturnsLowercase()
    {
        // Arrange
        var mac = new MacAddress(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF);

        // Act
        var result = mac.ToString(':', false);

        // Assert
        Assert.Equal("aa:bb:cc:dd:ee:ff", result);
    }

    [Fact]
    public void CompareTo_WithSmallerAddress_ReturnsPositive()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:00");

        // Act
        int result = mac1.CompareTo(mac2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_WithLargerAddress_ReturnsNegative()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:00");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act
        int result = mac1.CompareTo(mac2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_WithEqualAddress_ReturnsZero()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act
        int result = mac1.CompareTo(mac2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void ImplicitConversion_FromString_WorksCorrectly()
    {
        // Act
        MacAddress mac = "AA:BB:CC:DD:EE:FF";

        // Assert
        Assert.Equal("AA:BB:CC:DD:EE:FF", mac.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromByteArray_WorksCorrectly()
    {
        // Arrange
        byte[] bytes = { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        // Act
        MacAddress mac = bytes;

        // Assert
        Assert.Equal("AA:BB:CC:DD:EE:FF", mac.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromULong_WorksCorrectly()
    {
        // Act
        MacAddress mac = 0xAABBCCDDEEFFUL;

        // Assert
        Assert.Equal("AA:BB:CC:DD:EE:FF", mac.ToString());
    }

    [Fact]
    public void ExplicitConversion_ToULong_WorksCorrectly()
    {
        // Arrange
        var mac = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act
        ulong value = (ulong)mac;

        // Assert
        Assert.Equal(0xAABBCCDDEEFFUL, value);
    }

    [Fact]
    public void JsonSerialization_RoundTrip_WorksCorrectly()
    {
        // Arrange
        var original = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act
        string json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<MacAddress>(json);

        // Assert
        Assert.Equal(original, deserialized);
        Assert.Equal("\"AA:BB:CC:DD:EE:FF\"", json);
    }

    [Fact]
    public void Equality_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act & Assert
        Assert.Equal(mac1, mac2);
        Assert.True(mac1 == mac2);
        Assert.False(mac1 != mac2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:00");

        // Act & Assert
        Assert.NotEqual(mac1, mac2);
        Assert.False(mac1 == mac2);
        Assert.True(mac1 != mac2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHash()
    {
        // Arrange
        var mac1 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");
        var mac2 = MacAddress.Parse("AA:BB:CC:DD:EE:FF");

        // Act & Assert
        Assert.Equal(mac1.GetHashCode(), mac2.GetHashCode());
    }
}
