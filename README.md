**NOTE**: This is a fork of the original IdleMaster (**discontinued**) master branch. There are no guarantees for this version of the software to work and Valve may update how the Steam API functions, how cards are dropped while playing games, how badges are crafted, etc.

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
