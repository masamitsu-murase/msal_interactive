from requests.auth import AuthBase


class MsalAuth(AuthBase):
    def __init__(self, token_acquirer):
        self._token_acquirer = token_acquirer

    def __call__(self, r):
        r.headers["Authorization"] = ("Bearer " +
                                      self._token_acquirer.access_token())
        return r
