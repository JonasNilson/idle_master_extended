﻿using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using IdleMaster.Properties;

namespace IdleMaster
{
    public partial class frmBrowser : Form
    {

        public int SecondsWaiting = 30;

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(int hInternet, int dwOption, string lpBuffer, int dwBufferLength);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        public frmBrowser()
        {
            // This initializes the components on the form
            InitializeComponent();

            // TODO: Move this somewhere else (changes the loading spinner depending on the theme)
            pictureBox1.Image = Settings.Default.customTheme ? Resources.imgSpinInv : Resources.imgSpin;
        }

        private void frmBrowser_Load(object sender, EventArgs e)
        {
            // Remove any existing session state data
            InternetSetOption(0, 42, null, 0);

            // Localize form
            this.Text = localization.strings.please_login;
            lblSaving.Text = localization.strings.saving_info;

            // Delete Steam cookie data from the browser control
            InternetSetCookie("https://steamcommunity.com", "sessionid", ";expires=Mon, 01 Jan 0001 00:00:00 GMT");
            InternetSetCookie("https://steamcommunity.com", "steamLogin", ";expires=Mon, 01 Jan 0001 00:00:00 GMT");
            InternetSetCookie("https://steamcommunity.com", "steamLoginSecure", ";expires=Mon, 01 Jan 0001 00:00:00 GMT");
            InternetSetCookie("https://steamcommunity.com", "steamRememberLogin", ";expires=Mon, 01 Jan 0001 00:00:00 GMT");

            // When the form is loaded, navigate to the Steam login page using the web browser control
            wbAuth.Navigate("https://steamcommunity.com/login/home/?goto=my/profile", "_self", null, "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko");

            this.BackColor = Settings.Default.customTheme ? Settings.Default.colorBgd : Settings.Default.colorBgdOriginal;
            this.ForeColor = Settings.Default.customTheme ? Settings.Default.colorTxt : Settings.Default.colorTxtOriginal;
        }

        // This code block executes each time a new document is loaded into the web browser control
        private void wbAuth_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Find the page header, and remove it.  This gives the login form a more streamlined look.
            dynamic htmldoc = wbAuth.Document.DomDocument;
            dynamic globalHeader = htmldoc.GetElementById("global_header");
            if (globalHeader != null && !(globalHeader is DBNull))
            {
                try
                {
                    globalHeader.parentNode.removeChild(globalHeader);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "frmBrowswer -> wbAuth_DocumentCompleted -> Removing the global_header resulted in an exception");
                }

            }

            // Get the URL of the page that just finished loading
            var url = wbAuth.Url.AbsoluteUri;

