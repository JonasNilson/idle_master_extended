Idle Master Extended [![Build status](https://ci.appveyor.com/api/projects/status/96wf12emnlbmo4sj?svg=true)](https://ci.appveyor.com/project/JonasNilson/idle-master-extended)
===========
 Get your Steam Trading Cards the Fast Way (Fast Mode Extension)
 ⭐️ Accepted by the official [Idle Master community](https://steamcommunity.com/groups/idlemastery/discussions/0/1485487749771924917/)

Download
-------
The latest release is always available here: https://github.com/JonasNilson/idle_master_extended/releases

Features
-------
This is a fork of the original [Idle Master](https://github.com/jshackles/idle_master) (**discontinued** in early 2017) master branch, with a few extensions:

* **Fast mode** (File > Settings > Fast mode): Get your cards faster by automagically switching between simultaneous and individual idling (based on: https://steamcommunity.com/groups/idlemastery/discussions/0/1485487749771924917/)
<p align="center">
  <img src ="https://i.imgur.com/5DSvi3e.jpg"/>
</p>

* **Dark theme** (File > Settings > Dark theme): Applies color changes to several of the interface components to better match the Steam interface color scheme.
<p align="center">
  <img src ="https://i.imgur.com/DM8wnbm.png"/>
</p>

* **Updated Steamworks.NET** (https://steamworks.github.io): Includes the latest version of Steamworks.NET (v11.0.0) (available here: https://github.com/rlabrecque/Steamworks.NET/releases), which no longer requires CSteamworks.dll.

* **Cookies and HTTPS**: All links within the application uses the HTTPS protocol and the cookie has been updated to include *steamLoginSecure*. The browser window used to login to Steam displays more information about the site being visited.

* **Additional fixes**: For example, previously the "current badge" was skipped when changing from individual game idling to simultaneous idling - it should now work as intended. 

*Note*: the "Remember me on this computer" checkbox is automatically checked when using the login browser window due to user feedback. According to the Steam website this allows for automatic logins for **30 days** - but it is only valid for Steam Guard enabled users.

*The following was included in the original Idle Master README:*

Idle Master
===========

This program will determine which of your Steam games still have Steam Trading Card drops remaining, and will go through each application to simulate you being “in-game” so that cards will drop.  It will check periodically to see if the game you’re idling has card drops remaining.  When only one drop remains, it will start checking more frequently.  When the game you’re idling has no more cards, it’ll move on to the next game.  When no more cards are available, the program will terminate.

**This project has been discontinued**, no further bug fixes or changes will be made.  Issues and pull requests will be ignored.  The program should still work (as of Jan 3, 2018) but Valve may make a change that causes the program to become non-functioning at any time.  There are a multitude of forks of this project that are being currently maintained.

Requirements
-------

This application requires Steam to be open and for you to be logged in.  This program is now being developed exclusively for Microsoft Windows.

Non-Windows versions are available in the [Python repository](https://github.com/jshackles/idle_master_py) but may be deprecated or feature incomplete.

Setup
-------

If you are an end user you can download an install Idle Master directly from http://www.steamidlemaster.com or by launching setup.exe included in the root of this repository.  You can also download the source repository above and compile the application using Microsoft Visual Studio.

Translation
-------

You can contribute your translation suggestions and vote on existing translations using our new [Translation Page](http://translate.steamidlemaster.com).

Credits
-------

Idle Master was created by jshackles, based on the original code created by Stumpokapow.

Idle Master was writen in C# using Steamworks.NET and CSteamworks by Riley Labrecque (https://github.com/rlabrecque/CSteamworks), and using open source icons from Open Iconic (https://github.com/iconic/open-iconic).

License
-------

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation.  A copy of the GNU General Public License can be found at http://www.gnu.org/licenses/.  For your convenience, a copy of this license is included.
