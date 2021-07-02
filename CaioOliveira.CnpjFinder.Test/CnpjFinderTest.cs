using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaioOliveira.CnpjFinder.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CaioOliveira.CnpjFinder.Test
{
    [TestClass]
    public class CnpjFinderTest
    {
        private readonly Finder _finder;

        public CnpjFinderTest()
        {
            var loggerMock = new Mock<ILogger<Finder>>();
            var configurationMock = new Mock<IOptions<ServiceConfiguration>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var configuration = new ServiceConfiguration
            {
                WebBasePath = "http://cnpj.info"
            };
            var httpClient = new HttpClient();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            configurationMock.Setup(x => x.Value).Returns(configuration);
            _finder = new Finder(
                loggerMock.Object,
                configurationMock.Object,
                httpClientFactoryMock.Object);
        }

        [TestMethod]
        public async Task GetCNPJ_03740726000159()
        {
            var result = await _finder.Get("02930076000141");

            Assert.AreEqual("02930076000141", result.CNPJ);
            Assert.AreEqual("SUBMARINO S/A.", result.CompanyName);
            Assert.AreEqual(DateTime.Parse("1999-01-04"), result.CreationDate);
            Assert.AreEqual(false, result.IsActive);
            Assert.AreEqual("643", result.Number);
            Assert.AreEqual("06210108", result.CEP);
        }
    }
}