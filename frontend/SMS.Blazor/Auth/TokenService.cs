using Microsoft.JSInterop;

namespace SMS.Blazor.Auth
{
    public class TokenService
    {
        private const string AccessTokenStorageKey = "sms.accessToken";
        private const string ExpiresAtStorageKey = "sms.expiresAt";
        private readonly IJSRuntime _jsRuntime;
        private string _accessToken = string.Empty;
        private DateTime _expiresAt = DateTime.MinValue;
        private bool _initialized;

        public TokenService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public string AccessToken => _accessToken;
        public bool IsTokenValid => !string.IsNullOrEmpty(_accessToken) && _expiresAt > DateTime.UtcNow;

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", AccessTokenStorageKey);
                var expiresAtRaw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", ExpiresAtStorageKey);

                if (!string.IsNullOrWhiteSpace(token) &&
                    DateTime.TryParse(expiresAtRaw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiresAt))
                {
                    _accessToken = token;
                    _expiresAt = expiresAt;
                }
            }
            catch
            {
            }

            _initialized = true;
        }

        public async Task SetTokenAsync(string token, DateTime expiresAt)
        {
            _accessToken = token;
            _expiresAt = expiresAt;

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AccessTokenStorageKey, token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ExpiresAtStorageKey, expiresAt.ToString("O"));
            }
            catch
            {
            }
        }

        public async Task ClearTokenAsync()
        {
            _accessToken = string.Empty;
            _expiresAt = DateTime.MinValue;

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AccessTokenStorageKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ExpiresAtStorageKey);
            }
            catch
            {
            }
        }

        public bool IsExpiringSoon()
        {
            // Check if token expires within 60 seconds
            return _expiresAt <= DateTime.UtcNow.AddSeconds(60);
        }
    }
}
