using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using IdleMasterExtended.Properties;

namespace IdleMasterExtended
{
    public class Badge
    {
        public double AveragePrice { get; set; }

        public int AppId { get; set; }

        public string Name { get; set; }

        public string StringId
        {
            get { return AppId.ToString(); }
            set { AppId = string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value); }
        }

        public int RemainingCard { get; set; }

        public double HoursPlayed { get; set; }


        private Process idleProcess;

        public bool InIdle { get { return idleProcess != null && !idleProcess.HasExited; } }

        public Process Idle()
        {
            if (InIdle)
                return idleProcess;

            idleProcess = Process.Start(new ProcessStartInfo("steam-idle.exe", AppId.ToString()) { WindowStyle = ProcessWindowStyle.Hidden });
            return idleProcess;
        }

        public void StopIdle()
        {
            if (InIdle)
                idleProcess.Kill();
        }

        public async Task<bool> CanCardDrops()
        {
            try
            {
                var document = new HtmlDocument();
                var response = await CookieClient.GetHttpAsync(Settings.Default.myProfileURL + "/gamecards/" + StringId);
                // Response should be empty. User should be unauthorised.
                if (string.IsNullOrEmpty(response))
                {
                    return false;
                }
                document.LoadHtml(response);

                var hoursNode = document.DocumentNode.SelectSingleNode("//div[@class=\"badge_title_stats_playtime\"]");
                var hours = Regex.Match(hoursNode.InnerText, @"[0-9\.,]+").Value;

                var cardNode = hoursNode.ParentNode.SelectSingleNode(".//span[@class=\"progress_info_bold\"]");
                var cards = cardNode == null ? string.Empty : Regex.Match(cardNode.InnerText, @"[0-9]+").Value;

                UpdateStats(cards, hours);
                return RemainingCard != 0;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Badge -> CanCardDrops, for id = " + AppId);
            }
            return false;
        }

        public void UpdateStats(string remaining, string hours)
        {
            RemainingCard = string.IsNullOrWhiteSpace(remaining) ? 0 : int.Parse(remaining);
            HoursPlayed = string.IsNullOrWhiteSpace(hours) ? 0 : double.Parse(hours, new NumberFormatInfo());
        }

        public override bool Equals(object obj)
        {
            var badge = obj as Badge;
            return badge != null && Equals(AppId, badge.AppId);
        }

        public override int GetHashCode()
        {
            return AppId.GetHashCode();
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? StringId : Name;
        }

        public Badge(string id, string name, string remaining, string hours)
          : this()
        {
            StringId = id;
            Name = name;
            UpdateStats(remaining, hours);
        }

        // CONSTRUCTOR
        public Badge()
        { }
    }
}
