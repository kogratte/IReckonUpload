using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Shouldly;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Tests.Controllers
{
    [TestClass]
    public class UploadTests
    {
        [TestMethod]
        public async Task UploadEndpointShouldRespondWithHttpOK()
        {
            var _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            var _client = _server.CreateClient();

            var response = await _client.PostAsync("/api/upload", new StringContent("'toto'",  Encoding.UTF8, "text/json"));
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            responseString.ShouldBeEmpty();
        }
    }
}
