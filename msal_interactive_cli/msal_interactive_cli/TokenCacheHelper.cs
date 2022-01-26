using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace msal_interactive_cli
{
    class TokenCacheHelper
    {
        private static byte[] PROTECT_DATA_ENTROPY = {
            102, 82, 159, 62, 168, 246, 213, 232, 90, 35, 114, 0, 101, 28, 102, 107,
            70, 226, 179, 2, 34, 116, 132, 85, 150, 14, 145, 125, 252, 70, 127, 44 };
        private byte[] cacheData;
        private DataProtectionScope dataProtectionScope;

        public TokenCacheHelper(byte[] data, DataProtectionScope protectionScope)
        {
            cacheData = data;
            dataProtectionScope = protectionScope;
        }

        public byte[] CacheData
        {
            get { return cacheData; }
        }

        public void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            byte[] data = null;
            if (cacheData != null)
            {
                data = ProtectedData.Unprotect(cacheData, PROTECT_DATA_ENTROPY, dataProtectionScope);
            }
            args.TokenCache.DeserializeMsalV3(data);
        }

        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                cacheData = ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                    PROTECT_DATA_ENTROPY,
                    dataProtectionScope);
            }
        }

        internal void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }

}
