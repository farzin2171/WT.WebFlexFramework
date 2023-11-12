namespace WT.WebApplication.Infrastructure.Authentication
{
    public interface IAuthToken
    {
        Task<JwtToken> AuthenticateAsync();
    }
}
