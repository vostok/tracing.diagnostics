using System.Diagnostics;

namespace Vostok.Tracing.Diagnostics;

public class ActivitySourceTracerSettings
{
    public ActivitySource ActivitySource { get; set; } = new ActivitySource(TracingConstants.VostokTracerActivitySourceName);
}