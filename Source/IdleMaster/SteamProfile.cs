using System;
using System.Net;
using System.Text;
using System.Xml;
using IdleMaster.Properties;

namespace IdleMaster
{
  internal class SteamProfile
  {
    internal static string GetSteamId()
    {
      var steamid = WebUtility.UrlDecode(Settings.Default.steamLoginSecure);
      var index = steamid.IndexOfAny(new[] { '|' }, 0);
      return index != -1 ? steamid.Remove(index) : steamid;
    }

    internal static string GetSteamUrl()
    {
      return "https://steamcommunity.com/profiles/" + GetSteamId();
    }

    internal static string GetSignedAs()
    {
      var steamUrl = GetSteamUrl();
      var userName = "User " + GetSteamId();
      try
      {
        var xmlRaw = new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(string.Format("{0}/?xml=1", steamUrl));
        var xml = new XmlDocument();
        xml.LoadXml(xmlRaw);
        var nameNode = xml.SelectSingleNode("//steamID");
        if (nameNode != null)
          userName = WebUtility.HtmlDecode(nameNode.InnerText);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Logger.Exception(ex, "frmMain -> GetSignedAs, for steamUrl = " + steamUrl);
      }
      return localization.strings.signed_in_as + " " + userName;
    }
  }
}
