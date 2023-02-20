using System;
using System.Diagnostics;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Diagnostics.Models;

internal class ActivitySpanBuilder : ISpanBuilder
{
    private readonly Activity? activity;

    public ActivitySpanBuilder(Activity? activity) =>
        this.activity = activity;

    public ISpan CurrentSpan => activity == null
        ? DevNullSpan.Instance
        : new ActivitySpan(activity);

    public void Dispose() =>
        activity?.Dispose();

    public void SetAnnotation(string key, object value, bool allowOverwrite = true) =>
        activity?.SetTag(key, value);

    public void SetBeginTimestamp(DateTimeOffset timestamp) =>
        activity?.SetStartTime(timestamp.UtcDateTime);

    public void SetEndTimestamp(DateTimeOffset? timestamp)
    {
        if (timestamp != null)
            activity?.SetEndTime(timestamp.Value.UtcDateTime);
    }
}