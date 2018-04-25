using System;
using System.Deployment.Application;
using System.Reflection;
using System.Windows.Forms;

namespace IdleMaster
{
  public partial class frmAbout : Form
  {
    public frmAbout()
    {
      InitializeComponent();
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void frmAbout_Load(object sender, EventArgs e)
    {
      // Localize the form
      btnOK.Text = localization.strings.ok;

      // Color text and background according to the current theme
      var defSettings = Properties.Settings.Default;
      this.BackColor = defSettings.customTheme ? defSettings.colorBgd : defSettings.colorBgdOriginal;
      this.ForeColor = defSettings.customTheme ? defSettings.colorTxt : defSettings.colorTxtOriginal;
      FlatStyle buttonStyle = defSettings.customTheme ? FlatStyle.Flat : FlatStyle.Standard;
      btnOK.FlatStyle = buttonStyle; btnOK.BackColor = this.BackColor; btnOK.ForeColor = this.ForeColor; 
      

      if (ApplicationDeployment.IsNetworkDeployed)
      {
        var version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
        lblVersion.Text = "Idle Master v" + version;
      }
      else
      {
        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        lblVersion.Text = "Idle Master v" + version;
      }
    }
  }
}
