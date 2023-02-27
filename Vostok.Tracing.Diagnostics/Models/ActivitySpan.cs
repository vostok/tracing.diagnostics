using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Helpers;

namespace Vostok.Tracing.Diagnostics.Models;

internal class ActivitySpan : ISpan
{
    private readonly Activity activity;

    public ActivitySpan(Activity activity) =>
        this.activity = activity;

    public Guid TraceId =>
        activity.TraceId.ToGuid();

    public Guid SpanId =>
        activity.SpanId.ToGuid();

    public Guid? ParentSpanId =>
        activity.ParentSpanId == default ? null : activity.ParentSpanId.ToGuid();

    public DateTimeOffset BeginTimestamp =>
        activity.StartTimeUtc;

    public DateTimeOffset? EndTimestamp =>
        DateTimeOffset.MinValue;

    public IReadOnlyDictionary<string, object?> Annotations =>
        activity.TagObjects.ToDictionary(p => p.Key, p => p.Value);
}