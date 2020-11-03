using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        public string login_hint { get; set; }
        public DataProtectionScope data_protection_scope { get; set; }

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
        GetToken(InputParameter input_parameter, string redirectUri)
        {
            TokenCacheHelper tokenCache = null;
            try
            {
                var app = CreatePublicClient(input_parameter.tenant,
                    input_parameter.client_id, redirectUri);
                tokenCache = new TokenCacheHelper(input_parameter.cacheData, input_parameter.data_protection_scope);
                tokenCache.EnableSerialization(app.UserTokenCache);

                AuthenticationResult result;
                var accounts = await app.GetAccountsAsync();
                var account = accounts.FirstOrDefault();
                if (input_parameter.interactive)
                {
                    try
                    {
                        result = await app.AcquireTokenSilent(input_parameter.scopes, account)
                            .WithForceRefresh(true)
                            .ExecuteAsync();
                    }
                    catch (MsalUiRequiredException)
                    {
                        var token_acquirer = app.AcquireTokenInteractive(input_parameter.scopes)
                            .WithPrompt(Prompt.SelectAccount);
                        if (input_parameter.login_hint != null)
                        {
                            token_acquirer = token_acquirer.WithLoginHint(input_parameter.login_hint);
                        }
                        result = await token_acquirer.ExecuteAsync();
                    }
                }
                else
                {
                    result = await app.AcquireTokenSilent(input_parameter.scopes, account)
                        .WithForceRefresh(true)
                        .ExecuteAsync();
                }
                return (result, tokenCache.CacheData, null);
            }
            catch (Exception ex)
            {
                if (tokenCache != null)
                {
                    return (null, tokenCache.CacheData, ex.Message);
                }
                else
                {
                    return (null, null, ex.Message);
                }
            }
        }

        static InputParameter ProcessInput()
        {
            while (true)
            {
                try
                {
                    var line = Console.ReadLine();
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new JsonStringEnumConverter());
                    return JsonSerializer.Deserialize<InputParameter>(line, options);
                }
                catch (Exception ex)
                {
                    SendOutput(null, null, ex.Message);
                }
            }
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
                var task = GetToken(input_parameter, redirectUri);
                var (result, cacheData, error) = task.Result;
                SendOutput(result, cacheData, error);
                input_parameter = ProcessInput();
            }
        }
    }
}

