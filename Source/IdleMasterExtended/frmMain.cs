using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdleMasterExtended.Properties;
using Steamworks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Globalization;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace IdleMasterExtended
{
    public partial class frmMain : Form
    {
        private Statistics statistics = new Statistics();
        public List<Badge> AllBadges { get; set; }

        public IEnumerable<Badge> CanIdleBadges
        {
            get { return AllBadges.Where(b => b.RemainingCard != 0); }
        }

        public bool IsCookieReady;
        public bool IsSteamReady;
        public int MaxSimultanousCards = 30;
        public int TimeLeft = 900;
        public int TimeSet = 300;
        public int RetryCount = 0;
        public int ReloadCount = 0;
        public int CardsRemaining { get { return CanIdleBadges.Sum(b => b.RemainingCard); } }
        public int GamesRemaining { get { return CanIdleBadges.Count(); } }
        public Badge CurrentBadge;

        private bool IsCurrentThemeCustom;
        private bool IsCurrentIconsWhite;

        public frmMain()
        {
            InitializeComponent();
            AllBadges = new List<Badge>();
        }

        #region BADGES
        public async Task LoadBadgesAsync()
        {
            picReadingPage.Visible = true;

            if (Settings.Default.IdlingModeWhitelist)
            {
                AllBadges.Clear();

                foreach (var whitelistID in Settings.Default.whitelist)
                {
                    int applicationID;
                    if (int.TryParse(whitelistID, out applicationID)
                        && !AllBadges.Any(badge => badge.AppId.Equals(applicationID)))
                    {
                        AllBadges.Add(new Badge(whitelistID, "App ID: " + whitelistID, "-1", "0"));
                    }
                }
            }
            else
            {
                try
                {
                    HtmlDocument htmlDocument;
                    int totalBadgePages = 1;

                    for (var currentBadgePage = 1; currentBadgePage <= totalBadgePages; currentBadgePage++)
                    {
                        if (totalBadgePages == 1)
                        {
                            htmlDocument = await GetBadgePageAsync(currentBadgePage);
                            totalBadgePages = ExtractTotalBadgePages(htmlDocument);
                        }

                        lblDrops.Text = string.Format(localization.strings.reading_badge_page + " {0}/{1}, " + localization.strings.please_wait, currentBadgePage, totalBadgePages);
                        htmlDocument = await GetBadgePageAsync(currentBadgePage);
                        ProcessBadgesOnPage(htmlDocument);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Badge -> LoadBadgesAsync, for profile = " + Settings.Default.myProfileURL);
                    ResetFormDesign();
                    ReloadCount = 1;
                    tmrBadgeReload.Enabled = true;
                    return;
                }
            }

            ResetRetryCountAndUpdateApplicationState();
        }

        private async Task<HtmlDocument> GetBadgePageAsync(int pageNumber)
        {
            var document = new HtmlDocument();
            var profileLink = Settings.Default.myProfileURL + "/badges";
            var pageURL = string.Format("{0}/?p={1}", profileLink, pageNumber);
            var response = await CookieClient.GetHttpAsync(pageURL);
            CheckIfResponseIsNullWithRetryCount(response);
            document.LoadHtml(response);

            return document;
        }

        private static int ExtractTotalBadgePages(HtmlDocument document)
        {
            // If user is authenticated, check page count. If user is not authenticated, pages are different.
            var pages = new List<string>() { "?p=1" };
            var pageNodes = document.DocumentNode.SelectNodes("//a[@class=\"pagelink\"]");
            if (pageNodes != null)
            {
                pages.AddRange(pageNodes.Select(p => p.Attributes["href"].Value).Distinct());
                pages = pages.Distinct().ToList();
            }

            string lastpage = pages.Last().ToString().Replace("?p=", "");
            int pagesCount = Convert.ToInt32(lastpage);
            return pagesCount;
        }
        public void SortBadges(string method)
        {
            lblDrops.Text = localization.strings.sorting_results;
            switch (method)
            {
                case "mostcards":
                    AllBadges = AllBadges.OrderByDescending(b => b.RemainingCard).ToList();
                    break;
                case "leastcards":
                    AllBadges = AllBadges.OrderBy(b => b.RemainingCard).ToList();
                    break;
                default:
                    return;
            }
        }

        private void CheckIfResponseIsNullWithRetryCount(string response)
        {
            // Response should be empty. User should be unauthorised.
            if (string.IsNullOrEmpty(response))
            {
                RetryCount++;
                if (RetryCount == 18)
                {
                    ResetClientStatus();
                    return;
                }
                throw new Exception("Response is null or empty. Added (+1) to RetryCount");
            }
        }
        
        /// <summary>
        /// Processes all badges on page
        /// </summary>
        /// <param name="document">HTML document (1 page) from x</param>
        private void ProcessBadgesOnPage(HtmlDocument document)
        {
            foreach (var badge in document.DocumentNode.SelectNodes("//div[@class=\"badge_row is_link\"]"))
            {
                var appIdNode = badge.SelectSingleNode(".//a[@class=\"badge_row_overlay\"]").Attributes["href"].Value;
                var appid = Regex.Match(appIdNode, @"gamecards/(\d+)/").Groups[1].Value;

                if (string.IsNullOrWhiteSpace(appid) || Settings.Default.blacklist.Contains(appid) || appid == "368020" || appid == "335590" || appIdNode.Contains("border=1"))
                {
                    continue;
                }

                var hoursNode = badge.SelectSingleNode(".//div[@class=\"badge_title_stats_playtime\"]");
                var hours = hoursNode == null ? string.Empty : Regex.Match(hoursNode.InnerText, @"[0-9\.,]+").Value;

                var nameNode = badge.SelectSingleNode(".//div[@class=\"badge_title\"]");
                var name = WebUtility.HtmlDecode(nameNode.FirstChild.InnerText).Trim();

                var cardNode = badge.SelectSingleNode(".//span[@class=\"progress_info_bold\"]");
                var cards = cardNode == null ? string.Empty : Regex.Match(cardNode.InnerText, @"[0-9]+").Value;

                var badgeInMemory = AllBadges.FirstOrDefault(b => b.StringId == appid);

                if (badgeInMemory != null)
                {
                    badgeInMemory.UpdateStats(cards, hours);
                }
                else
                {
                    AllBadges.Add(new Badge(appid, name, cards, hours));
                }
            }
        }

        public async Task CheckCardDrops(Badge badge)
        {
            if (!await badge.CanCardDrops())
                NextIdle();
            else
            {
                // Resets the clock based on the number of remaining drops
                TimeLeft = badge.RemainingCard == 1 ? 300 : 900;
            }

            lblCurrentRemaining.Text = badge.RemainingCard == -1 ? "" : badge.RemainingCard + " " + localization.strings.card_drops_remaining;
            pbIdle.Maximum = CardsRemaining > pbIdle.Maximum ? CardsRemaining : pbIdle.Maximum;
            pbIdle.Value = pbIdle.Maximum - CardsRemaining;
            lblHoursPlayed.Text = badge.HoursPlayed + " " + localization.strings.hrs_on_record;
            UpdateStateInfo();
        }

        private void ResetRetryCountAndUpdateApplicationState()
        {
            RetryCount = 0;
            SortBadges(Settings.Default.sort);

            picReadingPage.Visible = false;
            UpdateStateInfo();

            if (CardsRemaining == 0)
            {
                IdleComplete();
            }
        }
        #endregion

        #region IDLING
        private void StartIdle()
        {
            // Kill all existing processes before starting any new ones
            // This prevents rogue processes from interfering with idling time and slowing card drops
            try
            {
                String username = WindowsIdentity.GetCurrent().Name;
                foreach (var process in Process.GetProcessesByName("steam-idle"))
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID = " + process.Id);
                    ManagementObjectCollection processList = searcher.Get();

                    foreach (ManagementObject obj in processList)
                    {
                        string[] argList = new string[] { string.Empty, string.Empty };
                        int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                        if (returnVal == 0)
                        {
                            if (argList[1] + "\\" + argList[0] == username)
                            {
                                process.Kill();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "frmMain -> StartIdle -> The attempt to kill rogue processes resulted in an exception.");
            }

            // Check if user is authenticated and if any badge left to idle
            // There should be check for IsCookieReady, but property is set in timer tick, so it could take some time to be set.
            if (string.IsNullOrWhiteSpace(Settings.Default.sessionid) || !IsSteamReady)
            {
                ResetClientStatus();
            }
            else
            {
                if (ReloadCount != 0)
                {
                    return;
                }

                if (CanIdleBadges.Any())
                {
                    EnableCardDropCheckTimer();
                    lblCurrentStatus.Enabled = false;
                    statistics.setRemainingCards((uint)CardsRemaining);
                    tmrStatistics.Enabled = true;
                    tmrStatistics.Start();

                    if (Settings.Default.OnlyOneGameIdle)
                    {
                        StartSoloIdle(CanIdleBadges.First());
                    }
                    else
                    {
                        if (Settings.Default.OneThenMany)
                        {
                            var multi = CanIdleBadges.Where(b => b.HoursPlayed >= 2);

                            if (multi.Count() >= 1)
                                StartSoloIdle(multi.First());
                            else
                                StartMultipleIdle();
                        }
                        else
                        {
                            var multi = CanIdleBadges.Where(b => (b.HoursPlayed < 2 || Settings.Default.fastMode));

                            if (multi.Count() >= 2)
                            {
                                if (Settings.Default.fastMode)
                                    StartMultipleIdleFastMode();
                                else
                                    StartMultipleIdle();
                            }
                            else
                            {
                                StartSoloIdle(CanIdleBadges.First());
                            }
                        }


                    }
                }
                else
                {
                    IdleComplete();
                }

                UpdateStateInfo();
            }
        }

        public void StopIdle()
        {
            try
            {
                lblGameName.Visible = false;
                picApp.Image = null;
                picApp.Visible = false;
                GamesState.Visible = false;
                btnPause.Visible = false;
                btnSkip.Visible = false;
                lblCurrentStatus.Text = localization.strings.not_ingame;
                lblHoursPlayed.Visible = false;
                picIdleStatus.Visible = false;

                // Stop the card drop check timer
                DisableCardDropCheckTimer();

                // Stop the statistics timer
                tmrStatistics.Stop();

                // Hide the status bar
                ssFooter.Visible = false;

                // Resize the form
                var graphics = CreateGraphics();
                var scale = graphics.DpiY * 1.9583;
                Height = Convert.ToInt32(scale);

                // Kill the idling process
                foreach (var badge in AllBadges.Where(b => b.InIdle))
                    badge.StopIdle();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "frmMain -> StopIdle -> An attempt to stop the idling processes resulted in an exception.");
            }
        }

        private void NextIdle()
        {
            // Stop idling the current game
            StopIdle();

            // Check if user is authenticated and if any badge left to idle
            // There should be check for IsCookieReady, but property is set in timer tick, so it could take some time to be set.
            if (string.IsNullOrWhiteSpace(Settings.Default.sessionid) || !IsSteamReady)
            {
                ResetClientStatus();
            }
            else
            {
                if (CanIdleBadges.Any())
                {
                    // Give the user notification that the next game will start soon
                    lblCurrentStatus.Text = localization.strings.loading_next;

                    // Make a short but random amount of time pass
                    var rand = new Random();
                    var wait = rand.Next(3, 9);
                    wait = wait * 1000;

                    tmrStartNext.Interval = wait;
                    tmrStartNext.Enabled = true;

                    UpdateStateInfo();
                }
                else
                {
                    IdleComplete();
                }
            }
        }

        public void StartSoloIdle(Badge badge)
        {
            // Set the currentAppID value
            CurrentBadge = badge;

            // Place user "In game" for card drops
            CurrentBadge.Idle();

            // Update game name
            lblCurrentStatus.Enabled = false;
            lblGameName.Visible = true;
            lblGameName.Text = CurrentBadge.Name;

            GamesState.Visible = false;
            gameToolStripMenuItem.Enabled = true;

            // Update game image
            try
            {
                picApp.Load("http://cdn.akamai.steamstatic.com/steam/apps/" + CurrentBadge.StringId + "/header_292x136.jpg");
                picApp.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "frmMain -> StartIdle -> load pic, for id = " + CurrentBadge.AppId);
            }

            // Update label controls
            lblCurrentRemaining.Text = badge.RemainingCard == -1 ? "" : CurrentBadge.RemainingCard + " " + localization.strings.card_drops_remaining;
            lblCurrentStatus.Text = localization.strings.currently_ingame;
            lblHoursPlayed.Visible = !Settings.Default.IdlingModeWhitelist;
            lblHoursPlayed.Text = CurrentBadge.HoursPlayed + " " + localization.strings.hrs_on_record;

            // Set progress bar values and show the footer
            pbIdle.Maximum = CardsRemaining > pbIdle.Maximum ? CardsRemaining : pbIdle.Maximum;
            ssFooter.Visible = true;

            // Start the animated "working" gif
            picIdleStatus.Visible = true; // Settings.Default.customTheme ? Resources.imgSpinInv : Resources.imgSpin;

            // Start the timer that will check if drops remain
            EnableCardDropCheckTimer();

            // Reset the timer
            TimeLeft = CurrentBadge.RemainingCard == 1 ? 300 : 900;

            // Set the correct buttons on the form for pause / resume
            ShowInterruptiveButtons();

            var scale = CreateGraphics().DpiY * 3.9;
            Height = Convert.ToInt32(scale);
        }

        public void StartMultipleIdle()
        {
            // Start the idling processes
            UpdateIdleProcesses();

            // Update label controls
            lblCurrentRemaining.Text = localization.strings.update_games_status;
            lblCurrentStatus.Text = localization.strings.currently_ingame;
            lblCurrentStatus.Enabled = false;

            lblGameName.Visible = false;
            lblHoursPlayed.Visible = false;
            ssFooter.Visible = true;
            gameToolStripMenuItem.Enabled = false;

            // Start the animated "working" gif
            picIdleStatus.Visible = true; //Image = Settings.Default.customTheme ? Resources.imgSpinInv : Resources.imgSpin;

            // Start the timer that will check if drops remain
            EnableCardDropCheckTimer();

            // Reset the timer
            TimeLeft = 360;

            // Show game
            GamesState.Visible = true;
            picApp.Visible = false;
            RefreshGamesStateListView();

            ShowInterruptiveButtons();

            var scale = CreateGraphics().DpiY * 3.86;
            Height = Convert.ToInt32(scale);
        }

        /// <summary>
        /// FAST MODE: Idle simultaneous for a short period
        /// </summary>
        private void StartMultipleIdleFastMode()
        {
            CurrentBadge = null;
            StartMultipleIdle();
            TimeLeft = 5 * 60;
        }

        /// <summary>
        /// FAST MODE: Stop (simultaneous idling), wait, idle games individually, change back to simultaneous idling
        /// </summary>
        private async Task StartSoloIdleFastMode()
        {
            bool paused = false;
            
            StopIdle();

            lblDrops.Text = localization.strings.loading_next;
            lblDrops.Visible = picReadingPage.Visible = true;
            lblIdle.Visible = false;

            await Task.Delay(5 * 1000);
            picReadingPage.Visible = false;
            lblIdle.Visible = lblDrops.Visible = true;


            foreach (var badge in (CanIdleBadges.Where(b => (!Equals(b, CurrentBadge)
                                                            && CanIdleBadges.ToList().IndexOf(b) < MaxSimultanousCards))))
            {
                StartSoloIdle(badge);               // Idle current game
                TimeLeft = 5;                       // Set the timer to 5 sec
                UpdateStateInfo();                  // Update information labels
                await Task.Delay(TimeLeft * 1000);  // Wait 5 sec

                if (!tmrCardDropCheck.Enabled)
                {
                    paused = true;                  // The pause button has been triggered
                    break;                          // Breaks the loop to "pause" (cancel) idling
                }

                StopIdle();                         // Stop idling before moving on to the next game
            }
            
            if (!paused)
            {
                StartMultipleIdleFastMode();        // Start the simultaneous idling
            }
        }

        public void IdleComplete()
        {
            // Deactivate the timer control and inform the user that the program is finished
            lblCurrentStatus.Text = localization.strings.idling_complete;
            lblCurrentStatus.Enabled = true;

            lblGameName.Visible = false;
            btnPause.Visible = false;
            btnSkip.Visible = true;
            // TODO: Refresh button?

            // Resize the form
            var graphics = CreateGraphics();
            var scale = graphics.DpiY * 1.9583;
            Height = Convert.ToInt32(scale);

            if (Settings.Default.ShutdownWindowsOnDone)
            {
                Settings.Default.ShutdownWindowsOnDone = false;
                Settings.Default.Save();

                StartShutdownProcess();

                if (MessageBox.Show("Your computer is about to shut down.\n\nNote: Press Cancel to abort.",
                                    "Idling Completed", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    AbortShutdownProcess();
                }
                else
                {
                    Form1_Closing(this, null);
                }
            }
        }

        internal void UpdateStateInfo()
        {
            if (ReloadCount == 0)
            {
                int numberOfCardsInIdle = CanIdleBadges.Count(b => b.InIdle);

                lblIdle.Text = string.Format(
                    "{0} " + localization.strings.games_left_to_idle
                    + ", {1} " + localization.strings.idle_now
                    + ".", (CardsRemaining > 0 ? GamesRemaining : numberOfCardsInIdle), numberOfCardsInIdle);
                lblDrops.Text = CardsRemaining + " " + localization.strings.card_drops_remaining;
                lblIdle.Visible = GamesRemaining != 0;
                lblDrops.Visible = CardsRemaining > 0;
            }
        }

        public void UpdateIdleProcesses()
        {
            foreach (var badge in CanIdleBadges)
            {
                if (Settings.Default.fastMode)
                {
                    if (CanIdleBadges.Count(b => b.InIdle) <= MaxSimultanousCards)
                        badge.Idle();
                }
                else
                {
                    if (badge.HoursPlayed >= 2 && badge.InIdle)
                        badge.StopIdle();

                    if (badge.HoursPlayed < 2 && CanIdleBadges.Count(b => b.InIdle) <= MaxSimultanousCards)
                        badge.Idle();
                }
            }

            RefreshGamesStateListView();

            if (!CanIdleBadges.Any(b => b.InIdle))
                NextIdle();

            UpdateStateInfo();
        }

        private void RefreshGamesStateListView()
        {
            GamesState.Items.Clear();
            foreach (var badge in CanIdleBadges.Where(b => b.InIdle))
            {
                var line = new ListViewItem(badge.Name);
                line.SubItems.Add(badge.HoursPlayed.ToString());
                GamesState.Items.Add(line);
            }

            GamesState.Columns[GamesState.Columns.IndexOf(Hours)].Width = Settings.Default.IdlingModeWhitelist ? 0 : 45;

            // JN: Recolor the listview
            GamesState.BackColor = Settings.Default.customTheme ? Settings.Default.colorBgd : Settings.Default.colorBgdOriginal;
            GamesState.ForeColor = Settings.Default.customTheme ? Settings.Default.colorTxt : Settings.Default.colorTxtOriginal;
        }
        #endregion

        #region MISC
        private static void PreventSleep() => NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionState.EsContinuous | NativeMethods.ExecutionState.EsSystemRequired);
        private static void AllowSleep() => NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionState.EsContinuous);

        private static void CreateShutdownProcess(String parameters)
        {
            var psi = new ProcessStartInfo("shutdown", parameters);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        private static void AbortShutdownProcess()
        {
            CreateShutdownProcess("/a");
        }

        private static void StartShutdownProcess()
        {
            CreateShutdownProcess("/s /c \"Idle Master Extended is about to shutdown Windows.\" /t 300");
        }

        private void CopyResource(string resourceName, string file)
        {
            using (var resource = GetType().Assembly.GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    return;
                }
                using (Stream output = File.OpenWrite(file))
                {
                    resource.CopyTo(output);
                }
            }
        }

        private void GetLatestVersion()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Idle Master Extended application");
            webClient.Encoding = Encoding.UTF8;

            string jsonResponse = webClient.DownloadString("https://api.github.com/repos/JonasNilson/idle_master_extended/releases/latest");
            string githubReleaseTagKey = "tag_name";

            if (jsonResponse.Contains(githubReleaseTagKey))
            {
                string jsonResponseShortened = jsonResponse
                    .Substring(jsonResponse
                    .IndexOf(githubReleaseTagKey));
                string[] releaseTagKeyValue = jsonResponseShortened
                    .Substring(0, jsonResponseShortened.IndexOf(','))
                    .Replace("\"", String.Empty)
                    .Split(':');

                if (releaseTagKeyValue[1].StartsWith("v"))
                {
                    string githubReleaseTag = releaseTagKeyValue[1];        // "vX.Y.Z-rc1"
                    string[] tagElements = githubReleaseTag.Split('-');     // "vX.Y.Z"
                    string versionNumber = tagElements[0].Substring(1);     // "X.Y.Z"
                    string[] versionElements = versionNumber.Split('.');    // [X, Y, Z]

                    int latestMajorVersion;
                    int latestMinorVersion;
                    int latestPatchVersion;

                    if (int.TryParse(versionElements[0], out latestMajorVersion)
                        && int.TryParse(versionElements[1], out latestMinorVersion) 
                        && int.TryParse(versionElements[2], out latestPatchVersion))
                    {
                        System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        if (latestMajorVersion > version.Major 
                            || (latestMajorVersion == version.Major && latestMinorVersion > version.Minor) 
                            || latestMajorVersion == version.Major && latestMinorVersion == version.Minor && latestPatchVersion > version.Build)
                        {
                            lnkLatestRelease.Text = String.Format("(Latest: v{0}.{1}.{2})", latestMajorVersion, latestMinorVersion, latestPatchVersion);
                        }
                        else
                        {
                            lnkLatestRelease.Text = String.Format("(Current: v{0}.{1}.{2})", version.Major, version.Minor, version.Build);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs reset to initial state
        /// </summary>
        private void ResetClientStatus()
        {
            // Clear the settings
            Settings.Default.sessionid = string.Empty;
            Settings.Default.steamLogin = string.Empty;
            Settings.Default.steamLoginSecure = string.Empty;
            Settings.Default.steamMachineAuth = string.Empty;
            Settings.Default.steamRememberLogin = string.Empty;
            Settings.Default.myProfileURL = string.Empty;
            Settings.Default.steamparental = string.Empty;
            Settings.Default.Save();

            // Stop the steam-idle process
            StopIdle();

            // Clear the badges list
            AllBadges.Clear();

            // Resize the form
            var graphics = CreateGraphics();
            var scale = graphics.DpiY * 1.625;
            Height = Convert.ToInt32(scale);

            // Set timer intervals
            tmrCheckSteam.Interval = 500;
            tmrCheckCookieData.Interval = 500;

            // Hide signed user name
            if (Settings.Default.showUsername)
            {
                lblSignedOnAs.Text = String.Empty;
                lblSignedOnAs.Visible = false;
            }

            // Hide spinners
            picReadingPage.Visible = false;

            // Hide lblDrops and lblIdle
            lblDrops.Visible = false;
            lblIdle.Visible = false;

            // Set IsCookieReady to false
            IsCookieReady = false;

            // Re-enable tmrReadyToGo
            tmrReadyToGo.Enabled = true;
        }
        #endregion

        #region FORM
        private void ResetFormDesign()
        {
            picReadingPage.Visible = false;
            picIdleStatus.Visible = false;
            lblDrops.Text = localization.strings.badge_didnt_load.Replace("__num__", "10");
            lblIdle.Text = "";

            // Set the form height
            var graphics = CreateGraphics();
            var scale = graphics.DpiY * 1.625;
            Height = Convert.ToInt32(scale);
            ssFooter.Visible = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "\\steam_api.dll") == false)
            {
                CopyResource("IdleMasterExtended.Resources.steam_api.dll", Environment.CurrentDirectory + @"\steam_api.dll");
            }
            if (File.Exists(Environment.CurrentDirectory + "\\steam-idle.exe") == false)
            {
                CopyResource("IdleMasterExtended.Resources.steam-idle.exe", Environment.CurrentDirectory + @"\steam-idle.exe");
            }

            // Update the settings, if needed.  When the application updates, settings will persist.
            if (Settings.Default.updateNeeded)
            {
                Settings.Default.Upgrade();
                Settings.Default.updateNeeded = false;
                Settings.Default.Save();
            }

            // Set the interface language from the settings
            if (Settings.Default.language != "")
            {
                string language_string = "";
                switch (Settings.Default.language)
                {
                    case "Bulgarian":
                        language_string = "bg";
                        break;
                    case "Chinese (Simplified, China)":
                        language_string = "zh-CN";
                        break;
                    case "Chinese (Traditional, China)":
                        language_string = "zh-TW";
                        break;
                    case "Czech":
                        language_string = "cs";
                        break;
                    case "Danish":
                        language_string = "da";
                        break;
                    case "Dutch":
                        language_string = "nl";
                        break;
                    case "English":
                        language_string = "en";
                        break;
                    case "Finnish":
                        language_string = "fi";
                        break;
                    case "French":
                        language_string = "fr";
                        break;
                    case "German":
                        language_string = "de";
                        break;
                    case "Greek":
                        language_string = "el";
                        break;
                    case "Hungarian":
                        language_string = "hu";
                        break;
                    case "Italian":
                        language_string = "it";
                        break;
                    case "Japanese":
                        language_string = "ja";
                        break;
                    case "Korean":
                        language_string = "ko";
                        break;
                    case "Norwegian":
                        language_string = "no";
                        break;
                    case "Polish":
                        language_string = "pl";
                        break;
                    case "Portuguese":
                        language_string = "pt-PT";
                        break;
                    case "Portuguese (Brazil)":
                        language_string = "pt-BR";
                        break;
                    case "Romanian":
                        language_string = "ro";
                        break;
                    case "Russian":
                        language_string = "ru";
                        break;
                    case "Spanish":
                        language_string = "es";
                        break;
                    case "Swedish":
                        language_string = "sv";
                        break;
                    case "Thai":
                        language_string = "th";
                        break;
                    case "Turkish":
                        language_string = "tr";
                        break;
                    case "Ukrainian":
                        language_string = "uk";
                        break;
                    case "Croatian":
                        language_string = "hr";
                        break;
                    default:
                        language_string = "en";
                        break;
                }
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language_string);
            }

            // Localize form elements
            fileToolStripMenuItem.Text = localization.strings.file;
            gameToolStripMenuItem.Text = localization.strings.game;
            helpToolStripMenuItem.Text = localization.strings.help;
            settingsToolStripMenuItem.Text = localization.strings.settings;
            blacklistToolStripMenuItem.Text = localization.strings.blacklist;
            exitToolStripMenuItem.Text = localization.strings.exit;
            pauseIdlingToolStripMenuItem.Text = localization.strings.pause_idling;
            resumeIdlingToolStripMenuItem.Text = localization.strings.resume_idling;
            skipGameToolStripMenuItem.Text = localization.strings.skip_current_game;
            blacklistCurrentGameToolStripMenuItem.Text = localization.strings.blacklist_current_game;
            statisticsToolStripMenuItem.Text = localization.strings.statistics;
            changelogToolStripMenuItem.Text = localization.strings.release_notes;
            officialGroupToolStripMenuItem.Text = localization.strings.official_group;
            aboutToolStripMenuItem.Text = localization.strings.about;
            lnkSignIn.Text = "(" + localization.strings.sign_in + ")";
            lnkResetCookies.Text = "(" + localization.strings.sign_out + ")";
            // TODO: lnkLatestRelease = "(" + localization.strings.latest_release + ")";
            toolStripStatusLabel1.Text = localization.strings.next_check;
            toolStripStatusLabel1.ToolTipText = localization.strings.next_check;

            lblSignedOnAs.Text = localization.strings.signed_in_as;
            GamesState.Columns[0].Text = localization.strings.name;
            GamesState.Columns[1].Text = localization.strings.hours;

            // Set the form height
            var graphics = CreateGraphics();
            var scale = graphics.DpiY * 1.625;
            Height = Convert.ToInt32(scale);

            // Set the location of certain elements so that they scale correctly for different DPI settings
            var point = new Point(Convert.ToInt32(graphics.DpiX * 1.14), Convert.ToInt32(lblGameName.Location.Y));
            lblGameName.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.35), Convert.ToInt32(lnkSignIn.Location.Y));
            lnkSignIn.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.15), Convert.ToInt32(lnkResetCookies.Location.Y));
            lnkResetCookies.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.15), Convert.ToInt32(lnkLatestRelease.Location.Y));
            lnkLatestRelease.Location = point;

            SetTheme();
            GetLatestVersion();

            //Prevent Sleep
            if (Settings.Default.NoSleep)
            {
                PreventSleep();
            }
        }

        private void frmMain_FormClose(object sender, FormClosedEventArgs e)
        {
            StopIdle();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (Settings.Default.minToTray)
                {
                    notifyIcon1.Visible = true;
                    Hide();
                }
            }
            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Restore Sleep Settings on close
            if (Settings.Default.NoSleep == true)
            {
                AllowSleep();
            }
            this.Close();
        }
        #endregion

        #region BUTTONS
        private void ShowInterruptiveButtons()
        {
            // Set the correct buttons on the form for pause / resume
            btnResume.Visible = false;
            resumeIdlingToolStripMenuItem.Enabled = false;

            btnPause.Visible = true;
            pauseIdlingToolStripMenuItem.Enabled = true;

            if (Settings.Default.fastMode || Settings.Default.IdlingModeWhitelist)
            {
                btnSkip.Visible = false;
                skipGameToolStripMenuItem.Enabled = false;
            }
            else
            {
                btnSkip.Visible = true;
                skipGameToolStripMenuItem.Enabled = true;
            }
        }

        private async void btnSkip_Click(object sender, EventArgs e)
        {
            if (!IsSteamReady)
                return;

            StopIdle();
            AllBadges.RemoveAll(b => Equals(b, CurrentBadge));

            if (!CanIdleBadges.Any())
            {
                // If there are no more games to idle, reload the badges
                picReadingPage.Visible = true;
                lblIdle.Visible = false;
                lblDrops.Visible = true;
                lblDrops.Text = localization.strings.reading_badge_page + ", " + localization.strings.please_wait;
                await LoadBadgesAsync();
            }

            StartIdle();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (!IsSteamReady)
                return;

            // Stop the steam-idle process
            StopIdle();

            // Indicate to the user that idling has been paused
            lblCurrentStatus.Text = localization.strings.idling_paused;

            // Set the correct button visibility
            btnResume.Visible = true;
            btnPause.Visible = false;
            pauseIdlingToolStripMenuItem.Enabled = false;
            resumeIdlingToolStripMenuItem.Enabled = true;

            // Focus the resume button
            btnResume.Focus();
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            // Resume idling
            StartIdle();

            pauseIdlingToolStripMenuItem.Enabled = true;
            resumeIdlingToolStripMenuItem.Enabled = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
        #endregion

        #region MENU ITEMS
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void blacklistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmBlacklist();
            frm.ShowDialog();

            if (CurrentBadge != null && Settings.Default.blacklist.Cast<string>().Any(appid => appid == CurrentBadge.StringId))
                btnSkip.PerformClick();
        }

        private void whitelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmWhitelist(this);
            frm.ShowDialog();
        }

        private void blacklistCurrentGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.blacklist.Add(CurrentBadge.StringId);
            Settings.Default.Save();

            btnSkip.PerformClick();
        }
        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/JonasNilson/idle_master_extended/releases");
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmStatistics(statistics);
            frm.ShowDialog();
        }

        private void officialGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://steamcommunity.com/groups/idlemastery");
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/JonasNilson/idle_master_extended/wiki/Donate");
        }

        private void wikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/JonasNilson/idle_master_extended/wiki");
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the form
            String previous = Settings.Default.sort;
            Boolean previous_behavior = Settings.Default.OnlyOneGameIdle;
            Boolean previous_behavior2 = Settings.Default.OneThenMany;
            Boolean previous_behavior3 = Settings.Default.fastMode;
            Boolean previous_behavior4 = Settings.Default.IdlingModeWhitelist;
            Form frm = new frmSettings();
            frm.ShowDialog();

            if (previous != Settings.Default.sort || previous_behavior != Settings.Default.OnlyOneGameIdle || previous_behavior2 != Settings.Default.OneThenMany
                || previous_behavior3 != Settings.Default.fastMode || previous_behavior4 != Settings.Default.IdlingModeWhitelist)
            {
                StopIdle();
                AllBadges.Clear();
                tmrReadyToGo.Enabled = true;
            }

            if (Settings.Default.showUsername && IsCookieReady)
            {
                lblSignedOnAs.Text = SteamProfile.GetSignedAs();
                lblSignedOnAs.Visible = Settings.Default.showUsername;
            }
        }

        private void pauseIdlingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnPause.PerformClick();
        }

        private void resumeIdlingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnResume.PerformClick();
        }

        private void skipGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSkip.PerformClick();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmAbout();
            frm.ShowDialog();
        }
        #endregion

        #region LINKS
        private void lblGameName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://store.steampowered.com/app/" + CurrentBadge.AppId);
        }

        private void lblCurrentStatus_LinkClicked(object sender, EventArgs e)
        {
            Process.Start("https://github.com/JonasNilson/idle_master_extended/wiki/Idling-complete");
        }

        private void lblCurrentRemaining_Click(object sender, EventArgs e)
        {
            if (TimeLeft > 2)
            {
                TimeLeft = 2;
            }
        }

        private void lnkResetCookies_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ResetClientStatus();
        }

        private void lnkSignIn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var frm = new frmSettingsAdvanced();
            frm.ShowDialog();
        }

        private void lnkLatestRelease_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/JonasNilson/idle_master_extended/releases");
        }
        #endregion

        #region TIMERS
        private void tmrBadgeReload_Tick(object sender, EventArgs e)
        {
            ReloadCount = ReloadCount + 1;
            lblDrops.Text = localization.strings.badge_didnt_load.Replace("__num__", (10 - ReloadCount).ToString());

            if (ReloadCount == 10)
            {
                tmrBadgeReload.Enabled = false;
                tmrReadyToGo.Enabled = true;
                ReloadCount = 0;
            }
        }

        private void tmrStatistics_Tick(object sender, EventArgs e)
        {
            statistics.increaseMinutesIdled();
            statistics.checkCardRemaining((uint)CardsRemaining);
        }

        private void tmrStartNext_Tick(object sender, EventArgs e)
        {
            tmrStartNext.Enabled = false;
            StartIdle();
        }

        private async void tmrReadyToGo_Tick(object sender, EventArgs e)
        {
            if (!IsCookieReady || !IsSteamReady)
                return;

            if (Settings.Default.showUsername)
            {
                lblSignedOnAs.Text = SteamProfile.GetSignedAs();
                lblSignedOnAs.Visible = true;
            }

            lblDrops.Visible = true;
            lblDrops.Text = localization.strings.reading_badge_page + ", " + localization.strings.please_wait;
            lblIdle.Visible = false;
            picReadingPage.Visible = true;

            tmrReadyToGo.Enabled = false;

            if (await CookieClient.IsLogined())
            {
                await LoadBadgesAsync();
                StartIdle();
            }
            else
            {
                if (string.IsNullOrEmpty(Settings.Default.myProfileURL))
                {
                    ResetClientStatus();
                }
                else
                {
                    await CookieClient.RefreshLoginToken();
                }
            }
        }

        private async void tmrCardDropCheck_Tick(object sender, EventArgs e)
        {
            if (Settings.Default.IdlingModeWhitelist)
            {
                DisableCardDropCheckTimer();
            }
            else if (TimeLeft <= 0)
            {
                DisableCardDropCheckTimer();
                if (CurrentBadge != null)
                {
                    CurrentBadge.Idle();
                    await CheckCardDrops(CurrentBadge);
                }

                var isMultipleIdle = CanIdleBadges.Any(b => !Equals(b, CurrentBadge) && b.InIdle);
                if (isMultipleIdle)
                {
                    lblDrops.Visible = true;
                    lblDrops.Text = localization.strings.reading_badge_page + ", " + localization.strings.please_wait;
                    lblIdle.Visible = false;
                    picReadingPage.Visible = true;
                    await LoadBadgesAsync();

                    // If the fast mode is enabled, switch from simultaneous idling to individual idling
                    if (Settings.Default.fastMode)
                    {
                        await StartSoloIdleFastMode();
                        return;
                    }
                    else
                    {
                        UpdateIdleProcesses();

                        isMultipleIdle = CanIdleBadges.Any(b => b.HoursPlayed < 2 && b.InIdle);
                        if (isMultipleIdle)
                            TimeLeft = 360;
                    }
                }

                // Check if user is authenticated and if any badge left to idle
                // There should be check for IsCookieReady, but property is set in timer tick, so it could take some time to be set.
                if (!string.IsNullOrWhiteSpace(Settings.Default.sessionid) && IsSteamReady && CanIdleBadges.Any() && TimeLeft != 0)
                {
                    EnableCardDropCheckTimer();
                }
                else
                {
                    DisableCardDropCheckTimer();
                }
            }
            else
            {
                TimeLeft = TimeLeft - 1;
                lblTimer.Text = TimeSpan.FromSeconds(TimeLeft).ToString(@"mm\:ss");
            }
        }

        private void tmrCheckCookieData_Tick(object sender, EventArgs e)
        {
            // JN: White icons
            var whiteIcons = Settings.Default.whiteIcons;
            var imgFalse = whiteIcons ? Resources.imgFalse_w : Resources.imgFalse;
            var imgTrue = whiteIcons ? Resources.imgTrue_w : Resources.imgTrue;
            SetTheme();

            var connected = !string.IsNullOrWhiteSpace(Settings.Default.sessionid) && !string.IsNullOrWhiteSpace(Settings.Default.steamLoginSecure);

            var colorGreen = Settings.Default.customTheme ? Settings.Default.colorSteamGreen : Color.Green; // Adjust the green depending on the theme

            lblCookieStatus.Text = connected ? localization.strings.idle_master_connected : localization.strings.idle_master_notconnected;
            lblCookieStatus.ForeColor = connected ? colorGreen : this.ForeColor; // JN: Changed the color of "not connected" message
            picCookieStatus.Image = connected ? imgTrue : imgFalse; // JN: Supports dark theme
            lnkSignIn.Visible = !connected;
            lnkResetCookies.Visible = connected;
            IsCookieReady = connected;
        }

        private void tmrCheckSteam_Tick(object sender, EventArgs e)
        {
            // JN: White icons
            var whiteIcons = Settings.Default.whiteIcons;
            var imgFalse = whiteIcons ? Resources.imgFalse_w : Resources.imgFalse;
            var imgTrue = whiteIcons ? Resources.imgTrue_w : Resources.imgTrue;

            var colorGreen = Settings.Default.customTheme ? Settings.Default.colorSteamGreen : Color.Green; // Adjust the green depending on the theme

            var isSteamRunning = SteamAPI.IsSteamRunning() || Settings.Default.ignoreclient;
            lblSteamStatus.Text = isSteamRunning ? (Settings.Default.ignoreclient ? localization.strings.steam_ignored : localization.strings.steam_running) : localization.strings.steam_notrunning;
            lblSteamStatus.ForeColor = isSteamRunning ? colorGreen : this.ForeColor; // JN: Changed color of the not connected status
            picSteamStatus.Image = isSteamRunning ? imgTrue : imgFalse; // JN: Supports dark theme
            tmrCheckSteam.Interval = isSteamRunning ? 5000 : 500;
            skipGameToolStripMenuItem.Enabled = isSteamRunning;
            pauseIdlingToolStripMenuItem.Enabled = isSteamRunning;
            IsSteamReady = isSteamRunning;

        }

        public void DisableCardDropCheckTimer()
        {
            tmrCardDropCheck.Stop();
            toolStripStatusLabel1.Visible = lblTimer.Visible = false;
        }

        public void EnableCardDropCheckTimer()
        {
            tmrCardDropCheck.Start();
            toolStripStatusLabel1.Visible = lblTimer.Visible = true;
        }
        #endregion

        #region THEME
        /// <summary>
        /// Changes the color of the main window components to match a Steam-like dark theme
        /// </summary>
        private void SetTheme()
        {
            // Icon images
            ApplyIcons();

            // Read settings
            bool customTheme = Settings.Default.customTheme;

            if (IsCurrentThemeCustom != customTheme)
            {
                IsCurrentThemeCustom = customTheme;

                // Define colors
                FlatStyle buttonStyle = customTheme ? FlatStyle.Flat : FlatStyle.Standard;
                Color colorBgd = customTheme ? Settings.Default.colorBgd : Settings.Default.colorBgdOriginal;
                Color colorTxt = customTheme ? Settings.Default.colorTxt : Settings.Default.colorTxtOriginal;

                // --------------------------
                // -- APPLY THEME SETTINGS --
                // --------------------------

                // Main frame window
                this.BackColor = colorBgd;
                this.ForeColor = colorTxt;

                // Link colors
                lnkLatestRelease.LinkColor
                    = lnkSignIn.LinkColor
                    = lnkResetCookies.LinkColor
                    = lblCurrentRemaining.ForeColor
                    = lblGameName.LinkColor
                    = lblCurrentStatus.LinkColor
                    = customTheme ? Color.GhostWhite : Color.Blue;

                // ToolStripMenu Top
                mnuTop.BackColor = colorBgd;
                mnuTop.ForeColor = colorTxt;

                // ToolStripMenuItem and the ToolStripMenuItem dropdowns
                foreach (ToolStripMenuItem item in mnuTop.Items)
                {
                    // Menu item coloring
                    item.BackColor = colorBgd;
                    item.ForeColor = colorTxt;

                    // Dropdown coloring
                    item.DropDown.BackColor = colorBgd;
                    item.DropDown.ForeColor = colorTxt;
                }

                // Game state list (needs to be colored in RefreshGamesStateListView)
                GamesState.BackColor = colorBgd;
                GamesState.ForeColor = colorTxt;

                // lblTimer
                lblTimer.BackColor = colorBgd;
                lblTimer.ForeColor = colorTxt;

                // toolStripStatusLabel1
                toolStripStatusLabel1.BackColor = colorBgd;

                // Footer
                ssFooter.BackColor = colorBgd;

                // Buttons
                btnPause.FlatStyle = btnResume.FlatStyle = btnSkip.FlatStyle = buttonStyle;
                btnPause.BackColor = btnResume.BackColor = btnSkip.BackColor = colorBgd;
                btnPause.ForeColor = btnResume.ForeColor = btnSkip.ForeColor = colorTxt;
            }
        }

        /// <summary>
        /// Replaces the main frame window images with white ones for the dark theme
        /// </summary>
        private void ApplyIcons()
        {
            bool whiteIcons = Settings.Default.whiteIcons;

            if (IsCurrentIconsWhite != whiteIcons)
            {
                IsCurrentIconsWhite = whiteIcons;

                // TOOL STRIP MENU ITEMS
                // File
                settingsToolStripMenuItem.Image = whiteIcons ? Resources.imgSettings_w : Resources.imgSettings;
                blacklistToolStripMenuItem.Image = whiteIcons ? Resources.imgBlacklist_w : Resources.imgBlacklist;
                exitToolStripMenuItem.Image = whiteIcons ? Resources.imgExit_w : Resources.imgExit;
                whitelistToolStripMenuItem.Image = whiteIcons ? Resources.imgTrue_w : Resources.imgTrue;
                donateToolStripMenuItem.Image = whiteIcons ? Resources.imgView_w : Resources.imgView;
                // Game
                pauseIdlingToolStripMenuItem.Image = whiteIcons ? Resources.imgPause_w : Resources.imgPause;
                resumeIdlingToolStripMenuItem.Image = whiteIcons ? Resources.imgPlay_w : Resources.imgPlay;
                skipGameToolStripMenuItem.Image = whiteIcons ? Resources.imgSkip_w : Resources.imgSkip;
                blacklistCurrentGameToolStripMenuItem.Image = whiteIcons ? Resources.imgBlacklist_w : Resources.imgBlacklist;
                // Help
                wikiToolStripMenuItem.Image = whiteIcons ? Resources.imgInfo_w : Resources.imgInfo;
                statisticsToolStripMenuItem.Image = whiteIcons ? Resources.imgStatistics_w : Resources.imgStatistics;
                changelogToolStripMenuItem.Image = whiteIcons ? Resources.imgDocument_w : Resources.imgDocument;
                officialGroupToolStripMenuItem.Image = whiteIcons ? Resources.imgGlobe_w : Resources.imgGlobe;

                // STATUS
                // Handled in respective tick drawing functions

                // BUTTONS
                btnPause.Image = whiteIcons ? Resources.imgPauseSmall_w : Resources.imgPauseSmall;
                btnResume.Image = whiteIcons ? Resources.imgPlaySmall_w : Resources.imgPlaySmall;
                btnSkip.Image = whiteIcons ? Resources.imgSkipSmall_w : Resources.imgSkipSmall;

                // LOADING GIF
                picIdleStatus.Image = whiteIcons ? Resources.imgSpinInv : Resources.imgSpin;
                picReadingPage.Image = whiteIcons ? Resources.imgSpinInv : Resources.imgSpin;
            }
        }
        #endregion
    }
}

internal static class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);
    [FlagsAttribute]
    internal enum ExecutionState : uint
    {
        EsAwaymodeRequired = 0x00000040,
        EsContinuous = 0x80000000,
        EsDisplayRequired = 0x00000002,
        EsSystemRequired = 0x00000001
    }
}