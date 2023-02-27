using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Vostok.Tracing.Diagnostics.Helpers;

[PublicAPI]
public static class ActivityExtensions
{
    public static Guid ToGuid(this ActivityTraceId traceId) =>
        Guid.TryParse(traceId.ToHexString(), out var guid) ? guid : default;

    public static Guid ToGuid(this ActivitySpanId spanId) =>
        Guid.TryParse(spanId.ToHexString().PadRight(32, '0'), out var guid) ? guid : default;
}