﻿using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Diagnostics.Tests;

[TestFixture]
internal class ActivitySourceTracer_Tests
{
    private ActivitySource activitySource = null!;
    private TracerSettings tracerSettings = null!;
    private ActivitySourceTracer activitySourceTracer = null!;
    private Tracer tracer = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var activitySourceName = "TestActivitySource";
        ActivitySource.AddActivityListener(new ActivityListener
        {
            ShouldListenTo = s => s.Name == activitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        });
        activitySource = new ActivitySource(activitySourceName);

        tracerSettings = new TracerSettings(new DevNullSpanSender());
        activitySourceTracer = new ActivitySourceTracer(new ActivitySourceTracerSettings
        {
            ActivitySource = activitySource
        });
        tracer = new Tracer(tracerSettings);
    }

    [Test]
    public void StartActivity_should_not_be_null()
    {
        activitySource.StartActivity().Should().NotBeNull();
    }

    [Test]
    public void BeginSpan_should_fill_CurrentSpan_ids()
    {
        using var span1 = activitySourceTracer.BeginSpan();
        using var span2 = activitySourceTracer.BeginSpan();

        span1.CurrentSpan.TraceId.Should().NotBe(Guid.Empty);
        span1.CurrentSpan.SpanId.Should().NotBe(Guid.Empty);
        span1.CurrentSpan.ParentSpanId.Should().BeNull();

        span2.CurrentSpan.TraceId.Should().NotBe(Guid.Empty);
        span2.CurrentSpan.SpanId.Should().NotBe(Guid.Empty);
        span2.CurrentSpan.ParentSpanId.Should().Be(span1.CurrentSpan.SpanId);
    }

    [Test]
    public void BeginSpan_should_sync_with_Tracer()
    {
        using (var span1 = activitySourceTracer.BeginSpan())
        using (var span2 = tracer.BeginSpan())
        {
            span2.CurrentSpan.TraceId.Should().Be(span1.CurrentSpan.TraceId);
            span2.CurrentSpan.ParentSpanId.Should().Be(span1.CurrentSpan.SpanId);
        }

        using (var span1 = tracer.BeginSpan())
        using (var span2 = activitySourceTracer.BeginSpan())
        {
            span2.CurrentSpan.TraceId.Should().Be(span1.CurrentSpan.TraceId);
            span2.CurrentSpan.ParentSpanId.Should().Be(span1.CurrentSpan.SpanId.Shortify());
        }
    }

    [Test]
    public void CurrentContext_should_be_editable()
    {
        var traceContext = new TraceContext(Guid.NewGuid(), Guid.NewGuid());

        activitySourceTracer.CurrentContext = traceContext;
        activitySourceTracer.CurrentContext.TraceId.Should().Be(traceContext.TraceId);
        activitySourceTracer.CurrentContext.SpanId.Should().Be(traceContext.SpanId.Shortify());
    }

    [Test]
    public void CurrentContext_should_sync_with_Tracer()
    {
        tracer.CurrentContext = null;

        using (var span = activitySourceTracer.BeginSpan())
        {
            tracer.CurrentContext.Should().NotBeNull();
            tracer.CurrentContext.Should().BeEquivalentTo(activitySourceTracer.CurrentContext);
            tracer.CurrentContext!.TraceId.Should().Be(span.CurrentSpan.TraceId);
            tracer.CurrentContext!.SpanId.Should().Be(span.CurrentSpan.SpanId);
        }

        tracer.CurrentContext.Should().BeNull();
        activitySourceTracer.CurrentContext.Should().BeNull();

        using (var span = tracer.BeginSpan())
        {
            activitySourceTracer.CurrentContext.Should().NotBeNull();
            activitySourceTracer.CurrentContext.Should().BeEquivalentTo(tracer.CurrentContext);
            activitySourceTracer.CurrentContext!.TraceId.Should().Be(span.CurrentSpan.TraceId);
            activitySourceTracer.CurrentContext!.SpanId.Should().Be(span.CurrentSpan.SpanId.Shortify());
        }

        tracer.CurrentContext.Should().BeNull();
        activitySourceTracer.CurrentContext.Should().BeNull();
    }
}