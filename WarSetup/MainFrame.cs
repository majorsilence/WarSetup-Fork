using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;

namespace WarSetup
{
    public partial class MainFrame : Form
    {
        private delegate void AddToCompilerOutputDelegate(string text);

        private string _openFileName = null;
        private string last_selected_folder = "";

        public string LastSelectedFolder
        {
            get 
            {
                if (last_selected_folder == "")
                    return last_selected_folder = currentProject.projectTargetDirectory;
                return last_selected_folder; 
            }
            set { last_selected_folder = value; }
        }

        private static MainFrame _this = null;

        private SetupProject currentProject;
        private bool projectNeedSave; 
        private string currentFileName; // Save to this name
        public readonly string[] targetDirs = 
        {
            "[Install Dir]",
            "[System Dir]"
        };

        struct FileBinding
        {
            public SetupFeature mFeature;
            public SetupComponent mComponent;
            public ListViewItem mListViewItm;
        };

        public MainFrame(string[] args)
        {
            _this = this;

            currentFileName = "Untitled";
            projectNeedSave = false;
            currentProject = new SetupProject();
            currentProject.InitializeDefaults();
            InitializeComponent();
            LoadRecentMenu();

            if (args.Length == 1)
                _openFileName = args[0];
        }

        static public MainFrame GetMainFrame()
        {
            return _this;
        }


