using System;
using System.Linq;
using System.Windows.Forms;
using IdleMasterExtended.Properties;

namespace IdleMasterExtended
{
    public partial class frmWhitelist : Form
    {
        FrmMain mainForm;

        public frmWhitelist(FrmMain parentForm)
        {
            this.mainForm = parentForm;
            InitializeComponent();
        }

        public void SaveWhitelist()
        {
            Settings.Default.whitelist.Clear();
            Settings.Default.whitelist.AddRange(lstWhitelist.Items.Cast<string>().ToArray());
            Settings.Default.Save();
        }

        private void frmWhitelist_Load(object sender, EventArgs e)
        {
            // Localize form
            btnAdd.Text = localization.strings.add;
            btnSave.Text = localization.strings.save;
            this.Text = localization.strings.manage_whitelist;
            grpAdd.Text = localization.strings.add_game_whitelist;

            lstWhitelist.Items.AddRange(Settings.Default.whitelist.Cast<string>().ToArray());

            ThemeHandler.SetTheme(this, Settings.Default.customTheme);
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            SaveWhitelist();

            if (Settings.Default.IdlingModeWhitelist)
            {
                mainForm.StopIdle();
                await mainForm.LoadBadgesAsync();

                if (lstWhitelist.Items.Count == 1)
                {
                    mainForm.StartSoloIdle(
                        BadgePageHandler.AllBadges.FirstOrDefault(b => b.AppId == int.Parse(lstWhitelist.Items[0].ToString()))
                    );
                }
                else if (lstWhitelist.Items.Count > 1)
                {
                    mainForm.StartMultipleIdle();
                }

                mainForm.DisableCardDropCheckTimer();
                mainForm.RefreshGamesAndCardsRemaining();
            }
            else
            {
                mainForm.EnableCardDropCheckTimer();
            }

            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int result;
            
            if (int.TryParse(txtAppid.Text, out result)
                && lstWhitelist.Items.Cast<string>().All(blApp => blApp != txtAppid.Text))
            {
                lstWhitelist.Items.Add(txtAppid.Text);
            }

            txtAppid.Text = string.Empty;
            txtAppid.Focus();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            lstWhitelist.Items.Remove(lstWhitelist.SelectedItem);
        }
    }
}
