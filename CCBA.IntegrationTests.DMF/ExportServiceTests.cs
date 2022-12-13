using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Xunit;

namespace CCBA.IntegrationTests.DMF
{
    public class ExportServiceTests
    {
        [Fact]
        public async Task BasicIntegrationTest()
        {
            // Arrange
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    // Add TestServer
                    webHost.UseTestServer();
                 
                    // Specify the environment
                    webHost.UseEnvironment("Test");

                    webHost.Configure(app => app.Run(async ctx => await ctx.Response.WriteAsync("Hello World!")));
                });

            // Create and start up the host
            var host = await hostBuilder.StartAsync();

            // Create an HttpClient which is setup for the test host
            var client = host.GetTestClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello World!", responseString);
        }
    }
}
