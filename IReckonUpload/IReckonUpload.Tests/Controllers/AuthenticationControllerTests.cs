using IReckonUpload.Controllers;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Authentication;
using IReckonUpload.Tools;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IReckonUpload.DAL;

namespace IReckonUpload.Tests.Controllers
{
    [TestClass]
    public class AuthenticationControllerTests
    {
        private AppConfigurationOptions stubedConfiguration;
        private Mock<IConsumerRepository> mockedConsumerRepository;
        private Mock<IOptions<AppConfigurationOptions>> mockedConfiguration;
        private AuthenticationController controller;

        [TestInitialize]
        public void Init()
        {
            this.stubedConfiguration = new AppConfigurationOptions
            {

            };
            this.mockedConsumerRepository = new Mock<IConsumerRepository>();
            this.mockedConfiguration = new Mock<IOptions<AppConfigurationOptions>>();
            this.mockedConfiguration.SetupGet(c => c.Value).Returns(this.stubedConfiguration);
            this.controller = new AuthenticationController(this.mockedConsumerRepository.Object, this.mockedConfiguration.Object);
        }

        [TestMethod]
        public void ItShouldThrowANullReferenceExceptionIfConfigurationIsMissing()
        {
            Should.Throw<NullReferenceException>(() =>
            {
                new AuthenticationController(this.mockedConsumerRepository.Object, null);
            });
            this.mockedConfiguration.SetupGet(c => c.Value).Returns((AppConfigurationOptions)null);
            Should.Throw<NullReferenceException>(() =>
            {
                new AuthenticationController(this.mockedConsumerRepository.Object, this.mockedConfiguration.Object);
            });
        }

        [TestMethod]
        public void ItShouldReturnNullIfCredentialsDoesNotMatchAKnownUser()
        {
            this.mockedConsumerRepository.Setup(r => r.Find(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IConsumer)null)
                .Verifiable();
            var request = new LoginRequest { Username = "toto", Password = "*******" };
            var result = this.controller.Post(request);

            var hashedPwd = Sha256Builder.Compute(request.Password);

            this.mockedConsumerRepository.Verify(r => r.Find("toto", hashedPwd), Times.Once);
            result.ShouldBeNull();
        }

        [TestMethod]
        public void ItShouldReturnAResponseContainingTokenIfCredentialsMatchAKnownUser()
        {
            this.stubedConfiguration.JsonWebTokenConfig = new JsonWebTokenConfiguration
            {
                Issuer = "test",
                Secret = "this is unit test, so my secret should be anything if bullshit",
                Validity = 1
            };

            var mockedUser = new Mock<IConsumer>();

            this.mockedConsumerRepository.Setup(r => r.Find(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockedUser.Object);

            var request = new LoginRequest { Username = "toto", Password = "*******" };
            var result = this.controller.Post(request);

            result.ShouldBeOfType<LoginSuccessResponse>();
            result.JsonWebToken.ShouldNotBeEmpty();
        }

        [TestMethod]
        public async Task ItShouldBeOkToAuthenticateUsingApi()
        {
            var stubedKnownConsumer = new Mock<IConsumer>();

            this.mockedConsumerRepository.Setup(r => r.Find(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(stubedKnownConsumer.Object);

            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.IntegrationTest.json");

            var _server = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((ctx, conf) =>
                {
                    conf.AddJsonFile(configPath);
                })
                .UseStartup<Startup>()
                .ConfigureTestServices(serviceCollection => {
                    serviceCollection.AddSingleton(this.mockedConsumerRepository.Object);
                }));

            var _client = _server.CreateClient();
            var request = new LoginRequest
            {
                Username = "myUsername",
                Password = "****"
            };

            var response = await _client.PostAsync("/api/authentication", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "text/json"));

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            LoginSuccessResponse loginSuccessResponse = JsonConvert.DeserializeObject<LoginSuccessResponse>(responseString);

            loginSuccessResponse.ShouldNotBeNull();

            // Despite the fact we own the configuration AND the consumer, the validity header make the jwt unpredicatable. We cannot test for the output.
        }
    }
}
