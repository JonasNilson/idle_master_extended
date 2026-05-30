using System;
using System.IO;
using System.Net;
using System.Linq;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using DateTime = System.DateTime;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DataReceivedEventArgs = System.Diagnostics.DataReceivedEventArgs;

namespace IdleMasterExtended.Browser
{
    public enum BrowserType
    {
        Chrome,
        MSEdge,
    }

    public static class BrowserInfo
    {
        public static string ToString(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                    return "Chrome";
                case BrowserType.MSEdge:
                    return "Edge";
                default:
                    throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null);
            }
        }

        public static BrowserType From(string source)
        {
            foreach (BrowserType browserType in Enum.GetValues(typeof(BrowserType)))
            {
                if (source.IndexOf(ToString(browserType), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return browserType;
                }
            }

            throw new ArgumentException($"{source} -> INVALID");
        }

        public static string GetUserDataDir(BrowserType browserType)
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string driveLetter = Path.GetPathRoot(userProfilePath).ToUpper();

            switch (browserType)
            {
                case BrowserType.Chrome:
                    return Path.Combine(driveLetter, "Users", Path.GetFileName(userProfilePath), "AppData", "Local", "Google", "Chrome", "User Data");
                case BrowserType.MSEdge:
                    return Path.Combine(driveLetter, "Users", Path.GetFileName(userProfilePath), "AppData", "Local", "Microsoft", "Edge", "User Data");
                default:
                    throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null);
            }
        }

        public static List<string> GetProfilesOf(string userDataDir)
        {
            List<string> profilePaths = new List<string>();

            if (Directory.Exists(userDataDir))
            {
                var directories = Directory.GetDirectories(userDataDir);

                foreach (var directory in directories)
                {
                    string networkPath = Path.Combine(directory, "Network");
                    if (Directory.Exists(networkPath))
                    {
                        profilePaths.Add(directory);
                    }
                }
            }

            return profilePaths;
        }

        public static string GetExecutablePath(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                    return @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                case BrowserType.MSEdge:
                    return @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                default:
                    throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null);
            }
        }
    }
    
    internal class RootObject
    {
        public int Id { get; set; }
        public IResult Result { get; set; }

        public class IResult
        {
            public List<CookieData> Cookies { get; set; }
        }

        public class CookieData
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Domain { get; set; }
            public string Path { get; set; }
            public long Expires { get; set; }
            public int Size { get; set; }
            public bool HttpOnly { get; set; }
            public bool Secure { get; set; }
            public bool Session { get; set; }
            public string SameSite { get; set; }
            public string Priority { get; set; }
            public bool SameParty { get; set; }
            public string SourceScheme { get; set; }
            public int SourcePort { get; set; }
        }
    }

    public class Cookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HasExpire { get; set; }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Name) &&
                       string.IsNullOrEmpty(Value) &&
                       !HasExpire;
            }
        }
    }

    public class Chromium
    {

        public Chromium(string profileDir, string steamID)
        {
            port = GetAvailablePort();
            browserType = BrowserInfo.From(profileDir); 
            proc = Launch(profileDir, $"https://steamcommunity.com/profiles/{steamID}");
        }

        private static int port;
        private static Process proc;
        private static BrowserType browserType;
        private static readonly Random random = new Random();

        public List<Cookie> Extract()
        {
            List<Cookie> cookies = new List<Cookie>();

            using (var webClient = new WebClient())
            {
                try
                {
                    var regex = new Regex($"\"webSocketDebuggerUrl\":\\s*\"(ws://localhost:{port}/.*)\"");
                    var response = webClient.DownloadString($"http://localhost:{port}/json");
                    var match = regex.Match(response);
                    if (!match.Success)
                    {
                        return cookies;
                    }

                    var debugUrl = match.Groups[1].Value;
                    const string cookiesRequest = "{\"id\": 1, \"method\": \"Storage.getCookies\"}";

                    var json = WebSocketRequest35(debugUrl, cookiesRequest);
                    var rootObject = JsonConvert.DeserializeObject<RootObject>(json);

                    if (rootObject == null || rootObject.Result == null || rootObject.Result.Cookies == null)
                    {
                        return cookies;
                    }

                    foreach (var cookieData in rootObject.Result.Cookies)
                    {
                        var cookie = new Cookie
                        {
                            Name = cookieData.Name,
                            Value = cookieData.Value,
                            HasExpire = cookieData.Expires != -1,
                        };

                        cookies.Add(cookie);
                    }

                    return cookies;
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, $"Extract() -> {ex.GetType()}");
                    return cookies;
                }
            }
        }

        private static Process Launch(string profileDir, string url)
        {
            var browserProcess = new Process();
            var path = BrowserInfo.GetExecutablePath(browserType);

            browserProcess.StartInfo.FileName = path;
            browserProcess.StartInfo.CreateNoWindow = true;
            browserProcess.StartInfo.UseShellExecute = false;
            browserProcess.StartInfo.RedirectStandardError = true;
            browserProcess.StartInfo.RedirectStandardOutput = true;

            browserProcess.ErrorDataReceived += LogData;
            browserProcess.OutputDataReceived += LogData;

            browserProcess.StartInfo.Arguments = $"\"{url}\" --headless=new --profile-directory=\"{Path.GetFileName(profileDir)}\" --remote-debugging-port={port} --remote-allow-origins=ws://localhost:{port}";

            browserProcess.Start();
            browserProcess.BeginOutputReadLine();
            browserProcess.BeginErrorReadLine();

            Process.GetProcessById(browserProcess.Id);

            if (WaitForPort(port)) return browserProcess;

            return null;
        }
        
        private static void LogData(object sender, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Console.WriteLine($"LogData() -> {args.Data}");
            }
        }

        public void Close()
        {
            try
            {
                if (proc == null) return;
                proc.ErrorDataReceived -= LogData;
                proc.OutputDataReceived -= LogData;
                Process.GetProcessById(proc.Id).Kill();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"KILLED {proc?.Id}?");
            }
        }

        private static string WebSocketRequest35(string server, string data)
        {
            var result = "";
            var dataReceived = false;
            var websocket = new WebSocket(server);

            websocket.MessageReceived += WebsocketMessageReceived;
            websocket.Opened += WebsocketOpened;
            websocket.Open();

            var start = DateTime.Now;
            var now = DateTime.Now;

            while (!dataReceived && now.Subtract(start).TotalMilliseconds < 30000)
            {
                Thread.Sleep(500);
            }

            return result;

            void WebsocketOpened(object sender, EventArgs e)
            {
                websocket.Send(data);
            }

            void WebsocketMessageReceived(object sender, MessageReceivedEventArgs e)
            {
                result += e.Message;
                dataReceived = true;
            }
        }

        private static bool WaitForPort(int port)
        {
            var start = DateTime.Now;
            var now = DateTime.Now;

            while (now.Subtract(start).TotalMilliseconds < 30000)
            {
                using (var tcpClient = new TcpClient())
                {
                    try
                    {
                        tcpClient.Connect("127.0.0.1", port);
                        return true;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(500);
                        now = DateTime.Now;
                    }
                }
            }

            return false;
        }

        private static int GetAvailablePort()
        {
            var usedPorts = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Select(endpoint => endpoint.Port)
                .ToHashSet();

            int randomPort;

            do
            {
                randomPort = random.Next(1024, 65536);
            } while (usedPorts.Contains(randomPort));

            return randomPort;
        }
    }
}