using System;
using System.Deployment.Application;
using System.Reflection;
using System.Text;
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
            SetLocalization();
            SetTheme();
            SetVersion();
        }

        private void SetLocalization()
        {
            btnOK.Text = localization.strings.ok;
        }

        private void SetTheme()
        {
            var settings = Properties.Settings.Default;
            var customTheme = settings.customTheme;

            if (customTheme)
            {
                this.BackColor = settings.colorBgd;
                this.ForeColor = settings.colorTxt;

                btnOK.FlatStyle = FlatStyle.Flat;
                btnOK.BackColor = this.BackColor;
                btnOK.ForeColor = this.ForeColor;

                linkLabelVersion.LinkColor = this.ForeColor;
            }
        }

        private void SetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var major = version.Major;
            var minor = version.Minor;

            linkLabelVersion.Text = String.Format("Idle Master Extended v{0}.{1}", major, minor);
        }

        private void linkLabelVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/JonasNilson/idle_master_extended/releases");
        }
    }
}
