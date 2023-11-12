using WT.WebApplication.Infrastructure.Authorization;
using System.Text.Json;

namespace WT.WebApplication.Infrastructure.Authentication
{
    public class AuthToken : IAuthToken
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthToken(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<JwtToken> AuthenticateAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("MainWebAPI");
            var res = await httpClient.PostAsJsonAsync("auth", new Credential { UserName = "admin", Password = "password" });
            res.EnsureSuccessStatusCode();
            string strJwt = await res.Content.ReadAsStringAsync();
            _httpContextAccessor.HttpContext?.Session?.SetString("access_token", strJwt);

            return JsonSerializer.Deserialize<JwtToken>(strJwt) ?? new JwtToken();
        }
    }
}
