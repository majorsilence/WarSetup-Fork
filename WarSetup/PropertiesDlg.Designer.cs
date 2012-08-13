namespace WarSetup
{
    partial class PropertiesDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesDlg));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.propertiesWixPathButton = new System.Windows.Forms.Button();
            this.propertiesWixPath = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabInterface = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.tabWix = new System.Windows.Forms.TabPage();
            this.tabMerge = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.propertiesMergePathX32Button = new System.Windows.Forms.Button();
            this.propertiesMergePathX32 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.propertiesMergePathX64Button = new System.Windows.Forms.Button();
            this.propertiesMergePathX64 = new System.Windows.Forms.TextBox();
            this.tabWarSetup = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.propertiesWarSetupResPathButton = new System.Windows.Forms.Button();
            this.propertiesWarSetupResPath = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabInterface.SuspendLayout();
            this.tabWix.SuspendLayout();
            this.tabMerge.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabWarSetup.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.propertiesWixPathButton);
            this.groupBox1.Controls.Add(this.propertiesWixPath);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(546, 84);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "WIX 3 Path";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(341, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Path to the WIX 3 \"bin\" directory. Leave blank for automatic detection.";
            // 
            // propertiesWixPathButton
            // 
            this.propertiesWixPathButton.BackColor = System.Drawing.Color.Transparent;
            this.propertiesWixPathButton.Image = ((System.Drawing.Image)(resources.GetObject("propertiesWixPathButton.Image")));
            this.propertiesWixPathButton.Location = new System.Drawing.Point(510, 47);
            this.propertiesWixPathButton.Name = "propertiesWixPathButton";
            this.propertiesWixPathButton.Size = new System.Drawing.Size(30, 23);
            this.propertiesWixPathButton.TabIndex = 10;
            this.propertiesWixPathButton.UseVisualStyleBackColor = false;
            this.propertiesWixPathButton.Click += new System.EventHandler(this.propertiesWixPathButton_Click);
            // 
            // propertiesWixPath
            // 
            this.propertiesWixPath.Location = new System.Drawing.Point(9, 47);
            this.propertiesWixPath.Name = "propertiesWixPath";
            this.propertiesWixPath.Size = new System.Drawing.Size(498, 20);
            this.propertiesWixPath.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(485, 361);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(111, 26);
            this.okButton.TabIndex = 14;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(368, 361);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(111, 26);
            this.button2.TabIndex = 15;
            this.button2.Text = "&Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabInterface);
            this.tabControl1.Controls.Add(this.tabWix);
            this.tabControl1.Controls.Add(this.tabMerge);
            this.tabControl1.Controls.Add(this.tabWarSetup);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(584, 343);
            this.tabControl1.TabIndex = 16;
            // 
            // tabInterface
            // 
            this.tabInterface.Controls.Add(this.checkBox2);
            this.tabInterface.Controls.Add(this.checkBox1);
            this.tabInterface.Controls.Add(this.numericUpDown1);
            this.tabInterface.Controls.Add(this.label5);
            this.tabInterface.Location = new System.Drawing.Point(4, 22);
            this.tabInterface.Name = "tabInterface";
            this.tabInterface.Size = new System.Drawing.Size(576, 317);
            this.tabInterface.TabIndex = 3;
            this.tabInterface.Text = "Interface";
            this.tabInterface.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Number of projects in recent-list";
            // 
            // tabWix
            // 
            this.tabWix.Controls.Add(this.groupBox1);
            this.tabWix.Location = new System.Drawing.Point(4, 22);
            this.tabWix.Name = "tabWix";
            this.tabWix.Padding = new System.Windows.Forms.Padding(3);
            this.tabWix.Size = new System.Drawing.Size(576, 317);
            this.tabWix.TabIndex = 0;
            this.tabWix.Text = "Wix";
            this.tabWix.UseVisualStyleBackColor = true;
            // 
            // tabMerge
            // 
            this.tabMerge.Controls.Add(this.groupBox2);
            this.tabMerge.Controls.Add(this.groupBox3);
            this.tabMerge.Location = new System.Drawing.Point(4, 22);
            this.tabMerge.Name = "tabMerge";
            this.tabMerge.Padding = new System.Windows.Forms.Padding(3);
            this.tabMerge.Size = new System.Drawing.Size(576, 317);
            this.tabMerge.TabIndex = 1;
            this.tabMerge.Text = "Merge Modules";
            this.tabMerge.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.propertiesMergePathX32Button);
            this.groupBox2.Controls.Add(this.propertiesMergePathX32);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(546, 84);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Merge Modules Path X32";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(388, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Path to the 32 bit Merge Modules Directory. Leave blank for automatic detection.";
            // 
            // propertiesMergePathX32Button
            // 
            this.propertiesMergePathX32Button.BackColor = System.Drawing.Color.Transparent;
            this.propertiesMergePathX32Button.Image = ((System.Drawing.Image)(resources.GetObject("propertiesMergePathX32Button.Image")));
            this.propertiesMergePathX32Button.Location = new System.Drawing.Point(510, 47);
            this.propertiesMergePathX32Button.Name = "propertiesMergePathX32Button";
            this.propertiesMergePathX32Button.Size = new System.Drawing.Size(30, 23);
            this.propertiesMergePathX32Button.TabIndex = 10;
            this.propertiesMergePathX32Button.UseVisualStyleBackColor = false;
            this.propertiesMergePathX32Button.Click += new System.EventHandler(this.propertiesMergePathButton_Click);
            // 
            // propertiesMergePathX32
            // 
            this.propertiesMergePathX32.Location = new System.Drawing.Point(9, 47);
            this.propertiesMergePathX32.Name = "propertiesMergePathX32";
            this.propertiesMergePathX32.Size = new System.Drawing.Size(498, 20);
            this.propertiesMergePathX32.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.propertiesMergePathX64Button);
            this.groupBox3.Controls.Add(this.propertiesMergePathX64);
            this.groupBox3.Location = new System.Drawing.Point(6, 96);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(546, 84);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Merge Modules Path X64";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(388, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Path to the 64 bit Merge Modules Directory. Leave blank for automatic detection.";
            // 
            // propertiesMergePathX64Button
            // 
            this.propertiesMergePathX64Button.BackColor = System.Drawing.Color.Transparent;
            this.propertiesMergePathX64Button.Image = ((System.Drawing.Image)(resources.GetObject("propertiesMergePathX64Button.Image")));
            this.propertiesMergePathX64Button.Location = new System.Drawing.Point(510, 47);
            this.propertiesMergePathX64Button.Name = "propertiesMergePathX64Button";
            this.propertiesMergePathX64Button.Size = new System.Drawing.Size(30, 23);
            this.propertiesMergePathX64Button.TabIndex = 10;
            this.propertiesMergePathX64Button.UseVisualStyleBackColor = false;
            this.propertiesMergePathX64Button.Click += new System.EventHandler(this.propertiesMergePathX64Button_Click);
            // 
            // propertiesMergePathX64
            // 
            this.propertiesMergePathX64.Location = new System.Drawing.Point(9, 47);
            this.propertiesMergePathX64.Name = "propertiesMergePathX64";
            this.propertiesMergePathX64.Size = new System.Drawing.Size(498, 20);
            this.propertiesMergePathX64.TabIndex = 0;
            // 
            // tabWarSetup
            // 
            this.tabWarSetup.Controls.Add(this.groupBox4);
            this.tabWarSetup.Location = new System.Drawing.Point(4, 22);
            this.tabWarSetup.Name = "tabWarSetup";
            this.tabWarSetup.Size = new System.Drawing.Size(576, 317);
            this.tabWarSetup.TabIndex = 2;
            this.tabWarSetup.Text = "War Setup";
            this.tabWarSetup.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.propertiesWarSetupResPathButton);
            this.groupBox4.Controls.Add(this.propertiesWarSetupResPath);
            this.groupBox4.Location = new System.Drawing.Point(12, 13);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(546, 84);
            this.groupBox4.TabIndex = 16;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "War Setup Resource Files";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(391, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Path to the resource files use by War Setup. Leave blank for automatic detection." +
                "";
            // 
            // propertiesWarSetupResPathButton
            // 
            this.propertiesWarSetupResPathButton.BackColor = System.Drawing.Color.Transparent;
            this.propertiesWarSetupResPathButton.Image = ((System.Drawing.Image)(resources.GetObject("propertiesWarSetupResPathButton.Image")));
            this.propertiesWarSetupResPathButton.Location = new System.Drawing.Point(510, 47);
            this.propertiesWarSetupResPathButton.Name = "propertiesWarSetupResPathButton";
            this.propertiesWarSetupResPathButton.Size = new System.Drawing.Size(30, 23);
            this.propertiesWarSetupResPathButton.TabIndex = 10;
            this.propertiesWarSetupResPathButton.UseVisualStyleBackColor = false;
            this.propertiesWarSetupResPathButton.Click += new System.EventHandler(this.propertiesWarSetupResPathButton_Click);
            // 
            // propertiesWarSetupResPath
            // 
            this.propertiesWarSetupResPath.Location = new System.Drawing.Point(9, 47);
            this.propertiesWarSetupResPath.Name = "propertiesWarSetupResPath";
            this.propertiesWarSetupResPath.Size = new System.Drawing.Size(498, 20);
            this.propertiesWarSetupResPath.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = global::WarSetup.Properties.Settings.Default.compileOnLowerPriority;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WarSetup.Properties.Settings.Default, "compileOnLowerPriority", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBox1.Location = new System.Drawing.Point(17, 62);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(405, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Compile on lower priority (Makes the PC usable for other work during compilation)" +
                "";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::WarSetup.Properties.Settings.Default, "maxRecentProjects", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.numericUpDown1.Location = new System.Drawing.Point(175, 13);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.Value = global::WarSetup.Properties.Settings.Default.maxRecentProjects;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = global::WarSetup.Properties.Settings.Default.compileInOwnThread;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::WarSetup.Properties.Settings.Default, "compileInOwnThread", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBox2.Location = new System.Drawing.Point(17, 85);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(401, 17);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "Compile in a seperate thread (Makes the program look better during compilation)";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // PropertiesDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 396);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.okButton);
            this.Name = "PropertiesDlg";
            this.Text = "Properties";
            this.Load += new System.EventHandler(this.PropertiesDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabInterface.ResumeLayout(false);
            this.tabInterface.PerformLayout();
            this.tabWix.ResumeLayout(false);
            this.tabMerge.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabWarSetup.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox propertiesWixPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button propertiesWixPathButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabWix;
        private System.Windows.Forms.TabPage tabMerge;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button propertiesMergePathX32Button;
        private System.Windows.Forms.TextBox propertiesMergePathX32;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button propertiesMergePathX64Button;
        private System.Windows.Forms.TextBox propertiesMergePathX64;
        private System.Windows.Forms.TabPage tabWarSetup;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button propertiesWarSetupResPathButton;
        private System.Windows.Forms.TextBox propertiesWarSetupResPath;
        private System.Windows.Forms.TabPage tabInterface;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}