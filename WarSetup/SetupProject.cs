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
using System.Threading;


namespace WarSetup
{
    [XmlRoot("WarSetup-Project")]
    public class SetupProject
    {

        #region enums
        enum ProgressE { INIT, GEN_WIX, COMPILE_OBJ_MODULES, COMPILE_OBJ, GENERATE_MSI, FINISH };
        #endregion

        #region Child Classes
        public class LicenseData
        {
            private string _Name;
            private bool _System = true;

            public bool System
            {
                get { return _System; }
                set { _System = value; }
            }

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            override public string ToString()
            {
                return _Name + (System ? "" : " (local)");
            }

            public LicenseData()
            {
                _Name = SetupProject.defaultLicense;
            }
        }

        struct NodeCont
        {
            public string Name;
            public string PrevId;
        }

        List<NodeCont> NodeContainer;

        // Get "after" secuence.
        private string GetPrev(string wantName, string myId)
        {
            string rval = wantName;

            foreach (NodeCont cnt in NodeContainer)
            {
                if (wantName == cnt.Name)
                {
                    rval = cnt.PrevId;
                    NodeContainer.Remove(cnt);
                    break;
                }
            }

            NodeCont node = new NodeCont();
            node.Name = wantName;
            node.PrevId = myId;
            NodeContainer.Add(node);
            return rval;
        }

        private class TargetDir
        {
            public TargetDir()
            {
            }

            public TargetDir(string path, XmlElement node)
            {
                mPath = path;
                mXmlNode = node;
            }

            public string mPath;
            public XmlElement mXmlNode;
        };


        #endregion

        #region Variables
        private string _projectName;
        private string _projectVersion;
        private string _projectOrganization;
        private string _projectFromWindowsVersion;
        private string _projectRequireDotNetVersion;
        private bool _projectMustBeAdministratorToInstall;
        private bool _project64BitTarget;
        private string _projectTargetDirectory; // msi files are buiilt here
        private string _projectTargetName; // Name without extension
        private int _projectType; // 0 = normal, 1 = merge-module
        private string _projectUuid;
        private string _projectUpgradeUuid;
        private List<WarSetup.SetupFeature> _projectFeatures;
        private List<MergeModule> _projectMergeModules;
        private int _projectInstallForCurrentOrAllUsers = 0;
        private string _projectUserInterface;
        private LicenseData _License;
        //private string _projectLicense;
        private ProjectProperties _projectProperties;
        private List<WixModule> _projectWixModules;
        private int _nextUniqueId = 1000;
        private string _mainTargetApp;
        private bool _projectRunAfterInstall = true;
        private bool _virgin = true;
        private bool _useSeqenceNumers = true;
        private bool _overRideMenuShortcutDir = false;
        private string _menuShortcutDirName = "";

        #region Compile-time variables

        private CompileDlg _compileDlg;
        private XmlElement installdir_directory;
        private XmlDocument doc;
        private XmlElement root_directory;
        private XmlElement package;
        private XmlElement product;
        private XmlElement program_menu;
        private XmlElement my_program_menu;
        private XmlElement my_start_menu;
        private XmlElement desktop;
        private XmlElement quicklaunch;
        private XmlElement appdata;
        private XmlElement features_root;
        private XmlElement upgrade;
        private XmlElement curr_media_node = null;
        private string pf_directory_id;
        private Dictionary<string, XmlElement> product_binaries;
        private XmlElement _install_execute_sequence;
        private XmlElement _install_ui_sequence_sequence;
        private Dictionary<string, XmlElement> vc_file_conditions;
        private List<string> _includePathsForLight;
        private List<string> AddDeleteFolders;
        private int current_media_id = 0;
        private long current_media_size;

        // Index based on the full path
        private Dictionary<string, TargetDir> currentTargetDirs;

        // Index based on the Directory Id attribute;
        private Dictionary<string, TargetDir> currentTargetDirIdList;

        #endregion

        #endregion

        #region Properties

        public List<String> LicencePaths
        {
            get
            {
                List<String> paths = new List<String>();
                paths.Add(Path.Combine(Shell.warSetupResPath, "Licenses"));
                if (projectProperties.LicencePath != "")
                    paths.Add(projectProperties.LicencePath);

                return paths;
            }
        }

        public string id
        {
            get
            {
                string id = "ID_" + projectUuid;
                id = id.Replace('-', '_');
                return id;
            }
        }

        public string TargetExt
        {
            get { return (isMergeModle ? ".msm" : ".msi"); }
        }


        public string TargetDirectory
        {
            get
            {
                string target_dir = projectTargetDirectory;
                if (null == target_dir)
                    target_dir = ".\\";

                return target_dir;
            }
        }

        public string TargetFileNoExt
        {
            get
            {
                string target_file = projectTargetName;
                if (null == target_file)
                    target_file = "Setup";
                return target_file;
            }
        }

        public string TargetFile
        {
            get
            {
                return TargetFileNoExt + TargetExt;
            }
        }

        public string TargetPathNoExt
        {
            get
            {
                return Path.Combine(TargetDirectory, TargetFileNoExt);
            }
        }

        public string TargetPath
        {
            get
            {
                return TargetPathNoExt + TargetExt;
            }
        }

        public bool Virgin
        {
            get { return _virgin; }
            set { _virgin = value; }
        }

        [XmlAttribute("menuShortcutDirName")]
        public string menuShortcutDirName
        {
            get { return _menuShortcutDirName; }
            set { _menuShortcutDirName = value; }
        }


        [XmlAttribute("overRideMenuShortcutDir")]
        public bool overRideMenuShortcutDir
        {
            get { return _overRideMenuShortcutDir; }
            set { _overRideMenuShortcutDir = value; }
        }


        [XmlAttribute("projectRunAfterInstall")]
        public bool projectRunAfterInstall
        {
            get { return _projectRunAfterInstall; }
            set { _projectRunAfterInstall = value; }
        }

        [XmlAttribute("mainTargetApp")]
        public string mainTargetApp
        {
            // Refers to /one/ file.fileId componentGuid or null.
            get { return _mainTargetApp; }
            set { _mainTargetApp = value; }
        }

        [XmlAttribute("nextUniqueId")]
        public int nextUniqueId
        {
            get { return _nextUniqueId; }
            set { _nextUniqueId = value; }
        }

        [XmlAttribute("projectName")]
        public string projectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }

        [XmlAttribute("projectUuid")]
        public string projectUuid
        {
            get { return _projectUuid; }
            set { _projectUuid = value; }
        }

        [XmlAttribute("projectUpgradeUuid")]
        public string projectUpgradeUuid
        {
            get { return _projectUpgradeUuid; }
            set { _projectUpgradeUuid = value; }
        }

        [XmlAttribute("projectVersion")]
        public string projectVersion
        {
            get { return _projectVersion; }
            set { _projectVersion = value; }
        }

        [XmlAttribute("projectOrganization")]
        public string projectOrganization
        {
            get { return _projectOrganization; }
            set { _projectOrganization = value; }
        }


        [XmlAttribute("projectFromWindowsVersion")]
        public string projectFromWindowsVersion
        {
            get { return _projectFromWindowsVersion; }
            set { _projectFromWindowsVersion = value; }
        }

        [XmlAttribute("projectMustBeAdministratorToInstall")]
        public bool projectMustBeAdministratorToInstall
        {
            get { return _projectMustBeAdministratorToInstall; }
            set { _projectMustBeAdministratorToInstall = value; }
        }

        private void AddPropertyRef(string name)
        {
            XmlElement propRef = doc.CreateElement("PropertyRef");
            propRef.SetAttribute("Id", name);
            product.AppendChild(propRef);
        }

        [XmlAttribute("project64BitTarget")]
        public bool project64BitTarget
        {
            get { return _project64BitTarget; }
            set { _project64BitTarget = value; }
        }

        [XmlAttribute("projectTargetDirectory")]
        public string projectTargetDirectory
        {
            get { return _projectTargetDirectory; }
            set { _projectTargetDirectory = value; }
        }

        [XmlAttribute("projectRequireDotNetVersion")]
        public string projectRequireDotNetVersion
        {
            get { return _projectRequireDotNetVersion; }
            set { _projectRequireDotNetVersion = value; }
        }

        [XmlAttribute("projectTargetName")]
        public string projectTargetName
        {
            get { return _projectTargetName; }
            set { _projectTargetName = value; }
        }

        [XmlAttribute("projectType")]
        public int projectType
        {
            get { return _projectType; }
            set { _projectType = value; }
        }

        [XmlElement("projectFeatures")]
        public List<WarSetup.SetupFeature> projectFeatures
        {
            get
            {
                return _projectFeatures;
            }

            set
            {
                _projectFeatures = value;
            }
        }

        [XmlElement("projectMergeModules")]
        public List<MergeModule> projectMergeModules
        {
            get { return _projectMergeModules; }
            set { _projectMergeModules = value; }
        }

        [XmlElement("projectWixModules")]
        public List<WixModule> projectWixModules
        {
            get { return _projectWixModules; }
            set { _projectWixModules = value; }
        }

        //[XmlElement("buildNoValidiation")]
        //public bool buildNoValidiation
        //{
        //    get { return _buildNoValidiation; }
        //    set { _buildNoValidiation = value; }
        //}

        [XmlElement("projectInstallForCurrentOrAllUsers")]
        public int projectInstallForCurrentOrAllUsers
        {
            get { return _projectInstallForCurrentOrAllUsers; }
            set { _projectInstallForCurrentOrAllUsers = value; }
        }

        [XmlAttribute("projectUserInterface")]
        public string projectUserInterface
        {
            get { return _projectUserInterface; }
            set { _projectUserInterface = value; }
        }

        // Obsolete. Kept for backwards compatibility
        [XmlAttribute("projectLicense")]
        public string projectLicense
        {
            //get { return _projectLicense; }
            set
            {
                _License = new LicenseData();
                _License.Name = value;
            }
        }

        [XmlElement("License")]
        public LicenseData License
        {
            get { return _License; }
            set { _License = value; }
        }

        [XmlElement("projectProperties")]
        public ProjectProperties projectProperties
        {
            get { return _projectProperties; }
            set { _projectProperties = value; }
        }

        static public string defaultLicense
        {
            get { return "Common Public License Version 1.0"; }
        }

        static public string[] WindowsInstallerKnownDirs
        {
            get
            {
                return new string[]
                {
                    "AdminToolsFolder",
                    "AppDataFolder",
                    "CommonAppDataFolder]",
                    "CommonFiles64Folder]",
                    "CommonFilesFolder",
                    "DesktopFolder",
                    "FavoritesFolder ",
                    "FontsFolder",
                    "LocalAppDataFolder",
                    "MyPicturesFolder",
                    "PersonalFolder",
                    "ProgramFiles64Folder",
                    "ProgramFilesFolder",
                    "ProgramMenuFolder",
                    "ROOTDRIVE",
                    "SendToFolder",
                    "StartMenuFolder",
                    "StartupFolder",
                    "System64Folder",
                    "SystemFolder",
                    "TempFolder",
                    "TemplateFolder",
                    "WindowsFolder",
                    "WindowsVolume"
                };
            }
        }

        #endregion

        #region Support Methods
        public bool isMergeModle
        {
            get { return _projectType == 1; }
        }

