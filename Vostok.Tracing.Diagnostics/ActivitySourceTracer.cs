using System.Collections.Generic;
using System.Diagnostics;
using Vostok.Commons.Environment;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Models;

namespace Vostok.Tracing.Diagnostics;

/// <summary>
/// <see cref="ITracer"/> implementation based on <see cref="ActivitySource"/>.
/// </summary>
public class ActivitySourceTracer : ITracer
{
    private readonly ActivitySource activitySource;
    private readonly IEnumerable<KeyValuePair<string, object?>> initialTags;

    static ActivitySourceTracer()
    {
        //Helper.SynchronizeContexts();
    }
    
    /// <summary>
    /// <para>Creates <see cref="ITracer"/> implementation based on given <paramref name="activitySource"/>.</para>
    /// <para>If <paramref name="activitySource"/> isn't specified, creates a new one with <see cref="TracingConstants.VostokTracerActivitySourceName"/> name.</para>
    /// </summary>
    // todo (kungurtsev, 20.02.2023): delete settings?
    public ActivitySourceTracer(TracerSettings settings, ActivitySource? activitySource = null)
    {
        this.activitySource = activitySource ?? new ActivitySource(TracingConstants.VostokTracerActivitySourceName);

        var initialTagsList = new List<KeyValuePair<string, object?>>
        {
            new(WellKnownAnnotations.Common.Host, settings.Host ?? EnvironmentInfo.Host),
            new(WellKnownAnnotations.Common.Application, settings.Application ?? EnvironmentInfo.Application)
        };
        if (settings.Environment != null)
            initialTagsList.Add(new(WellKnownAnnotations.Common.Environment, settings.Environment));
        initialTags = initialTagsList.ToArray();
    }

    /// <summary>
    /// Returns <see cref="TraceContext"/> constructed from <see cref="Activity.Current"/> <see cref="Activity"/>.
    /// </summary>
    public TraceContext? CurrentContext
    {
        get => Activity.Current?.ToTraceContext();
        set => Activity.Current = value?.ToActivity();
    }

    /// <summary>
    /// Starts new <see cref="Activity"/> with <see cref="TracingConstants.VostokTracerActivityName"/> name.
    /// </summary>
    public ISpanBuilder BeginSpan()
    {
        var activity = activitySource.StartActivity(TracingConstants.VostokTracerActivityName, ActivityKind.Internal, new ActivityContext(), initialTags);
        return new ActivitySpanBuilder(activity);
    }
}