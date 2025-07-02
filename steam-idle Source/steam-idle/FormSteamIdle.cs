using System;
using System.Windows.Forms;

namespace steam_idle
{
    public partial class FormSteamIdle : Form
    {
        public FormSteamIdle(long appid)
        {
            InitializeComponent();
            try
            {
                picApp.Load($"https://cdn.akamai.steamstatic.com/steam/apps/{appid}/header_292x136.jpg");
                picApp.Visible = true;
            }
            catch
            {
                picApp.Image = null;
                picApp.Visible = false;
            }
        }

        private void FormSteamIdle_Load(object sender, EventArgs e)
        {

        }
    }
}
