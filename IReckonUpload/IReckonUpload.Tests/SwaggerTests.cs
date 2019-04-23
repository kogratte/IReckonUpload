using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Shouldly;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http;
using System;

namespace IReckonUpload.Tests
{
    [TestClass]
    public class StartupTests
    {
        [TestMethod]
        public async Task HttpsShouldBeUsed()
        {
            var options = new RewriteOptions().AddRedirectToHttpsPermanent();
            var _server = new TestServer(new WebHostBuilder().UseStartup<Startup>()
                .Configure(app => app.UseRewriter(options)));
            var _client = _server.CreateClient();

            var response = await _client.GetAsync(new Uri("http://example.com/index.html"));

            response.Headers.Location.OriginalString.ShouldBe("https://example.com/index.html");

            ((int)response.StatusCode).ShouldBe(StatusCodes.Status301MovedPermanently);
        }
    }
}
