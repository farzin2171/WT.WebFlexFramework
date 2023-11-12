using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WT.WebApplication.DTO;
using WT.WebApplication.Infrastructure.Authentication;

namespace WT.WebApplication.Pages
{
    [Authorize(Policy = "HRManagerOnly")]
    public class HRManagerModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public List<WeatherForecastDTO> weatherForecastItems { get; set; } = new List<WeatherForecastDTO>();

        private IAuthToken _authToken;
        public HRManagerModel(IHttpClientFactory httpClientFactory, IAuthToken authToken)
        {
            _httpClientFactory = httpClientFactory;
            _authToken = authToken;
        }
        public async Task OnGet()
        {

            // get token from session
            JwtToken token = new JwtToken();

            var strTokenObj = HttpContext.Session.GetString("access_token");
            if (string.IsNullOrEmpty(strTokenObj))
            {
                token = await _authToken.AuthenticateAsync();
            }
            else
            {
                token = JsonSerializer.Deserialize<JwtToken>(strTokenObj) ?? new JwtToken();
            }

            if (token == null ||
                string.IsNullOrWhiteSpace(token.AccessToken) ||
                token.ExpiresAt <= DateTime.UtcNow)
            {
                token = await _authToken.AuthenticateAsync();
            }

            var httpClient = _httpClientFactory.CreateClient("MainWebAPI");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token?.AccessToken ?? string.Empty);
            weatherForecastItems = await httpClient.GetFromJsonAsync<List<WeatherForecastDTO>>("WeatherForecast") ?? new List<WeatherForecastDTO>();
        }
    }
}
