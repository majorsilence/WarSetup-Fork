using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Globalization;


namespace WarSetup
{
    [XmlRoot("SetupFeature")]
    public class SetupFeature
    {
        #region Variables
        private string _featureId;
        private string _featureName;
        private string _featureDescription;
        private bool _enableInDefaultInstall;
        private bool _enableInMinimalInstall;
        private List<SetupFeature> _childFeatures;
        private List<SetupComponent> _components;
        private string _configurableDirectory;
        private string _configurableDirectoryDefaultPath;
        private InstallModeE _defaultInstallMode;
        private List<SetupDirectory> _directories;
        private List<SetupShortcut> _shortcuts;
        private bool _excludeFromBuild = false;
        //private SetupComponent _component; // Component used for things like shortcuts
        #endregion


        #region Enums

        public enum InstallLevelE
        {
            LEVEL_MINIMUM = 1, /* My idea ...*/
            LEVEL_DEFAULT = 3, /* As in the wix Tutorial */
            LEVEL_COMPLETE = 1000 /* As in the wix Tutorial */
        };

        public enum InstallModeE
        {
            NOT_INSTALLED,
            SAME_AS_PARENT_FEATURE,
            RUN_FROM_SOURCE,
            INSTALL_LOCALLY
        };

        #endregion

        #region Properties
        [
            BrowsableAttribute(false),
            XmlAttribute("featureId")
        ]
        public string featureId
        {            
            get { return _featureId; }
            set { _featureId = value; }
        }

        [
            BrowsableAttribute(false),
            XmlAttribute("featureName")
        ]
        public string featureName
        {            
            get { return _featureName; }
            set { _featureName = value; }
        }

        [
            BrowsableAttribute(false),
            XmlAttribute("featureDescription")
        ]
        public string featureDescription
        {
            get { return _featureDescription; }
            set { _featureDescription = value; }
        }

        [
            BrowsableAttribute(false),
            XmlAttribute("configurableDirectory")
        ]
        public string configurableDirectory
        {
            get { return _configurableDirectory; }
            set { _configurableDirectory = value; }
        }

        /* The default name of the configurable directory.
         * This name may be a name of a directory, or
         * a name relative to the APPLICATIONFOLDER.
         * References to existing paths are enclosed in
         * square brackets
         */
        [
            BrowsableAttribute(false),
            XmlAttribute("configurableDirectoryDefaultPath")
        ]
        public string configurableDirectoryDefaultPath
        {
            get { return _configurableDirectoryDefaultPath; }
            set { _configurableDirectoryDefaultPath = value; }
        }

        [
            BrowsableAttribute(false),
            XmlAttribute("enableInDefaultInstall")
        ]        
        public bool enableInDefaultInstall
        {
            get { return _enableInDefaultInstall; }
            set { _enableInDefaultInstall = value; }
        }

        [
            BrowsableAttribute(false),
            XmlAttribute("enableInMinimalInstall")
        ]
        public bool enableInMinimalInstall
        {
            get { return _enableInMinimalInstall; }
            set { _enableInMinimalInstall = value; }
        }

        [
            BrowsableAttribute(false),
            XmlElement("childFeatures")
        ]
        public List<SetupFeature> childFeatures
        {
            get { return _childFeatures; }
            set { _childFeatures = value; }
        }

        [
            BrowsableAttribute(false),
            XmlElement("components")
        ]
        public List<SetupComponent> components
        {
            get { return _components; }
            set { _components = value; }
        }

        [
            BrowsableAttribute(false),
            XmlElement("defaultInstallMode")
        ]
        public int defaultInstallMode
        {
            get { return (int)_defaultInstallMode; }
            set { _defaultInstallMode = (InstallModeE)value; }
        }

        [
            BrowsableAttribute(false),
            XmlElement("directories")
        ]
        public List<SetupDirectory> directories
        {
            get { return _directories; }
            set { _directories = value; }
        }

        [
            BrowsableAttribute(false),
            XmlElement("shortcuts")
        ]
        public List<SetupShortcut> shortcuts
        {
            get { return _shortcuts; }
            set { _shortcuts = value; }
        }

        //[XmlElement("component")]
        //public SetupComponent component
        //{
        //    get { return _component; }
        //    set { _component = value; }
        //}

        [
            CategoryAttribute("Project"),
            DescriptionAttribute("Exclude this feature and all its child-feature from the build."),
            XmlAttribute("excludeFromBuild")
        ]
        public bool excludeFromBuild
        {
            get { return _excludeFromBuild; }
            set { _excludeFromBuild = value; }
        }


        #endregion

        #region Methods
        public int installLevel
        {
            get
            {
                if (enableInMinimalInstall)
                    return (int)InstallLevelE.LEVEL_MINIMUM;
                if (enableInDefaultInstall)
                    return (int)InstallLevelE.LEVEL_DEFAULT;
                return (int)InstallLevelE.LEVEL_COMPLETE;
            }
        }

        public SetupFeature()
        {
            enableInDefaultInstall = true;
            enableInMinimalInstall = true;
            _childFeatures = new List<SetupFeature>();
            _components = new List<SetupComponent>();
            _featureId = "ID" + SetupDirectory.GetMd5Hash(Guid.NewGuid().ToString());
            _featureId = _featureId.Replace('-', '_').ToUpper();
            _defaultInstallMode = InstallModeE.NOT_INSTALLED;
            _featureName = "New Feature";
            _featureDescription = "Enter Description Here";
            _directories = new List<SetupDirectory>();
            //_component = new SetupComponent();
            _shortcuts = new List<SetupShortcut>();
        }

        public SetupFile AddFile(string path)
        {
            SetupFile file = new SetupFile();

            file.srcName = Path.GetFileName(path);
            file.srcDirectory = Path.GetDirectoryName(path);
            file.dstName = file.srcName;

            // Create a component
            SetupComponent component = new SetupComponent();
            component.componentFiles.Add(file);

            components.Add(component);

            // Return the file
            return file;
        }

        public void RemoveComponent(SetupComponent component)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Equals(component))
                {
                    _components.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion
    }
}