        public void MakeNewGuid()
        {
            _projectUuid = System.Guid.NewGuid().ToString();
        }



        public void Clear()
        {
            MakeNewGuid();
            _projectMustBeAdministratorToInstall = false;
            _project64BitTarget = false;
            _projectTargetName = "Setup";
            _projectType = 0;
            _projectVersion = "1.0.0";
            _projectName = "";
            _projectUpgradeUuid = System.Guid.NewGuid().ToString();
            _projectFeatures = new List<WarSetup.SetupFeature>();
            _projectMergeModules = new List<MergeModule>();
            _projectInstallForCurrentOrAllUsers = 0;
            _projectUserInterface = "WixUI_Mondo";
            _projectProperties = new ProjectProperties();
            _projectWixModules = new List<WixModule>();
            _projectProperties.UiCultures.Clear();
            _License = new LicenseData();
        }

        public void InitializeDefaults()
        {
            SetupFeature features = new WarSetup.SetupFeature();
            features.featureName = "";
            features.featureDescription = "The complete package";
            features.configurableDirectory = "APPLICATIONFOLDER";
            features.configurableDirectoryDefaultPath = "[ProjectName]";
            features.defaultInstallMode = (int)SetupFeature.InstallModeE.INSTALL_LOCALLY;

            {
                SetupFeature program_feature = new SetupFeature();
                program_feature.featureName = "Program";
                program_feature.featureDescription = "The Main Executable";
                program_feature.featureId = "PROGRAM";
                program_feature.defaultInstallMode = (int)SetupFeature.InstallModeE.INSTALL_LOCALLY;
                features.childFeatures.Add(program_feature);
            }

            {
                SetupFeature program_feature = new SetupFeature();
                program_feature.featureName = "Documentation";
                program_feature.featureDescription = "The documentation";
                program_feature.featureId = "DOCUMENTATION";
                program_feature.defaultInstallMode = (int)SetupFeature.InstallModeE.INSTALL_LOCALLY;
                features.childFeatures.Add(program_feature);
            }

            _projectFeatures.Add(features);
            projectProperties.AddAllUiCulture();
        }

        public SetupProject()
        {
            Clear();
        }

        public void AddMergeModule(string path)
        {
            MergeModule mm = new MergeModule();
            mm.SrcPath = path;
            mm.LoadInfo();
            projectMergeModules.Add(mm);
        }

        public string GetUniqueId()
        {
            string id = "ID" + nextUniqueId.ToString();
            nextUniqueId++;
            return id;
        }

        private SetupFile ResolveMainFeature(List<WarSetup.SetupFeature> features)
        {
            foreach (SetupFeature feature in features)
            {

                foreach (SetupComponent component in feature.components)
                {
                    foreach (SetupFile file in component.componentFiles)
                    {
                        if (mainTargetApp == file.fileId)
                            return file; // Found
                    }
                }

                SetupFile target = ResolveMainFeature(feature.childFeatures);
                if (null != target)
                    return target;
            }

            return null;
        }


        // Lookup the "main target" in the project. This is a single file in some
        // component (if it is defined).
        // If the file is not found, the mainTargetApp is reset to null
        // (if this happens, the main-feature is most likely deleted).
        // If showWarning is true, a dialog-box pops up if the launch ... checkbox is
        // checked, and no main-feature is defined.
        public SetupFile ResolveMainFeature(bool showWarning)
        {
            if (mainTargetApp != null)
            {
                SetupFile file = ResolveMainFeature(projectFeatures);

                if (file != null)
                {
                    if (mainTargetApp == file.fileId)
                        return file;
                }

                mainTargetApp = null;
            }

            if ((mainTargetApp == null) && projectRunAfterInstall && showWarning)
            {
                MessageBox.Show("You must select a file as Main Application Target in order to suggest to run the \"Main Application Target\"\r\n Choose the main executable in your project (or whatever file you want to lauch, right-click on it\r\n and select the \"Make Main Application Target\" pop-up menu.");
            }

            return null;
        }

        // Get a valid value for the UpgradeVersion/@Maximum, by getting the highest possible version less than the current
        private string GetMaxVersion()
        {
            return projectVersion;

            //int a = 0, b = 0, c = 0;
            //char[] delim =  { '.' };
            //string[] v = projectVersion.Split(delim);
            //if (v.Length > 0)
            //    a = int.Parse(v[0]);
            //if (v.Length > 1)
            //    b = int.Parse(v[1]);
            //if (v.Length > 2)
            //    c = int.Parse(v[2]);

            //if (c > 0)
            //{
            //    c--;
            //}
            //else if (b > 0)
            //{
            //    b--;
            //}
            //else if (a > 0)
            //{
            //    a--;
            //}

            //return a.ToString() + "." + b.ToString() + "." + c.ToString();
        }

        public int GetVersionAsInt(string version)
        {
            int a = 0, b = 0, c = 0;
            char[] delim = { '.' };
            string[] v = version.Split(delim);
            if (v.Length > 0)
                a = int.Parse(v[0]);
            if (v.Length > 1)
                b = int.Parse(v[1]);
            if (v.Length > 2)
                c = int.Parse(v[2]);

            return (a * 1000) + (b * 100) + c;
        }

        public void EnumTargetDirs(List<string> dirs)
        {
            EnumTargetDirs(projectFeatures, dirs);
        }

        private void EnumTargetDirs(List<WarSetup.SetupFeature> features, List<string> dirs)
        {
            foreach (SetupFeature feature in features)
            {

                if ((feature.configurableDirectory != null)
                    && (feature.configurableDirectory != ""))
                {
                    dirs.Add("[" + feature.configurableDirectory + "]");
                }

                EnumTargetDirs(feature.childFeatures, dirs);
            }
        }

        #endregion

        #region Compiler

        #region Support methods for the compiler

        private void AddCondition(string message, string condition)
        {
            if (isMergeModle)
                return;
            XmlElement cond = doc.CreateElement("Condition");
            cond.SetAttribute("Message", message);
            cond.AppendChild(doc.CreateTextNode(condition));
            product.AppendChild(cond);
        }


        private void AddIes(XmlElement parent, string ies, int seq, string condition)
        {
            XmlElement cond = doc.CreateElement(ies);

            if (_useSeqenceNumers && (seq > 9))
                cond.SetAttribute("Sequence", seq.ToString());
            if (null != condition)
                cond.AppendChild(doc.CreateTextNode(condition));
            parent.AppendChild(cond);
        }


        /* Returns the XML-node where the component belong. 
         * The node is created on demand. 
         * The id is set on the created directory if it is non-null
         */
        private TargetDir GetDirectoryNode(string path, string id)
        {
            TargetDir rval = null;

            // Split the path into a list
            char[] separator = { '/', '\\' };
            string[] subpaths = path.Split(separator);
            XmlElement last_good_node = root_directory;
            int level = 0;
            string current = "";

            // Use the APPLICATIONFOLDER as start-node.
            if (currentTargetDirIdList.TryGetValue("APPLICATIONFOLDER", out rval))
            {
                current = rval.mPath;
                last_good_node = rval.mXmlNode;
            }

            foreach (string name in subpaths)
            {
                bool resolved = false;
                if ((level++ == 0) && (name.Length > 2) && (name[0] == '['))
                {
                    // Map this path to the absolute name of the node if it exists in the table
                    string my_name = name.Substring(1, name.Length - 2);
                    if (currentTargetDirIdList.TryGetValue(my_name, out rval))
                    {
                        resolved = true;
                    }
                    else
                    {
                        //Check if it is a system-dir
                        foreach (string msi_name in WindowsInstallerKnownDirs)
                        {
                            if (my_name == msi_name)
                            {
                                // Make the node
                                rval = new TargetDir();
                                rval.mXmlNode = doc.CreateElement("Directory");
                                rval.mPath = current = my_name;
                                root_directory.AppendChild(rval.mXmlNode);

                                rval.mXmlNode.SetAttribute("Id", msi_name);

                                SetNameAttribute(rval.mXmlNode, msi_name + " Directory");

                                // Add the key
                                currentTargetDirs.Add(rval.mPath, rval);
                                currentTargetDirIdList.Add(rval.mXmlNode.GetAttribute("Id"), rval);

                                resolved = true;

                                break;
                            }
                        }
                    }
                }

                if (!resolved)
                {
                    if (current.Length > 0)
                        current = Path.Combine(current, name);
                    else
                        current = name;

                    if (!currentTargetDirs.TryGetValue(current, out rval))
                    {
                        // Make the node
                        rval = new TargetDir();
                        rval.mXmlNode = doc.CreateElement("Directory");
                        rval.mPath = current;
                        last_good_node.AppendChild(rval.mXmlNode);

                        if ((level == subpaths.Length) && (null != id))
                            rval.mXmlNode.SetAttribute("Id", id);
                        else
                            rval.mXmlNode.SetAttribute("Id", "Id_" + SetupDirectory.GetMd5Hash(current));

                        SetNameAttribute(rval.mXmlNode, name);
                        //rval.mXmlNode.SetAttribute("DiskId", GetMainFrame().GetMedia(0).ToString());

                        // Add the key
                        currentTargetDirs.Add(rval.mPath, rval);
                        currentTargetDirIdList.Add(rval.mXmlNode.GetAttribute("Id"), rval);
                    }
                }

                current = rval.mPath;
                last_good_node = rval.mXmlNode;
            }

            return rval;
        }

        private void SetNameAttribute(XmlElement node, string name)
        {
            node.SetAttribute("Name", name);
            //string short_name = SetupFile.GetShortPath(name);
            //if (short_name != name)
            //    node.SetAttribute("ShortName", short_name);
        }

        // Find the parent node of feature.
        // Since we have no "parent" property, we have to start a recursive scan from
        // the projects feature-list
        private SetupFeature GetParentFeature(SetupFeature feature, SetupFeature parent, List<SetupFeature> parentList)
        {
            // Scan down
            foreach (SetupFeature child in parentList)
            {
                if (child.featureId == feature.featureId)
                    return parent;

                SetupFeature probe = GetParentFeature(feature, child, child.childFeatures);
                if (null != probe)
                    return probe;
            }

            return null;
        }

        private string GetDefaultDir(SetupFeature feature)
        {
            if ((null != feature.configurableDirectory) && ("" != feature.configurableDirectory))
                return feature.configurableDirectory;

            // try parent
            SetupFeature parent = GetParentFeature(feature, null, projectFeatures);
            if (null != parent)
                return GetDefaultDir(parent);

            return null; // No luck
        }

