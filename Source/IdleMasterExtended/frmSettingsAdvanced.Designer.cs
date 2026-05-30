using System.ComponentModel;
using System.Windows.Forms;

namespace IdleMasterExtended
{
    partial class frmSettingsAdvanced
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettingsAdvanced));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSessionID = new System.Windows.Forms.TextBox();
            this.txtSteamLoginSecure = new System.Windows.Forms.TextBox();
            this.txtSteamParental = new System.Windows.Forms.TextBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnView = new System.Windows.Forms.Button();
            this.ttHelp = new System.Windows.Forms.ToolTip(this.components);
            this.linkLabelWhatIsThis = new System.Windows.Forms.LinkLabel();
            this.btnQuickLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(28, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 35);
            this.label1.TabIndex = 0;
            this.label1.Text = "sessionid:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(20, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 35);
            this.label2.TabIndex = 1;
            this.label2.Text = "steamLoginSecure:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(24, 132);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 49);
            this.label3.TabIndex = 2;
            this.label3.Text = "steamparental / steamMachineAuth:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.ttHelp.SetToolTip(this.label3, "Check your browser to see if you have one of these cookies available. They are typically not required.");
            // 
            // txtSessionID
            // 
            this.txtSessionID.Location = new System.Drawing.Point(184, 52);
            this.txtSessionID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSessionID.Name = "txtSessionID";
            this.txtSessionID.PasswordChar = '*';
            this.txtSessionID.Size = new System.Drawing.Size(418, 26);
            this.txtSessionID.TabIndex = 3;
            this.txtSessionID.TextChanged += new System.EventHandler(this.txtSessionID_TextChanged);
            // 
            // txtSteamLoginSecure
            // 
            this.txtSteamLoginSecure.Location = new System.Drawing.Point(184, 92);
            this.txtSteamLoginSecure.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSteamLoginSecure.Name = "txtSteamLoginSecure";
            this.txtSteamLoginSecure.PasswordChar = '*';
            this.txtSteamLoginSecure.Size = new System.Drawing.Size(418, 26);
            this.txtSteamLoginSecure.TabIndex = 4;
            this.txtSteamLoginSecure.TextChanged += new System.EventHandler(this.txtSteamLogin_TextChanged);
            // 
            // txtSteamParental
            // 
            this.txtSteamParental.Location = new System.Drawing.Point(184, 132);
            this.txtSteamParental.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtSteamParental.Name = "txtSteamParental";
            this.txtSteamParental.PasswordChar = '*';
            this.txtSteamParental.Size = new System.Drawing.Size(418, 26);
            this.txtSteamParental.TabIndex = 5;
            this.txtSteamParental.TextChanged += new System.EventHandler(this.txtSteamParental_TextChanged);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(490, 174);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(112, 35);
            this.btnUpdate.TabIndex = 7;
            this.btnUpdate.Text = "&Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnView
            // 
            this.btnView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnView.Image = global::IdleMasterExtended.Properties.Resources.imgView;
            this.btnView.Location = new System.Drawing.Point(184, 174);
            this.btnView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(40, 35);
            this.btnView.TabIndex = 6;
            this.ttHelp.SetToolTip(this.btnView, "Do not share this information with anyone, as it could potentially be used by an " +
        "attacker to log into your Steam account.");
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // ttHelp
            // 
            this.ttHelp.AutoPopDelay = 9000;
            this.ttHelp.InitialDelay = 500;
            this.ttHelp.ReshowDelay = 100;
            // 
            // linkLabelWhatIsThis
            // 
            this.linkLabelWhatIsThis.AutoSize = true;
            this.linkLabelWhatIsThis.Location = new System.Drawing.Point(435, 14);
            this.linkLabelWhatIsThis.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelWhatIsThis.Name = "linkLabelWhatIsThis";
            this.linkLabelWhatIsThis.Size = new System.Drawing.Size(168, 20);
            this.linkLabelWhatIsThis.TabIndex = 8;
            this.linkLabelWhatIsThis.TabStop = true;
            this.linkLabelWhatIsThis.Text = "What are these fields?";
            this.linkLabelWhatIsThis.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWhatIsThis_LinkClicked);
            // 
            // btnQuickLogin
            // 
            this.btnQuickLogin.Location = new System.Drawing.Point(362, 174);
            this.btnQuickLogin.Name = "btnQuickLogin";
            this.btnQuickLogin.Size = new System.Drawing.Size(112, 35);
            this.btnQuickLogin.TabIndex = 0;
            this.btnQuickLogin.Text = "Quick Login";
            this.btnQuickLogin.Click += new System.EventHandler(this.btnQuickLogin_Click);
            // 
            // frmSettingsAdvanced
            // 
            this.AcceptButton = this.btnUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 228);
            this.Controls.Add(this.btnQuickLogin);
            this.Controls.Add(this.linkLabelWhatIsThis);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.txtSteamParental);
            this.Controls.Add(this.txtSteamLoginSecure);
            this.Controls.Add(this.txtSessionID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "frmSettingsAdvanced";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Steam Authentication Data";
            this.Load += new System.EventHandler(this.frmSettingsAdvanced_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtSessionID;
        private TextBox txtSteamLoginSecure;
        private TextBox txtSteamParental;
        private Button btnView;
        private Button btnUpdate;
        private ToolTip ttHelp;
        private LinkLabel linkLabelWhatIsThis;
        private Button btnQuickLogin;
    }
}