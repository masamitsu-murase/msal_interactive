from datetime import datetime, timedelta, timezone
import json
from pathlib import Path
import subprocess

BASE_DIR = Path(__file__).absolute().parent
MSAL_EXE = str(BASE_DIR / "tools" / "msal_interactive_cli.exe")


class MsalError(RuntimeError):
    pass


class MsalInteractiveTokenAcquirer(object):
    def __init__(self,
                 tenant,
                 client_id,
                 scopes,
                 *,
                 data_protection_scope="CurrentUser"):
        self._tenant = tenant
        self._client_id = client_id
        self._scopes = scopes
        self._expires_at = None
        self._access_token = None
        self._cache_data_base64 = None
        self._data_protection_scope = data_protection_scope

    def token_acquisition_parameter(self, interactive, login_hint):
        parameter = {
            "action": "acquire_token",
            "tenant": self._tenant,
            "client_id": self._client_id,
            "scopes": self._scopes,
            "cache_data_base64": self._cache_data_base64,
            "interactive": interactive,
            "login_hint": login_hint,
            "data_protection_scope": self._data_protection_scope
        }
        return parameter

    def acquire_token(self, interactive, login_hint=None):
        with subprocess.Popen([MSAL_EXE],
                              bufsize=1,
                              stdin=subprocess.PIPE,
                              stdout=subprocess.PIPE,
                              stderr=subprocess.DEVNULL,
                              universal_newlines=True) as pipe:
            try:
                parameter = self.token_acquisition_parameter(
                    interactive, login_hint)
                pipe.stdin.write(json.dumps(parameter))
                pipe.stdin.write("\n")
                pipe.stdin.flush()

                result = json.loads(pipe.stdout.readline())
                if result.get("error"):
                    raise MsalError(result["error"])

                access_token = result["access_token"]
                cache_data_base64 = result["cache_data_base64"]
                expires_at = datetime(*result["expires_at"],
                                      tzinfo=timezone.utc)
                self._access_token = access_token
                self._cache_data_base64 = cache_data_base64
                self._expires_at = expires_at
            finally:
                pipe.stdin.write(json.dumps({"action": "quit"}))
                pipe.stdin.write("\n")
                pipe.stdin.flush()

    def acquire_token_interactively(self, login_hint=None):
        self.acquire_token(True, login_hint)

    def update_token(self):
        self.acquire_token(False)

    def expires_in(self, seconds):
        delta = timedelta(seconds=seconds)
        time = datetime.now(tz=timezone.utc) + delta
        return self._expires_at <= time

    def access_token(self,
                     refresh_as_needed=True,
                     expiration_margin_sec=10 * 60):
        if self._access_token is None:
            self.acquire_token(False)
        if refresh_as_needed and self.expires_in(expiration_margin_sec):
            self.update_token()
        return self._access_token
