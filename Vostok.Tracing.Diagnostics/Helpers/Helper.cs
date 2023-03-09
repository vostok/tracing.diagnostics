using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Diagnostics.Helpers;

internal static class Helper
{
    private static readonly Action<Activity, string> ActivityTraceIdSetter;
    private static readonly Action<Activity, string> ActivitySpanIdSetter;

    static Helper()
    {
        ActivityTraceIdSetter = BuildSetter("_traceId");
        ActivitySpanIdSetter = BuildSetter("_spanId");
    }

    public static TraceContext? ToTraceContext(this Activity activity)
    {
        var traceId = activity.TraceId.ToGuid();
        var spanId = activity.SpanId.ToGuid();
        return new TraceContext(traceId, spanId);
    }

    public static Activity ToActivity(this TraceContext context)
    {
        var activity = new Activity(TracingConstants.VostokTracerActivityName);
        var traceId = context.TraceId.ToString("N");
        var spanId = context.SpanId.ToString("N")[..16];

        ActivityTraceIdSetter(activity, traceId);
        ActivitySpanIdSetter(activity, spanId);

        return activity;
    }

    private static Action<Activity, string> BuildSetter(string fieldName)
    {
        var type = typeof(Activity);
        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new Exception($"Activity don't have '{fieldName}' field.");

        var inputActivity = Expression.Parameter(typeof(Activity));
        var inputValue = Expression.Parameter(typeof(string));
        var fieldInformation = Expression.Field(inputActivity, field);
        var fieldAssignment = Expression.Assign(fieldInformation, inputValue);

        return Expression.Lambda<Action<Activity, string>>(fieldAssignment, inputActivity, inputValue).Compile();
    }
}