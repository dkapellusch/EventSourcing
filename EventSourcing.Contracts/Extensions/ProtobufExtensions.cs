using System;

namespace EventSourcing.Contracts.Extensions
{
    public static class ProtobufExtensions
    {
        public static bool IsInactive(this Lock resourceLock) => resourceLock?.Expiry.ToDateTimeOffset() <= DateTimeOffset.UtcNow;
    }
}