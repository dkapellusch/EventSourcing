using System;
using Google.Protobuf.WellKnownTypes;

namespace EventSourcing.Contracts.Extensions
{
    public static class ProtobufExtensions
    {
        public static bool IsInactive(this Lock resourceLock) => resourceLock.Released || resourceLock.Expiry <= DateTime.UtcNow.ToTimestamp();
    }
}