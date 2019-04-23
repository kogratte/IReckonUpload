using IReckonUpload.DAL;
using IReckonUpload.Models.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var _server = new TestServer(GetTestHostBuilder());

            var _client = _server.CreateClient();

            var response = await _client.PostAsync("/api/upload", new StringContent("'toto'",  Encoding.UTF8, "text/json"));
            ((int)response.StatusCode).ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [TestMethod]
        public async Task UploadEndpointShouldRespondWithPermissionDeniedIfCalledWithAnInvalidJWT()
        {
            var jwt = await GetJWT();
            var _server = new TestServer(GetTestHostBuilder());

            var _client = _server.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt + "44");

            var response = await _client.PostAsync("/api/upload", new StringContent("'toto'", Encoding.UTF8, "text/json"));
            ((int)response.StatusCode).ShouldBe(StatusCodes.Status401Unauthorized);
        }

        [TestMethod]
        public async Task UploadEndpointShouldRespondWithBadRequestIfCalledWithValidJWTAndBullshitAsPayload()
        {
            var jwt = await GetJWT();

            var _server = new TestServer(GetTestHostBuilder());

            var _client = _server.CreateClient();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await _client.PostAsync("/api/upload", new StringContent("'toto'", Encoding.UTF8, "text/json"));
            ((int)response.StatusCode).ShouldBe(StatusCodes.Status401Unauthorized);
        }

        private async Task<string> GetJWT()
        {
            var stubedKnownConsumer = new Mock<IConsumer>();
            var mockedConsumerRepository = new Mock<IConsumerRepository>();

            mockedConsumerRepository.Setup(r => r.Find(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(stubedKnownConsumer.Object);

            var _server = new TestServer(GetTestHostBuilder()
                .ConfigureTestServices(serviceCollection => {
                    serviceCollection.AddSingleton(mockedConsumerRepository.Object);
                }));

            var _client = _server.CreateClient();
            var request = new LoginRequest
            {
                Username = "myUsername",
                Password = "****"
            };

            var response = await _client.PostAsync("/api/authentication", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "text/json"));

            response.EnsureSuccessStatusCode();

            var loginSuccessResponse = JsonConvert.DeserializeObject<LoginSuccessResponse>(await response.Content.ReadAsStringAsync());

            return loginSuccessResponse.JsonWebToken;
        }

        private IWebHostBuilder GetTestHostBuilder()
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.IntegrationTest.json");

            return new WebHostBuilder()
                .ConfigureAppConfiguration((ctx, conf) =>
                {
                    conf.AddJsonFile(configPath);
                })
                .UseStartup<Startup>();
        }
    }
}