        private void MainFrame_Load(object sender, EventArgs e)
        {
            // Set icons
            try
            {
                ImageList il = new ImageList();
                foreach (string s in this.GetType().Assembly.GetManifestResourceNames())
                    System.Diagnostics.Trace.WriteLine(s);

                il.Images.Add((Icon)Properties.Resources.CLSDFOLD);
                il.Images.Add((Icon)Properties.Resources.OPENFOLD);
                
                featuresTree.ImageList = il;

                BindData();
                SynchFeatures(true);

                ReloadMergeModuleList();
                ReloadWixModuleList();
                InitLicense();

                if ((null != _openFileName) && ("" != _openFileName))
                {
                    LoadFile(_openFileName);
                }

                ProcessCommandLineArgs();
                LoadBootstrapOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void LoadBootstrapOptions()
        {
            string cwd = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Bootstrap");
            string[] filePaths = Directory.GetFiles(cwd, "*.xml");
            var dict = new Dictionary<string, string>();
            

            foreach (string file in filePaths)
            {
                dict.Add(System.IO.Path.GetFileName(file), file);
            }

            comboBoxBootStrap.DataSource = new BindingSource(dict, null);
            comboBoxBootStrap.DisplayMember = "Key";
            comboBoxBootStrap.ValueMember = "Value";
        }

        private void ProcessCommandLineArgs()
        {
            bool compile = false;
            string newVersion = "";
            bool exit = false;
            bool autoIncrement = false;
            string inFile = "";

            try
            {
                for (int i = 0; i < System.Environment.GetCommandLineArgs().Length; i++)
                {

                    string arg = System.Environment.GetCommandLineArgs()[i];
                    
                    // The file to open
                    if (arg == "-i")
                    {
                        inFile = System.Environment.GetCommandLineArgs()[i + 1];
                    }
                    
                    // Compile msi automatically
                    if (arg == "-c")
                    {
                        compile = true;
                    }

                    // manually specify new version number and save
                    if (arg == "-v")
                    {
                        newVersion = System.Environment.GetCommandLineArgs()[i + 1];
                    }

                    // auto increment version number before build and save
                    if (arg == "-a")
                    {
                        autoIncrement = true;
                    }

                    // exit
                    if (arg == "-e")
                    {
                        exit = true;
                    }
                }

                if (inFile == "")
                {
                    System.Console.WriteLine("No input file supplied.");
                    return;
                }
                else
                {
                    LoadFile(inFile);
                }

                if (autoIncrement)
                {
                    string[] version = projectVersion.Text.Split('.');
                   
                    projectVersion.Text = version[0] + "." + version[1] + "." + (int.Parse(version[2]) + 1);
                    currentProject.projectVersion = projectVersion.Text;
                    OnSave(false);
                }
                if (newVersion != "")
                {
                    projectVersion.Text = newVersion;
                    currentProject.projectVersion = projectVersion.Text;
                    OnSave(false);
                }
                if (compile)
                {
                    CurrentProject.BuldTarget(!exit);
                }
                if (exit)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BindData()
        {
            try
            {
                projectName.DataBindings.Clear();
                projectName.DataBindings.Add("Text", currentProject, "projectName");


                projectTargetDirectory.DataBindings.Clear();
                projectTargetDirectory.DataBindings.Add("Text", currentProject, "projectTargetDirectory");

                projectOrganization.DataBindings.Clear();
                projectOrganization.DataBindings.Add("Text", currentProject, "projectOrganization");
                
                projectTargetName.DataBindings.Clear();
                projectTargetName.DataBindings.Add("Text", currentProject, "projectTargetName");
                
                projectVersion.DataBindings.Clear();
                projectVersion.DataBindings.Add("Text", currentProject, "projectVersion");

                projectFromWindowsVersion.DataBindings.Clear();
                projectFromWindowsVersion.DataBindings.Add("Text", currentProject, "projectFromWindowsVersion");

                projectRequireDotNetVersion.DataBindings.Clear();
                projectRequireDotNetVersion.DataBindings.Add("Text", currentProject, "projectRequireDotNetVersion");

                projectMustBeAdministratorToInstall.DataBindings.Clear();
                projectMustBeAdministratorToInstall.DataBindings.Add("Checked", currentProject,
                    "projectMustBeAdministratorToInstall");

                project64BitTarget.DataBindings.Clear();
                project64BitTarget.DataBindings.Add("Checked", currentProject,
                    "project64BitTarget");

                projectType.DataBindings.Clear();
                projectType.DataBindings.Add("SelectedIndex", currentProject, "projectType");

                projectDefaultInstallMode.DataBindings.Clear();
                projectDefaultInstallMode.DataBindings.Add("SelectedIndex", currentProject,
                    "projectInstallForCurrentOrAllUsers");

                projectInstallerUserInterface.DataBindings.Clear();
                projectInstallerUserInterface.DataBindings.Add("Text", currentProject,
                    "projectUserInterface");

                //projectLicense.DataBindings.Clear();
                //projectLicense.DataBindings.Add("Text", currentProject, "License");


                projectRunAfterInstall.DataBindings.Clear();
                projectRunAfterInstall.DataBindings.Add("Checked", currentProject,
                    "projectRunAfterInstall");

                projectProperties.SelectedObject = currentProject.projectProperties;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnChange(object sender, EventArgs e)
        {
            projectNeedSave = true;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "War Setup Project files (*.warsetup)|*.warsetup|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dlg.FileName);
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            OnSave(false);
        }
            
        private void OnSave(bool  forceFileDlg)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "War Setup Project files (*.warsetup)|*.warsetup|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            dlg.AddExtension = true;

            if (forceFileDlg || (currentFileName.Length == 0) || (currentFileName == "Untitled"))
            {
                if (currentProject.Virgin)
                {
                    string path = currentProject.projectTargetDirectory + "\\" + currentProject.projectTargetName;
                    dlg.FileName = path;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    currentFileName = dlg.FileName;
                }
                else
                {
                    return; // Aborted
                }
            }

            SynchFeatures(false);
            SaveFile();
            InitLicense();
            SetSelectedLicense();

            // Handle lru
            try
            {
                if (Properties.Settings.Default.recentProjects == null)
                    Properties.Settings.Default.recentProjects 
                        = new System.Collections.Specialized.StringCollection();
                Properties.Settings.Default.recentProjects.Remove(currentFileName);
                Properties.Settings.Default.recentProjects.Insert(0, currentFileName);

                while ((Properties.Settings.Default.recentProjects.Count > 
                    Properties.Settings.Default.maxRecentProjects)
                    && (Properties.Settings.Default.recentProjects.Count > 1))
                {
                    Properties.Settings.Default.recentProjects.RemoveAt(
                        Properties.Settings.Default.recentProjects.Count - 1);
                }


                Properties.Settings.Default.Save();
                LoadRecentMenu();
            }
            catch(Exception)
            {
                ;
            }
        }

        private void LoadFile(string path)
        {
            Cursor = System.Windows.Forms.Cursors.WaitCursor;
            currentFileName = path;
            try
            {
                Environment.CurrentDirectory = (Path.GetDirectoryName(currentFileName));
                currentProject.Clear();
                projectNeedSave = false;
                XmlSerializer serializer = new XmlSerializer(Type.GetType("WarSetup.SetupProject"));
                using (StreamReader file = new StreamReader(currentFileName))
                {
                    currentProject = (SetupProject)serializer.Deserialize(file);
                }

                // Previously the merge-modules were re-loaded each time the document was 
                // loaded. We have to load the information from the modules if this is an
                // old warsetup file...
                foreach (MergeModule mm in currentProject.projectMergeModules)
                {
                    if ((null == mm.ModuleId) || ("" == mm.ModuleId))
                        mm.LoadInfo();
                }

                BindData();
                projectNeedSave = false;
                SynchFeatures(true);
                ReloadMergeModuleList();
                ReloadWixModuleList();
                currentProject.projectProperties.AddAllUiCulture();
                currentProject.projectProperties.RemoveUiCultureDuplicates();
                InitLicense();
                SetSelectedLicense();
                CurrentProject.ResolveMainFeature(false);
                CurrentProject.Virgin = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Cursor = System.Windows.Forms.Cursors.Default;
            }
        }

        private void SynchFeatures(TreeNode parent, List<SetupFeature> features)
        {
            foreach (SetupFeature feature in features)
            {
                TreeNode node = new TreeNode(feature.featureName, 0, 1);
                node.Tag = feature;
                node.ExpandAll();

                if (null != parent)
                {
                    parent.Nodes.Add(node);
                }
                else
                {
                    featuresTree.Nodes.Add(node);
                }

                SynchFeatures(node, feature.childFeatures);
                node.ExpandAll();
            }
        }

        // Syncronize the features
        private void SynchFeatures(bool isLoading)
        {
            if (isLoading)
            {
                featuresTree.Nodes.Clear();
                SynchFeatures(null, currentProject.projectFeatures);
                if (featuresTree.Nodes.Count > 0)
                    featuresTree.SelectedNode = featuresTree.Nodes[0];
            }
        }

        private void SaveFile()
        {
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(currentFileName);
                XmlSerializer serializer = new XmlSerializer(Type.GetType("WarSetup.SetupProject"));
                using (StreamWriter file = new StreamWriter(currentFileName))
                {
                    serializer.Serialize(file, currentProject);
                }
                projectNeedSave = false;
                currentProject.Virgin = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnNewProject(object sender, EventArgs e)
        {
            if (projectNeedSave)
            {
                switch (MessageBox.Show("The current project has changed. Do you want to save it?",
                    "Save?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.OK:
                        OnSave(null, null);
                        // Fall troug
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        return; // Do not create a new project
                }
            }

            currentFileName = "";
            currentProject = new SetupProject();
            currentProject.InitializeDefaults();
            BindData();
            SynchFeatures(true);
            ReloadMergeModuleList();
            ReloadWixModuleList();
            ReloadFileList();
        }

        private void featuresTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            checkPartOfDefaultInstall.DataBindings.Clear();
            checkPartOfMinimalInstall.DataBindings.Clear();
            featureName.DataBindings.Clear();
            featureDescription.DataBindings.Clear();
            featureDefaultInstall.DataBindings.Clear();
            featurePathPropertyName.DataBindings.Clear();
            featurePathDefaultPath.DataBindings.Clear();
            featureId.DataBindings.Clear();
            shortcutProperties.SelectedObject = null;
            directoryProperties.SelectedObject = null;
            currentFileProperties.SelectedObject = null;
            featurePropertiesGrid.SelectedObject = null;

            if (e.Node != null)
            {
                SetupFeature feature = (SetupFeature)e.Node.Tag;

                checkPartOfDefaultInstall.DataBindings.Add("Checked", feature,
                    "enableInDefaultInstall");
                checkPartOfMinimalInstall.DataBindings.Add("Checked", feature,
                    "enableInMinimalInstall");
                featureName.DataBindings.Add("Text", feature, 
                    "featureName");
                featureDescription.DataBindings.Add("Text", feature, 
                    "featureDescription");
                featureDefaultInstall.DataBindings.Add("SelectedIndex", feature, 
                    "defaultInstallMode");
                featurePathPropertyName.DataBindings.Add("Text", feature,
                    "configurableDirectory");
                featurePathDefaultPath.DataBindings.Add("Text", feature,
                    "configurableDirectoryDefaultPath");
                featureId.DataBindings.Add("Text", feature, "featureId");
                featurePropertiesGrid.SelectedObject = feature;
            }

            ReloadFileList();
            ReloadDirectories();
            ReloadShortcuts();
        }

        private void fileList_DragDrop(object sender, DragEventArgs e)
        {
            // Find the selcted feature

            SetupFeature feature = null;
            try
            {
                feature = (SetupFeature)featuresTree.SelectedNode.Tag;
            }
            catch(NullReferenceException)
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string my_file = currentProject.projectProperties.UseRelativePaths
                    ? Shell.GetRelativePath(file) : file;
                SetupFile sf = feature.AddFile(my_file);
            }

            // Reload the file-list
            ReloadFileList();
        }

        private void fileList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void ReloadFileList()
        {
            fileList.Items.Clear();

            SetupFeature feature = null;
            try
            {
                feature = (SetupFeature)featuresTree.SelectedNode.Tag;
            }
            catch (NullReferenceException)
            {
                return;
            }

            foreach (SetupComponent cmp in feature.components)
            {
                foreach (SetupFile file in cmp.componentFiles)
                {
                    ListViewItem item = new ListViewItem(file.dstName);
                    item.SubItems.Add(cmp.targetDirectory);
                    item.SubItems.Add(file.srcName);
                    item.SubItems.Add(file.srcDirectory);

                    FileBinding bind = new FileBinding();
                    bind.mComponent = cmp;
                    bind.mFeature = feature;
                    bind.mListViewItm = item;
                    item.Tag = bind;

                    if (cmp.GetFirstFileId() == currentProject.mainTargetApp)
                    {
                        item.ImageIndex = 1; // Main target
                    }

                    fileList.Items.Add(item);
                }
            }
        }

        // File List Delete
        private void toolStripDelete_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 1)
            {
                FileBinding bind = (FileBinding)fileList.SelectedItems[0].Tag;
                bind.mFeature.RemoveComponent(bind.mComponent);
            }

            ReloadFileList();
        }

        private void fileList_ItemSelectionChanged(object sender, 
            ListViewItemSelectionChangedEventArgs e)
        {
            if ((null != fileList.SelectedItems) && (0 < fileList.SelectedItems.Count))
            {
                FileBinding bind = (FileBinding)fileList.SelectedItems[0].Tag;
                currentFileProperties.SelectedObject = bind.mComponent.componentFiles[0];
            }
            else
                currentFileProperties.SelectedObject = null;
        }

        private void currentFileDstDirectory_TextChanged(object sender, EventArgs e)
        {
            // Update the selected item
            if (fileList.SelectedItems.Count == 1)
            {
                FileBinding bind = (FileBinding)fileList.SelectedItems[0].Tag;
                bind.mListViewItm.Text = bind.mComponent.componentFiles[0].dstName;
                bind.mListViewItm.SubItems[1].Text = bind.mComponent.targetDirectory;
                int idx = fileList.SelectedItems[0].Index;

                //  BUG: The list is not updated immediately
                bind.mListViewItm.ListView.Refresh();
            }
        }

        private void projectName_Validated(object sender, EventArgs e)
        {
            // Set the name of the root-feature when the projevt-name is
            // decided.
            if ((currentProject.projectFeatures.Count > 0)
                && ((currentProject.projectFeatures[0].featureName == "")
                    || (currentProject.projectFeatures[0].featureName == null)))
            {
                currentProject.projectFeatures[0].featureName = currentProject.projectName;
                SynchFeatures(true);
            }
        }

        private void mergeModulesList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void mergeModulesList_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                string my_file = currentProject.projectProperties.UseRelativePaths
                    ? Shell.GetRelativePath(file) : file;
                currentProject.AddMergeModule(my_file);
            }
            
            // Reload the file-list
            ReloadMergeModuleList();
        }

        private void ReloadMergeModuleList()
        {
            mergeModulesList.Items.Clear();

            foreach (MergeModule mm in currentProject.projectMergeModules)
            {
                ListViewItem li = new ListViewItem(mm.FileName);
                li.Tag = mm;
                li.SubItems.Add(mm.ModuleManufacturer);
                li.SubItems.Add(mm.MouleDescription);
                li.SubItems.Add(mm.ModuleComments);

                mergeModulesList.Items.Add(li);
            }
        }

        private void menuDeleteMergeModule_Click(object sender, EventArgs e)
        {

            foreach(ListViewItem item in mergeModulesList.SelectedItems)
            {
                MergeModule mm = (MergeModule)item.Tag;
                currentProject.projectMergeModules.Remove(mm);
            }

            ReloadMergeModuleList();
        }

        private void browseMergeModulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = GetMergeModulesPath();

            Shell.Browse(path);
        }

