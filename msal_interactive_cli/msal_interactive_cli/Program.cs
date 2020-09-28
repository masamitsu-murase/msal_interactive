using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace msal_interactive_cli
{
    class InputParameter
    {
        public string action { get; set; }
        public string tenant { get; set; }
        public string client_id { get; set; }
        public string[] scopes { get; set; }
        public string cache_data_base64 { get; set; }
        public bool interactive { get; set; }

        public byte[] cacheData
        {
            get
            {
                if (cache_data_base64 == null || !cache_data_base64.Any())
                {
                    return null;
                }

                return Convert.FromBase64String(cache_data_base64);
            }
        }
    }

    class OutputParameter
    {
        public string error { get; set; }
        public string access_token { get; set; }
        public string cache_data_base64 { get; set; }
        public int[] expires_at { get; set; }
    }

    class Program
    {
        private const string BASE_LOGIN_URL = "https://login.microsoftonline.com/";

        static IPublicClientApplication CreatePublicClient(string tenant, string clientId, string redirectUri)
        {
            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority($"{BASE_LOGIN_URL}{tenant}")
                .WithRedirectUri(redirectUri)
                .Build();
            return app;
        }

        static async Task<(AuthenticationResult, byte[], string)>
        GetToken(string tenant, string clientId, string redirectUri,
            string[] scopes, byte[] cacheData, bool interactive)
        {
            var app = CreatePublicClient(tenant, clientId, redirectUri);
            var tokenCache = new TokenCacheHelper(cacheData);
            tokenCache.EnableSerialization(app.UserTokenCache);

            AuthenticationResult result;
            try
            {
                var accounts = await app.GetAccountsAsync();
                var account = accounts.FirstOrDefault();
                if (interactive)
                {
                    try
                    {
                        result = await app.AcquireTokenSilent(scopes, account)
                            .WithForceRefresh(true)
                            .ExecuteAsync();
                    }
                    catch (MsalUiRequiredException)
                    {
                        result = await app.AcquireTokenInteractive(scopes)
                            .WithPrompt(Prompt.SelectAccount)
                            .ExecuteAsync();
                    }
                }
                else
                {
                    result = await app.AcquireTokenSilent(scopes, account)
                        .WithForceRefresh(true)
                        .ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                return (null, tokenCache.CacheData, ex.Message);
            }
            return (result, tokenCache.CacheData, null);
        }

        static InputParameter ProcessInput()
        {
            var line = Console.ReadLine();
            return JsonSerializer.Deserialize<InputParameter>(line);
        }

        static void SendOutput(AuthenticationResult result, byte[] cacheData, string error_message)
        {
            OutputParameter obj;
            if (error_message != null)
            {
                obj = new OutputParameter
                {
                    error = error_message
                };
            }
            else
            {
                var expires_on = result.ExpiresOn.UtcDateTime;
                obj = new OutputParameter
                {
                    error = error_message,
                    access_token = result.AccessToken,
                    cache_data_base64 = Convert.ToBase64String(cacheData),
                    expires_at = new int[] { expires_on.Year, expires_on.Month, expires_on.Day, expires_on.Hour, expires_on.Minute, expires_on.Second }
                };
            }
            Console.WriteLine(JsonSerializer.Serialize(obj));
        }

        static void Main(string[] args)
        {
            var redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";

            var input_parameter = ProcessInput();
            while (input_parameter.action != "quit")
            {
                var task = GetToken(input_parameter.tenant, input_parameter.client_id,
                    redirectUri, input_parameter.scopes, input_parameter.cacheData,
                    input_parameter.interactive);
                var (result, cacheData, error) = task.Result;
                SendOutput(result, cacheData, error);
                input_parameter = ProcessInput();
            }
        }
    }
}