            // If the page it just finished loading is the login page
            if (url.StartsWith("https://steamcommunity.com/login/home/?goto=") ||
                url == "https://store.steampowered.com/login/transfer" ||
                url == "https://store.steampowered.com//login/transfer")
            {
                // Get a list of cookies from the current page
                CookieContainer container = GetUriCookieContainer(wbAuth.Url);
                var cookies = container.GetCookies(wbAuth.Url);
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name.StartsWith("steamMachineAuth"))
                        Settings.Default.steamMachineAuth = cookie.Value;
                }

                browserBarVisibility(true);
                setRememberMeCheckbox(htmldoc);
                createQuickLoginButton(htmldoc);
            }
            // If the page it just finished loading isn't the login page
            else if (url.StartsWith("javascript:") == false && url.StartsWith("about:") == false)
            {
                try
                {
                    dynamic parentalNotice = htmldoc.GetElementById("parental_notice");
                    if (parentalNotice != null && !(parentalNotice is DBNull))
                    {
                        if (parentalNotice.OuterHtml != "")
                        {
                            // Steam family options enabled
                            wbAuth.Show();
                            Width = 1000;
                            Height = 350;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "parental_notice = " + htmldoc.GetElementById("parental_notice"));
                }

                extractSteamCookies();
                Close();
            }
        }

        // Creates a separate buttong to trigger a login call to the Steam Client Javascript API (localhost)
        private void createQuickLoginButton(dynamic htmldoc)
        {
            if (htmldoc != null)
            {
                try
                {
                    dynamic steamLoginDiv = htmldoc.GetElementById("login_btn_signin");

                    if (steamLoginDiv != null && !(steamLoginDiv is DBNull))
                    {
                        const string evalJavascript = "eval(function V_SetCookie() {}, function V_GetCookie() {})";
                        const string loginJavascript = "LoginUsingSteamClient('https://steamcommunity.com/')";
                        const string onclickHtml = "onclick=\"" + evalJavascript + ";" + loginJavascript + "\"";

                        steamLoginDiv.InnerHtml = steamLoginDiv.InnerHtml + 
                            "<input class=\"btn_green_white_innerfade btn_medium\" type=\"submit\" id=\"QuickSteamLogin\" " +
                            "value=\"Quick-Login\" " + onclickHtml + "border=\"0\" tabindex=\"6\">";
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "login_btn_signin = " + htmldoc.GetElementById("login_btn_signin"));
                }
            }
        }

        private static void setRememberMeCheckbox(dynamic htmldoc)
        {
            if (htmldoc != null)
            {
                try
                {
                    dynamic rememberMeCheckBox = htmldoc.GetElementById("remember_login");
                    
                    if (rememberMeCheckBox != null && !(rememberMeCheckBox is DBNull))
                    {
                        rememberMeCheckBox.Checked = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "rember_login = " + htmldoc.GetElementById("remember_login"));
                }
            }
        }

        private void extractSteamCookies()
        {
            var container = GetUriCookieContainer(wbAuth.Url);
            var cookies = container.GetCookies(wbAuth.Url);

            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == "sessionid")
                {
                    Settings.Default.sessionid = cookie.Value;
                }
                else if (cookie.Name == "steamLogin")
                {
                    Settings.Default.steamLogin = cookie.Value;
                    Settings.Default.myProfileURL = SteamProfile.GetSteamUrl();
                }
                else if (cookie.Name == "steamLoginSecure")
                {
                    Settings.Default.steamLoginSecure = cookie.Value;
                    Settings.Default.myProfileURL = SteamProfile.GetSteamUrl();
                }
                else if (cookie.Name == "steamparental")
                {
                    Settings.Default.steamparental = cookie.Value;
                }
                else if (cookie.Name == "steamRememberLogin")
                {
                    Settings.Default.steamRememberLogin = cookie.Value;
                }
            }

            Settings.Default.Save();
        }

        // Extract and display information about the browser URL, protocol and domain
        private void browserBarVisibility(bool visibility)
        {
            // Toggle visibility of the browser bar
            pbWebBrowserLock.Visible = lblWebBrowserAuth.Visible = lblWebBrowser.Visible = visibility;

            if (visibility)
            {
                // Update browser bar content
                pbWebBrowserLock.Image = wbAuth.Url.Scheme == "https" ? Resources.imgLock_w : Resources.imgLock;
                lblWebBrowserAuth.Text = wbAuth.Document.Domain + " (" + wbAuth.Url.Scheme + ")";
                if (wbAuth.Url.Scheme == "https") { lblWebBrowserAuth.BackColor = Settings.Default.colorSteamGreen; lblWebBrowserAuth.ForeColor = Settings.Default.colorBgd; }
                lblWebBrowser.Text = wbAuth.Url.AbsoluteUri;
            }
        }

        // Imports the InternetGetCookieEx function from wininet.dll which allows the application to access the cookie data from the web browser control
        // Reference: http://stackoverflow.com/questions/3382498/is-it-possible-to-transfer-authentication-from-webbrowser-to-webrequest
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            int dwFlags,
            IntPtr lpReserved);

        // Assigns the hex value for the DLL flag that allows Idle Master to be able to access cookie data marked as "HTTP Only"
        private const int InternetCookieHttponly = 0x2000;

        // This function returns cookie data based on a uniform resource identifier
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            // First, create a null cookie container
            CookieContainer cookies = null;

            // Determine the size of the cookie
            var datasize = 8192 * 16;
            var cookieData = new StringBuilder(datasize);

            // Call InternetGetCookieEx from wininet.dll
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }

            // If the cookie contains data, add it to the cookie container
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }

            // Return the cookie container
            return cookies;
        }

        // This code executes each time the web browser control is in the process of navigating
        private void wbAuth_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // Get the url that's being navigated to
            var url = e.Url.AbsoluteUri;

            // Check to see if the page it's navigating to isn't the Steam login page or related calls
            if (url != "https://steamcommunity.com/login/home/?goto=my/profile" &&
                url != "https://store.steampowered.com/login/transfer" &&
                url != "https://store.steampowered.com//login/transfer" &&
                url.StartsWith("javascript:") == false &&
                url.StartsWith("about:") == false)
            {
                // start the sanity check timer
                tmrCheck.Enabled = true;

                // If it's navigating to a page other than the Steam login page, hide the browser control and resize the form
                wbAuth.Visible = false;

                // Scale the form based on the user's DPI settings
                var graphics = CreateGraphics();
                var scaleY = graphics.DpiY * 0.84375;
                var scaleX = graphics.DpiX * 2.84;
                Height = Convert.ToInt32(scaleY);
                Width = Convert.ToInt32(scaleX);

                browserBarVisibility(false); // Hide the browser bar
            }
            else if (url.StartsWith("https://steamcommunity.com/login/home/?goto=my/profile"))
            {
                tmrCheck.Enabled = true;
            }
        }

        private void tmrCheck_Tick(object sender, EventArgs e)
        {
            if (wbAuth.Url.AbsoluteUri.StartsWith("https://steamcommunity.com/id/")
                && wbAuth.ReadyState.Equals(WebBrowserReadyState.Complete))
            {
                // The login is completed, and the profile is visible
                extractSteamCookies();
                stopTimerAndCloseForm();
            }
            else if (wbAuth.Url.AbsoluteUri.StartsWith("https://steamcommunity.com/login/")
              && wbAuth.ReadyState.Equals(WebBrowserReadyState.Loading))
            {
                dynamic htmldoc = wbAuth.Document.DomDocument;
                dynamic quickLoginButton = htmldoc.GetElementById("QuickSteamLogin");

                if (quickLoginButton != null && !(quickLoginButton is DBNull))
                {
                    quickLoginButton.Value = "Loading...";
                }
            }

            if (SecondsWaiting > 0 || wbAuth.ReadyState.Equals(WebBrowserReadyState.Uninitialized))
            {
                SecondsWaiting -= 1;
            }
            else
            {
                stopTimerAndCloseForm();
            }
        }

        private void stopTimerAndCloseForm()
        {
            tmrCheck.Enabled = false;
            Close();
        }
    }
}
