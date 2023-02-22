using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Diagnostics.Models;

internal static class Helper
{
    public static TraceContext? ToTraceContext(this Activity activity)
    {
        var traceId = activity.TraceId.ToGuid();
        var spanId = activity.SpanId.ToGuid();
        return new TraceContext(traceId, spanId);
    }

    public static Activity ToActivity(this TraceContext context)
    {
        var activity = new Activity(TracingConstants.VostokTracerActivityName);
        activity.SetParentId(
            ActivityTraceId.CreateFromString(context.TraceId.ToString("N")),
            ActivitySpanId.CreateFromString(context.SpanId.ToString("N").AsSpan()[..16]));
        return activity;
    }

    public static Guid ToGuid(this ActivityTraceId traceId) =>
        Guid.TryParse(traceId.ToHexString(), out var guid) ? guid : default;

    public static Guid ToGuid(this ActivitySpanId spanId) =>
        Guid.TryParse(spanId.ToHexString().PadRight(32, '0'), out var guid) ? guid : default;
}