        private string GetMergeModulesPath()
        {
            if (currentProject.project64BitTarget)
                return Shell.mergeModulesPathX64;

            return Shell.mergeModulesPathX32; 
        }

        private void menuAddFeature_Click(object sender, EventArgs e)
        {
            SetupFeature feature = new SetupFeature();
            currentProject.projectFeatures.Add(feature);
            SynchFeatures(true);
        }

        private void menuAddChildFeature_Click(object sender, EventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                SetupFeature feature = new SetupFeature();
                current.childFeatures.Add(feature);
            }
            SynchFeatures(true);
        }

        private void menuDeleteFeature_Click(object sender, EventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                if (featuresTree.SelectedNode.Parent == null)
                {
                    currentProject.projectFeatures.Remove(current);
                }
                else
                {
                    SetupFeature parent = (SetupFeature)featuresTree.SelectedNode.Parent.Tag;
                    parent.childFeatures.Remove(current);
                }

                SynchFeatures(true);
            }
        }

        private void featureName_TextChanged(object sender, EventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                featuresTree.SelectedNode.Text = featureName.Text;
            }
        }

        private void featureDirectoryList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if ((File.GetAttributes(file) & FileAttributes.Directory) == 0)
                        return; // Not a directory
                }
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void featureDirectoryList_DragDrop(object sender, DragEventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if ((File.GetAttributes(file) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string my_file = currentProject.projectProperties.UseRelativePaths
                    ? Shell.GetRelativePath(file) : file;  
                        current.directories.Add(new SetupDirectory(my_file));
                    }
                }

                ReloadDirectories();
            }
        }

        private void ReloadDirectories()
        {
            featureDirectoryList.Items.Clear();
            directoryProperties.SelectedObject = null;

            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;

                foreach (SetupDirectory dir in current.directories)
                {
                    ListViewItem item = new ListViewItem(dir.dstPath);
                    item.SubItems.Add(dir.srcPath);
                    item.Tag = dir;

                    featureDirectoryList.Items.Add(item);
                }

                if (featureDirectoryList.Items.Count > 0)
                {
                    featureDirectoryList.Items[0].Selected = true;
                }
            }
        }

        private void featureDirectoryList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            SetupDirectory dir = null;
            if (1 == featureDirectoryList.SelectedItems.Count)
                dir = (SetupDirectory)featureDirectoryList.SelectedItems[0].Tag;
            //if (e.Item != null)
            //    dir = (SetupDirectory)e.Item.Tag;

            directoryProperties.SelectedObject = dir;
        }

        private void shortcutList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //shortcutProperties.DataBindings.Clear();
            shortcutProperties.SelectedObject = null;
            if (null != e.Item)
            {
                SetupShortcut sc = (SetupShortcut)e.Item.Tag;
                shortcutProperties.SelectedObject = sc;
            }
        }

        private void ReloadShortcuts()
        {
            shortcutList.Items.Clear();

            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;

                if ((null != current) && (null != current.shortcuts))
                {
                    foreach (SetupShortcut shortcut in current.shortcuts)
                    {
                        ListViewItem i = new ListViewItem(Path.GetFileName(shortcut.dstPath));
                        i.SubItems.Add(Path.GetDirectoryName(shortcut.dstPath));
                        i.SubItems.Add(shortcut.srcPath);
                        i.Tag = shortcut;

                        shortcutList.Items.Add(i);
                    }
                }
            }
        }

        private void MenuAddShortcut_Click(object sender, EventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                SetupShortcut shortcut = new SetupShortcut();
                shortcut.srcPath = "[APPLICATIONFOLDER]";
                if ((null != current.configurableDirectory) && ("" != current.configurableDirectory))
                    shortcut.srcPath = "[" + current.configurableDirectory + "]";
                shortcut.dstPath = "[APPLICATIONFOLDER]\\New Shortcut";
                shortcut.description = "Some Description";

                current.shortcuts.Add(shortcut);
                ReloadShortcuts();

                // Set the new one as selected
                foreach (ListViewItem item in shortcutList.Items)
                {
                    if (item.Tag.Equals(shortcut))
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
        }


        private void MenuDeleteShortcut_Click(object sender, EventArgs e)
        {
            if (featuresTree.SelectedNode != null)
            {
                SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;

                foreach (ListViewItem item in shortcutList.SelectedItems)
                {
                    current.shortcuts.Remove((SetupShortcut)item.Tag);
                }
            }
            ReloadShortcuts();
        }

        private void shortcutProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if ((null != shortcutList.SelectedItems) && (shortcutList.SelectedItems.Count > 0))
            {
                ListViewItem item = shortcutList.SelectedItems[0];
                SetupShortcut shortcut = (SetupShortcut)shortcutProperties.SelectedObject;
                item.Text = Path.GetFileName(shortcut.dstPath);
                item.SubItems[1].Text = Path.GetDirectoryName(shortcut.dstPath);
                item.SubItems[2].Text = shortcut.srcPath;
            }           
        }

        private void MenuDeleteDirectory_Click(object sender, EventArgs e)
        {
            if ((null != featureDirectoryList.SelectedItems) && (featureDirectoryList.SelectedItems.Count == 1))
            {
                if (featuresTree.SelectedNode != null)
                {
                    SetupFeature current = (SetupFeature)featuresTree.SelectedNode.Tag;
                    current.directories.Remove((SetupDirectory)featureDirectoryList.SelectedItems[0].Tag);
                    ReloadDirectories();
                }
            }
        }

        private void targetDirButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            if ((null != currentProject.projectTargetDirectory)
                && ("" != currentProject.projectTargetDirectory))
            {
                dlg.SelectedPath = currentProject.projectTargetDirectory;
            }

            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    currentProject.projectTargetDirectory = dlg.SelectedPath;
                    projectTargetDirectory.DataBindings.Clear();
                    projectTargetDirectory.DataBindings.Add("Text", currentProject, "projectTargetDirectory");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an Exception: \r\n" + ex.ToString());
            }
        }


        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertiesDlg dlg = new PropertiesDlg();
            dlg.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDlg about = new AboutDlg();
            about.ShowDialog();
        }

        private void directoryProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Get selected directory
            if (featureDirectoryList.SelectedItems.Count == 1)
            {
                //SetupDirectory dir = (SetupDirectory)featureDirectoryList.SelectedItems[0].Tag;

                if ("dstPath" == e.ChangedItem.Label)
                    featureDirectoryList.SelectedItems[0].Text = e.ChangedItem.Value.ToString();
                else if ("srcPath" == e.ChangedItem.Label)
                    featureDirectoryList.SelectedItems[0].SubItems[1].Text = e.ChangedItem.Value.ToString();
            }

        }

        private void wixModulesList_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            foreach (string file in files)
            {
                try
                {
                    string my_file = currentProject.projectProperties.UseRelativePaths
                        ? Shell.GetRelativePath(file) : file;


                    string extension = Path.GetExtension(my_file).ToLower();
                    if ((extension != ".wxs") && (extension != ".wixobj"))
                    {
                        throw new ApplicationException(
                            "The Wix-module \"" + my_file + "\" must have extension \".wix\" or \".wixobj.\"");
                    }

                    WixModule wix = new WixModule();
                    wix.Path = my_file;
                    wix.GetInfo();

                    currentProject.projectWixModules.Add(wix);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to add the wix-module: \""
                        + file + "\".\r\n" + ex.Message);

                    break;
                }    
            
            }

            ReloadWixModuleList();
        }
        

        private void wixModulesList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void wixModulesList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (wixModulesList.SelectedItems.Count == 1)
                wixModuleProperties.SelectedObject = (WixModule)wixModulesList.SelectedItems[0].Tag;
            else
                wixModuleProperties.SelectedObject = null;
        }

        private void ReloadWixModuleList()
        {
            wixModulesList.Items.Clear();
            foreach(WixModule wix in currentProject.projectWixModules)
            {
                ListViewItem item = new ListViewItem(wix.Name);
                item.SubItems.Add(wix.Path);
                item.Tag = wix;
                wixModulesList.Items.Add(item);
            }
        }

        private void menuDeleteWixModule_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in wixModulesList.SelectedItems)
            {
                WixModule wm = (WixModule)item.Tag;
                currentProject.projectWixModules.Remove(wm);
            }

            ReloadWixModuleList();
        }

        private void menuCompileWixModule_Click(object sender, EventArgs e)
        {
            compilerOutput.Text = "";

            if (wixModulesList.SelectedItems.Count != 1)
                return;

            WixModule wm = (WixModule)wixModulesList.SelectedItems[0].Tag;
            if (CurrentProject.CompileWixModule(wm))
                MessageBox.Show("Compilation of \"" + wm.Name + "\" completed OK");
        }

        private void menuEditWixModule_Click(object sender, EventArgs e)
        {
            if (wixModulesList.SelectedItems.Count != 1)
                return;

            WixModule wm = (WixModule)wixModulesList.SelectedItems[0].Tag;
            Shell.Browse(wm.Path);
        }

        private void openWarSetupsHomepageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shell.Browse("http://warsetup.jgaa.com");
        }

        private void usersManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string manual = Path.Combine(Shell.warSetupResPath, "WarSetup_3_Users_Guide.pdf");
            if (File.Exists(manual))
                Shell.Browse(manual);
            else
                MessageBox.Show("The Users Manual does not appear to be installed.");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectNeedSave)
            {
                switch (MessageBox.Show("The current project has changed. Do you want to save it?",
                    "Save?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.OK:
                        OnSave(null, null);
                        // Fall troug
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        return; // Do not create a new project
                }
            }

            Close();
        }

        private void tagNewMajorVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentProject.MakeNewGuid(); 
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSave(false);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSave(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnOpen(sender, e);
        }

        public TextBox CompilerOutput
        {
            get { return compilerOutput; }
        }

        private void mergeModulesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mergeModulesList.SelectedItems.Count == 1)
            {
                MergeModule module = (MergeModule)mergeModulesList.SelectedItems[0].Tag;
                mergeModuleProperties.SelectedObject = module;
            }
            else
                mergeModuleProperties.SelectedObject = null;
        }

        private void reloadPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in mergeModulesList.SelectedItems)
            {
                MergeModule mm = (MergeModule)item.Tag;
                mm.LoadInfo();
            }

            ReloadMergeModuleList();
        }


        public static SetupProject CurrentProject
        {
            get { return _this.currentProject; }
        }

        private void projectLicense_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int selected = projectLicense.SelectedIndex;
            if (-1 == selected)
            {
                projectLicense.Text = SetupProject.defaultLicense;

            }
            else
            {
                currentProject.License = (SetupProject.LicenseData)projectLicense.Items[selected];
                projectLicense.Text = currentProject.License.ToString();
            }
        }

        private void SetSelectedLicense()
        {
            for (int i = 0; i < projectLicense.Items.Count; i++)
            {
                SetupProject.LicenseData ld = (SetupProject.LicenseData)projectLicense.Items[i];
                if (currentProject.License.Name == ld.Name)
                {
                    projectLicense.SelectedIndex = i;
                    return;
                }
            }

            // Fallback
            projectLicense.Text = SetupProject.defaultLicense;
        }

        private void LoadLicenses()
        {
            projectLicense.Items.Clear();

            foreach (string path in CurrentProject.LicencePaths)
            {
                foreach (string file in Directory.GetFiles(path, "*.rtf"))
                {
                    SetupProject.LicenseData ld = new SetupProject.LicenseData();
                    ld.Name = Path.GetFileNameWithoutExtension(file);
                    int item = projectLicense.Items.Add(ld);
                }
            }
        }

        private void InitLicense()
        {
            LoadLicenses();
            projectLicense.Text = SetupProject.defaultLicense;
        }

        private void setAsMainFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count == 1)
            {
                FileBinding bind = (FileBinding)fileList.SelectedItems[0].Tag;
                currentProject.mainTargetApp = bind.mComponent.GetFirstFileId();
            }

            ReloadFileList();
        }


        private void addFilesMenu_Click(object sender, EventArgs e)
        {
            SetupFeature feature = null;
            try
            {
                feature = (SetupFeature)featuresTree.SelectedNode.Tag;
            }
            catch (NullReferenceException)
            {
                return;
            }

            // Open file-dialog
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Add the target-files

                string[] files = dlg.FileNames;
                foreach (string file in files)
                {
                    string my_file = currentProject.projectProperties.UseRelativePaths
                        ? Shell.GetRelativePath(file) : file;
                    SetupFile sf = feature.AddFile(my_file);
                }

                // Reload the file-list
                ReloadFileList();
            }
        }

        private void addDirectoryMenu_Click(object sender, EventArgs e)
        {
            SetupFeature feature = null;
            try
            {
                feature = (SetupFeature)featuresTree.SelectedNode.Tag;
            }
            catch (NullReferenceException)
            {
                return;
            }

            // Open file-dialog
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select a directory to add";
            dlg.SelectedPath = LastSelectedFolder;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LastSelectedFolder = dlg.SelectedPath;
                string file = dlg.SelectedPath;
                if ((File.GetAttributes(file) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string my_file = currentProject.projectProperties.UseRelativePaths
                        ? Shell.GetRelativePath(file) : file;
                    feature.directories.Add(new SetupDirectory(my_file));
                }

                ReloadDirectories();
            }
        }

        private void addMergeModuleMenu_Click(object sender, EventArgs e)
        {
            // Open file-dialog
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.InitialDirectory = GetMergeModulesPath();
            dlg.RestoreDirectory = true;
            dlg.Filter = "Merge-modules (*.msm)|*.msm|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string[] files = dlg.FileNames;
                foreach (string file in files)
                {
                    string my_file = currentProject.projectProperties.UseRelativePaths
                        ? Shell.GetRelativePath(file) : file;
                    currentProject.AddMergeModule(my_file);
                }

                // Reload the file-list
                ReloadMergeModuleList();
            }
        }

        private void addWixModuleMenu_Click(object sender, EventArgs e)
        {
            // Open file-dialog
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.InitialDirectory = LastSelectedFolder;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Wix source-files (*.wxs)|*.wxs|Wix object-files (*.wixobj)|*.wixobj|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in dlg.FileNames)
                {
                    try
                    {
                        string my_file = currentProject.projectProperties.UseRelativePaths
                            ? Shell.GetRelativePath(file) : file;


                        string extension = Path.GetExtension(my_file).ToLower();
                        if ((extension != ".wxs") && (extension != ".wixobj"))
                        {
                            throw new ApplicationException(
                                "The Wix-module \"" + my_file + "\" must have extension \".wxs\" or \".wixobj.\"");
                        }

                        WixModule wix = new WixModule();
                        wix.Path = my_file;
                        wix.GetInfo();

                        currentProject.projectWixModules.Add(wix);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to add the wix-module: \""
                            + file + "\".\r\n" + ex.Message);

                        break;
                    }

                }

                ReloadWixModuleList();
            }
        }

        private void LoadRecentFile(object sender, EventArgs e)
        {
            LoadFile(sender.ToString());
        }

        private void LoadRecentMenu()
        {
            openRecentProjectToolStripMenuItem.DropDownItems.Clear();
            if (Properties.Settings.Default.recentProjects != null)
            {
                foreach (string fn in Properties.Settings.Default.recentProjects)
                {
                    openRecentProjectToolStripMenuItem.DropDownItems.Add(
                        fn, null, LoadRecentFile);
                }
            }

            openRecentProjectToolStripMenuItem.Enabled 
                = (openRecentProjectToolStripMenuItem.DropDownItems.Count > 0);
        }

        private void BuildTargetBtn_Click(object sender, EventArgs e)
        {
            compilerOutput.Text = "";
            CurrentProject.BuldTarget();
            //BuildTarget();
        }

        static public void AddToCompilerOutputWindow(string text)
        {
            AddToCompilerOutputDelegate aco;
            aco = delegate(string addText)
            {
                _this.compilerOutput.Text += addText;
            };

            _this.BeginInvoke(aco, new object[] { text });
        }

        private void runTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shell.ShellExecute(CurrentProject.TargetPath);
        }

        private void openTargetDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shell.ShellExecute(CurrentProject.TargetDirectory);
        }

        private void projectLicense_DropDown(object sender, EventArgs e)
        {
            LoadLicenses();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBoxBootStrap.SelectedValue == null)
            {
                return;
            }

            listBoxBootstrap.Items.Add(comboBoxBootStrap.SelectedValue.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBoxBootStrap.SelectedValue == null)
            {
                return;
            }

            listBoxBootstrap.Items.Remove(comboBoxBootStrap.SelectedValue);
        }
    }
}

