using System.ComponentModel;
using System.Windows.Forms;

namespace IdleMaster
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
            this.wbAuth = new System.Windows.Forms.WebBrowser();
            this.lblSaving = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tmrCheck = new System.Windows.Forms.Timer(this.components);
            this.lblWebBrowser = new System.Windows.Forms.Label();
            this.pbWebBrowserLock = new System.Windows.Forms.PictureBox();
            this.lblWebBrowserAuth = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebBrowserLock)).BeginInit();
            this.SuspendLayout();
            // 
            // wbAuth
            // 
            this.wbAuth.AllowWebBrowserDrop = false;
            this.wbAuth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbAuth.Location = new System.Drawing.Point(0, 0);
            this.wbAuth.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbAuth.Name = "wbAuth";
            this.wbAuth.ScriptErrorsSuppressed = true;
            this.wbAuth.ScrollBarsEnabled = false;
            this.wbAuth.Size = new System.Drawing.Size(976, 798);
            this.wbAuth.TabIndex = 0;
            this.wbAuth.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wbAuth_DocumentCompleted);
            this.wbAuth.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.wbAuth_Navigating);
            // 
            // lblSaving
            // 
            this.lblSaving.AutoSize = true;
            this.lblSaving.Location = new System.Drawing.Point(34, 11);
            this.lblSaving.Name = "lblSaving";
            this.lblSaving.Size = new System.Drawing.Size(180, 13);
            this.lblSaving.TabIndex = 1;
            this.lblSaving.Text = "Idle Master is saving your information";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::IdleMaster.Properties.Resources.imgSpin;
            this.pictureBox1.Location = new System.Drawing.Point(13, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // tmrCheck
            // 
            this.tmrCheck.Interval = 1000;
            this.tmrCheck.Tick += new System.EventHandler(this.tmrCheck_Tick);
            // 
            // lblWebBrowser
            // 
            this.lblWebBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWebBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblWebBrowser.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWebBrowser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(166)))), ((int)(((byte)(75)))));
            this.lblWebBrowser.Location = new System.Drawing.Point(204, 0);
            this.lblWebBrowser.Name = "lblWebBrowser";
            this.lblWebBrowser.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblWebBrowser.Size = new System.Drawing.Size(772, 16);
            this.lblWebBrowser.TabIndex = 3;
            this.lblWebBrowser.Text = "Loading...";
            this.lblWebBrowser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbWebBrowserLock
            // 
            this.pbWebBrowserLock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.pbWebBrowserLock.Location = new System.Drawing.Point(0, 0);
            this.pbWebBrowserLock.Name = "pbWebBrowserLock";
            this.pbWebBrowserLock.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.pbWebBrowserLock.Size = new System.Drawing.Size(24, 16);
            this.pbWebBrowserLock.TabIndex = 4;
            this.pbWebBrowserLock.TabStop = false;
            // 
            // lblWebBrowserAuth
            // 
            this.lblWebBrowserAuth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWebBrowserAuth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblWebBrowserAuth.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWebBrowserAuth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(126)))), ((int)(((byte)(166)))), ((int)(((byte)(75)))));
            this.lblWebBrowserAuth.Location = new System.Drawing.Point(24, 0);
            this.lblWebBrowserAuth.Name = "lblWebBrowserAuth";
            this.lblWebBrowserAuth.Size = new System.Drawing.Size(180, 16);
            this.lblWebBrowserAuth.TabIndex = 5;
            this.lblWebBrowserAuth.Text = "Verifying...";
            this.lblWebBrowserAuth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 798);
            this.Controls.Add(this.lblWebBrowserAuth);
            this.Controls.Add(this.pbWebBrowserLock);
            this.Controls.Add(this.lblWebBrowser);
            this.Controls.Add(this.wbAuth);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblSaving);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmBrowser";
            this.Text = "Please Login to Steam";
            this.Load += new System.EventHandler(this.frmBrowser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebBrowserLock)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WebBrowser wbAuth;
        private Label lblSaving;
        private PictureBox pictureBox1;
        private Timer tmrCheck;
        private Label lblWebBrowser;
        private PictureBox pbWebBrowserLock;
        private Label lblWebBrowserAuth;
    }
}