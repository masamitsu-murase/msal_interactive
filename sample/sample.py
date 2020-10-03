from msal_interactive_token_acquirer import MsalInteractiveTokenAcquirer
from msal_interactive_token_acquirer.requests_auth import MsalAuth
import pickle
import requests
import sys

filename = "status.pickle"
if len(sys.argv) >= 2 and sys.argv[1] == "--load":
    with open(filename, "rb") as f:
        msal = pickle.load(f)
    msal.update_token()
else:
    client_id = "5d2a0ea0-a46a-4626-835e-04e13f75fed0"
    msal = MsalInteractiveTokenAcquirer("common", client_id, ["User.Read"])
    msal.acquire_token_interactively()

result = requests.get("https://graph.microsoft.com/v1.0/me",
                      auth=MsalAuth(msal)).json()
print(result)

with open(filename, "wb") as f:
    pickle.dump(msal, f)
