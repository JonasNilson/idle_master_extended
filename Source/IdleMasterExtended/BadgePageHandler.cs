using HtmlAgilityPack;
using IdleMasterExtended.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdleMasterExtended
{
    static internal class BadgePageHandler
    {
        /// <summary>
        /// Static utility class to handle the Steam badge page contents. Stores all the current user's badges (in `List<Badge>` AllBadges).
        /// </summary>

        internal const int MaxRetryCount = 18;
        internal static int RetryCount = 0;
        internal static int ReloadCount = 0;

        internal static List<Badge> AllBadges { get; set; }
        internal static int CardsRemaining { get { return CanIdleBadges.Sum(b => b.RemainingCard); } }
        internal static int GamesRemaining { get { return CanIdleBadges.Count(); } }
        internal static IEnumerable<Badge> CanIdleBadges
        {
            get
            {
                return AllBadges.Where(badge => badge.RemainingCard != 0)
                                .Where(badge => !Settings.Default.IdleOnlyPlayed || badge.HoursPlayed > 0.0);
            }
        }

        private const string NodePageLink = "//a[@class=\"pagelink\"]";
        private const string NodeBadgeRowOverlay = ".//a[@class=\"badge_row_overlay\"]";
        private const string NodeBadgeRowIsLink = "//div[@class=\"badge_row is_link\"]";
        private const string NodeBadgeTitleStats = ".//div[@class=\"badge_title_stats_playtime\"]";
        private const string NodeBadgeTitle = ".//div[@class=\"badge_title\"]";
        private const string NodeProgressInfo = ".//span[@class=\"progress_info_bold\"]";

        private const string IgnoredAppIdNode = "border=1";
        private static readonly string[] IgnoredAppIds = { "368020", "335590" };

        /// <summary>
        /// Asynchronously load the HTML-page(s) with the Steam game badges and extract the badge information (time spent, cards remaining, etc.)
        /// </summary>
        /// <returns></returns>
        internal static async Task LoadBadgesAsync()
        {
            if (Settings.Default.IdlingModeWhitelist)
            {
                SetWhitelistAsAllBadges();
            }
            else
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

                    htmlDocument = await GetBadgePageAsync(currentBadgePage);
                    ProcessBadgesOnPage(htmlDocument);
                }
            }
        }

        /// <summary>
        /// Sort the active badges to be idled by the preferred method provided (e.g. "mostcards" or "leastcards")
        /// </summary>
        /// <param name="method"></param>
        internal static void SortBadges(string method)
        {
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

        private static async Task<HtmlDocument> GetBadgePageAsync(int pageNumber)
        {
            var document = new HtmlDocument();
            var profileLink = Settings.Default.myProfileURL + "/badges";
            var pageURL = string.Format("{0}/?p={1}", profileLink, pageNumber);
            var response = await CookieClient.GetHttpAsync(pageURL);
            CheckIfResponseIsNullWithRetryCount(response);
            document.LoadHtml(response);

            return document;
        }

        private static void CheckIfResponseIsNullWithRetryCount(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                // User should be unauthorised.
                RetryCount++;
                throw new Exception("Response is null or empty. Added (+1) to RetryCount");
            }
        }

        private static int ExtractTotalBadgePages(HtmlDocument document)
        {
            // If user is authenticated, check page count. If user is not authenticated, pages are different.
            
            var pages = new List<string>() { "?p=1" };
            var pageNodes = document.DocumentNode.SelectNodes(NodePageLink);
            
            if (pageNodes != null)
            {
                pages.AddRange(pageNodes.Select(p => p.Attributes["href"].Value).Distinct());
                pages = pages.Distinct().ToList();
            }

            string lastpage = pages.Last().ToString().Replace("?p=", "");
            int pagesCount = Convert.ToInt32(lastpage);
            return pagesCount;
        }

        private static void ProcessBadgesOnPage(HtmlDocument document)
        {
            foreach (var badge in document.DocumentNode.SelectNodes(NodeBadgeRowIsLink))
            {
                var appIdNode = badge.SelectSingleNode(NodeBadgeRowOverlay).Attributes["href"].Value;
                var appid = Regex.Match(appIdNode, @"gamecards/(\d+)/").Groups[1].Value;

                if (string.IsNullOrWhiteSpace(appid) || Settings.Default.blacklist.Contains(appid) || IgnoredAppIds.Contains(appid) || appIdNode.Contains(IgnoredAppIdNode))
                {
                    continue;
                }

                var hoursNode = badge.SelectSingleNode(NodeBadgeTitleStats);
                var hours = hoursNode == null ? string.Empty : Regex.Match(hoursNode.InnerText, @"[0-9\.,]+").Value;

                var nameNode = badge.SelectSingleNode(NodeBadgeTitle);
                var name = WebUtility.HtmlDecode(nameNode.FirstChild.InnerText).Trim();

                var cardNode = badge.SelectSingleNode(NodeProgressInfo);
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

        private static void SetWhitelistAsAllBadges()
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
    }
}
