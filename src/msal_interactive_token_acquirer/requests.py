import requests
from .requests_auth import MsalAuth


class MsalSession(requests.Session):
    def __init__(self, token_acquirer, proxies=None):
        super().__init__()
        self.auth = MsalAuth(token_acquirer)
        self.proxies = proxies
