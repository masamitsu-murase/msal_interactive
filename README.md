![Wheel](https://github.com/masamitsu-murase/msal_interactive/workflows/Wheel/badge.svg)

# MSAL Interactive Token Acquirer

This is a library to acquire Microsoft OAuth2 access token **interactively**.

## Overview

This library supports **interactive** authentication.  
If you need a library for non-interactive authentication, you can use [MSAL for python](https://github.com/AzureAD/microsoft-authentication-library-for-python), which supports many authentication flows.

## How to use

You can acquire Microsoft OAuth2 access token interactively as follows:

```python
from msal_interactive_token_acquirer import MsalInteractiveTokenAcquirer

tenant = "common"
client_id = "5d2a0ea0-a46a-4626-835e-04e13f75fed0"
scopes = ["User.Read"]
msal = MsalInteractiveTokenAcquirer(tenant, client_id, scopes)
msal.acquire_token_interactively()  # (*1)
msal.access_token()
# => This returns Bearer token.
```

This shows the following window at `(*1)`.  
![window](https://raw.githubusercontent.com/masamitsu-murase/msal_interactive/master/resources/window.py)

You can save and load `MsalInteractiveTokenAcquirer` as follows:

```python
from msal_interactive_token_acquirer import MsalInteractiveTokenAcquirer
import pickle

# ...snip...

# msal is an instance of MsalInteractiveTokenAcquirer.
with open("msal.pickle", "wb") as f:
    pickle.dump(msal, f)

# ...snip...

with open("msal.pickle", "rb") as f:
    msal = pickle.load(f)
msal.access_token()
# => This returns Bearer token.
```


## License

You can use this library under the MIT License.

Copyright 2020 Masamitsu MURASE

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

