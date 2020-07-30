Idle Master Extended [![Build status](https://ci.appveyor.com/api/projects/status/96wf12emnlbmo4sj?svg=true)](https://ci.appveyor.com/project/JonasNilson/idle-master-extended)
===========
 Get your [Steam Trading Cards](https://steamcommunity.com/tradingcards/) the Fast Way (Fast Mode Extension)
 ⭐️ Accepted by the official [Idle Master community](https://steamcommunity.com/groups/idlemastery/discussions/0/1485487749771924917/)
 
 > This program will determine which of your Steam games still have Steam Trading Card drops remaining, and will go through each application to simulate you being “in-game” so that cards will drop. It will check periodically to see if the game you’re idling has card drops remaining. When only one drop remains, it will start checking more frequently. When the game you’re idling has no more cards, it’ll move on to the next game. When no more cards are available, the program will terminate. 
> 
> This application requires Steam to be open and for you to be logged in.  This program is now being developed exclusively for Microsoft Windows.
Non-Windows versions are available in the [Python repository](https://github.com/jshackles/idle_master_py) but may be deprecated or feature incomplete.
> 
> [`jshackles/idle_master`](https://github.com/jshackles/idle_master) 
 
This is a fork of the original [Idle Master](https://github.com/jshackles/idle_master) project (**discontinued** in early 2017) by [jshackles](https://github.com/jshackles) (also known for [Enhanced Steam](https://github.com/jshackles/Enhanced_Steam)).

Download
-------
* The latest release is always available here: https://github.com/JonasNilson/idle_master_extended/releases
  * 💹 Total number of downloads: `492 317` ([source](https://somsubhra.com/github-release-stats/?username=JonasNilson&repository=idle_master_extended), 2020-07-30)

Donate
-------
* 🎉 [Send me a donation through Steam](https://steamcommunity.com/tradeoffer/new/?partner=180303553&token=gOgA5lWk). I accept anything you throw at me. The Steam account id is `JonasNilson` (https://steamcommunity.com/id/JonasNilson)
  * **Note**: Only use the trade link directly from here to avoid fake accounts.

Troubleshooting and common solutions
-------
* Make sure you are eligeable for card drops:
  * https://steamcommunity.com/tradingcards/
  * https://steamcommunity.com/tradingcards/faq/
  * https://steamcommunity.com/my/badges
* Idle Master Steam Group:
  * ⭐ [Read this Thread before posting, it saves time](https://steamcommunity.com/groups/idlemastery/discussions/0/152392786912268315/) 
  * 🔧 [How To Fix Most Idle Master + Steam problems](https://steamcommunity.com/groups/idlemastery/discussions/0/133257636766989675/)
  * 🔧 [Temporary solution for IM can't login problem](https://steamcommunity.com/groups/idlemastery/discussions/0/1697168437864920721/)
* GitHub Issues
  * 🔧 [How to idle any game with Idle Master Extended (v1.5)](https://github.com/JonasNilson/idle_master_extended/releases/tag/v1.5)
  * 🔧 [(Legacy) How to idle any game with Idle Master Extended](https://github.com/JonasNilson/idle_master_extended/issues/36)
  * 🔧 [Solve login issues by using an existing Steam cookie](https://github.com/JonasNilson/idle_master_extended/issues/27#issuecomment-577597720)
  * 🔧 [Free games does not have card drops available](https://github.com/JonasNilson/idle_master_extended/issues/38#issuecomment-604059701)
  * 🔧 [Steam is not running, but actually is](https://github.com/JonasNilson/idle_master_extended/issues/45#issuecomment-611694923)

Contribute
-------

Contribute directly to the code:
1. [Clone the repository](https://help.github.com/en/github/creating-cloning-and-archiving-repositories/cloning-a-repository)
1. Open the `.sln`-file with [Visual Studio](https://visualstudio.microsoft.com/)
1. Make your code changes
1. Test Idle Master with your code changes (run through Visual Studio)
1. [Create a pull request with the code change](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/proposing-changes-to-your-work-with-pull-requests)

Alternatively: 
* [Open an issue](https://github.com/JonasNilson/idle_master_extended/issues)

Features
-------
Idle Master Extended comes with a few extensions and fixes:

* **Fast mode** (`File > Settings > Idling Behavior > Fast mode`): Get your cards faster by automagically switching between simultaneous and individual idling (based on: https://steamcommunity.com/groups/idlemastery/discussions/0/1485487749771924917/)
<p align="center">
  <img src ="https://i.imgur.com/5DSvi3e.jpg"/>
</p>

* **Dark theme** (`File > Settings > General > Dark theme`): Applies color changes to several of the interface components to better match the Steam interface color scheme.
<p align="center">
  <img src ="https://i.imgur.com/DM8wnbm.png"/>
</p>

* **Quick login**: (`File > Settings > General > Quick login`) If you have Steam running it is possible to login without entering any credentials
<p align="center">
  <img src ="https://i.imgur.com/6tnHIk4.png"/>
</p>

* **Latest release**: Always keep track of the latest release of Idle Master Extended (available in v1.5)
<p align="center">
  <img src ="https://i.imgur.com/EosesDk.png"/>
</p>

* **Whitelist mode** (`File > Whitelist` & `File > Settings > Idling Behavior > Whitelist mode`): Add the game's Steam ID to a list and idle any game in your library (v1.5)
<p align="center">
  <img src ="https://i.imgur.com/CAwgi68.png"/>
</p>

* **Updated Steamworks.NET** (https://steamworks.github.io): Includes the latest version of Steamworks.NET (v11.0.0) (available here: https://github.com/rlabrecque/Steamworks.NET/releases), which no longer requires `CSteamworks.dll`.

* **Cookies and HTTPS**: All links within the application uses the `HTTPS` protocol and the cookie has been updated to include *steamLoginSecure*. The browser window used to login to Steam displays more information about the site being visited.

* **Additional fixes**: For example, previously the "current badge" was skipped when changing from individual game idling to simultaneous idling - it should now work as intended. 

*Note*: the "Remember me on this computer" checkbox is automatically checked when using the login browser window due to user feedback. According to the Steam website this allows for automatic logins for **30 days** - but it is only valid for Steam Guard enabled users.

Credits
-------
Idle Master was created by jshackles, based on the original code created by Stumpokapow.

Idle Master was writen in C# using Steamworks.NET and CSteamworks by Riley Labrecque (https://github.com/rlabrecque/CSteamworks), and using open source icons from Open Iconic (https://github.com/iconic/open-iconic).
