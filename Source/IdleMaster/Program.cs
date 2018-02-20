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
        // - Add card slots for cards idling [!][][] (fill in when card is dropped)
        // - 
        // - Discard the requirement for Steam login
        // - Use the Steam API to find the currently logged in user (user me)

        Application.Run(new frmMain());
    }
  }
}