        private XmlElement GenerateComponent(SetupComponent component, SetupFeature feature, XmlElement feature_node)
        {
            // Make the reference from Feature
            XmlElement component_ref = doc.CreateElement("ComponentRef");
            component_ref.SetAttribute("Id", component.componentId);
            feature_node.AppendChild(component_ref);

            // Make the actual component and file
            XmlElement component_node = doc.CreateElement("Component");
            component_node.SetAttribute("Id", component.componentId);
            component_node.SetAttribute("Guid", component.componentGuid);

            if (component.addToCreateFolder)
            {
                XmlElement node = doc.CreateElement("CreateFolder");
                component_node.AppendChild(node);
            }

            // Link the component to the appropriate directory
            string target_dir_name = component.targetDirectory; // Not accessible from the UI. We use the file.dstPath in stead.

            // Now, lets see if the file overrides the default target. We only have one file
            // for each component (unless the older i sscanned, in wich case the dstPath in the
            // file is guaranteed to be empty
            if ((component.componentFiles.Count == 1)
                && ("" != component.componentFiles[0].dstPath))
            {
                target_dir_name = component.componentFiles[0].dstPath;
            }

            if ((null == target_dir_name) || ("" == target_dir_name))
            {
                target_dir_name = GetDefaultDir(feature);
                if ((null == target_dir_name) || ("" == target_dir_name))
                    target_dir_name = "[APPLICATIONFOLDER]";
                else
                    target_dir_name = "[" + target_dir_name + "]";

            }

            TargetDir target_dir = GetDirectoryNode(target_dir_name, null);
            target_dir.mXmlNode.AppendChild(component_node);

            // Path
            if ((null != component.pathEntry) && ("" != component.pathEntry))
            {
                XmlElement path_node = doc.CreateElement("Environment");
                path_node.SetAttribute("Id", GetUniqueId());
                path_node.SetAttribute("Name", "PATH");
                path_node.SetAttribute("Action", "set");
                path_node.SetAttribute("System", "yes");
                path_node.SetAttribute("Part", "last");
                path_node.SetAttribute("Value", component.pathEntry);
                component_node.AppendChild(path_node);
            }


            foreach (SetupFile file in component.componentFiles)
            {

                XmlElement file_node = doc.CreateElement("File");
                file_node.SetAttribute("Id", file.fileId);
                string src_path = Path.Combine(file.srcDirectory, file.srcName);
                file_node.SetAttribute("Source", src_path);
                if (!isMergeModle)
                    file_node.SetAttribute("DiskId", GetMedia(src_path).ToString());

                SetNameAttribute(file_node, file.dstName);

                if (file.ExecuteOnInstall)
                {
                    AddExecuteOnInstall(file);
                }

                /* TODO: Create a dedicated component for user-shortcuts (for each file)
                 * to avoid the "ICE57: Component 'IDC5A7B8F2554B02E5FC9D5ED7E684B285' 
                 * has both per-user and per-machine data with a per-machine KeyPath."
                 * warning.
                 */

                //if (file.shortcutInProgramFilesMenu)
                //    file_node.AppendChild(AddShortcut(file, my_program_menu, "pf"));

                //if (file.shortcutInQuickLaunch)
                //    file_node.AppendChild(AddShortcut(file, quicklaunch, "ql"));

                //if (file.shortcutOnDesktop)
                //    file_node.AppendChild(AddShortcut(file, desktop, "dt"));

                if (file.shortcutInProgramFilesMenu)
                    AddShortcut(file, feature_node, my_program_menu, target_dir.mXmlNode);

                if (file.shortcutInQuickLaunch)
                    AddShortcut(file, feature_node, quicklaunch, target_dir.mXmlNode);

                if (file.shortcutOnDesktop)
                    AddShortcut(file, feature_node, desktop, target_dir.mXmlNode);

                if (file.shortcutInStartupFolder)
                    AddShortcut(file, feature_node, my_start_menu, target_dir.mXmlNode);


                if (file.isTrueTypeFont)
                {
                    file_node.SetAttribute("TrueType", "yes");
                    //if ((file.dstPath == null) || (file.dstPath == ""))
                }

                if (file.isComModule)
                {
                    XmlElement typelib = doc.CreateElement("TypeLib");
                    typelib.SetAttribute("Id", file.typelibGuid);
                    typelib.SetAttribute("Language", "0");
                    typelib.SetAttribute("Advertise", "yes"); // Just to avoid manual version mess
                    file_node.AppendChild(typelib);
                    if ((file.typelibGuid == null) || (file.typelibGuid == ""))
                    {
                        throw new ApplicationException(
                            "The COM/Typelib file \"" + file.srcName + "\" is missing a GUID. This must be specifield.");
                    }
                }

                if (file.service.isService)
                {
                    XmlElement svc = doc.CreateElement("ServiceInstall");
                    component_node.AppendChild(svc);

                    svc.SetAttribute("Id", file.service.id);
                    if ("" != file.service.userAccount)
                        svc.SetAttribute("Account", file.service.userAccount);
                    if ("" != file.service.userPassword)
                        svc.SetAttribute("Password", file.service.userPassword);
                    if ("" != file.service.cmdLineArguments)
                        svc.SetAttribute("Arguments", file.service.cmdLineArguments);
                    if ("" != file.service.description)
                        svc.SetAttribute("Description", file.service.description);
                    svc.SetAttribute("ErrorControl", file.service.errorControl.ToString());
                    svc.SetAttribute("Interactive", file.service.interactive ? "yes" : "no");
                    if ("" != file.service.loadOrderGroup)
                        svc.SetAttribute("LoadOrderGroup", file.service.loadOrderGroup);
                    svc.SetAttribute("Name", file.service.serviceName);
                    svc.SetAttribute("Start", file.service.startMode.ToString());
                    svc.SetAttribute("Type", "ownProcess");
                    svc.SetAttribute("Vital", file.service.vital ? "yes" : "no");

                    XmlElement svc_ctl = doc.CreateElement("ServiceControl");
                    component_node.AppendChild(svc_ctl);

                    svc_ctl.SetAttribute("Id", file.service.id);
                    svc_ctl.SetAttribute("Name", file.service.serviceName);
                    svc_ctl.SetAttribute("Remove", file.service.Remove.ToString());
                    if (file.service.StartWhenInstalled)
                        svc_ctl.SetAttribute("Start", "install");
                }

                foreach (FileExtension ext in file.fileExtensions)
                {
                    XmlElement prog_id_node = doc.CreateElement("ProgId");
                    prog_id_node.SetAttribute("Id", ext.Id);
                    prog_id_node.SetAttribute("Description", ext.Description);
                    prog_id_node.SetAttribute("Icon", file.fileId);
                    prog_id_node.SetAttribute("IconIndex", ext.IconIndex.ToString());
                    component_node.AppendChild(prog_id_node);

                    XmlElement ext_node = doc.CreateElement("Extension");
                    ext_node.SetAttribute("Id", ext.Extension);
                    ext_node.SetAttribute("ContentType", ext.MimeType);
                    prog_id_node.AppendChild(ext_node);

                    XmlElement verb_node = doc.CreateElement("Verb");
                    verb_node.SetAttribute("Id", "Open");
                    verb_node.SetAttribute("Command", "Open");
                    verb_node.SetAttribute("TargetFile", file.fileId);
                    verb_node.SetAttribute("Argument", "\"%1\""); // BUG 1944386 FIXED

                    ext_node.AppendChild(verb_node);
                }

                component_node.AppendChild(file_node);
            }

            return component_node;
        }

        private void AddExecuteOnInstall(SetupFile file)
        {

            if (null != this.product)
            {
                string id = GetUniqueId().ToUpper();
                XmlElement p1 = doc.CreateElement("CustomAction");
                p1.SetAttribute("Id", id);
                p1.SetAttribute("FileKey", file.fileId);
                p1.SetAttribute("ExeCommand", file.ExecuteOnInstallParameters);
                p1.SetAttribute("Execute", "immediate");
                p1.SetAttribute("Return", "asyncNoWait");
                this.product.AppendChild(p1);


                if (null != _install_execute_sequence)
                {
                    XmlElement p2 = doc.CreateElement("Custom");
                    p2.SetAttribute("Action", id);
                    p2.SetAttribute("After", "InstallFinalize");
                    _install_execute_sequence.AppendChild(p2);
                }
            }
        }

        private bool FixupInstallDir(List<SetupFeature> features)
        {
            foreach (SetupFeature feature in features)
            {
                if (feature.excludeFromBuild)
                    continue;

                if (feature.configurableDirectory == "APPLICATIONFOLDER")
                {
                    if ((null != feature.configurableDirectoryDefaultPath)
                        && ("" != feature.configurableDirectoryDefaultPath))
                    {
                        // TODO: Handle alternative APPLICATIONFOLDER paths 
                        //  Currently we only handle an alternative name in "Program Files"



                        string name = feature.configurableDirectoryDefaultPath;
                        if (name == "[ProjectName]")
                            name = projectName;

                        AddProperty("ApplicationFolderName", name);
                        //installdir_directory.SetAttribute("Name", name);

                        if (!isMergeModle)
                        {
                            if ("WixUI_Advanced" != projectUserInterface)
                                installdir_directory.SetAttribute("Name", name);
                            if ("WixUI_InstallDir" == projectUserInterface)
                                AddProperty("WIXUI_INSTALLDIR", "APPLICATIONFOLDER");
                        }
                    }

                    return true;
                }
                if (FixupInstallDir(feature.childFeatures))
                    return true;
            }

            return false;
        }

        private void GenerateFeature(SetupFeature feature, XmlElement parent)
        {
            if (feature.excludeFromBuild)
                return;

            XmlElement node = doc.CreateElement("Feature");

            if (null == parent)
            {
                // Root
                if (!isMergeModle)
                    product.AppendChild(node);
                features_root = node;
            }
            else
            {
                parent.AppendChild(node);
            }

            node.SetAttribute("Id", feature.featureId);
            node.SetAttribute("Title", feature.featureName);
            node.SetAttribute("Level", feature.installLevel.ToString());
            node.SetAttribute("Description", feature.featureDescription);
            string install_mode = null;
            switch ((SetupFeature.InstallModeE)feature.defaultInstallMode)
            {
                case SetupFeature.InstallModeE.INSTALL_LOCALLY:
                    install_mode = "local";
                    break;
                case SetupFeature.InstallModeE.RUN_FROM_SOURCE:
                    install_mode = "source";
                    break;
                case SetupFeature.InstallModeE.SAME_AS_PARENT_FEATURE:
                    install_mode = "followParent";
                    break;
            }

            // Handle configurable directories for the feature
            // Add the dir for this node.
            if ((null != feature.configurableDirectory)
                && ("" != feature.configurableDirectory)
                && ("APPLICATIONFOLDER" != feature.configurableDirectory))
            {
                GetDirectoryNode(feature.configurableDirectoryDefaultPath,
                    feature.configurableDirectory);
            }

            // Generate the components (and files) for the directories
            List<SetupComponent> dir_components = new List<SetupComponent>();
            foreach (SetupDirectory dir in feature.directories)
            {
                // Make the Directory-node if it don't exist
                GetDirectoryNode(dir.dstPath, dir.dirId);

                // Make components under the dir
                dir.MakeComponents(dir_components, dir.srcPath, dir.dstPath);

                if (dir.addToPath)
                {
                    dir.pathComponent.pathEntry = "[" + GetDirectoryNode(dir.dstPath, null).mXmlNode.GetAttribute("Id") + "]";
                    dir_components.Add(dir.pathComponent);
                }
                else
                    dir.pathComponent.pathEntry = null;

                // Visual Studio Integration
                if (dir.ideMicrosoftVisualStudio2003)
                    MakeVcIntegration("Microsoft Visual Studio 2003", dir, feature, node);
                if (dir.ideMicrosoftVisualStudio2005)
                    MakeVcIntegration("Microsoft Visual Studio 2005", dir, feature, node);
            }

            if (install_mode != null)
                node.SetAttribute("InstallDefault", install_mode);

            if ((feature.configurableDirectory != null) && ("" != feature.configurableDirectory))
                node.SetAttribute("ConfigurableDirectory", feature.configurableDirectory);

            foreach (SetupComponent component in feature.components)
                GenerateComponent(component, feature, node);

            foreach (SetupComponent component in dir_components)
                GenerateComponent(component, feature, node);

            {
                //XmlElement my_component = GenerateComponent(feature.component, node);

                // Add shortcuts
                foreach (SetupShortcut shortcut in feature.shortcuts)
                {
                    AddShortcut(shortcut, feature, null, null);

                    if (shortcut.showOnDesktop)
                        AddShortcut(shortcut, feature, desktop.GetAttribute("Id"), "dt");
                    if (shortcut.showOnProgramMenu)
                        AddShortcut(shortcut, feature, my_program_menu.GetAttribute("Id"), "pf");
                    if (shortcut.showOnQuickLaunchBar)
                        AddShortcut(shortcut, feature, quicklaunch.GetAttribute("Id"), "ql");
                }
            }

            GenerateFeature(feature.childFeatures, node);
            return;
        }

