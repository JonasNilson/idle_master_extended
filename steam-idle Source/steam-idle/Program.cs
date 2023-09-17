﻿using System;
using System.Windows.Forms;
using Steamworks;

namespace steam_idle
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            long appId = long.Parse(args[0]);
            Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());

            if (!SteamAPI.Init())
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormSteamIdle(appId));
        }
    }
}