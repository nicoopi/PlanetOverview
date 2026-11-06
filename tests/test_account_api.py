"""Account HTTP integration tests. Build first; uses only the Python stdlib."""
import http.cookiejar
from contextlib import closing
import json
import os
from pathlib import Path
import shutil
import socket
import sqlite3
import subprocess
import tempfile
import time
import unittest
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "PlanetOverview"
LOCAL_DOTNET = Path(os.environ.get("LOCALAPPDATA", "")) / "PlanetOverview/dotnet/dotnet.exe"
DOTNET = str(LOCAL_DOTNET) if LOCAL_DOTNET.is_file() else shutil.which("dotnet")
PASSWORD = "PlanetTest123"


class Client:
    def __init__(self, base):
        self.base = base
        self.cookies = http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.cookies))

    def request(self, method="GET", path="me", payload=None, csrf=True):
        headers = {"Content-Type": "application/json"}
        if csrf and method != "GET":
            headers["X-CSRF-TOKEN"] = self.request(path="csrf")[1]["token"]
        request = urllib.request.Request(self.base + "/api/account/" + path,
            data=json.dumps(payload).encode() if payload is not None else None,
            method=method, headers=headers)
        try:
            response = self.opener.open(request, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            body = response.read()
            try:
                data = json.loads(body) if body else None
            except ValueError:
                data = body.decode()
            return response.status, data, response.headers


class AccountApiTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory(prefix="planet-account-")
        self.addCleanup(self.directory.cleanup)
        self.database = Path(self.directory.name) / "accounts.db"
        with socket.socket() as sock:
            sock.bind(("127.0.0.1", 0))
            self.base = f"http://127.0.0.1:{sock.getsockname()[1]}"
        self.client = Client(self.base)
        self.start()
        self.addCleanup(self.stop)

    def start(self):
        self.process = subprocess.Popen(
            [DOTNET, str(PROJECT / "bin/Debug/net8.0/PlanetOverview.dll")], cwd=PROJECT,
            env={**os.environ, "ASPNETCORE_ENVIRONMENT": "Development", "ASPNETCORE_URLS": self.base,
                 "ConnectionStrings__Accounts": f"Data Source={self.database}"},
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
            creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
        for _ in range(200):
            if self.process.poll() is not None:
                raise RuntimeError("API startup failed. Build the project first.")
            try:
                self.client.request(path="csrf")
                return
            except OSError:
                time.sleep(.1)
        self.stop()
        raise RuntimeError("API startup timed out.")

    def stop(self):
        if self.process.poll() is None:
            self.process.terminate()
            self.process.wait(timeout=10)

    def register(self, client=None, email="explorer@example.test"):
        return (client or self.client).request("POST", "register",
            {"name": "Explorador", "email": email, "password": PASSWORD})

    def test_account_crud_password_change_logout_and_session_revocation(self):
        self.assertEqual(self.client.request()[0], 401)
        status, user, headers = self.register()
        self.assertEqual(status, 201)
        self.assertTrue(headers["Location"].endswith("/api/account/me"))
        self.assertNotIn("password", json.dumps(user).lower())
        self.assertEqual(self.client.request()[1], user)
        # Signed-in users opening auth forms must reach the site, not profile editing.
        for route in ("login", "register"):
            with self.client.opener.open(self.base + "/account/" + route, timeout=10) as response:
                self.assertEqual(response.geturl(), self.base + "/")
        other_session = Client(self.base)
        self.assertEqual(other_session.request("POST", "login", {"email": user["email"], "password": PASSWORD})[0], 200)
        update = {"name": "Novo nome", "email": "updated@example.test", "currentPassword": PASSWORD,
                  "newPassword": "weakpass"}
        self.assertEqual(self.client.request("PUT", payload=update)[0], 400)
        self.assertEqual(self.client.request()[1], user, "Failed updates must roll back the whole transaction")
        update["newPassword"] = "NewPlanet456"
        status, updated, _ = self.client.request("PUT", payload=update)
        self.assertEqual(status, 200)
        self.assertEqual(updated["id"], user["id"])
        self.assertEqual(updated["name"], "Novo nome")
        self.assertEqual(updated["email"], update["email"])
        self.assertEqual(other_session.request()[0], 401, "Other sessions must be revoked")
        self.assertEqual(self.client.request("POST", "logout")[0], 204)
        self.assertEqual(self.client.request()[0], 401)
        self.assertEqual(self.client.request("POST", "login", {"email": update["email"], "password": PASSWORD})[0], 401)
        self.assertEqual(self.client.request("POST", "login", {"email": update["email"], "password": update["newPassword"]})[0], 200)
        self.assertEqual(self.client.request("DELETE", payload={"currentPassword": "wrong"})[0], 400)
        self.assertEqual(self.client.request("DELETE", payload={"currentPassword": update["newPassword"]})[0], 204)
        self.assertEqual(self.client.request()[0], 401)
        with closing(sqlite3.connect(self.database)) as connection:
            self.assertEqual(connection.execute('SELECT count(*) FROM AspNetUsers').fetchone()[0], 0)

    def test_validation_duplicate_email_and_csrf(self):
        for payload in ({}, {"name": "A", "email": "bad", "password": "123"},
                        {"name": "  ", "email": "a@example.test", "password": PASSWORD},
                        {"name": "Valid", "email": "a@example.test", "password": "onlylowercase"}):
            self.assertEqual(self.client.request("POST", "register", payload)[0], 400)
        self.assertEqual(self.register()[0], 201)
        self.assertEqual(self.register(email="EXPLORER@example.test")[0], 409)
        self.assertEqual(self.client.request("PUT", payload={"name": "x"}, csrf=False)[0], 400)
        self.assertEqual(self.client.request("DELETE", payload={"currentPassword": PASSWORD}, csrf=False)[0], 400)
        self.assertEqual(self.client.request("POST", "logout", csrf=False)[0], 400)
        anonymous = Client(self.base)
        self.assertEqual(anonymous.request("POST", "login", {"email": "explorer@example.test", "password": PASSWORD}, csrf=False)[0], 400)
        self.assertEqual(self.client.request()[0], 200)

    def test_persistence_hash_and_cookie_flags(self):
        status, _, headers = self.register()
        self.assertEqual(status, 201)
        session_cookie = next(cookie for cookie in headers.get_all("Set-Cookie") if cookie.startswith("PlanetOverview.Session="))
        self.assertIn("httponly", session_cookie.lower())
        self.assertIn("samesite=lax", session_cookie.lower())
        with closing(sqlite3.connect(self.database)) as connection:
            stored_hash = connection.execute('SELECT PasswordHash FROM AspNetUsers').fetchone()[0]
            self.assertNotEqual(stored_hash, PASSWORD)
            self.assertNotIn(PASSWORD, stored_hash)
        self.stop()
        self.start()
        self.assertEqual(self.client.request()[0], 200)
        anonymous = Client(self.base)
        self.assertEqual(anonymous.request("POST", "login", {"email": "explorer@example.test", "password": PASSWORD})[0], 200)

    def test_accounts_are_isolated_and_conflicting_update_is_atomic(self):
        self.assertEqual(self.register()[0], 201)
        first = self.client.request()[1]
        other = Client(self.base)
        self.assertEqual(self.register(other, "other@example.test")[0], 201)
        second = other.request()[1]
        self.assertNotEqual(first["id"], second["id"])
        self.assertEqual(other.request("PUT", payload={"name": "Changed", "email": first["email"], "currentPassword": PASSWORD})[0], 409)
        self.assertEqual(other.request()[1], second)
        self.assertEqual(other.request("DELETE", payload={"currentPassword": PASSWORD})[0], 204)
        self.assertEqual(self.client.request()[1], first)

    def test_lockout_and_rate_limiting(self):
        self.assertEqual(self.register()[0], 201)
        anonymous = Client(self.base)
        for _ in range(5):
            self.assertEqual(anonymous.request("POST", "login", {"email": "explorer@example.test", "password": "incorrect"})[0], 401)
        self.assertEqual(anonymous.request("POST", "login", {"email": "explorer@example.test", "password": PASSWORD})[0], 401)
        statuses = [anonymous.request("POST", "login", {"email": "unknown@example.test", "password": PASSWORD})[0] for _ in range(5)]
        self.assertIn(429, statuses)

    def test_password_recovery_without_email_is_protected_and_single_use(self):
        self.assertEqual(self.register()[0], 201)
        self.assertEqual(self.client.request('POST', 'recovery-code', {'currentPassword': 'wrong'})[0], 400)
        status, generated, _ = self.client.request('POST', 'recovery-code', {'currentPassword': PASSWORD})
        self.assertEqual(status, 200)
        code = generated['recoveryCode']
        with closing(sqlite3.connect(self.database)) as connection:
            stored = connection.execute("SELECT Value FROM AspNetUserTokens WHERE Name = 'PasswordRecovery'").fetchone()[0]
            self.assertNotEqual(stored, code.replace('-', ''))
            self.assertEqual(len(stored), 64)
        anonymous = Client(self.base)
        self.assertEqual(anonymous.request('POST', 'recovery-code', {'currentPassword': PASSWORD})[0], 401)
        payload = {'email': 'explorer@example.test', 'recoveryCode': 'INVALID', 'newPassword': 'Recovered123'}
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 400)
        payload['recoveryCode'] = code
        payload['newPassword'] = 'weakpass'
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 400)
        payload['newPassword'] = 'Recovered123'
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 204)
        self.assertEqual(self.client.request()[0], 401, 'Recovery must revoke existing sessions')
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 400, 'Codes must be single use')
        self.assertEqual(anonymous.request('POST', 'login', {'email': payload['email'], 'password': PASSWORD})[0], 401)
        self.assertEqual(anonymous.request('POST', 'login', {'email': payload['email'], 'password': payload['newPassword']})[0], 200)

    def test_regenerating_recovery_code_invalidates_previous_code(self):
        self.assertEqual(self.register()[0], 201)
        old = self.client.request('POST', 'recovery-code', {'currentPassword': PASSWORD})[1]['recoveryCode']
        new = self.client.request('POST', 'recovery-code', {'currentPassword': PASSWORD})[1]['recoveryCode']
        self.assertNotEqual(old, new)
        anonymous = Client(self.base)
        payload = {'email': 'explorer@example.test', 'recoveryCode': old, 'newPassword': 'Recovered123'}
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 400)
        payload['recoveryCode'] = new
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload, csrf=False)[0], 400)
        payload['email'] = 'someoneelse@example.test'
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 400)
        payload['email'] = 'explorer@example.test'
        self.assertEqual(anonymous.request('POST', 'forgot-password', payload)[0], 204)


if __name__ == "__main__":
    unittest.main(verbosity=2)
