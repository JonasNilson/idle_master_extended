using System;
using System.Linq;
using System.Windows.Forms;
using IdleMaster.Properties;

namespace IdleMaster
{
  public partial class frmBlacklist : Form
  {
    public frmBlacklist()
    {
      InitializeComponent();
    }

    public void SaveBlacklist()
    {
      Settings.Default.blacklist.Clear();
      Settings.Default.blacklist.AddRange(lstBlacklist.Items.Cast<string>().ToArray());
      Settings.Default.Save();
    }

    private void frmBlacklist_Load(object sender, EventArgs e)
    {
      // Localize form
      btnAdd.Text = localization.strings.add;
      btnSave.Text = localization.strings.save;
      this.Text = localization.strings.manage_blacklist;
      grpAdd.Text = localization.strings.add_game_blacklist;
        
      lstBlacklist.Items.AddRange(Settings.Default.blacklist.Cast<string>().ToArray());

      if (Settings.Default.customTheme)
      {
        runtimeCustomThemeBlacklist(); // JN: Apply the dark theme
      }
    }

    // Make everything dark to match the dark theme
    private void runtimeCustomThemeBlacklist()
    {
        System.Drawing.Color colorBgd = Settings.Default.colorBgd; // Dark gray (from Steam)
        System.Drawing.Color colorTxt = Settings.Default.colorTxt; // Light gray (from Steam)

        // Form
        this.BackColor = colorBgd;
        this.ForeColor = colorTxt;

        // Button
        btnAdd.FlatStyle = btnSave.FlatStyle = btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAdd.BackColor = btnSave.BackColor = btnRemove.BackColor = colorBgd;
        btnAdd.ForeColor = btnSave.ForeColor = btnRemove.ForeColor = colorTxt;

        // List
        lstBlacklist.BackColor = colorBgd;
        lstBlacklist.ForeColor = colorTxt;

        // Group
        grpAdd.BackColor = colorBgd;
        grpAdd.ForeColor = colorTxt;

        // Textbox Appid
        txtAppid.BackColor = colorBgd;
        txtAppid.ForeColor = colorTxt;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      SaveBlacklist();
      Close();
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
      int result;
      if (int.TryParse(txtAppid.Text, out result))
      {
        if (lstBlacklist.Items.Cast<string>().All(blApp => blApp != txtAppid.Text))
          lstBlacklist.Items.Add(txtAppid.Text);
      }
      txtAppid.Text = string.Empty;
      txtAppid.Focus();
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
      lstBlacklist.Items.Remove(lstBlacklist.SelectedItem);
    }
  }
}
