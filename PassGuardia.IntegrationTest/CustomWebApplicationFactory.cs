using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace PassGuardia.IntegrationTest;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseSerilog((ctx, cfg) =>
        {
            cfg.ReadFrom.Configuration(ctx.Configuration);
            cfg.MinimumLevel.Is(Serilog.Events.LogEventLevel.Warning);
        });

        return base.CreateHost(builder);
    }
}