using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace IdleMaster
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Set the Browser emulation version for embedded browser control
        try
        {
            RegistryKey ie_root = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION");
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            String programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            key.SetValue(programName, (int)10001, RegistryValueKind.DWord);
        }
        catch (Exception)
        {
                // TODO: Display an error message
        }

        Application.ThreadException += (o, a) => Logger.Exception(a.Exception);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // TODO LIST:
        // - Does running in "Release" work? Try to move the EXE file to where IdleMaster.exe is available (in my downloads folder)
        // - 
        // - Make the progress bar more fun (estimate the progress somehow, then adjust when card drops)
        // - Add card slots for cards idling [!][][] (fill in when card is dropped)
        // - 
        // - Add setting checkbox to let user choose idle mode (simultaneous fast mode)
        // - Enable simultaneous idling (even if two hours have passed)
        // - Enable fast mode where the idling is paused now and then
        // - Discard the requirement for Steam login
        // - Use the Steam API to find the currently logged in user (user me)
        // - 

        // DONE:
        // + Change the UI style to better match Steam (gray tones, white text)
        // + Does Steam really require you to spend 2 hours in-game for cards to drop? NOPE
        // 
        Application.Run(new frmMain());
    }
  }
}
