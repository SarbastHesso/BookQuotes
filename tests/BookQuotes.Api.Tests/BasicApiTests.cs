using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BookQuotes.Api.Models;
using BookQuotes.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
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

        [Fact]
        public void TokenService_Uses_Staging_Jwt_Key_Fallback()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["STAGING_JWT_KEY"] = "BookQuotes_Staging_Test_Key_ChangeMe_1234567890",
                    ["Jwt:Issuer"] = "BookQuotesApi",
                    ["Jwt:Audience"] = "BookQuotesClient",
                    ["Jwt:ExpiresInMinutes"] = "60"
                })
                .Build();

            var tokenService = new TokenService(configuration);

            var token = tokenService.CreateToken(new User
            {
                Id = 1,
                UserName = "tester"
            });

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task Register_Then_Login_Works_With_Staging_Jwt_Key_Fallback()
        {
            Environment.SetEnvironmentVariable("STAGING_JWT_KEY", "BookQuotes_Staging_Test_Key_ChangeMe_1234567890");

            try
            {
                var client = _factory.CreateClient();
                var userName = $"tester{Guid.NewGuid():N}";
                var password = "TestPass123";

                var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
                {
                    userName,
                    password
                });

                Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

                var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
                {
                    userName,
                    password
                });

                Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            }
            finally
            {
                Environment.SetEnvironmentVariable("STAGING_JWT_KEY", null);
            }
        }
    }
}
