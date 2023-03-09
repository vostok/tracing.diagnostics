using System;

namespace Vostok.Tracing.Diagnostics.Tests;

internal static class Helper
{
    public static Guid Shortify(this Guid guid) =>
        Guid.Parse(guid.ToString("N")[..16].PadRight(32, '0'));
}