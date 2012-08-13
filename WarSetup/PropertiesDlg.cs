using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WarSetup
{
    public partial class PropertiesDlg : Form
    {
        public PropertiesDlg()
        {
            InitializeComponent();
        }

        private void propertiesMergePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void propertiesWixPathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = propertiesWixPath.Text;
            if ("" == dlg.SelectedPath)
                dlg.SelectedPath = Shell.wixPath;

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    propertiesWixPath.Text = dlg.SelectedPath;
                    //Properties.Settings.Default.wixPath = dlg.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }
        }

        private void propertiesMergePathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = propertiesMergePathX32.Text;
            if ("" == dlg.SelectedPath)
                dlg.SelectedPath = Shell.mergeModulesPathX32;

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    propertiesMergePathX32.Text = dlg.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }
        }

        private void propertiesMergePathX64Button_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = propertiesMergePathX64.Text;
            if ("" == dlg.SelectedPath)
                dlg.SelectedPath = Shell.mergeModulesPathX64;

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    propertiesMergePathX64.Text = dlg.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.wixPath = propertiesWixPath.Text;
            Properties.Settings.Default.mergePathX32 = propertiesMergePathX32.Text;
            Properties.Settings.Default.mergePathX64 = propertiesMergePathX64.Text;
            Properties.Settings.Default.warSetupResPath = propertiesWarSetupResPath.Text;
            
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }

            Close();
        }

        private void PropertiesDlg_Load(object sender, EventArgs e)
        {
            propertiesWixPath.Text = Properties.Settings.Default.wixPath;
            propertiesMergePathX32.Text = Properties.Settings.Default.mergePathX32;
            propertiesMergePathX64.Text = Properties.Settings.Default.mergePathX64;
            propertiesWarSetupResPath.Text = Properties.Settings.Default.warSetupResPath;
        }

        private void propertiesWarSetupResPathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = propertiesWarSetupResPath.Text;
            if ("" == dlg.SelectedPath)
                dlg.SelectedPath = Shell.warSetupResPath;

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    propertiesWarSetupResPath.Text = dlg.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }
        }
    }
}