        private XmlElement AddShortcut(SetupFile file, XmlElement target, string shortcutType)
        {
            XmlElement shortcut = doc.CreateElement("Shortcut");
            shortcut.SetAttribute("Id", shortcutType + "_shortcut_" + file.fileId);
            shortcut.SetAttribute("Directory", target.GetAttribute("Id"));
            SetNameAttribute(shortcut, file.menuName);

            if ("" != file.shortcutCommandArguments)
                shortcut.SetAttribute("Arguments", file.shortcutCommandArguments);
            if ("" != file.shortcutDescription)
                shortcut.SetAttribute("Description", file.shortcutDescription);
            if ("" != file.shortcutWorkingDirectory)
            {
                TargetDir dir = GetDirectoryNode(file.shortcutWorkingDirectory, null);
                string id = dir.mXmlNode.GetAttribute("Id");
                shortcut.SetAttribute("WorkingDirectory", id);
            }

            return shortcut;
        }

        private void AddShortcut(SetupFile file, XmlElement feature, XmlElement target, XmlElement parentDir)
        {
            AddShortcut(file.fileId, target.GetAttribute("Id"), file.menuName,
                file.shortcutCommandArguments, file.shortcutDescription,
                file.shortcutWorkingDirectory, feature, parentDir);
        }

        private void AddShortcut(string targetId, string createInDirId, string name, string cmdLine,
            string description, string workDir, XmlElement feature, XmlElement parentDir)
        {
            XmlElement shortcut = doc.CreateElement("Shortcut");
            shortcut.SetAttribute("Id", GetUniqueId());
            shortcut.SetAttribute("Target", "[#" + targetId + "]");
            shortcut.SetAttribute("Directory", createInDirId);
            SetNameAttribute(shortcut, name);

            if ((null != cmdLine) && ("" != cmdLine))
                shortcut.SetAttribute("Arguments", cmdLine);
            if ((null != description) && ("" != description))
                shortcut.SetAttribute("Description", description);
            if ((null != workDir) && ("" != workDir))
            {
                TargetDir dir = GetDirectoryNode(workDir, null);
                string id = dir.mXmlNode.GetAttribute("Id");
                shortcut.SetAttribute("WorkingDirectory", id);
            }

            XmlElement comp = CreateComponent(parentDir, true, feature);
            comp.AppendChild(shortcut);

        }

