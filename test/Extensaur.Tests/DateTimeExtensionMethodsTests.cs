#nullable enable

namespace Extensaur.Tests;

public class DateTimeExtensionMethodsTests
{
    private static readonly TimeZoneInfo EasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    private static readonly TimeZoneInfo PacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    private static readonly TimeZoneInfo UtcTimeZone = TimeZoneInfo.Utc;

    [Fact]
    public void ToTimeZone_DateTime_WithNullTimeZone_ThrowsArgumentNullException()
    {
        // Arrange
        var dateTime = DateTime.Now;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dateTime.ToTimeZone(null!));
    }

    [Fact]
    public void ToTimeZone_DateTime_WithUtcKind_ConvertsCorrectly()
    {
        // Arrange
        var utcDateTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var easternDateTime = utcDateTime.ToTimeZone(EasternTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, easternDateTime.Kind);
        // In June, Eastern Time is UTC-4 (EDT)
        Assert.Equal(8, easternDateTime.Hour); // 12 UTC becomes 8 AM EDT
    }

    [Fact]
    public void ToTimeZone_DateTime_WithLocalKind_ConvertsCorrectly()
    {
        // Arrange
        var localDateTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Local);

        // Act
        var utcDateTime = localDateTime.ToTimeZone(UtcTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Utc, utcDateTime.Kind);
    }

    [Fact]
    public void ToTimeZone_DateTime_WithUnspecifiedKind_TreatsAsUtc()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Unspecified);

        // Act
        var easternDateTime = unspecifiedDateTime.ToTimeZone(EasternTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, easternDateTime.Kind);
        // Should treat unspecified as UTC, so 12 UTC becomes 8 AM EDT in June
        Assert.Equal(8, easternDateTime.Hour);
    }

    [Fact]
    public void ToTimeZone_DateTime_ToSameTimeZone_ReturnsEquivalentTime()
    {
        // Arrange
        var utcDateTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var convertedUtcDateTime = utcDateTime.ToTimeZone(UtcTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Utc, convertedUtcDateTime.Kind);
        Assert.Equal(utcDateTime.Hour, convertedUtcDateTime.Hour);
        Assert.Equal(utcDateTime.Minute, convertedUtcDateTime.Minute);
        Assert.Equal(utcDateTime.Second, convertedUtcDateTime.Second);
    }

    [Fact]
    public void ToTimeZone_DateTime_DuringDaylightSavingTime_HandlesCorrectly()
    {
        // Arrange - Summer time (DST active)
        var utcDateTime = new DateTime(2023, 7, 15, 16, 30, 0, DateTimeKind.Utc);

        // Act
        var easternDateTime = utcDateTime.ToTimeZone(EasternTimeZone);

        // Assert
        // In July, Eastern Time is UTC-4 (EDT)
        Assert.Equal(12, easternDateTime.Hour); // 16 UTC becomes 12 PM EDT
        Assert.Equal(30, easternDateTime.Minute);
    }

    [Fact]
    public void ToTimeZone_DateTime_DuringStandardTime_HandlesCorrectly()
    {
        // Arrange - Winter time (DST not active)
        var utcDateTime = new DateTime(2023, 1, 15, 16, 30, 0, DateTimeKind.Utc);

        // Act
        var easternDateTime = utcDateTime.ToTimeZone(EasternTimeZone);

        // Assert
        // In January, Eastern Time is UTC-5 (EST)
        Assert.Equal(11, easternDateTime.Hour); // 16 UTC becomes 11 AM EST
        Assert.Equal(30, easternDateTime.Minute);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToTimeZone_DateTime_WithDifferentKinds_ReturnsUnspecifiedKind(DateTimeKind kind)
    {
        // Arrange
        var dateTime = new DateTime(2023, 6, 15, 12, 0, 0, kind);

        // Act
        var convertedDateTime = dateTime.ToTimeZone(PacificTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, convertedDateTime.Kind);
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_WithNullTimeZone_ThrowsArgumentNullException()
    {
        // Arrange
        var dateTimeOffset = DateTimeOffset.Now;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => dateTimeOffset.ToTimeZone(null!));
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_ConvertsCorrectly()
    {
        // Arrange
        var utcOffset = new DateTimeOffset(2023, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        var easternOffset = utcOffset.ToTimeZone(EasternTimeZone);

        // Assert
        // In June, Eastern Time is UTC-4 (EDT)
        Assert.Equal(8, easternOffset.Hour); // 12 UTC becomes 8 AM EDT
        Assert.Equal(0, easternOffset.Minute);
        Assert.Equal(TimeSpan.FromHours(-4), easternOffset.Offset); // EDT offset
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_BetweenTimeZones_MaintainsInstant()
    {
        // Arrange
        var pacificOffset = new DateTimeOffset(2023, 6, 15, 9, 30, 0, TimeSpan.FromHours(-7)); // 9:30 AM PDT

        // Act
        var easternOffset = pacificOffset.ToTimeZone(EasternTimeZone);

        // Assert
        // 9:30 AM PDT should become 12:30 PM EDT (same instant)
        Assert.Equal(12, easternOffset.Hour);
        Assert.Equal(30, easternOffset.Minute);
        Assert.Equal(TimeSpan.FromHours(-4), easternOffset.Offset); // EDT offset

        // Verify they represent the same instant
        Assert.Equal(pacificOffset.UtcDateTime, easternOffset.UtcDateTime);
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_ToUtc_ConvertsCorrectly()
    {
        // Arrange
        var easternOffset = new DateTimeOffset(2023, 1, 15, 10, 0, 0, TimeSpan.FromHours(-5)); // 10 AM EST

        // Act
        var utcOffset = easternOffset.ToTimeZone(UtcTimeZone);

        // Assert
        Assert.Equal(15, utcOffset.Hour); // 10 AM EST becomes 3 PM UTC
        Assert.Equal(0, utcOffset.Minute);
        Assert.Equal(TimeSpan.Zero, utcOffset.Offset); // UTC offset
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_ToSameTimeZone_ChangesOffsetButNotTime()
    {
        // Arrange
        // Create a DateTimeOffset with a custom offset that doesn't match Eastern Time
        var customOffset = new DateTimeOffset(2023, 6, 15, 12, 0, 0, TimeSpan.FromHours(-3));

        // Act
        var easternOffset = customOffset.ToTimeZone(EasternTimeZone);

        // Assert
        // The time should be adjusted to match the Eastern timezone
        Assert.Equal(TimeSpan.FromHours(-4), easternOffset.Offset); // EDT offset in June
        // The instant should remain the same
        Assert.Equal(customOffset.UtcDateTime, easternOffset.UtcDateTime);
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_DuringDaylightSavingTransition_HandlesCorrectly()
    {
        // Arrange - Test during DST transition
        var utcOffset = new DateTimeOffset(2023, 3, 12, 7, 0, 0, TimeSpan.Zero); // 7 AM UTC on DST transition day

        // Act
        var easternOffset = utcOffset.ToTimeZone(EasternTimeZone);

        // Assert
        // March 12, 2023 is when DST begins in Eastern Time
        // 7 AM UTC should be 3 AM EDT (after the "spring forward")
        Assert.Equal(3, easternOffset.Hour);
        Assert.Equal(TimeSpan.FromHours(-4), easternOffset.Offset); // EDT offset
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_PreservesMilliseconds()
    {
        // Arrange
        var preciseOffset = new DateTimeOffset(2023, 6, 15, 12, 30, 45, 123, TimeSpan.Zero);

        // Act
        var convertedOffset = preciseOffset.ToTimeZone(PacificTimeZone);

        // Assert
        Assert.Equal(30, convertedOffset.Minute);
        Assert.Equal(45, convertedOffset.Second);
        Assert.Equal(123, convertedOffset.Millisecond);
    }

    [Fact]
    public void ToTimeZone_DateTime_MinValue_HandlesCorrectly()
    {
        // Arrange
        var minDateTime = DateTime.MinValue; // Has DateTimeKind.Unspecified

        // Act
        var convertedDateTime = minDateTime.ToTimeZone(UtcTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Utc, convertedDateTime.Kind);
        // Should not throw and should handle the edge case
    }

    [Fact]
    public void ToTimeZone_DateTime_MaxValue_HandlesCorrectly()
    {
        // Arrange
        var maxDateTime = DateTime.MaxValue; // Has DateTimeKind.Unspecified

        // Act
        var convertedDateTime = maxDateTime.ToTimeZone(UtcTimeZone);

        // Assert
        Assert.Equal(DateTimeKind.Utc, convertedDateTime.Kind);
        // Should not throw and should handle the edge case
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_MinValue_HandlesCorrectly()
    {
        // Arrange
        var minDateTimeOffset = DateTimeOffset.MinValue;

        // Act
        var convertedOffset = minDateTimeOffset.ToTimeZone(UtcTimeZone);

        // Assert
        // Should not throw and should handle the edge case
        Assert.Equal(TimeSpan.Zero, convertedOffset.Offset);
    }

    [Fact]
    public void ToTimeZone_DateTimeOffset_MaxValue_HandlesCorrectly()
    {
        // Arrange
        var maxDateTimeOffset = DateTimeOffset.MaxValue;

        // Act
        var convertedOffset = maxDateTimeOffset.ToTimeZone(UtcTimeZone);

        // Assert
        // Should not throw and should handle the edge case
        Assert.Equal(TimeSpan.Zero, convertedOffset.Offset);
    }

    [Theory]
    [InlineData("Eastern Standard Time")]
    [InlineData("Pacific Standard Time")]
    [InlineData("Central Standard Time")]
    [InlineData("Greenwich Standard Time")]
    public void ToTimeZone_DateTime_WithVariousTimeZones_WorksCorrectly(string timeZoneId)
    {
        // Arrange
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var utcDateTime = new DateTime(2023, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var convertedDateTime = utcDateTime.ToTimeZone(timeZone);

        // Assert
        Assert.Equal(DateTimeKind.Unspecified, convertedDateTime.Kind);
        // The conversion should work without throwing
    }

    [Theory]
    [InlineData("UTC")]
    [InlineData("Eastern Standard Time")]
    [InlineData("Pacific Standard Time")]
    [InlineData("Central Standard Time")]
    [InlineData("Greenwich Standard Time")]
    public void ToTimeZone_DateTimeOffset_WithVariousTimeZones_WorksCorrectly(string timeZoneId)
    {
        // Arrange
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var utcOffset = new DateTimeOffset(2023, 6, 15, 12, 0, 0, TimeSpan.Zero);

        // Act
        var convertedOffset = utcOffset.ToTimeZone(timeZone);

        // Assert
        // The conversion should work without throwing
        Assert.Equal(timeZone.GetUtcOffset(utcOffset), convertedOffset.Offset);
    }
}
