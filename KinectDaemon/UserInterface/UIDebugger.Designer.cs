namespace KinectDaemon.UserInterface
{
    partial class UIDebugger
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
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.uiDepthViewer = new KinectDaemon.UserInterface.UIDepthViewer();
            this.btDumpDepth = new System.Windows.Forms.Button();
            this.btDump5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.elementHost1.Location = new System.Drawing.Point(12, 12);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(300, 300);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.uiDepthViewer;
            // 
            // btDumpDepth
            // 
            this.btDumpDepth.Location = new System.Drawing.Point(15, 318);
            this.btDumpDepth.Name = "btDumpDepth";
            this.btDumpDepth.Size = new System.Drawing.Size(76, 23);
            this.btDumpDepth.TabIndex = 1;
            this.btDumpDepth.Text = "Dump";
            this.btDumpDepth.UseVisualStyleBackColor = true;
            this.btDumpDepth.Click += new System.EventHandler(this.btDumpDepth_Click);
            // 
            // btDump5
            // 
            this.btDump5.Location = new System.Drawing.Point(97, 318);
            this.btDump5.Name = "btDump5";
            this.btDump5.Size = new System.Drawing.Size(76, 23);
            this.btDump5.TabIndex = 2;
            this.btDump5.Text = "Dump (5s)";
            this.btDump5.UseVisualStyleBackColor = true;
            this.btDump5.Click += new System.EventHandler(this.btDump5_Click);
            // 
            // UIDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 365);
            this.Controls.Add(this.btDump5);
            this.Controls.Add(this.btDumpDepth);
            this.Controls.Add(this.elementHost1);
            this.Name = "UIDebugger";
            this.Text = "UIDebugger";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private UIDepthViewer uiDepthViewer;
        private System.Windows.Forms.Button btDumpDepth;
        private System.Windows.Forms.Button btDump5;

    }
}