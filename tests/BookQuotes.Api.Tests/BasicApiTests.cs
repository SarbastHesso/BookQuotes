using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BookQuotes.Api.Tests
{
    public class BasicApiTests : IClassFixture<WebApplicationFactory<BookQuotes.Api.Program>>
    {
        private readonly WebApplicationFactory<BookQuotes.Api.Program> _factory;

        public BasicApiTests(WebApplicationFactory<BookQuotes.Api.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_Books_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/books");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
