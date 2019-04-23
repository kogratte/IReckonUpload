using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Tests.Controllers
{
    [TestClass]
    public class UploadTests
    {
        [TestMethod]
        public async Task UploadEndpointShouldRespondWithPermissionDeniedIfCalledWithoutAValidJWT()
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.IntegrationTest.json");

            var _server = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((ctx, conf) =>
                {
                    conf.AddJsonFile(configPath);
                })
                .UseStartup<Startup>());

            var _client = _server.CreateClient();

            var response = await _client.PostAsync("/api/upload", new StringContent("'toto'",  Encoding.UTF8, "text/json"));
            ((int)response.StatusCode).ShouldBe(StatusCodes.Status401Unauthorized);
        }
    }
}
