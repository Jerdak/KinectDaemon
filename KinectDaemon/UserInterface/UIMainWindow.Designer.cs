﻿namespace KinectDaemon.UserInterface
{
    partial class UIMainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.SuspendLayout();
            // 
            // UIMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 335);
            this.Name = "UIMainWindow";
            this.Text = "UIMainWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UIMainWindow_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UIMainWindow_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion
    }
}