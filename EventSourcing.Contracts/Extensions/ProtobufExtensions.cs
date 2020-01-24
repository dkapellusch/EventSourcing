using System;
using Google.Protobuf.WellKnownTypes;

namespace EventSourcing.Contracts.Extensions
{
    public static class ProtobufExtensions
    {
        public static bool IsInactive(this Lock resourceLock) => resourceLock?.Expiry.ToDateTimeOffset() <= DateTimeOffset.UtcNow;

        public static Timestamp ParseTimeStamp(this string timestamp) =>
            DateTime.Parse(timestamp)
                .ToUniversalTime()
                .AddHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours)
                .ToTimestamp();
    }
}