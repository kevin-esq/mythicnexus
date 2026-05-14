using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MythicNexus.Api.IntegrationTests;

/// <summary>
/// Hosts the API with environment <c>Testing</c> so <c>appsettings.Testing.json</c> from the API project is applied.
/// </summary>
public sealed class MythicNexusApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Testing");
}
