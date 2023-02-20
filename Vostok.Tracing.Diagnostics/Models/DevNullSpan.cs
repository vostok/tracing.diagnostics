using System;
using System.Collections.Generic;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Diagnostics.Models;

internal class DevNullSpan : ISpan
{
    public static readonly DevNullSpan Instance = new DevNullSpan();

    public Guid TraceId => Guid.Empty;

    public Guid SpanId => Guid.Empty;

    public Guid? ParentSpanId => null;

    public DateTimeOffset BeginTimestamp => DateTimeOffset.MinValue;

    public DateTimeOffset? EndTimestamp => DateTimeOffset.MinValue;

    public IReadOnlyDictionary<string, object> Annotations { get; } = new Dictionary<string, object>();
}