        // Create an empty component
        private XmlElement CreateComponent(XmlElement parent, bool makeRegEntry, XmlElement feature)
        {
            XmlElement comp = doc.CreateElement("Component");
            comp.SetAttribute("Id", GetUniqueId());
            comp.SetAttribute("Guid", Guid.NewGuid().ToString());

            if (null != parent)
                parent.AppendChild(comp);

            if (makeRegEntry)
            {
                //XmlElement reg = doc.CreateElement("Registry");
                //reg.SetAttribute("Root", "HKCU");
                //reg.SetAttribute("Key", @"SOFTWARE\"
                //    + projectOrganization
                //    + @"\" + projectName
                //    + @"\Components\" + comp.GetAttribute("Id"));
                //reg.SetAttribute("KeyPath", "yes");

                XmlElement reg = doc.CreateElement("RegistryValue");
                reg.SetAttribute("Root", "HKCU");
                reg.SetAttribute("Key", @"SOFTWARE\"
                    + projectOrganization
                    + @"\" + projectName
                    + @"\Components\" + comp.GetAttribute("Id"));
                reg.SetAttribute("Type", "string");
                reg.SetAttribute("KeyPath", "yes");
                reg.SetAttribute("Value", "");

                comp.AppendChild(reg);
            }

            if (null != feature)
            {
                XmlElement comp_ref = doc.CreateElement("ComponentRef");
                comp_ref.SetAttribute("Id", comp.GetAttribute("Id"));
                feature.AppendChild(comp_ref);
            }

            return comp;
        }

        private XmlElement AddShortcut(SetupShortcut shortcut, SetupFeature feature, string directoryId, string shortcutType)
        {
            XmlElement sc_node = doc.CreateElement("Shortcut");


            if (null == shortcutType)
            {
                sc_node.SetAttribute("Id", shortcut.shortcutId);
            }
            else
            {
                sc_node.SetAttribute("Id", shortcutType + "_" + shortcut.shortcutId);
            }

            if ((null != shortcut.commandArguments) && ("" != shortcut.commandArguments))
                sc_node.SetAttribute("Arguments", shortcut.commandArguments);

            sc_node.SetAttribute("Description", shortcut.description);

            if (null == directoryId)
            {
                sc_node.SetAttribute("Directory", GetDirectoryNode(
                    Path.GetDirectoryName(shortcut.dstPath), null).mXmlNode.GetAttribute("Id"));
            }
            else
            {
                sc_node.SetAttribute("Directory", directoryId);
            }

            sc_node.SetAttribute("Name", Path.GetFileName(shortcut.dstPath));

            if ((null != shortcut.workingDir) && ("" != shortcut.workingDir))
            {
                TargetDir dir = GetDirectoryNode(shortcut.workingDir, null);
                string id = dir.mXmlNode.GetAttribute("Id");
                sc_node.SetAttribute("WorkingDirectory", id);
            }

            XmlElement target_dir = GetDirectoryNode(Path.GetDirectoryName(shortcut.srcPath), null).mXmlNode;
            XmlElement target_node = null;
            string target_name = Path.GetFileName(shortcut.srcPath);

            // Scan the target-dir for a directory with the target-name
            foreach (XmlNode xml_node in target_dir.ChildNodes)
            {
                if ((xml_node.NodeType == XmlNodeType.Element)
                    && (xml_node.Name == "Directory"))
                {
                    XmlElement subdir = (XmlElement)xml_node;
                    string name = subdir.GetAttribute("Name");
                    if (name == target_name)
                    {
                        target_node = subdir;
                        break;
                    }
                }
            }

            // Scan the dir for a file with the target_name
            if (null == target_node)
            {
                foreach (XmlNode xml_node in target_dir.GetElementsByTagName("File"))
                {
                    XmlElement file = (XmlElement)xml_node;
                    string name = file.GetAttribute("Name");
                    if (name == target_name)
                    {
                        target_node = file;
                        break;
                    }
                }
            }

            if (null == target_node)
            {
                throw new ApplicationException(
                    "The shortcut \"" + shortcut.dstPath + "\" in feature \"" +
                    feature.featureName + "\" does not resolve.\r\n" +
                    "I cannot find any file or directory on the target system " +
                    "with the path \"" + shortcut.srcPath + "\".");

            }

            //sc_node.SetAttribute("Target", "[" + target_node.GetAttribute("Id") + "]");
            target_node.AppendChild(sc_node);

            return sc_node;
        }

        private void GenerateFeature(List<SetupFeature> features, XmlElement parent)
        {
            foreach (SetupFeature feature in features)
                GenerateFeature(feature, parent);
        }

        int GetMedia(string srcPath)
        {
            FileInfo fi = new FileInfo(srcPath);
            return GetMedia(fi.Length);
        }

        int GetMedia(long willAddSize)
        {
            if (isMergeModle)
                return 0;

            bool need_new = (0 == current_media_id);
            current_media_size += willAddSize;

            if ((projectProperties.MaxCabSize > 0)
                && (projectProperties.MaxCabSize <= current_media_size))
            {
                need_new = true;
            }

            if (need_new)
            {
                current_media_id++;
                current_media_size = 0;

                XmlElement media = doc.CreateElement("Media");

                product.InsertAfter(media, (null == curr_media_node) ? package : curr_media_node);
                //product.AppendChild(media);
                media.SetAttribute("Id", current_media_id.ToString());
                media.SetAttribute("Cabinet", "Setup_" + current_media_id.ToString() + ".cab");
                media.SetAttribute("EmbedCab", projectProperties.EmbedCab ? "yes" : "no");
                media.SetAttribute("DiskPrompt", "CD-ROM #" + current_media_id.ToString());
                media.SetAttribute("CompressionLevel", projectProperties.CabCompressionLevel.ToString());
                curr_media_node = media;
            }

            return current_media_id;
        }

        private XmlElement GetBinDll(string name)
        {
            // Return the existing element if it exists.
            XmlElement bin = null;
            string file_name = Path.GetFileName(name);
            if (product_binaries.TryGetValue(file_name, out bin))
                return bin;

            bin = doc.CreateElement("Binary");
            bin.SetAttribute("Id", Path.GetFileNameWithoutExtension(name));
            bin.SetAttribute("SourceFile", name);
            product.AppendChild(bin);
            product_binaries.Add(file_name, bin);
            return bin;
        }

        private void SetPropertyValue(string name, string value)
        {
            SetPropertyValue(name, value, null);
        }

        private void SetPropertyValue(string name, string value, string after)
        {
            //string id = "WarSetup_Set" + name + "Value";
            string id = GetUniqueId().ToUpper();
            XmlElement property = doc.CreateElement("CustomAction");
            property.SetAttribute("Id", id);
            property.SetAttribute("Property", name);
            property.SetAttribute("Value", value);
            property.SetAttribute("HideTarget", "no");
            product.AppendChild(property);
            AddExecuteSequence(id, null, after);
            AddUiSequence(id, null, after);
        }


        private void AddExecuteSequence(string action, string condition)
        {
            AddExecuteSequence(action, condition, null);
        }

        private XmlElement GetInstallExecuteSequenceNode()
        {
            if (null == _install_execute_sequence)
            {
                _install_execute_sequence = doc.CreateElement("InstallExecuteSequence");
                product.AppendChild(_install_execute_sequence);
                SetPropertyValue("WarSetup_PRODUCT", projectUuid);
            }

            return _install_execute_sequence;
        }

        private XmlElement GetInstallUISequenceNode()
        {
            if (null == _install_ui_sequence_sequence)
            {
                _install_ui_sequence_sequence = doc.CreateElement("InstallUISequence");
                product.AppendChild(_install_ui_sequence_sequence);
            }

            return _install_ui_sequence_sequence;
        }

        private void AddExecuteSequence(string action, string condition, string after)
        {


            XmlElement custom = doc.CreateElement("Custom");
            custom.SetAttribute("Action", action);

            if (null == after)
                after = "CostFinalize";
            {
                string my_after = after; //GetPrev(after, action);
                custom.SetAttribute("After", my_after);
            }

            if ((null != condition) && ("" != condition))
            {
                custom.AppendChild(doc.CreateTextNode(condition));
            }

            GetInstallExecuteSequenceNode().AppendChild(custom);
        }

        private void AddUiSequence(string action, string condition, string after)
        {
            XmlElement custom = doc.CreateElement("Custom");
            custom.SetAttribute("Action", action);

            if (null == after)
                after = "CostFinalize";

            custom.SetAttribute("After", after);

            if ((null != condition) && ("" != condition))
                custom.AppendChild(doc.CreateTextNode(condition));

            GetInstallUISequenceNode().AppendChild(custom);
        }

        private string MakeVcFileCond(string path, string id)
        {
            //XmlElement dir = GetDirectoryNode(Path.GetDirectoryName(path), null);
            XmlElement property = null;
            string prop_id = id;
            if (null == prop_id)
                prop_id = "FILEEXISTS_" + SetupDirectory.GetMd5Hash(path).ToUpper();

            // Just check to see if we already have the property. If we do, return
            // the key. We can't add it twice.
            if (vc_file_conditions.TryGetValue(prop_id, out property))
                return prop_id;

            property = doc.CreateElement("Property");
            property.SetAttribute("Id", prop_id);
            XmlElement dir_src = doc.CreateElement("DirectorySearch");
            dir_src.SetAttribute("Id", "ChkFileDir" + prop_id);
            dir_src.SetAttribute("Path", Path.GetDirectoryName(path));
            property.AppendChild(dir_src);
            XmlElement file_src = doc.CreateElement("FileSearch");
            file_src.SetAttribute("Id", "FileSrc_" + prop_id);
            file_src.SetAttribute("Name", Path.GetFileName(path));
            dir_src.AppendChild(file_src);

            product.AppendChild(property);
            vc_file_conditions.Add(prop_id, property);
            return prop_id;
        }

        private void MakeVcIntegration(string ideName, SetupDirectory dir, SetupFeature feature, XmlElement featureNode)
        {
            string ide_version = "";
            string feature_option = "";
            switch (ideName)
            {
                case "Microsoft Visual Studio 2003":
                    ide_version = "7.1";
                    feature_option = projectProperties.VS2003Integration;
                    break;
                case "Microsoft Visual Studio 2005":
                    ide_version = "8.0";
                    feature_option = projectProperties.VS2005Integration;
                    break;
            }


            // Must modify C:\Documents and Settings\jgaa\Local Settings\Application Data\Microsoft\VisualStudio\7.1\VCComponents.dat
#if DEBUG
            XmlElement bin = GetBinDll(Path.Combine(Path.Combine(Shell.warSetupResPath, "CustomActions"), "WarSetupPluginD.dll"));
#else
            XmlElement bin = GetBinDll(Path.Combine(Path.Combine(Shell.warSetupResPath, "CustomActions"), "WarSetupPlugin.dll"));
#endif

            string ini_path = @"[LocalAppDataFolder]Microsoft\VisualStudio\" + ide_version + @"\VCComponents.dat";

            SetPropertyValue("VCCOMPONENTSFILEPATH", ini_path);

            string prop_key = MakeVcFileCond(ini_path, null);

            SetPropertyValue("VC_VERSION", ideName);

            string feature_cond_install = "";
            if ((null != feature_option) && ("" != feature_option))
            {
                // make a rule that only activates the integration i fa feature with Id feature_option is enabled
                feature_cond_install = " AND (&" + feature_option + "=3) ";
                //feature_cond_uninstall = " OR (&feature_option=2) ";
            }

            // TODO: Add CustomAction to abort installation if the feature is enabled and
            // the VCComponents.dat file is missing.
            {
                XmlElement abort = doc.CreateElement("CustomAction");
                abort.SetAttribute("Id", GetUniqueId().ToUpper());
                abort.SetAttribute("Error", "The required file " + ini_path
                    + " is missing. "
                    + ideName + " must be installed, and you must have entered the "
                    + "Tools/Options menu and selected Projects/C++ Directories "
                    + "at least once (That's when this file is created).");
                product.AppendChild(abort);

                AddExecuteSequence(abort.GetAttribute("Id"), "(NOT " + prop_key
                    + ") AND (&" + featureNode.GetAttribute("Id") + "=3)"
                    + feature_cond_install);
            }

            if (dir.ideExecutables)
                MakeVcIntegrationPath(bin, "VCCOMPONENTSFILEEXECDIR", dir.dstPath, featureNode, prop_key, feature_option);

            if (dir.ideHeaderFiles)
                MakeVcIntegrationPath(bin, "VCCOMPONENTSFILEHEADERDIR", dir.dstPath, featureNode, prop_key, feature_option);

            if (dir.ideLibrary)
                MakeVcIntegrationPath(bin, "VCCOMPONENTSFILELIBDIR", dir.dstPath, featureNode, prop_key, feature_option);
        }

        private void MakeVcIntegrationPath(XmlElement bin, string name, string path, XmlElement featureNode, string propKey, string integrationFeature)
        {
            SetPropertyValue(name, "[" + GetDirectoryNode(path, null).mXmlNode.GetAttribute("Id") + "]");

            // Install action
            {

                XmlElement action = doc.CreateElement("CustomAction");
                action.SetAttribute("Id", GetUniqueId().ToUpper());
                action.SetAttribute("BinaryKey", bin.GetAttribute("Id"));
                action.SetAttribute("DllEntry", "VCComponentsFileAdd");
                product.AppendChild(action);
                string cond = "(&" + featureNode.GetAttribute("Id") + "=3) AND " + propKey;
                if ((null != integrationFeature) && ("" != integrationFeature))
                {
                    cond = "(" + cond + " AND (&" + integrationFeature + "=3))"
                    + " OR ((!" + featureNode.GetAttribute("Id") + "=3) AND (&" + integrationFeature + "=3))";
                }
                AddExecuteSequence(action.GetAttribute("Id"), cond);
            }

            // Uninstall action
            {
                XmlElement action = doc.CreateElement("CustomAction");
                action.SetAttribute("Id", GetUniqueId().ToUpper());
                action.SetAttribute("BinaryKey", bin.GetAttribute("Id"));
                action.SetAttribute("DllEntry", "VCComponentsFileRemove");
                product.AppendChild(action);
                string cond = "(&" + featureNode.GetAttribute("Id") + "=2) AND " + propKey;
                //if ((null != integrationFeature) && ("" != integrationFeature))
                //{
                //    // Make a condition that uninstall the integration if the integration
                //    // feature is removed but the library remains installed.
                //    cond = "(" + cond + " AND (!" + integrationFeature + "=3))"
                //    + " OR ((!" + featureNode.GetAttribute("Id") + "=3) AND (&" + integrationFeature + "=2))";
                //}
                AddExecuteSequence(action.GetAttribute("Id"), cond);
            }
        }

        private void AddProperty(string name, string value)
        {
            XmlElement prop = doc.CreateElement("Property");
            prop.SetAttribute("Id", name);
            prop.SetAttribute("Value", value);
            product.AppendChild(prop);
        }

        private void AddIncludePathForLight(string path)
        {
            if (!_includePathsForLight.Contains(path))
                _includePathsForLight.Add(path);
        }

        private void AddWixVariable(string name, string value)
        {
            XmlElement var = doc.CreateElement("WixVariable");
            var.SetAttribute("Id", name);
            var.SetAttribute("Value", value);
            product.AppendChild(var);
        }

        private void AddDeleteFolder(string name, XmlElement parent)
        {
            XmlElement comp = CreateComponent(parent, true, null);

            XmlElement rmf = doc.CreateElement("RemoveFolder");
            rmf.SetAttribute("Id", GetUniqueId());
            rmf.SetAttribute("On", "uninstall");
            comp.AppendChild(rmf);

            AddDeleteFolders.Add(comp.GetAttribute("Id"));
        }


        private bool CompileWixModuleIfRequired(WixModule wm)
        {
            switch (wm.Compile)
            {
                case WixModule.CompileModeE.Auto:
                    if (Path.GetExtension(wm.Path) != ".wxs")
                        return true; // Source is not the source-file
                    if (File.GetLastWriteTime(wm.SrcPath) <= File.GetLastWriteTime(wm.DstPath))
                        return true; // No need to compile
                    break;
                case WixModule.CompileModeE.Yes:
                    break;
                case WixModule.CompileModeE.No:
                    return true;
            }

            return CompileWixModule(wm);
        }



        public bool CompileWixModule(WixModule wm)
        {
            string cmd = "";

            if (!File.Exists(wm.SrcPath))
            {
                MessageBox.Show("The file \"" + wm.SrcPath + "\" does not exist!");
                return false;
            }

            try
            {
                List<string> args = new List<string>();
                cmd = Shell.GetWixBinary("candle.exe");
                args.Add(wm.SrcPath);

                if (!isMergeModle)
                {
                    args.Add("-ext");
                    args.Add(Shell.GetWixBinary("WixUIExtension.dll"));
                }
                args.Add("-out");
                args.Add(wm.DstPath);
                if (!Shell.Execute(cmd, args))
                {
                    MessageBox.Show("Compilation failed. See the Output tab for details.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("\"" + cmd + "\"\r\nFailed with an exception: \r\n" + ex.ToString());
                return false;
            }

            return true;
        }

        private void CreateUninstallShortcut()
        {
            Guid uninstallShortcutGuid = Guid.NewGuid();

            XmlElement feature_node = doc.CreateElement("Feature");
            product.AppendChild(feature_node);
            feature_node.SetAttribute("Id", "UninstallShortcut");
            feature_node.SetAttribute("Title", "Create UninstallShortcut");
            feature_node.SetAttribute("Level", "1");
            feature_node.SetAttribute("Description", "Create uninstallshortcut in Start Menu");
            feature_node.SetAttribute("InstallDefault", "local");

            // Make the reference from Feature
            XmlElement component_ref = doc.CreateElement("ComponentRef");
            component_ref.SetAttribute("Id", "UninstallShortcutComponent");
            feature_node.AppendChild(component_ref);

            XmlElement comp = doc.CreateElement("Component");
            my_program_menu.AppendChild(comp);
            comp.SetAttribute("Id", "UninstallShortcutComponent");
            comp.SetAttribute("Guid", uninstallShortcutGuid.ToString());

            XmlElement regnode = doc.CreateElement("RegistryValue");
            comp.AppendChild(regnode);
            regnode.SetAttribute("Root", "HKCU");
            regnode.SetAttribute("Key", @"SOFTWARE\"
                + projectOrganization
                + @"\" + projectName
                + @"\Uninstall");
            regnode.SetAttribute("Type", "string");
            regnode.SetAttribute("KeyPath", "yes");
            regnode.SetAttribute("Value", "");

            XmlElement shortcut = doc.CreateElement("Shortcut");
            comp.AppendChild(shortcut);
            shortcut.SetAttribute("Id", "UninstallProduct");
            shortcut.SetAttribute("Name", "Uninstall " + projectName);
            shortcut.SetAttribute("Target", "[System64Folder]msiexec.exe");
            shortcut.SetAttribute("Arguments", "/X{" + projectUuid + "}");
            shortcut.SetAttribute("Directory", "ProgramMenuDir");
            shortcut.SetAttribute("Description", "Uninstall " + projectName);

            XmlElement remdir = doc.CreateElement("RemoveFolder");
            comp.AppendChild(remdir);
            remdir.SetAttribute("Id", "RemoveShortcutFolder");
            remdir.SetAttribute("On", "uninstall");
        }


        #endregion // Support-methods

        private void DoBuildTarget()
        {
            _compileDlg.WaitForSafeAccess();

            string ok_message = "Compilation of object-module completed OK";
            DateTime start_time = DateTime.Now;

            currentTargetDirs = new Dictionary<string, TargetDir>();
            currentTargetDirIdList = new Dictionary<string, TargetDir>();
            //compilerOutput.Text = "";
            product_binaries = new Dictionary<string, XmlElement>();
            //_lastActionId = null;
            _install_execute_sequence = null;
            _install_ui_sequence_sequence = null;
            vc_file_conditions = new Dictionary<string, XmlElement>();
            _includePathsForLight = new List<string>();
            current_media_size = 0;
            current_media_id = 0;
            curr_media_node = null;
            NodeContainer = new List<NodeCont>();
            AddDeleteFolders = new List<string>();

            if (projectProperties.AlwaysMajorUpgrade)
            {
                MakeNewGuid();
            }

            try
            {
                // Create a DOM
                doc = new XmlDocument();
                //XmlDocumentType doc_type = doc.CreateDocumentType("Wix", null, null, null);
                //doc.AppendChild(doc_type);
                SetupFile main_target_app = ResolveMainFeature(true);
                bool do_lauch_main_program = (projectRunAfterInstall && (main_target_app != null));

                _compileDlg.SetInfo("Generating .wix file", (int)ProgressE.GEN_WIX);

                XmlElement wix = doc.CreateElement("Wix");
                wix.SetAttribute("xmlns", "http://schemas.microsoft.com/wix/2006/wi");
                doc.AppendChild(wix);
                // Identify WarSetup
                {
                    XmlComment comment = doc.CreateComment("This file was generated by WarSetup version "
                        + Assembly.GetExecutingAssembly().GetName().Version.ToString()
                        + "\r\n\tSee http://warsetup.jgaa.com for information about WarSetup.");
                    wix.AppendChild(comment);
                }
                // Populate the namespace

                // Populate the Product node
                if (isMergeModle)
                {
                    product = doc.CreateElement("Module");
                    product.SetAttribute("Id", id);
                }
                else
                {
                    product = doc.CreateElement("Product");
                    product.SetAttribute("Id", projectUuid);
                    product.SetAttribute("Name", projectName);
                    product.SetAttribute("Manufacturer", projectOrganization);
                    product.SetAttribute("UpgradeCode", projectUpgradeUuid);

                }
                wix.AppendChild(product);

                if (0 != projectProperties.ProductLanguage)
                    product.SetAttribute("Language", projectProperties.ProductLanguage.ToString());
                if (0 != projectProperties.Codepage)
                    product.SetAttribute("Codepage", projectProperties.Codepage.ToString());
                product.SetAttribute("Version", projectVersion);


                // Populate the Package node
                package = doc.CreateElement("Package");
                product.AppendChild(package);
                package.SetAttribute("Description", projectName);
                package.SetAttribute("Manufacturer", projectOrganization);

                package.SetAttribute("SummaryCodepage", projectProperties.Codepage.ToString());
                package.SetAttribute("Languages", projectProperties.ProductLanguage.ToString());
                package.SetAttribute("InstallerVersion", projectProperties.InstallerVersion.ToString());
                package.SetAttribute("Keywords", "Installer");
                if (isMergeModle)
                    package.SetAttribute("Id", projectUuid);
                if (!isMergeModle)
                    package.SetAttribute("Compressed", (projectProperties.Compress ? "yes" : "no"));


                // Add property
                {
                    XmlElement property = doc.CreateElement("Property");
                    product.AppendChild(property);
                    property.SetAttribute("Id", "DiskPrompt");
                    property.SetAttribute("Value", projectName + " Installation [1]");
                }

                // Upgrade
                if (!isMergeModle)
                {
                    upgrade = doc.CreateElement("Upgrade");
                    upgrade.SetAttribute("Id", projectUpgradeUuid);
                    product.AppendChild(upgrade);

                    XmlElement version = doc.CreateElement("UpgradeVersion");
                    version.SetAttribute("OnlyDetect", "no");
                    version.SetAttribute("Property", "WARSETUP_PREVIOUSVERSIONFOUND");
                    version.SetAttribute("IncludeMinimum", "yes");
                    version.SetAttribute("Minimum", "0.0.0");
                    version.SetAttribute("Maximum", GetMaxVersion());
                    version.SetAttribute("IncludeMaximum", "yes");
                    upgrade.AppendChild(version);

                    //{
                    //    XmlElement ies = GetInstallExecuteSequenceNode();
                    //    XmlElement remove = doc.CreateElement("RemoveExistingProducts");
                    //    remove.SetAttribute("After", GetPrev("InstallValidate", "RemoveExistingProducts"));
                    //    ies.AppendChild(remove);
                    //}
                }



                if (!isMergeModle)
                {
                    XmlElement ies = GetInstallExecuteSequenceNode();
                    XmlElement aes = doc.CreateElement("AdvertiseExecuteSequence");
                    product.AppendChild(aes);

                    AddIes(ies, "LaunchConditions", 100, "NOT Installed");
                    AddIes(ies, "FindRelatedProducts", 200, null);
                    AddIes(ies, "AppSearch", 300, null);
                    AddIes(ies, "CCPSearch", 400, "NOT Installed");
                    AddIes(ies, "RMCCPSearch", 600, "NOT Installed AND CPP_TEST");
                    AddIes(ies, "ValidateProductID", 700, null);
                    AddIes(ies, "CostInitialize", 800, null);
                    AddIes(aes, "CostInitialize", 800, null);
                    AddIes(ies, "FileCost", 900, null);
                    AddIes(ies, "IsolateComponents", 990, "RedirectedDllSupport");
                    AddIes(ies, "CostFinalize", 1000, null);
                    AddIes(aes, "CostFinalize", 1000, null);
                    AddIes(ies, "SetODBCFolders", 1100, "NOT Installed");
                    AddIes(ies, "MigrateFeatureStates", 1200, null);
                    AddIes(ies, "InstallValidate", 1400, null);
                    AddIes(aes, "InstallValidate", 1400, null);
                    //AddIes(ies, "RemoveExistingProducts", seq++, "WARSETUP_PREVIOUSVERSIONFOUND"); 
                    AddIes(ies, "RemoveExistingProducts", 1480, "WARSETUP_PREVIOUSVERSIONFOUND"); // prev. null
                    AddIes(ies, "InstallInitialize", 1500, null);
                    AddIes(aes, "InstallInitialize", 1500, null);
                    //AddIes(ies, "AllocateRegistrySpace", 1580, "NOT Installed");
                    AddIes(ies, "ProcessComponents", 1600, null);
                    AddIes(ies, "UnpublishComponents", 1680, null);
                    AddIes(ies, "UnpublishFeatures", 1800, null);
                    AddIes(ies, "StopServices", 1900, "VersionNT");
                    AddIes(ies, "DeleteServices", 1980, "VersionNT");
                    AddIes(ies, "UnregisterComPlus", 2000, null);
                    AddIes(ies, "SelfUnregModules", 2100, null);
                    AddIes(ies, "UnregisterTypeLibraries", 2200, null);
                    AddIes(ies, "RemoveODBC", 2300, null);
                    AddIes(ies, "UnregisterFonts", 2400, null);
                    AddIes(ies, "RemoveRegistryValues", 2600, null);
                    AddIes(ies, "UnregisterClassInfo", 2700, null);
                    AddIes(ies, "UnregisterExtensionInfo", 2800, null);
                    AddIes(ies, "UnregisterProgIdInfo", 2900, null);
                    AddIes(ies, "UnregisterMIMEInfo", 3000, null);
                    AddIes(ies, "RemoveIniValues", 3100, null);
                    AddIes(ies, "RemoveShortcuts", 3200, null);
                    AddIes(ies, "RemoveEnvironmentStrings", 3300, null);
                    AddIes(ies, "RemoveDuplicateFiles", 3400, null);
                    AddIes(ies, "RemoveFiles", 3500, null);
                    AddIes(ies, "RemoveFolders", 3600, null);
                    AddIes(ies, "CreateFolders", 3700, null);
                    AddIes(ies, "MoveFiles", 3800, null);
                    AddIes(ies, "InstallFiles", 4000, null);
                    AddIes(ies, "DuplicateFiles", 4100, null);
                    AddIes(ies, "PatchFiles", 4200, null);
                    AddIes(ies, "BindImage", 4300, null);
                    AddIes(ies, "CreateShortcuts", 4500, null);
                    AddIes(aes, "CreateShortcuts", 4500, null);
                    AddIes(ies, "RegisterClassInfo", 4600, null);
                    AddIes(aes, "RegisterClassInfo", 4600, null);
                    AddIes(ies, "RegisterExtensionInfo", 4700, null);
                    AddIes(aes, "RegisterExtensionInfo", 4700, null);
                    AddIes(ies, "RegisterProgIdInfo", 4800, null);
                    AddIes(aes, "RegisterProgIdInfo", 4800, null);
                    AddIes(ies, "RegisterMIMEInfo", 4900, null);
                    AddIes(aes, "RegisterMIMEInfo", 4900, null);
                    AddIes(ies, "WriteRegistryValues", 5000, null);
                    AddIes(ies, "WriteIniValues", 5100, null);
                    AddIes(ies, "WriteEnvironmentStrings", 5200, null);
                    AddIes(ies, "RegisterFonts", 5300, null);
                    AddIes(ies, "InstallODBC", 5400, null);
                    AddIes(ies, "RegisterTypeLibraries", 5500, null);
                    AddIes(ies, "SelfRegModules", 5600, null);
                    AddIes(ies, "RegisterComPlus", 5700, null);
                    AddIes(ies, "InstallServices", 5800, "VersionNT");
                    AddIes(ies, "StartServices", 5900, "VersionNT");
                    AddIes(ies, "RegisterUser", 6000, null);
                    AddIes(ies, "RegisterProduct", 6100, null);
                    AddIes(ies, "PublishComponents", 6200, null);
                    AddIes(aes, "PublishComponents", 6200, null);
                    AddIes(ies, "PublishFeatures", 6300, null);
                    AddIes(aes, "PublishFeatures", 6300, null);
                    AddIes(ies, "PublishProduct", 6400, null);
                    AddIes(aes, "PublishProduct", 6400, null);
                    AddIes(ies, "MsiPublishAssemblies", 6500, null);
                    AddIes(aes, "MsiPublishAssemblies", 6500, null);
                    AddIes(ies, "MsiUnpublishAssemblies", 6550, null);
                    AddIes(ies, "InstallFinalize", 6600, null);
                    AddIes(aes, "InstallFinalize", 6600, null);
                }

                // Compitability with MS merge-modules
                if (!isMergeModle)
                {
                    SetPropertyValue("TARGETDIR", "[APPLICATIONFOLDER]", "FileCost");
                }

                // Shortcut locations

                desktop = doc.CreateElement("Directory");
                desktop.SetAttribute("Id", "DesktopFolder");
                desktop.SetAttribute("Name", "Desktop");
                AddDeleteFolder("DesktopFolder", desktop);

                program_menu = doc.CreateElement("Directory");
                program_menu.SetAttribute("Id", "ProgramMenuFolder");
                SetNameAttribute(program_menu, "Programs");
                AddDeleteFolder("ProgramMenuFolder", program_menu);

                my_program_menu = doc.CreateElement("Directory");
                my_program_menu.SetAttribute("Id", "ProgramMenuDir");
                if (overRideMenuShortcutDir)
                {
                    SetNameAttribute(my_program_menu, menuShortcutDirName);
                }
                else
                {
                    SetNameAttribute(my_program_menu, projectName);
                }
                program_menu.AppendChild(my_program_menu);
                AddDeleteFolder("ProgramMenuDir", my_program_menu);

                my_start_menu = doc.CreateElement("Directory");
                my_start_menu.SetAttribute("Id", "StartupFolder");
                SetNameAttribute(my_start_menu, "StartupFolder Directory");
                program_menu.AppendChild(my_start_menu);
                AddDeleteFolder("StartupFolder", my_start_menu);


                {
                    appdata = doc.CreateElement("Directory");
                    appdata.SetAttribute("Id", "AppDataFolder");
                    appdata.SetAttribute("Name", "AppData");
                    AddDeleteFolder("AppDataFolder", appdata);

                    XmlElement ms = doc.CreateElement("Directory");
                    ms.SetAttribute("Id", "Microsoft");
                    SetNameAttribute(ms, "Microsoft");
                    appdata.AppendChild(ms);
                    AddDeleteFolder("Microsoft", ms);

                    XmlElement ie = doc.CreateElement("Directory");
                    ie.SetAttribute("Id", "AppMsInternetExplorer");
                    SetNameAttribute(ie, "Internet Explorer");
                    ms.AppendChild(ie);
                    AddDeleteFolder("AppMsInternetExplorer", ie);

                    quicklaunch = doc.CreateElement("Directory");
                    quicklaunch.SetAttribute("Id", "QuickLaunchFolder");
                    SetNameAttribute(quicklaunch, "Quick Launch");
                    ie.AppendChild(quicklaunch);
                    AddDeleteFolder("QuickLaunchFolder", quicklaunch);
                }

                // Target dir for the product
                root_directory = doc.CreateElement("Directory");
                product.AppendChild(root_directory);
                root_directory.SetAttribute("Id", "TARGETDIR");
                root_directory.SetAttribute("Name", "SourceDir");
                root_directory.AppendChild(program_menu);
                root_directory.AppendChild(appdata);
                root_directory.AppendChild(desktop);

                {
                    TargetDir target = new TargetDir("", root_directory);
                    //currentTargetDirs.Add("", target);
                    currentTargetDirIdList.Add(root_directory.GetAttribute("Id"), target);
                }


                // ProgramFiles folder
                pf_directory_id = project64BitTarget
                    ? "ProgramFiles64Folder"
                    : "ProgramFilesFolder";

                XmlElement pf_directory = doc.CreateElement("Directory");
                root_directory.AppendChild(pf_directory);
                pf_directory.SetAttribute("Id", pf_directory_id);
                pf_directory.SetAttribute("Name", "Program Files Folder");
                {
                    //currentTargetDirs.Add("/Program Files Folder", new TargetDir("Program Files Folder", pf_directory));
                    TargetDir target = new TargetDir("/Program Files Folder", pf_directory);
                    currentTargetDirIdList.Add(pf_directory_id, target);
                }

                // Organization folder
                XmlElement org_directory = pf_directory;
                if (projectOrganization.Length > 0)
                {
                    org_directory = doc.CreateElement("Directory");
                    pf_directory.AppendChild(org_directory);
                    org_directory.SetAttribute("Id", "WarInstallOrganizationFolder");

                    {
                        string key = "/Program Files Folder/" + projectOrganization;
                        TargetDir target = new TargetDir("", pf_directory);
                        //currentTargetDirs.Add(key, target);
                        currentTargetDirIdList.Add(org_directory.GetAttribute("Id"), target);
                    }

                    org_directory.SetAttribute("Name", projectOrganization);
                }

                // Create a default APPLICATIONFOLDER
                {
                    installdir_directory = doc.CreateElement("Directory");
                    installdir_directory.SetAttribute("Id", "APPLICATIONFOLDER");
                    installdir_directory.SetAttribute("Name", "[ApplicationFolderName]");
                    string installdir = "[" + "WarInstallOrganizationFolder" + "]/" + projectName;
                    //SetNameAttribute(installdir_directory, installdir);
                    TargetDir target = new TargetDir("", installdir_directory);
                    currentTargetDirs.Add("", target);
                    currentTargetDirIdList.Add("APPLICATIONFOLDER", target);
                    org_directory.AppendChild(installdir_directory);
                }


                FixupInstallDir(projectFeatures);

                AddProperty("WixAppFolder", "WixPerUserFolder");

                // Add user-interface and licence override
                if (!isMergeModle && ("" != License.Name))
                {
                    if (License.System)
                    {
                        foreach (string path in LicencePaths)
                            AddIncludePathForLight(path);
                    }

                    AddWixVariable("WixUILicenseRtf", License.Name + ".rtf");
                }

                // Graphics apperance
                {
                    ProjectProperties.VariablePair[] pairs = projectProperties.GetVariables();
                    foreach (ProjectProperties.VariablePair var in pairs)
                    {
                        string path = Path.GetDirectoryName(var.mValue);
                        string filename = Path.GetFileName(var.mValue);

                        if ((null != path) && ("" != path))
                            AddIncludePathForLight(path);

                        AddWixVariable(var.mName, filename);
                    }
                }

                // Add features and components
                GenerateFeature(projectFeatures, null);

                if (!isMergeModle && projectProperties.CreateUninstallShortcut)
                    CreateUninstallShortcut();

                // Add UI

                if (!isMergeModle)
                {
                    XmlElement ui = doc.CreateElement("UI");
                    product.AppendChild(ui);

                    if ((null != projectUserInterface)
                    && ("" != projectUserInterface))
                    {
                        XmlElement iface = doc.CreateElement("UIRef");
                        ui.AppendChild(iface);
                        iface.SetAttribute("Id", projectUserInterface);
                    }

                    if (do_lauch_main_program)
                    {
                        XmlElement publish = doc.CreateElement("Publish");
                        ui.AppendChild(publish);
                        publish.SetAttribute("Dialog", "ExitDialog");
                        publish.SetAttribute("Control", "Finish");
                        publish.SetAttribute("Event", "DoAction");
                        publish.SetAttribute("Value", "LaunchApplication");
                        publish.AppendChild(doc.CreateTextNode("WIXUI_EXITDIALOGOPTIONALCHECKBOX = 1 and NOT Installed"));
                    }

                    if (projectProperties.UseWixUI_ErrorProgressText)
                    {
                        XmlElement errmsg = doc.CreateElement("UIRef");
                        ui.AppendChild(errmsg);
                        errmsg.SetAttribute("Id", "WixUI_ErrorProgressText");
                    }
                }

                // Add launch
                if (do_lauch_main_program)
                {
                    XmlElement property = doc.CreateElement("Property");
                    product.AppendChild(property);
                    property.SetAttribute("Id", "WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT");

                    if (projectProperties.LaunchAppText != "")
                        property.SetAttribute("Value", projectProperties.LaunchAppText);
                    else
                        property.SetAttribute("Value", "Launch " + projectName);

                    XmlElement property2 = doc.CreateElement("Property");
                    product.AppendChild(property2);
                    property2.SetAttribute("Id", "WixShellExecTarget");
                    property2.SetAttribute("Value", "[#" + main_target_app.fileId + "]");

                    XmlElement ca = doc.CreateElement("CustomAction");
                    product.AppendChild(ca);
                    ca.SetAttribute("Id", "LaunchApplication");
                    ca.SetAttribute("BinaryKey", "WixCA");
                    ca.SetAttribute("DllEntry", "WixShellExec");
                    ca.SetAttribute("Impersonate", "yes");
                }

                // Conditions
                if (projectMustBeAdministratorToInstall)
                    AddCondition("You must not be an administrator to install this product.", "Privileged");

                {
                    XmlElement property = doc.CreateElement("Property");
                    product.AppendChild(property);
                    property.SetAttribute("Id", "ALLUSERS");
                    property.SetAttribute("Value", projectInstallForCurrentOrAllUsers.ToString());
                }

                if (project64BitTarget)
                    AddCondition("This product can only run on native 64 bit hardware", "VersionNT64");

                if (project64BitTarget)
                    AddCondition("This product can only run on native 64 bit hardware", "VersionNT64");

                // See http://msdn.microsoft.com/en-us/library/windows/desktop/aa370556(v=vs.85).aspx for a 
                // complete list of windows versions
                switch (projectFromWindowsVersion)
                {
                    case "Windows 2000":
                        AddCondition("This product require Windows 2000 or newer", "VersionNT >= 400");
                        break;
                    case "Windows XP":
                        AddCondition("This product require Windows XP or newer", "VersionNT >= 501");
                        break;
                    case "Windows XP SP 2":
                        AddCondition("This product require Windows XP SP 2 or newer",
                            "(VersionNT = 501 AND ServicePackLevel >= 2) OR VersionNT > 501");
                        break;
                    case "Windows XP SP 3":
                        AddCondition("This product require Windows XP SP 3 or newer",
                            "(VersionNT = 501 AND ServicePackLevel >= 3) OR VersionNT > 501");
                        break;
                    case "Windows 2003":
                        AddCondition("This product require Windows 2003 or newer", "VersionNT >= 502");
                        break;
                    case "Windows 2003 SP 1":
                        AddCondition("This product require Windows XP or newer",
                            "(VersionNT = 502 AND ServicePackLevel >= 1) OR VersionNT > 502");
                        break;
                    case "Windows Vista":
                        AddCondition("This product require Windows Vista or newer", "VersionNT >= 600");
                        break;
                    case "Windows Vista SP 1":
                        AddCondition("This product require Windows Vista SP 1 or newer",
                            "(VersionNT >= 600 AND ServicePackLevel >= 1) OR VersionNT > 600");
                        break;
                    case "Windows Vista SP 2":
                        AddCondition("This product require Windows Vista SP 2 or newer",
                            "(VersionNT >= 600 AND ServicePackLevel >= 2) OR VersionNT > 600");
                        break;
                    case "Windows 7":
                        AddCondition("This product require Windows 7 or newer", "VersionNT >= 601");
                        break;
                    case "Windows 7 SP 1":
                        AddCondition("This product require Windows 7 SP 1 or newer",
                            "(VersionNT = 601 AND ServicePackLevel >= 1) OR VersionNT > 601");
                        break;
                    case "Windows 8":
                        AddCondition("This product require Windows 8 or newer", "VersionNT >= 602");
                        break;
                }

                // .NET 4.0 FULL
                // .NET 4.0 CLIENT
                switch (projectRequireDotNetVersion)
                {
                    case ".NET 2.0":
                        AddPropertyRef("NETFRAMEWORK20");
                        AddCondition("This product requires the .NET Framework 2.0 to be installed", "Installed OR NETFRAMEWORK20");
                        break;
                    case ".NET 3.0":
                        AddPropertyRef("NETFRAMEWORK30");
                        AddCondition("This product requires the .NET Framework 3.0 to be installed", "Installed OR NETFRAMEWORK30");
                        break;
                    case ".NET 3.5":
                        AddPropertyRef("NETFRAMEWORK35");
                        AddCondition("This product requires the .NET Framework 3.5 to be installed", "Installed OR NETFRAMEWORK35");
                        break;
                    case ".NET 4.0 FULL":
                        AddPropertyRef("NETFRAMEWORK40FULL");
                        AddCondition("This product requires the .NET Framework 4.0 Full to be installed", "Installed OR NETFRAMEWORK40FULL");
                        break;
                    case ".NET 4.0 CLIENT":
                        AddPropertyRef("NETFRAMEWORK40CLIENT");
                        AddCondition("This product requires the .NET Framework 4.0 Client to be installed", "Installed OR NETFRAMEWORK40CLIENT");
                        break;
                    case ".NET 4.5":
                        AddPropertyRef("NETFRAMEWORK45");
                        AddCondition("This application requires .NET Framework 4.5. Please install the .NET Framework then run this installer again.", "Installed OR NETFRAMEWORK45");
                        break;
                }

                if (!isMergeModle
                    && (projectMergeModules.Count > 0))
                {
                    // Add Merge Modules
                    XmlElement feature_merge_ref = doc.CreateElement("Feature");
                    feature_merge_ref.SetAttribute("Id", "Msm");
                    feature_merge_ref.SetAttribute("Title", "Msm");
                    feature_merge_ref.SetAttribute("Level", "1");
                    feature_merge_ref.SetAttribute("Display", "hidden");
                    features_root.AppendChild(feature_merge_ref);

                    foreach (MergeModule mm in projectMergeModules)
                    {
                        if (mm.ExcludeFromBuild)
                            continue;

                        XmlElement merge = doc.CreateElement("Merge");
                        merge.SetAttribute("Id", mm.ModuleId);
                        merge.SetAttribute("SourceFile", mm.SrcPath);
                        merge.SetAttribute("DiskId", GetMedia(mm.SrcPath).ToString());
                        merge.SetAttribute("Language", mm.ModuleLanguage);
                        installdir_directory.AppendChild(merge);

                        XmlElement merge_ref = doc.CreateElement("MergeRef");
                        merge_ref.SetAttribute("Id", mm.ModuleId);
                        feature_merge_ref.AppendChild(merge_ref);

                        foreach (MergeModule.Properties prop in mm.ModuleProperties)
                        {
                            //SetPropertyValue(prop.Id, prop.Value, "FileCost");
                            SetPropertyValue(prop.Id, prop.Value, "CostInitialize");
                        }
                    }
                }
                else if (isMergeModle)
                {
                    foreach (MergeModule mm in projectMergeModules)
                    {
                        XmlElement merge = doc.CreateElement("Dependency");
                        merge.SetAttribute("RequiredId", mm.ModuleId);
                        merge.SetAttribute("RequiredLanguage", "0");
                        product.AppendChild(merge);
                    }
                }

                AddProperty("WarSetup_VERSION", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                // Add properties
                if ("" != projectProperties.SupportUrl)
                    SetPropertyValue("ARPHELPLINK", projectProperties.SupportUrl);
                if ("" != projectProperties.SupportPhone)
                    SetPropertyValue("ARPHELPTELEPHONE", projectProperties.SupportPhone);
                if ("" != projectProperties.UpdateUrl)
                    SetPropertyValue("ARPURLUPDATEINFO", projectProperties.UpdateUrl);
                if ("" != projectProperties.Comments)
                    SetPropertyValue("ARPCOMMENTS", projectProperties.Comments);
                if ("" != projectProperties.Icon)
                    SetPropertyValue("ARPPRODUCTICON", projectProperties.Icon);
                if ("" != projectProperties.ReadMeFile)
                    SetPropertyValue("ARPREADME", projectProperties.ReadMeFile);
                if ("" != projectProperties.PublisherUrl)
                    SetPropertyValue("ARPURLINFOABOUT", projectProperties.PublisherUrl);
                if ("" != projectProperties.SupportContact)
                    SetPropertyValue("ARPCONTACT", projectProperties.SupportContact);

                // add AddDeleteFolders
                {
                    foreach (string id in AddDeleteFolders)
                    {
                        XmlElement compref = doc.CreateElement("ComponentRef");
                        compref.SetAttribute("Id", id);
                        features_root.AppendChild(compref);
                    }
                }

                // Create object-file
                _compileDlg.SetInfo("Compiling wix object modules", (int)ProgressE.COMPILE_OBJ_MODULES);
                foreach (WixModule wm in projectWixModules)
                {
                    if (!CompileWixModuleIfRequired(wm))
                        return;

                    string ref_type = null;
                    switch (wm.ReferenceType)
                    {
                        case WixModule.RefModeE.Feature:
                            ref_type = "FeatureRef";
                            break;
                        case WixModule.RefModeE.CustomAction:
                            ref_type = "CustomActionRef";
                            break;
                        case WixModule.RefModeE.Property:
                            ref_type = "PropertyRef";
                            break;
                        case WixModule.RefModeE.UI:
                            ref_type = "UIRef";
                            break;
                    }

                    if (null != ref_type)
                    {
                        XmlElement fref = doc.CreateElement(ref_type);
                        fref.SetAttribute("Id", wm.ReferenceId);
                        product.AppendChild(fref);
                    }
                }

                // Save to disk
                string target_path = TargetPathNoExt + ".wxs";

                XmlTextWriter writer = new XmlTextWriter(target_path, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
                writer.Close();
                doc = null;

                {
                    TimeSpan duration = DateTime.Now - start_time;
                    MainFrame.AddToCompilerOutputWindow(
                        "WarSetup: Time elapsed during wix-project generation: " + duration + "\r\n");
                }


                string target_msi = TargetPath;


                string cmd = "";

                DateTime wix_timer = DateTime.Now;
                try
                {
                    _compileDlg.SetInfo("Compiling .wix file", (int)ProgressE.COMPILE_OBJ);
                    List<string> args = new List<string>();
                    cmd = Shell.GetWixBinary("candle.exe");
                    args.Add(target_path);

                    if (!isMergeModle &&
                        projectProperties.UseWixUI)
                    {
                        args.Add("-ext");
                        args.Add(Shell.GetWixBinary("WixUIExtension.dll"));

                        args.Add("-ext");
                        args.Add(Shell.GetWixBinary("WixUtilExtension.dll"));
                    }
                    if (projectRequireDotNetVersion != null && projectRequireDotNetVersion != string.Empty)
                    {
                        args.Add("-ext");
                        args.Add(Shell.GetWixBinary("WixNetFxExtension.dll"));
                    }
                    args.Add("-out");
                    args.Add(TargetPathNoExt + ".wixobj");
                    if (!Shell.Execute(cmd, args))
                    {
                        ok_message = "Compilation failed. See the Output tab for details.";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ok_message = "\"" + cmd + "\"\r\nFailed with an exception: \r\n" + ex.ToString();
                    return;
                }
                finally
                {
                    TimeSpan duration = DateTime.Now - wix_timer;
                    MainFrame.AddToCompilerOutputWindow(
                        "WarSetup: Time elapsed by candle.exe (object generation): " + duration + "\r\n");
                }

                // Create msi-file
                if (projectProperties.BuildTarget)
                {
                    ok_message = "Compilation of target completed OK";

                    DateTime compile_timer = DateTime.Now;
                    try
                    {
                        _compileDlg.SetInfo("Generating " + TargetFile, (int)ProgressE.GENERATE_MSI);
                        List<string> args = new List<string>();
                        cmd = Shell.GetWixBinary("light.exe");

                        args.Add("-out");
                        args.Add(target_msi);
                        args.Add(TargetPathNoExt + ".wixobj");

                        if (!isMergeModle
                            && projectProperties.UseWixUI)
                        {
                            args.Add("-ext");
                            args.Add(Shell.GetWixBinary("WixUIExtension.dll"));

                            args.Add("-ext");
                            args.Add(Shell.GetWixBinary("WixUtilExtension.dll"));

                        }

                        if (projectRequireDotNetVersion != null && projectRequireDotNetVersion != string.Empty)
                        {
                            args.Add("-ext");
                            args.Add(Shell.GetWixBinary("WixNetFxExtension.dll"));
                        }

                        {
                            string my_cultures = "";
                            foreach (UiCulture culture in projectProperties.UiCultures)
                            {
                                if (culture.Enabled && ("" != culture.Name))
                                {
                                    if ("" != my_cultures)
                                        my_cultures += ";";
                                    my_cultures += culture.Name;
                                }
                            }
                            args.Add("-cultures:" + my_cultures);
                        }

                        // Add wix-modules
                        foreach (WixModule wm in projectWixModules)
                            args.Add(wm.DstPath);

                        if (projectProperties.SupressValidation)
                            args.Add("-sval");

                        {
                            foreach (string my_path in _includePathsForLight)
                            {
                                args.Add("-b");
                                args.Add(my_path);
                            }
                        }

                        if (!Shell.Execute(cmd, args))
                        {
                            ok_message = "Compilation failed. See the Output tab for details.";
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ok_message = "light.exe failed with an exception: \r\n" + ex.ToString();
                        return;
                    }
                    finally
                    {
                        TimeSpan duration = DateTime.Now - compile_timer;
                        MainFrame.AddToCompilerOutputWindow(
                            "WarSetup: Time elapsed by light.exe (target generation): " + duration + "\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                ok_message = "Compilation failed with an exception: " + ex.ToString();
                return;
            }
            finally
            {
                TimeSpan duration = DateTime.Now - start_time;
                MainFrame.AddToCompilerOutputWindow(
                    "WarSetup: Totel time elapsed during build: " + duration + "\r\n");
                MainFrame.AddToCompilerOutputWindow("\r\n\r\n" + ok_message);
                _compileDlg.SetInfo(ok_message, (int)ProgressE.FINISH);
            }
        }

        public void BuldTarget()
        {
            BuldTarget(true);
        }


        public void BuldTarget(bool showFinish)
        {
            if (Properties.Settings.Default.compileInOwnThread)
            {
                Thread worker = new Thread(this.DoBuildTarget);
                _compileDlg = new CompileDlg(worker, (int)ProgressE.FINISH, showFinish);

                if (Properties.Settings.Default.compileOnLowerPriority)
                    worker.Priority = ThreadPriority.BelowNormal;
                worker.Start();
                _compileDlg.ShowDialog();
            }
            else
            {

                _compileDlg = new CompileDlg(null, (int)ProgressE.FINISH, showFinish);
                _compileDlg.TopLevel = true;
                _compileDlg.Show();
                if (Properties.Settings.Default.compileOnLowerPriority)
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                DoBuildTarget();
            }

            _compileDlg.OnFinish();
        }

        #endregion
    }
}
