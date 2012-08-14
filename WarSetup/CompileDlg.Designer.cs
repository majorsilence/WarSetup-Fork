namespace WarSetup
{
    partial class CompileDlg
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
            this.CancelBtn = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.Info = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(121, 114);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(146, 23);
            this.CancelBtn.TabIndex = 0;
            this.CancelBtn.Text = "Cancel Compilation";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 68);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(377, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // Info
            // 
            this.Info.AutoSize = true;
            this.Info.Location = new System.Drawing.Point(12, 19);
            this.Info.Name = "Info";
            this.Info.Size = new System.Drawing.Size(52, 13);
            this.Info.TabIndex = 2;
            this.Info.Text = "Initializing";
            // 
            // CompileDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 149);
            this.Controls.Add(this.Info);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.CancelBtn);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompileDlg";
            this.ShowInTaskbar = false;
            this.Text = "Compiling. Please wait...";
            this.Shown += new System.EventHandler(this.CompileDlg_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label Info;
    }
}