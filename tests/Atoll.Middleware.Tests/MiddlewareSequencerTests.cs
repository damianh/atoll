using Atoll.Middleware.Pipeline;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests;

public sealed class MiddlewareSequencerTests
{
    private static EndpointRequest CreateRequest(string path)
    {
        return new EndpointRequest("GET", new Uri($"http://localhost{path}"));
    }

    private static MiddlewareContext CreateContext(string path)
    {
        return new MiddlewareContext(
            path,
            new Dictionary<string, string>(),
            CreateRequest(path));
    }

    // ---- Basic sequencing ----

    [Fact]
    public async Task EmptySequenceShouldCallNext()
    {
        var pipeline = MiddlewareSequencer.Sequence();
        var context = CreateContext("/");
        var terminalCalled = false;

        var response = await pipeline(context, () =>
        {
            terminalCalled = true;
            return Task.FromResult(AtollResponse.Text("terminal"));
        });

        terminalCalled.ShouldBeTrue();
        response.GetBodyAsString().ShouldBe("terminal");
    }

    [Fact]
    public async Task SingleMiddlewareShouldExecuteAndCallNext()
    {
        var log = new List<string>();

        MiddlewareHandler middleware = async (ctx, next) =>
        {
            log.Add("before");
            var response = await next();
            log.Add("after");
            return response;
        };

        var pipeline = MiddlewareSequencer.Sequence(middleware);
        var context = CreateContext("/");

        var response = await pipeline(context, () =>
        {
            log.Add("terminal");
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        log.ShouldBe(new[] { "before", "terminal", "after" });
        response.GetBodyAsString().ShouldBe("ok");
    }

    [Fact]
    public async Task MultipleMiddlewareShouldExecuteInOrder()
    {
        var log = new List<string>();

        MiddlewareHandler first = async (ctx, next) =>
        {
            log.Add("first-before");
            var response = await next();
            log.Add("first-after");
            return response;
        };

        MiddlewareHandler second = async (ctx, next) =>
        {
            log.Add("second-before");
            var response = await next();
            log.Add("second-after");
            return response;
        };

        MiddlewareHandler third = async (ctx, next) =>
        {
            log.Add("third-before");
            var response = await next();
            log.Add("third-after");
            return response;
        };

        var pipeline = MiddlewareSequencer.Sequence(first, second, third);
        var context = CreateContext("/");

        var response = await pipeline(context, () =>
        {
            log.Add("terminal");
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        log.ShouldBe(new[]
        {
            "first-before", "second-before", "third-before",
            "terminal",
            "third-after", "second-after", "first-after"
        });
    }

    // ---- Short-circuit ----

    [Fact]
    public async Task MiddlewareShouldShortCircuitWithoutCallingNext()
    {
        var terminalCalled = false;

        MiddlewareHandler auth = (ctx, next) =>
        {
            return Task.FromResult(AtollResponse.Text("Unauthorized", 401));
        };

        MiddlewareHandler logging = async (ctx, next) =>
        {
            return await next(); // Should never be reached
        };

        var pipeline = MiddlewareSequencer.Sequence(auth, logging);
        var context = CreateContext("/");

        var response = await pipeline(context, () =>
        {
            terminalCalled = true;
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        terminalCalled.ShouldBeFalse();
        response.StatusCode.ShouldBe(401);
        response.GetBodyAsString().ShouldBe("Unauthorized");
    }

    // ---- Modify response ----

    [Fact]
    public async Task MiddlewareShouldModifyResponseAfterNext()
    {
        MiddlewareHandler addHeader = async (ctx, next) =>
        {
            var response = await next();
            // Create a new response with an additional header
            var headers = new Dictionary<string, string>(response.Headers)
            {
                ["X-Custom"] = "added"
            };
            return new AtollResponse(response.StatusCode, headers, response.Body);
        };

        var pipeline = MiddlewareSequencer.Sequence(addHeader);
        var context = CreateContext("/");

        var response = await pipeline(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        response.Headers["X-Custom"].ShouldBe("added");
        response.GetBodyAsString().ShouldBe("ok");
    }

    // ---- Locals sharing ----

    [Fact]
    public async Task MiddlewareShouldShareDataViaLocals()
    {
        MiddlewareHandler setUser = async (ctx, next) =>
        {
            ctx.Locals["user"] = "alice";
            return await next();
        };

        MiddlewareHandler readUser = async (ctx, next) =>
        {
            var user = ctx.GetLocal<string>("user");
            ctx.Locals["greeting"] = $"Hello, {user}!";
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(setUser, readUser);
        var context = CreateContext("/");

        await pipeline(context, () =>
            Task.FromResult(AtollResponse.Text("ok")));

        context.Locals["user"].ShouldBe("alice");
        context.Locals["greeting"].ShouldBe("Hello, alice!");
    }

    // ---- Null handling ----

    [Fact]
    public void SequenceShouldThrowForNullArray()
    {
        Should.Throw<ArgumentNullException>(() =>
            MiddlewareSequencer.Sequence((MiddlewareHandler[])null!));
    }

    [Fact]
    public void SequenceFromListShouldThrowForNullList()
    {
        Should.Throw<ArgumentNullException>(() =>
            MiddlewareSequencer.Sequence((IReadOnlyList<MiddlewareHandler>)null!));
    }

    [Fact]
    public async Task SequenceShouldFilterNullHandlers()
    {
        var log = new List<string>();

        MiddlewareHandler valid = async (ctx, next) =>
        {
            log.Add("valid");
            return await next();
        };

        var pipeline = MiddlewareSequencer.Sequence(null!, valid, null!);
        var context = CreateContext("/");

        await pipeline(context, () =>
        {
            log.Add("terminal");
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        log.ShouldBe(new[] { "valid", "terminal" });
    }

    [Fact]
    public async Task SequenceShouldHandleAllNullHandlers()
    {
        var pipeline = MiddlewareSequencer.Sequence(null!, null!);
        var context = CreateContext("/");
        var terminalCalled = false;

        await pipeline(context, () =>
        {
            terminalCalled = true;
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        terminalCalled.ShouldBeTrue();
    }

    // ---- IReadOnlyList overload ----

    [Fact]
    public async Task SequenceFromListShouldComposeHandlers()
    {
        var log = new List<string>();

        MiddlewareHandler first = async (ctx, next) =>
        {
            log.Add("first");
            return await next();
        };

        MiddlewareHandler second = async (ctx, next) =>
        {
            log.Add("second");
            return await next();
        };

        var handlers = new List<MiddlewareHandler> { first, second };
        var pipeline = MiddlewareSequencer.Sequence(handlers);
        var context = CreateContext("/");

        await pipeline(context, () =>
        {
            log.Add("terminal");
            return Task.FromResult(AtollResponse.Text("ok"));
        });

        log.ShouldBe(new[] { "first", "second", "terminal" });
    }
}
