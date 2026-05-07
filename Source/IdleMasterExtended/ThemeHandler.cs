using IdleMasterExtended.Properties;
using mshtml;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace IdleMasterExtended
{
    internal class ThemeHandler
    {
        /// <summary>
        /// This class is a static utility class to handle the applied theme coloring setting of all Windows forms.
        /// 
        /// When a theme change is applied through the settings each form will be responsible to change it's colors,
        /// preferrably using this class as it automatically handles each embedded control of the form (e.g. labels, buttons, lists).
        /// </summary>

        static readonly Color DefaultBackColor = SystemColors.Control;
        static readonly Color DefaultBoxColor = SystemColors.Window;
        static readonly Color DefaultForeColor = SystemColors.ControlText;
        static readonly Color DefaultGreenColor = Color.Green;
        static readonly FlatStyle DefaultButtonStyle = FlatStyle.Standard;
        static readonly Color DefaultLinkColor = Color.Blue;

        static readonly Color DarkBackColor = Color.FromArgb(38, 38, 38);
        static readonly Color DarkBoxColor = Color.FromArgb(58, 58, 58);
        static readonly Color DarkForeColor = Color.FromArgb(196, 196, 196);
        static readonly Color DarkGreenColor = Color.FromArgb(126, 166, 75);
        static readonly FlatStyle DarkButtonStyle = FlatStyle.Flat;
        static readonly Color DarkLinkColor = Color.GhostWhite;

        const string PositiveStatusImage = "imgTrue";
        const string NegativeStatusImage = "imgFalse";

        /// <summary>
        /// Sets the theme of the Windows `Form` (default or dark theme). Automatically handles all the `Controls` inside the `Form`.
        /// <br/>
        /// Example: `ThemeHandler.SetTheme(this, Properties.DarkTheme)`
        /// </summary>
        /// <param name="form">The Windows form to apply the theme to</param>
        /// <param name="darkTheme">True if dark theme, else false</param>
        public static void SetTheme(Form form, bool darkTheme)
        {
            SetThemeForm(form, darkTheme);
            SetThemeControls(form.Controls, darkTheme);
        }

        /// <summary>
        /// Toggle the picturebox image to represent a positive or negative status
        /// </summary>
        /// <param name="pictureBox">The picturebox to toggle</param>
        /// <param name="isPositiveStatus">True if it's a positive status (e.g. Steam is running), else false/param>
        /// <param name="darkTheme">True if dark theme, else false</param>
        public static void ToggleStatusIcon(PictureBox pictureBox, bool isPositiveStatus, bool darkTheme)
        {
            if (isPositiveStatus && ResourceExists(PositiveStatusImage))
            {
                pictureBox.Image = GetImageFromResources(PositiveStatusImage, darkTheme);
            }
            else if (!isPositiveStatus && ResourceExists(NegativeStatusImage))
            {
                pictureBox.Image = GetImageFromResources(NegativeStatusImage, darkTheme);
            }
        }

        /// <summary>
        /// Toggle the label text to represent a positive or negative status
        /// </summary>
        /// <param name="label"></param>
        /// <param name="isPositiveStatus"></param>
        /// <param name="darkTheme"></param>
        public static void ToggleStatusLabelColor(Label label, bool isPositiveStatus, bool darkTheme)
        {
            if (isPositiveStatus)
            {
                label.ForeColor = darkTheme ? DarkGreenColor : DefaultGreenColor;
            }
            else if (!isPositiveStatus)
            {
                label.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
            }
        }

        /// <summary>
        /// Applies the theme colors on the `Form` (background and foreground)
        /// </summary>
        /// <param name="form">Windows form to apply the colors to</param>
        /// <param name="darkTheme">True if dark theme, else false</param>
        private static void SetThemeForm(Form form, bool darkTheme)
        {
            form.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
            form.BackColor = darkTheme ? DarkBackColor : DefaultBackColor;
        }

        /// <summary>
        /// Applies the theme colors for each component in the collection of `Controls` (available in each `Form` via `Form.Controls`)
        /// </summary>
        /// <param name="collection">Collection of `Controls`</param>
        /// <param name="darkTheme">True if dark theme, else false</param>
        private static void SetThemeControls(ControlCollection collection, bool darkTheme)
        {
            foreach (Control control in collection)
            {
                if (control is Button button)
                {
                    button.FlatStyle = darkTheme ? DarkButtonStyle : DefaultButtonStyle;
                    button.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;

                    if (button.Image != null && ResourceExists(button.Tag as string))
                    {
                        button.Image = GetImageFromResources(button.Tag as string, darkTheme);
                    }
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = darkTheme ? DarkBoxColor : DefaultBoxColor;
                    textBox.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                }
                else if (control is ListBox listBox)
                {
                    listBox.BackColor = darkTheme ? DarkBoxColor : DefaultBoxColor;
                    listBox.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                }
                else if (control is ListView listView)
                {
                    listView.BackColor = darkTheme ? DarkBoxColor : DefaultBoxColor;
                    listView.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                }
                else if (control is LinkLabel linklabel)
                {
                    linklabel.LinkColor = darkTheme ? DarkLinkColor : DefaultLinkColor;
                }
                else if (control is GroupBox groupBox)
                {
                    groupBox.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                    SetThemeControls(groupBox.Controls, darkTheme);
                }
                else if (control is PictureBox pictureBox)
                {
                    if (pictureBox.Image != null && ResourceExists(pictureBox.Tag as string))
                    {
                        pictureBox.Image = GetImageFromResources(pictureBox.Tag as string, darkTheme);
                    }
                }
                else if (control is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = darkTheme ? DarkBackColor : DefaultBackColor;
                    menuStrip.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                    SetThemeMenuStripItems(darkTheme, menuStrip);
                }

                else
                {
                    control.BackColor = darkTheme ? DarkBackColor : DefaultBackColor;
                    control.ForeColor = darkTheme ? DarkForeColor : DefaultForeColor;
                }
            }
        }

        /// <summary>
        /// Go through each menu strip item (dropdown) and apply the theme color and image.
        /// </summary>
        /// <param name="darkTheme"></param>
        /// <param name="menuStrip"></param>
        private static void SetThemeMenuStripItems(bool darkTheme, MenuStrip menuStrip)
        {
            foreach (ToolStripMenuItem menuStripItem in menuStrip.Items)
            {
                menuStripItem.BackColor = menuStripItem.DropDown.BackColor = menuStrip.BackColor;
                menuStripItem.ForeColor = menuStripItem.DropDown.ForeColor = menuStrip.ForeColor;
                SetImageDropDownItems(menuStripItem, darkTheme);
            }
        }

        /// <summary>
        /// Makes sure we only handle the necessary toolstrip sub-items, i.e. ToolStripMenuItems with a parent ToolStripMenuItem.
        /// This avoids issues with for example `ToolStripSeparator` that cannot be cast to `ToolStripMenuItem`.
        /// </summary>
        /// <param name="menuItem">The parent ToolStripMenuItem</param>
        /// <param name="darkTheme">True if a dark theme, otherwise False</param>
        private static void SetImageDropDownItems(ToolStripMenuItem menuItem, bool darkTheme)
        {
            foreach (object dropDownItem in menuItem.DropDownItems)
            {
                if (dropDownItem is ToolStripMenuItem dropDownMenuItem)
                {
                    string itemImageTag = dropDownMenuItem.Tag as string;

                    if (dropDownMenuItem.Image != null && ResourceExists(itemImageTag))
                    {
                        dropDownMenuItem.Image = GetImageFromResources(itemImageTag, darkTheme);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a resource is available or not.
        /// </summary>
        /// <param name="resourceName">The filename of the resource</param>
        /// <returns>True if the resource is available, else false</returns>
        private static bool ResourceExists(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            { 
                return false;
            }
            else
            {
                return Resources.ResourceManager.GetObject(resourceName) != null;
            }
        }

        /// <summary>
        /// Gets the image from the resources. If dark mode: append "_w" to the default filename.
        /// </summary>
        /// <param name="defaultImageName">The default filename of the resource</param>
        /// <param name="darkTheme">True if dark theme, else false</param>
        /// <returns>Image</returns>
        private static Image GetImageFromResources(string defaultImageName, bool darkTheme)
        {
            string imageResourceName = darkTheme ? $"{defaultImageName}_w" : defaultImageName;
            return Resources.ResourceManager.GetObject(imageResourceName) as Image;
        }
    }
}
