using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string cache_data_base64 { get; set; }

        public byte[] cacheData
        {
            get
            {
                if (cache_data_base64==null || !cache_data_base64.Any())
                {
                    return null;
                }

                return Convert.FromBase64String(cache_data_base64);
            }
        }
    }

    class OutputParameter
    {
        public string access_token { get; set; }
        public string cache_data_base64 { get; set; }
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

        static async Task<(string, byte[])> GetToken(string tenant, string clientId, string redirectUri,
            string[] scopes, byte[] cacheData)
        {
            var app = CreatePublicClient(tenant, clientId, redirectUri);
            var tokenCache = new TokenCacheHelper(cacheData);
            tokenCache.EnableSerialization(app.UserTokenCache);

            var accounts = await app.GetAccountsAsync();
            var account = accounts.FirstOrDefault();
            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
            }
            return ("result", tokenCache.CacheData);
        }

        static InputParameter ProcessInput()
        {
            var line = Console.ReadLine();
            return JsonSerializer.Deserialize<InputParameter>(line);
        }

        static void SendOutput(string accessToken, byte[] cacheData)
        {
            var obj = new OutputParameter
            {
                access_token = accessToken,
                cache_data_base64 = Convert.ToBase64String(cacheData)
            };
            Console.WriteLine(JsonSerializer.Serialize(obj));
        }

        static void Main(string[] args)
        {
            var tenant = args[0];
            var clientId = args[1];
            var scopes = args[2].Split(',');

            var redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";

            var input_parameter = ProcessInput();
            while (input_parameter.action != "quit")
            {
                var task = GetToken(tenant, clientId, redirectUri, scopes, input_parameter.cacheData);
                var (result, cacheData) = task.Result;
                SendOutput(result, cacheData);
                input_parameter = ProcessInput();
            }
        }
    }
}

