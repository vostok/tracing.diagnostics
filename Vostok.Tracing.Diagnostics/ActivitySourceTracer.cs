using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Helpers;
using Vostok.Tracing.Diagnostics.Models;

namespace Vostok.Tracing.Diagnostics;

/// <summary>
/// <see cref="ITracer"/> implementation based on <see cref="ActivitySource"/>.
/// </summary>
[PublicAPI]
public class ActivitySourceTracer : ITracer
{
    private readonly ActivitySourceTracerSettings settings;

    static ActivitySourceTracer()
    {
        // note (kungurtsev, 22.02.2023): in .NET 7 we can replace it with Activity.CurrentChanged
        FlowingContext.Globals.SetValueStorage(
            () => Activity.Current?.ToTraceContext(),
            x =>
            {
                // note (kungurtsev, 23.03.2023): modification of existing activity brakes traces
                // it happens because ToActivity creates fake activity which is not being recorded (Activity.Recorded)
                // so all child activities will be also arent' recorded
                // for example it may happens in Vostok.Applications.AspNetCore.Middlewares.DistributedContextMiddleware
                if (x == null || Activity.Current == null || Activity.Current.OperationName == TracingConstants.VostokTracerActivityName)
                    Activity.Current = x?.ToActivity();
            });
    }

    /// <summary>
    /// <para>Creates <see cref="ITracer"/> implementation based on given <paramref name="settings.ActivitySource"/>.</para>
    /// </summary>
    public ActivitySourceTracer(ActivitySourceTracerSettings settings) =>
        this.settings = settings;

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
        var activity = settings.ActivitySource.StartActivity(TracingConstants.VostokTracerActivityName, ActivityKind.Internal, Activity.Current?.Context ?? new ActivityContext());
        return new ActivitySpanBuilder(activity);
    }
}