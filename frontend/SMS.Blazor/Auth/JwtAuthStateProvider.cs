using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SMS.Blazor.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly TokenService _tokenService;

        public JwtAuthStateProvider(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await _tokenService.InitializeAsync();

            if (!_tokenService.IsTokenValid)
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }

            var claims = ParseClaimsFromJwt(_tokenService.AccessToken);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                claims.AddRange(token.Claims);
            }
            catch
            {
                // Return empty claims if token parsing fails
            }

            return claims;
        }
    }
}
