using System.ComponentModel;
using System.Windows.Forms;

namespace IdleMasterExtended
{
    partial class frmBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBrowser));
            this.webBrowserAuthentication = new System.Windows.Forms.WebBrowser();
            this.labelSaving = new System.Windows.Forms.Label();
            this.pictureBoxSpinningGif = new System.Windows.Forms.PictureBox();
            this.timerCheck = new System.Windows.Forms.Timer(this.components);
            this.labelWebBrowser = new System.Windows.Forms.Label();
            this.pictureBoxWebBrowserLock = new System.Windows.Forms.PictureBox();
            this.labelWebBrowserAuth = new System.Windows.Forms.Label();
            this.buttonQuickLogin = new System.Windows.Forms.Button();
            this.labelQuickLoginInstructions = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSpinningGif)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebBrowserLock)).BeginInit();
            this.SuspendLayout();
            // 
            // webBrowserAuthentication
            // 
            this.webBrowserAuthentication.AllowWebBrowserDrop = false;
            this.webBrowserAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowserAuthentication.Location = new System.Drawing.Point(0, 16);
            this.webBrowserAuthentication.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserAuthentication.Name = "webBrowserAuthentication";
            this.webBrowserAuthentication.ScriptErrorsSuppressed = true;
            this.webBrowserAuthentication.ScrollBarsEnabled = false;
            this.webBrowserAuthentication.Size = new System.Drawing.Size(604, 458);
            this.webBrowserAuthentication.TabIndex = 0;
            this.webBrowserAuthentication.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wbAuth_DocumentCompleted);
            this.webBrowserAuthentication.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.wbAuth_Navigating);
            // 
            // labelSaving
            // 
            this.labelSaving.AutoSize = true;
            this.labelSaving.Location = new System.Drawing.Point(34, 11);
            this.labelSaving.Name = "labelSaving";
            this.labelSaving.Size = new System.Drawing.Size(180, 13);
            this.labelSaving.TabIndex = 1;
            this.labelSaving.Text = "Idle Master is saving your information";
            // 
            // pictureBoxSpinningGif
            // 
            this.pictureBoxSpinningGif.Image = global::IdleMasterExtended.Properties.Resources.imgSpin;
            this.pictureBoxSpinningGif.Location = new System.Drawing.Point(13, 9);
            this.pictureBoxSpinningGif.Name = "pictureBoxSpinningGif";
            this.pictureBoxSpinningGif.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxSpinningGif.TabIndex = 2;
            this.pictureBoxSpinningGif.TabStop = false;
            // 
            // timerCheck
            // 
            this.timerCheck.Interval = 1000;
            this.timerCheck.Tick += new System.EventHandler(this.tmrCheck_Tick);
            // 
            // labelWebBrowser
            // 
            this.labelWebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWebBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.labelWebBrowser.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWebBrowser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(166)))), ((int)(((byte)(75)))));
            this.labelWebBrowser.Location = new System.Drawing.Point(204, 0);
            this.labelWebBrowser.Name = "labelWebBrowser";
            this.labelWebBrowser.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.labelWebBrowser.Size = new System.Drawing.Size(403, 16);
            this.labelWebBrowser.TabIndex = 3;
            this.labelWebBrowser.Text = "Loading...";
            this.labelWebBrowser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBoxWebBrowserLock
            // 
            this.pictureBoxWebBrowserLock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.pictureBoxWebBrowserLock.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxWebBrowserLock.Name = "pictureBoxWebBrowserLock";
            this.pictureBoxWebBrowserLock.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.pictureBoxWebBrowserLock.Size = new System.Drawing.Size(24, 16);
            this.pictureBoxWebBrowserLock.TabIndex = 4;
            this.pictureBoxWebBrowserLock.TabStop = false;
            // 
            // labelWebBrowserAuth
            // 
            this.labelWebBrowserAuth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.labelWebBrowserAuth.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWebBrowserAuth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(166)))), ((int)(((byte)(75)))));
            this.labelWebBrowserAuth.Location = new System.Drawing.Point(24, 0);
            this.labelWebBrowserAuth.Name = "labelWebBrowserAuth";
            this.labelWebBrowserAuth.Size = new System.Drawing.Size(180, 16);
            this.labelWebBrowserAuth.TabIndex = 5;
            this.labelWebBrowserAuth.Text = "Verifying...";
            this.labelWebBrowserAuth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonQuickLogin
            // 
            this.buttonQuickLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonQuickLogin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonQuickLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(130)))), ((int)(((byte)(225)))));
            this.buttonQuickLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonQuickLogin.ForeColor = System.Drawing.SystemColors.Control;
            this.buttonQuickLogin.Location = new System.Drawing.Point(424, 78);
            this.buttonQuickLogin.Name = "buttonQuickLogin";
            this.buttonQuickLogin.Size = new System.Drawing.Size(180, 24);
            this.buttonQuickLogin.TabIndex = 6;
            this.buttonQuickLogin.Text = "Quick Login";
            this.buttonQuickLogin.UseVisualStyleBackColor = false;
            this.buttonQuickLogin.Click += new System.EventHandler(this.buttonQuickLogin_Click);
            // 
            // labelQuickLoginInstructions
            // 
            this.labelQuickLoginInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelQuickLoginInstructions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.labelQuickLoginInstructions.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelQuickLoginInstructions.ForeColor = System.Drawing.SystemColors.Control;
            this.labelQuickLoginInstructions.Location = new System.Drawing.Point(0, 78);
            this.labelQuickLoginInstructions.Name = "labelQuickLoginInstructions";
            this.labelQuickLoginInstructions.Size = new System.Drawing.Size(604, 24);
            this.labelQuickLoginInstructions.TabIndex = 7;
            this.labelQuickLoginInstructions.Text = "To automatically log in using your Steam client credentials, press this button:";
            this.labelQuickLoginInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 472);
            this.Controls.Add(this.buttonQuickLogin);
            this.Controls.Add(this.labelQuickLoginInstructions);
            this.Controls.Add(this.labelWebBrowserAuth);
            this.Controls.Add(this.pictureBoxWebBrowserLock);
            this.Controls.Add(this.labelWebBrowser);
            this.Controls.Add(this.webBrowserAuthentication);
            this.Controls.Add(this.pictureBoxSpinningGif);
            this.Controls.Add(this.labelSaving);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmBrowser";
            this.Text = "Please Login to Steam";
            this.Load += new System.EventHandler(this.frmBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSpinningGif)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWebBrowserLock)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WebBrowser webBrowserAuthentication;
        private Label labelSaving;
        private PictureBox pictureBoxSpinningGif;
        private Timer timerCheck;
        private Label labelWebBrowser;
        private PictureBox pictureBoxWebBrowserLock;
        private Label labelWebBrowserAuth;
        private Button buttonQuickLogin;
        private Label labelQuickLoginInstructions;
    }
}