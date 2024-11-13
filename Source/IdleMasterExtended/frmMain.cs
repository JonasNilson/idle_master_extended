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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace IdleMasterExtended
{
    public partial class FrmMain : Form
    {
        private Badge CurrentBadge;

        private readonly Statistics statistics = new Statistics();

        private const int MaxSimultanousCards = 30;

        private const double BaseHeightFactor = 1.625;
        private const double PausedHeightFactor = 2.000;
        private const double IdlingHeightFactor = 3.9;


        private const int FifteenMinutesInSeconds = 900;
        private const int SixMinutesInSecondes = 360;
        private const int FiveMinutesInSeconds = 300;
        
        private const int FiveSecondsInMillisec = 500;
        private const int FiftySecondsInMillisec = 5000;
        
        private int ActiveTimeLeft = FifteenMinutesInSeconds;

        private bool IsCookieReady;
        private bool IsSteamReady;

        private const string ApiGitHubLatestRelease = "https://api.github.com/repos/JonasNilson/idle_master_extended/releases/latest";
        private const string UrlGitHubBase = "https://github.com/JonasNilson/idle_master_extended";
        private const string GitHubReleases = "/releases";
        private const string GitHubWiki = "/wiki";
        private const string GitHubWikiDonate = "/wiki/Donate";
        private const string GitHubWikiIdlingComplete = "/wiki/Idling-complete";

        private const string UrlSteamStoreApp = "https://store.steampowered.com/app/";
        private const string UrlSteamCommunityGroup = "https://steamcommunity.com/groups/idlemastery";
        private const string UrlSteamStaticGameLogo = "http://cdn.akamai.steamstatic.com/steam/apps/";
        private const string StaticGameLogoFile = "/header_292x136.jpg";

        #region MAIN FORM ACTIONS
        public FrmMain()
        {
            InitializeComponent();
            BadgePageHandler.AllBadges = new List<Badge>();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (Settings.Default.updateNeeded)
            {
                // Update the settings, if needed.  When the application updates, settings will persist.
                Settings.Default.Upgrade();
                Settings.Default.updateNeeded = false;
                Settings.Default.Save();
            }

            ThemeHandler.SetTheme(this, Settings.Default.customTheme);

            SetInterfaceLanguage();
            LocalizeFormElements();
            SetFormSizeToDefault();
            GetLatestVersion();

            if (Settings.Default.NoSleep)
            {
                PreventSleep();
            }
        }

        private void FrmMain_Resize(object sender, EventArgs e)
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

        private void FrmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Settings.Default.NoSleep == true)
            {
                //Restore Sleep Settings on close
                AllowSleep();
            }
            
            this.Close();
        }

        private void FrmMain_FormClose(object sender, FormClosedEventArgs e)
        {
            StopIdle();
        }

        private Graphics SetFormHeight(double dpiFactor)
        {
            var graphics = CreateGraphics();
            var scale = graphics.DpiY * dpiFactor;
            Height = Convert.ToInt32(scale);

            return graphics;
        }

        private void SetFormSizeToDefault()
        {
            // Set the form height
            Graphics graphics = SetFormHeight(BaseHeightFactor);

            // Set the location of certain elements so that they scale correctly for different DPI settings
            var point = new Point(Convert.ToInt32(graphics.DpiX * 1.14), Convert.ToInt32(lblGameName.Location.Y));
            lblGameName.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.35), Convert.ToInt32(lnkSignIn.Location.Y));
            lnkSignIn.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.15), Convert.ToInt32(lnkResetCookies.Location.Y));
            lnkResetCookies.Location = point;
            point = new Point(Convert.ToInt32(graphics.DpiX * 2.15), Convert.ToInt32(lnkLatestRelease.Location.Y));
            lnkLatestRelease.Location = point;
        }
        #endregion

        #region BUTTON ACTIONS
        private async void btnSkip_Click(object sender, EventArgs e)
        {
            if (!IsSteamReady)
                return;

            StopIdle();
            BadgePageHandler.AllBadges.RemoveAll(b => Equals(b, CurrentBadge));

            if (!BadgePageHandler.CanIdleBadges.Any())
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

        #region MENU ITEM ACTIONS
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
            if (CurrentBadge != null)
            {
                Settings.Default.blacklist.Add(CurrentBadge.StringId);
                Settings.Default.Save();
            }

            btnSkip.PerformClick();
        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start($"{UrlGitHubBase}{GitHubReleases}");
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmStatistics(statistics);
            frm.ShowDialog();
        }

        private void officialGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(UrlSteamCommunityGroup);
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start($"{UrlGitHubBase}{GitHubWikiDonate}");
        }

        private void wikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start($"{UrlGitHubBase}{GitHubWiki}");
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string previousSorting = Settings.Default.sort;
            bool previousOneGameIdle = Settings.Default.OnlyOneGameIdle;
            bool previousOneThenMany = Settings.Default.OneThenMany;
            bool previousFastMode = Settings.Default.fastMode;
            bool previousWhitelistMode = Settings.Default.IdlingModeWhitelist;
            bool previousCustomTheme = Settings.Default.customTheme;

            Form frm = new frmSettings();
            frm.ShowDialog();

            if (previousSorting != Settings.Default.sort 
                || previousOneGameIdle != Settings.Default.OnlyOneGameIdle 
                || previousOneThenMany != Settings.Default.OneThenMany
                || previousFastMode != Settings.Default.fastMode 
                || previousWhitelistMode != Settings.Default.IdlingModeWhitelist)
            {
                StopIdle();
                BadgePageHandler.AllBadges.Clear();
                tmrReadyToGo.Enabled = true;
            }

            if (Settings.Default.showUsername && IsCookieReady)
            {
                lblSignedOnAs.Text = SteamProfile.GetSignedAs();
                lblSignedOnAs.Visible = Settings.Default.showUsername;
            }

            if (previousCustomTheme != Settings.Default.customTheme)
            {
                ThemeHandler.SetTheme(this, Settings.Default.customTheme);
                ToggleCookieStatusIconAndText();
                ToggleSteamStatusIconAndText();
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

        #region LINKS ACTIONS
        private void lblGameName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(UrlSteamStoreApp + CurrentBadge.AppId);
        }

        private void lblCurrentStatus_LinkClicked(object sender, EventArgs e)
        {
            Process.Start($"{UrlGitHubBase}{GitHubWikiIdlingComplete}");
        }

        private void lblCurrentRemaining_Click(object sender, EventArgs e)
        {
            if (ActiveTimeLeft > 2)
            {
                ActiveTimeLeft = 2;
            }
        }

        private void lnkResetCookies_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StopIdlingAndResetApplicationState();
        }

        private void lnkSignIn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var frm = new frmSettingsAdvanced();
            frm.ShowDialog();
        }

        private void lnkLatestRelease_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"{UrlGitHubBase}{GitHubReleases}");
        }
        #endregion

        #region TIMER TICKS AND TOGGLES
        private void tmrBadgeReload_Tick(object sender, EventArgs e)
        {
            BadgePageHandler.ReloadCount = BadgePageHandler.ReloadCount + 1;
            lblDrops.Text = localization.strings.badge_didnt_load.Replace("__num__", (10 - BadgePageHandler.ReloadCount).ToString());

            if (BadgePageHandler.ReloadCount == 10)
            {
                tmrBadgeReload.Enabled = false;
                tmrReadyToGo.Enabled = true;
                BadgePageHandler.ReloadCount = 0;
            }
        }

        private void tmrStatistics_Tick(object sender, EventArgs e)
        {
            statistics.increaseMinutesIdled();
            statistics.checkCardRemaining((uint)BadgePageHandler.CardsRemaining);
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
                    StopIdlingAndResetApplicationState();
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
            else if (ActiveTimeLeft <= 0)
            {
                DisableCardDropCheckTimer();
                if (CurrentBadge != null)
                {
                    CurrentBadge.Idle();
                    await CheckCardDrops(CurrentBadge);
                }

                var isMultipleIdle = BadgePageHandler.CanIdleBadges.Any(b => !Equals(b, CurrentBadge) && b.InIdle);
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
                        UpdateIdlingProcesses();

                        isMultipleIdle = BadgePageHandler.CanIdleBadges.Any(b => b.HoursPlayed < 2 && b.InIdle);
                        if (isMultipleIdle)
                            ActiveTimeLeft = SixMinutesInSecondes;
                    }
                }

                // Check if user is authenticated and if any badge left to idle
                // There should be check for IsCookieReady, but property is set in timer tick, so it could take some time to be set.
                if (!string.IsNullOrWhiteSpace(Settings.Default.sessionid) && IsSteamReady && BadgePageHandler.CanIdleBadges.Any() && ActiveTimeLeft != 0)
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
                ActiveTimeLeft = ActiveTimeLeft - 1;
                lblTimer.Text = TimeSpan.FromSeconds(ActiveTimeLeft).ToString(@"mm\:ss");
            }
        }

        private void tmrCheckCookieData_Tick(object sender, EventArgs e)
        {
            
            ToggleCookieStatusIconAndText();
        }

        private void tmrCheckSteam_Tick(object sender, EventArgs e)
        {
            
            ToggleSteamStatusIconAndText();
        }

        internal void EnableCardDropCheckTimer()
        {
            tmrCardDropCheck.Start();
            toolStripStatusLabel1.Visible = lblTimer.Visible = true;
        }

        internal void DisableCardDropCheckTimer()
        {
            tmrCardDropCheck.Stop();
            toolStripStatusLabel1.Visible = lblTimer.Visible = false;
        }

        private void ResetSteamAndCookieTimers()
        {
            tmrCheckSteam.Interval = FiveSecondsInMillisec;
            tmrCheckCookieData.Interval = FiveSecondsInMillisec;
        }

        private void SetAndEnableStatistics()
        {
            statistics.setRemainingCards((uint)BadgePageHandler.CardsRemaining);
            tmrStatistics.Enabled = true;
            tmrStatistics.Start();
        }
        #endregion

        #region LOAD BADGES
        internal async Task LoadBadgesAsync()
        {
            try
            {
                await BadgePageHandler.LoadBadgesAsync();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Badge -> LoadBadgesAsync, for profile = " + Settings.Default.myProfileURL);
                SetTextToBadgePageDidNotLoad();

                BadgePageHandler.ReloadCount = 1;

                tmrBadgeReload.Enabled = true;

                if (BadgePageHandler.RetryCount == BadgePageHandler.MaxRetryCount)
                {
                    StopIdlingAndResetApplicationState();
                    return;
                }

                return;
            }

            ResetRetryCountAndUpdateApplicationState();
        }

        private async Task CheckCardDrops(Badge badge)
        {
            if (await badge.CanCardDrops())
            {
                ActiveTimeLeft = badge.RemainingCard == 1 ? FiveMinutesInSeconds : FifteenMinutesInSeconds;
            }
            else
            {
                NextIdle();
            }

            ShowAndUpdateIdlingStatusFooter();
            RefreshGamesAndCardsRemaining();
        }
        #endregion

        #region IDLING ACTIONS AND TOGGLES
        private void StartIdle()
        {
            KillExistingIdlingProcesses();

            if (BadgePageHandler.ReloadCount != 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.sessionid) || !IsSteamReady)
            {
                StopIdlingAndResetApplicationState();
                return;
            }

            if (BadgePageHandler.CanIdleBadges.Any())
            {
                EnableCardDropCheckTimer();
                lblCurrentStatus.Enabled = false;
                SetAndEnableStatistics();
                StartIdleBasedOnSettings();
            }
            else
            {
                IdleComplete();
            }

            RefreshGamesAndCardsRemaining();
        }

        private void StartIdleBasedOnSettings()
        {
            if (Settings.Default.OnlyOneGameIdle)
            {
                StartSoloIdle(BadgePageHandler.CanIdleBadges.First());
            }
            else
            {
                if (Settings.Default.OneThenMany)
                {
                    var multi = BadgePageHandler.CanIdleBadges.Where(b => b.HoursPlayed >= 2);

                    if (multi.Count() >= 1)
                        StartSoloIdle(multi.First());
                    else
                        StartMultipleIdle();
                }
                else
                {
                    var multi = BadgePageHandler.CanIdleBadges.Where(b => (b.HoursPlayed < 2 || Settings.Default.fastMode));

                    if (multi.Count() >= 2)
                    {
                        if (Settings.Default.fastMode)
                            StartMultipleIdleFastMode();
                        else
                            StartMultipleIdle();
                    }
                    else
                    {
                        StartSoloIdle(BadgePageHandler.CanIdleBadges.First());
                    }
                }
            }
        }

        public void StopIdle()
        {
            try
            {
                HideFormElementsWhenStopped();
                SetFormHeight(PausedHeightFactor);
                DisableCardDropCheckTimer();
                tmrStatistics.Stop();
                ssFooter.Visible = false;

                foreach (var badge in BadgePageHandler.AllBadges.Where(b => b.InIdle))
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
                StopIdlingAndResetApplicationState();
            }
            else
            {
                if (BadgePageHandler.CanIdleBadges.Any())
                {
                    WaitWhileLoadingNextGame();
                    RefreshGamesAndCardsRemaining();
                }
                else
                {
                    IdleComplete();
                }
            }
        }

        private void WaitWhileLoadingNextGame()
        {
            // Give the user notification that the next game will start soon
            lblCurrentStatus.Text = localization.strings.loading_next;

            var rand = new Random();
            var wait = rand.Next(3, 9);
            wait = wait * 1000;

            tmrStartNext.Interval = wait;
            tmrStartNext.Enabled = true;
        }

        public void StartSoloIdle(Badge badge)
        {
            SetFormHeight(IdlingHeightFactor);

            CurrentBadge = badge;
            CurrentBadge.Idle();

            listViewGamesState.Visible = false;
            gameToolStripMenuItem.Enabled = true;

            ShowSingleGameInfoAndImage();
            ShowInterruptiveButtons();
            ShowAndUpdateIdlingStatusFooter();
            EnableCardDropCheckTimer();
            ActiveTimeLeft = CurrentBadge.RemainingCard == 1 ? FiveMinutesInSeconds : FifteenMinutesInSeconds;
        }

        private void ShowSingleGameInfoAndImage()
        {
            lblCurrentStatus.Enabled = false;
            lblGameName.Visible = true;
            lblGameName.Text = CurrentBadge.Name;

            try
            {
                picApp.Load($"{UrlSteamStaticGameLogo}{CurrentBadge.StringId}{StaticGameLogoFile}");
                picApp.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"frmMain -> StartIdle -> load pic, for id = {CurrentBadge.AppId}");
            }
        }

        public void StartMultipleIdle()
        {
            // Start the idling processes
            UpdateIdlingProcesses();

            // Update label controls
            lblCurrentRemaining.Text = localization.strings.update_games_status;
            lblCurrentStatus.Text = localization.strings.currently_ingame;
            lblCurrentStatus.Enabled = false;

            lblGameName.Visible = false;
            lblHoursPlayed.Visible = false;
            ssFooter.Visible = true;
            gameToolStripMenuItem.Enabled = false;

            // Start the animated "working" gif
            picIdleStatus.Visible = true;

            // Start the timer that will check if drops remain
            EnableCardDropCheckTimer();

            // Reset the timer
            ActiveTimeLeft = SixMinutesInSecondes;

            // Show game
            listViewGamesState.Visible = true;
            picApp.Visible = false;
            RefreshGamesStateListView();

            ShowInterruptiveButtons();
            SetFormHeight(IdlingHeightFactor);
        }

        private void StartMultipleIdleFastMode()
        {
            CurrentBadge = null;
            StartMultipleIdle();
            ActiveTimeLeft = FiveMinutesInSeconds;
        }

        private async Task StartSoloIdleFastMode()
        {
            bool paused = false;

            StopIdle();

            lblDrops.Text = localization.strings.loading_next;
            lblDrops.Visible = picReadingPage.Visible = true;
            lblIdle.Visible = false;

            await Task.Delay(FiveSecondsInMillisec);
            picReadingPage.Visible = false;
            lblIdle.Visible = lblDrops.Visible = true;


            foreach (var badge in (BadgePageHandler.CanIdleBadges
                                                   .Where(b => (!Equals(b, CurrentBadge)
                                                                && BadgePageHandler.CanIdleBadges.ToList().IndexOf(b) < MaxSimultanousCards)
                                                          )
                                   )
                    )

            {
                StartSoloIdle(badge);                     // Idle current game
                ActiveTimeLeft = 5;                       // Set the timer to 5 sec
                RefreshGamesAndCardsRemaining();                        // Update information labels
                await Task.Delay(FiveSecondsInMillisec);  // Wait 5 sec

                if (!tmrCardDropCheck.Enabled)
                {
                    paused = true;                        // The pause button has been triggered
                    break;                                // Breaks the loop to "pause" (cancel) idling
                }

                StopIdle();                               // Stop idling before moving on to the next game
            }

            if (!paused)
            {
                StartMultipleIdleFastMode();              // Start the simultaneous idling
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

            SetFormHeight(PausedHeightFactor);

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
                    FrmMain_Closing(this, null);
                }
            }
        }

        internal void RefreshGamesAndCardsRemaining()
        {
            if (BadgePageHandler.ReloadCount == 0)
            {
                RefreshGamesAndCardsRemainingLabels();
            }
        }

        private void RefreshGamesAndCardsRemainingLabels()
        {
            int numberOfGamesInIdle = BadgePageHandler.CanIdleBadges.Count(b => b.InIdle);
            int totalNumberOfGamesToIdle = BadgePageHandler.CardsRemaining > 0 ? BadgePageHandler.GamesRemaining : numberOfGamesInIdle;

            lblIdle.Visible = BadgePageHandler.GamesRemaining != 0;
            lblIdle.Text = $"{totalNumberOfGamesToIdle} {localization.strings.games_left_to_idle}, {numberOfGamesInIdle} {localization.strings.idle_now}";

            lblDrops.Visible = BadgePageHandler.CardsRemaining > 0;
            lblDrops.Text = BadgePageHandler.CardsRemaining + " " + localization.strings.card_drops_remaining;
        }

        private void UpdateIdlingProcesses()
        {
            foreach (var badge in BadgePageHandler.CanIdleBadges)
            {
                if (Settings.Default.fastMode)
                {
                    if (BadgePageHandler.CanIdleBadges.Count(b => b.InIdle) <= MaxSimultanousCards)
                        badge.Idle();
                }
                else
                {
                    if (badge.HoursPlayed >= 2 && badge.InIdle)
                        badge.StopIdle();

                    if (badge.HoursPlayed < 2 && BadgePageHandler.CanIdleBadges.Count(b => b.InIdle) <= MaxSimultanousCards)
                        badge.Idle();
                }
            }

            RefreshGamesStateListView();

            if (!BadgePageHandler.CanIdleBadges.Any(b => b.InIdle))
                NextIdle();

            RefreshGamesAndCardsRemaining();
        }

        private void StopIdlingAndResetApplicationState()
        {
            StopIdle();
            ResetSteamCookies();
            ResetSteamAndCookieTimers();
            ResetFormState();
            SetFormHeight(BaseHeightFactor);
            BadgePageHandler.AllBadges.Clear();
        }
        #endregion

        #region RESET THINGS
        private static void ResetSteamCookies()
        {
            Settings.Default.sessionid = string.Empty;
            Settings.Default.steamLogin = string.Empty;
            Settings.Default.steamLoginSecure = string.Empty;
            Settings.Default.steamMachineAuth = string.Empty;
            Settings.Default.steamRememberLogin = string.Empty;
            Settings.Default.steamparental = string.Empty;
            Settings.Default.myProfileURL = string.Empty;

            Settings.Default.Save();
        }

        private void ResetFormState()
        {
            if (Settings.Default.showUsername)
            {
                lblSignedOnAs.Text = string.Empty;
                lblSignedOnAs.Visible = false;
            }

            picReadingPage.Visible = false;
            lblDrops.Visible = false;
            lblIdle.Visible = false;
            IsCookieReady = false;

            tmrReadyToGo.Enabled = true;
        }

        private void ResetRetryCountAndUpdateApplicationState()
        {
            BadgePageHandler.RetryCount = 0;
            BadgePageHandler.SortBadges(Settings.Default.sort);

            lblDrops.Text = localization.strings.sorting_results;
            picReadingPage.Visible = false;
            RefreshGamesAndCardsRemaining();

            if (BadgePageHandler.CardsRemaining == 0)
            {
                IdleComplete();
            }
        }
        #endregion

        private void ToggleCookieStatusIconAndText()
        {
            var connected = !string.IsNullOrWhiteSpace(Settings.Default.sessionid) && !string.IsNullOrWhiteSpace(Settings.Default.steamLoginSecure);

            lblCookieStatus.Text = connected
                ? localization.strings.idle_master_connected
                : localization.strings.idle_master_notconnected;
            
            lnkSignIn.Visible = !connected;
            lnkResetCookies.Visible = connected;

            IsCookieReady = connected;

            ThemeHandler.ToggleStatusIcon(picCookieStatus, connected, Settings.Default.customTheme);
            ThemeHandler.ToggleStatusLabelColor(lblCookieStatus, connected, Settings.Default.customTheme);
        }

        private void ToggleSteamStatusIconAndText()
        {
            var isSteamRunning = SteamAPI.IsSteamRunning() || Settings.Default.ignoreclient;

            lblSteamStatus.Text = isSteamRunning
                ? (Settings.Default.ignoreclient
                    ? localization.strings.steam_ignored
                    : localization.strings.steam_running)
                : localization.strings.steam_notrunning;

            tmrCheckSteam.Interval = isSteamRunning ? FiftySecondsInMillisec : FiveSecondsInMillisec;

            skipGameToolStripMenuItem.Enabled = isSteamRunning;
            pauseIdlingToolStripMenuItem.Enabled = isSteamRunning;

            IsSteamReady = isSteamRunning;

            ThemeHandler.ToggleStatusIcon(picSteamStatus, isSteamRunning, Settings.Default.customTheme);
            ThemeHandler.ToggleStatusLabelColor(lblSteamStatus, isSteamRunning, Settings.Default.customTheme);
        }

        private void SetTextToBadgePageDidNotLoad()
        {
            lblDrops.Text = localization.strings.badge_didnt_load.Replace("__num__", "10");

            picReadingPage.Visible = false;
            picIdleStatus.Visible = false;
            lblIdle.Text = "";

            SetFormHeight(BaseHeightFactor);
            ssFooter.Visible = false;
        }

        private void ShowAndUpdateIdlingStatusFooter()
        {
            ssFooter.Visible = true;

            lblCurrentRemaining.Text = (CurrentBadge.RemainingCard == -1) ? "" : $"{CurrentBadge.RemainingCard} {localization.strings.card_drops_remaining}";
            lblCurrentStatus.Text = localization.strings.currently_ingame;
            lblHoursPlayed.Text = $"{CurrentBadge.HoursPlayed} {localization.strings.hrs_on_record}";

            pbIdle.Maximum = BadgePageHandler.CardsRemaining > pbIdle.Maximum ? BadgePageHandler.CardsRemaining : pbIdle.Maximum;
            pbIdle.Value = pbIdle.Maximum - BadgePageHandler.CardsRemaining;

            picIdleStatus.Visible = true;
        }

        private void HideFormElementsWhenStopped()
        {
            lblGameName.Visible = false;
            picApp.Image = null;
            picApp.Visible = false;
            listViewGamesState.Visible = false;
            btnPause.Visible = false;
            btnSkip.Visible = false;
            lblHoursPlayed.Visible = false;
            picIdleStatus.Visible = false;
        }

        private void LocalizeFormElements()
        {
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
            listViewGamesState.Columns[0].Text = localization.strings.name;
            listViewGamesState.Columns[1].Text = localization.strings.hours;
        }

        private static void SetInterfaceLanguage()
        {
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
        }

        private void RefreshGamesStateListView()
        {
            listViewGamesState.Items.Clear();
            foreach (var badge in BadgePageHandler.CanIdleBadges.Where(b => b.InIdle))
            {
                var line = new ListViewItem(badge.Name);
                line.SubItems.Add(badge.HoursPlayed.ToString());
                listViewGamesState.Items.Add(line);
            }

            listViewGamesState.Columns[listViewGamesState.Columns.IndexOf(Hours)].Width = Settings.Default.IdlingModeWhitelist ? 0 : 45;

            listViewGamesState.BackColor = Settings.Default.customTheme ? Settings.Default.colorBgd : Settings.Default.colorBgdOriginal;
            listViewGamesState.ForeColor = Settings.Default.customTheme ? Settings.Default.colorTxt : Settings.Default.colorTxtOriginal;
        }

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

        private void GetLatestVersion()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Idle Master Extended application");
            webClient.Encoding = Encoding.UTF8;

            string jsonResponse = webClient.DownloadString(ApiGitHubLatestRelease);
            string githubReleaseTagKey = "tag_name";

            if (jsonResponse.Contains(githubReleaseTagKey))
            {
                string jsonResponseShortened = jsonResponse
                    .Substring(jsonResponse
                    .IndexOf(githubReleaseTagKey));

                string[] releaseTagKeyValue = jsonResponseShortened
                    .Substring(0, jsonResponseShortened.IndexOf(','))
                    .Replace("\"", string.Empty)
                    .Split(':');

                if (releaseTagKeyValue[1].StartsWith("v"))
                {
                    string githubReleaseTag = releaseTagKeyValue[1];        // "vX.Y.Z-rc1"
                    string[] tagElements = githubReleaseTag.Split('-');     // "vX.Y.Z"
                    string versionNumber = tagElements[0].Substring(1);     // "X.Y.Z"
                    string[] versionElements = versionNumber.Split('.');    // [X, Y, Z]

                    if (int.TryParse(versionElements[0], out int latestMajorVersion)
                        && int.TryParse(versionElements[1], out int latestMinorVersion)
                        && int.TryParse(versionElements[2], out int latestPatchVersion))
                    {
                        System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                        if (latestMajorVersion > version.Major
                            || (latestMajorVersion == version.Major && latestMinorVersion > version.Minor)
                            || latestMajorVersion == version.Major && latestMinorVersion == version.Minor && latestPatchVersion > version.Build)
                        {
                            lnkLatestRelease.Text = string.Format("(Latest: v{0}.{1}.{2})", latestMajorVersion, latestMinorVersion, latestPatchVersion);
                        }
                        else
                        {
                            lnkLatestRelease.Text = string.Format("(Current: v{0}.{1}.{2})", version.Major, version.Minor, version.Build);
                        }
                    }
                }
            }
        }

        private void KillExistingIdlingProcesses()
        {
            // Kill all existing processes before starting any new ones
            // This prevents rogue processes from interfering with idling time and slowing card drops
            try
            {
                string username = WindowsIdentity.GetCurrent().Name;
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
        }

        private static void PreventSleep() => NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionState.EsContinuous | NativeMethods.ExecutionState.EsSystemRequired);
        private static void AllowSleep() => NativeMethods.SetThreadExecutionState(NativeMethods.ExecutionState.EsContinuous);

        private static void CreateShutdownProcess(string parameters)
        {
            var psi = new ProcessStartInfo("shutdown", parameters)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            _ = Process.Start(psi);
        }

        private static void AbortShutdownProcess()
        {
            CreateShutdownProcess("/a");
        }

        private static void StartShutdownProcess()
        {
            CreateShutdownProcess("/s /c \"Idle Master Extended is about to shutdown Windows.\" /t 300");
        }
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