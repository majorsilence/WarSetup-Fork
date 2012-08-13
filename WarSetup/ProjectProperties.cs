using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;

namespace WarSetup
{
    [XmlRoot("ProjectProperties")]
    public class ProjectProperties
    {
        public enum CabCompressionLevelE
        {
            high,
            low,
            medium,
            mszip,
            none
        };

        #region Variables
        string _TopBanner = "";
        string _BigBanner = "";
        string _ExclamationIcon = "";
        string _InfoIcon = "";
        string _NewIcon = "";
        string _UpIcon = "";
        bool _UseRelativePaths = true;
        bool _UseWixUI_ErrorProgressText = true;
        bool _UseWixUI = true;
        private List<UiCulture> _UiCultures = null;
        private int _ProductLanguage = 1033;
        private int _Codepage = 1252;
        private string _VS2003Integration = "VS2003_INTEGRATION";
        private string _VS2005Integration = "VS2005_INTEGRATION";
        private bool _AlwaysMajorUpgrade = true;
        private bool _Compress = true;
        private bool _BuildTarget = true;
        private string _PublisherUrl = "";
        private string _SupportContact = "";
        private string _SupportUrl = "";
        private string _SupportPhone = "";
        private string _ReadMeFile = "";
        private string _UpdateUrl = "";
        private string _Comments = "";
        private string _Icon = "";
        private Int64 _MaxCabSize = 0;
        private bool _EmbedCab = true;
        private CabCompressionLevelE _CabCompressionLevel = CabCompressionLevelE.mszip;
        private int _InstallerVersion = 300;
        private bool _SupressValidation = false;
        private bool _CreateUninstallShortcut = false;
        private string _LicencePath = "";
        private string _LaunchAppText = "";
        #endregion

        #region Structs
        public struct VariablePair
        {
            public string mName;
            public string mValue;
        };
        #endregion

        #region Properties


        [
            CategoryAttribute("Project"),
            DescriptionAttribute("Path to the folder containing the projects License-file (*.rtf)"),
            XmlAttribute("LicencePath")
        ]
        public string LicencePath
        {
            get { return _LicencePath; }
            set { _LicencePath = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Top banner (493 × 58)"),
            XmlAttribute("TopBanner")
        ]
        public string TopBanner
        {
            get { return _TopBanner; }
            set { _TopBanner = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Background bitmap used on welcome and install-complete dialogs (493 × 312)"),
            XmlAttribute("BigBanner")
        ]
        public string BigBanner
        {
            get { return _BigBanner; }
            set { _BigBanner = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Exclamation icon on the wait-for-costing dialog (32 × 32)"),
            XmlAttribute("ExclamationIcon")
        ]
        public string ExclamationIcon
        {
            get { return _ExclamationIcon; }
            set { _ExclamationIcon = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Information icon on the cancel and error dialogs (32 × 32)"),
            XmlAttribute("InfoIcon")
        ]
        public string InfoIcon
        {
            get { return _InfoIcon; }
            set { _InfoIcon = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Button glyph on directory-browse dialog (16 × 16)"),
            XmlAttribute("NewIcon")
        ]
        public string NewIcon
        {
            get { return _NewIcon; }
            set { _NewIcon = value; }
        }

        [
            CategoryAttribute("Graphics"),
            DescriptionAttribute("Button glyph on directory-browse dialog (16 × 16 )"),
            XmlAttribute("UpIcon")
        ]
        public string UpIcon
        {
            get { return _UpIcon; }
            set { _UpIcon = value; }
        }

        [
            CategoryAttribute("Visual Studio Integration"),
            DescriptionAttribute("ID of a feature that will enable integration with Visual Studio 2003."
                + "Add a feature in the feature-tree with this ID in order to integrate your C++ "
                + "class library or C library with Visual Studio."),
            XmlAttribute("VS2003Integration")
        ]
        public string VS2003Integration
        {
            get { return _VS2003Integration; }
            set { _VS2003Integration = value; }
        }

        [
            CategoryAttribute("Visual Studio Integration"),
            DescriptionAttribute("ID of a feature that will enable integration with Visual Studio 2005."
                + "Add a feature in the feature-tree with this ID in order to integrate your C++ "
                + "class library or C library with Visual Studio."),
            XmlAttribute("VS2005Integration")
        ]
        public string VS2005Integration
        {
            get { return _VS2005Integration; }
            set { _VS2005Integration = value; }
        }

        [
            CategoryAttribute("Project"),
            DescriptionAttribute("Use absolute or relative paths on files and directories "
                + "that's dragged into the project. If true, the paths are made relative."),
            XmlAttribute("UseRelativePaths")
        ]
        public bool UseRelativePaths
        {
            get { return _UseRelativePaths; }
            set { _UseRelativePaths = value; }
        }

        [
            CategoryAttribute("Interface"),
            DescriptionAttribute("Text to display in the final dialog if the user have the option to launch the primary application."),
            XmlAttribute("LaunchAppText")
        ]
        public string LaunchAppText
        {
            get { return _LaunchAppText; }
            set { _LaunchAppText = value; }
        }

        [
            CategoryAttribute("Interface"),
            DescriptionAttribute("Include a reference to WixUI_ErrorProgressText. This is normally required if you use the wix UI module."),
            XmlAttribute("UseWixUI_ErrorProgressText")
        ]
        public bool UseWixUI_ErrorProgressText
        {
            get { return _UseWixUI_ErrorProgressText; }
            set { _UseWixUI_ErrorProgressText = value; }
        }

        [
            CategoryAttribute("Interface"),
            DescriptionAttribute("Include the WixUI module."),
            XmlAttribute("UseWixUI")
        ]
        public bool UseWixUI
        {
            get { return _UseWixUI; }
            set { _UseWixUI = value; }
        }


        [
           CategoryAttribute("Target Options"),
           DescriptionAttribute("Create uninstalll shortcut in Programs menu."),
           XmlAttribute("CreateUninstallShortcut")
       ]
        public bool CreateUninstallShortcut
        {
            get { return _CreateUninstallShortcut; }
            set { _CreateUninstallShortcut = value; }
        }

        [
            CategoryAttribute("Localization"),
            DescriptionAttribute("Localization \"cultures\" to support in the User Interface."),
            XmlElement("UiCultures")
        ]
        public List<UiCulture> UiCultures
        {
            get { return _UiCultures; }
            set 
            { 
                _UiCultures = value; 
                // Remove duplicates
                RemoveUiCultureDuplicates();
            }
        }

        [
           CategoryAttribute("Localization"),
           DescriptionAttribute("Optional codepage for the resulting MSI."),
           XmlAttribute("Codepage")
        ]
        public int Codepage
        {
            get { return _Codepage; }
            set { _Codepage = value; }
        }

        [
          CategoryAttribute("Localization"),
          DescriptionAttribute("The decimal language ID (LCID) for the product."),
          XmlAttribute("ProductLanguage")
        ]
        public int ProductLanguage
        {
            get { return _ProductLanguage; }
            set { _ProductLanguage = value; }
        }

        [
            CategoryAttribute("Build Options"),
            DescriptionAttribute("Always upgrade the Product-GUID to tag the build as major upgrade if the product already is installed."),
            XmlAttribute("AlwaysMajorUpgrade")
        ]
        public bool AlwaysMajorUpgrade
        {
            get { return _AlwaysMajorUpgrade; }
            set { _AlwaysMajorUpgrade = value; }
        }

        [
            CategoryAttribute("Build Options"),
            DescriptionAttribute("Supress validation of target. Required with some merge-modules."),
            XmlAttribute("SupressValidation")
        ]
        public bool SupressValidation
        {
            get { return _SupressValidation; }
            set { _SupressValidation = value; }
        }

        [
            CategoryAttribute("Build Options"),
            DescriptionAttribute("Compress the files in the target. (Ignored for MergeModules)"),
            XmlAttribute("Compress")
        ]
        public bool Compress
        {
            get { return _Compress; }
            set { _Compress = value; }
        }

        [
            CategoryAttribute("Build Options"),
            DescriptionAttribute("If true the target is built. If false, only the object-moduls is built. (This is useful when testing large setup projects.)"),
            XmlAttribute("BuildTarget")
        ]
        public bool BuildTarget
        {
            get { return _BuildTarget; }
            set { _BuildTarget = value; }
        }

        [
           CategoryAttribute("Add or Remove Programs Information"),
           DescriptionAttribute("Enter a general URL for your company or product."),
           XmlAttribute("PublisherUrl")
        ]
        public string PublisherUrl
        {
            get { return _PublisherUrl; }
            set { _PublisherUrl = value; }
        }

        [
          CategoryAttribute("Add or Remove Programs Information"),
          DescriptionAttribute("Enter the name of the person or department that end users should contact for technical support."),
          XmlAttribute("SupportContact")
        ]
        public string SupportContact
        {
            get { return _SupportContact; }
            set { _SupportContact = value; }
        }

        [
          CategoryAttribute("Add or Remove Programs Information"),
          DescriptionAttribute("The Support URL is the address of the Web site where end users can find technical support information for your product."),
          XmlAttribute("SupportUrl")
        ]
        public string SupportUrl
        {
            get { return _SupportUrl; }
            set { _SupportUrl = value; }
        }

        [
         CategoryAttribute("Add or Remove Programs Information"),
         DescriptionAttribute("This field should contain the technical support phone number for this product."),
         XmlAttribute("SupportPhone")
        ]
        public string SupportPhone
        {
            get { return _SupportPhone; }
            set { _SupportPhone = value; }
        }

        [
          CategoryAttribute("Add or Remove Programs Information"),
          DescriptionAttribute(@"Enter the name of the readme file for this product. Example: [APPLICATIONFOLDER]\README.txt"),
          XmlAttribute("ReadMeFile")
        ]
        public string ReadMeFile
        {
            get { return _ReadMeFile; }
            set { _ReadMeFile = value; }
        }

       
        [
          CategoryAttribute("Add or Remove Programs Information"),
          DescriptionAttribute("The Update URL should point to the Web site where end users can get information about product updates or download the latest version."),
          XmlAttribute("UpdateUrl")
        ]
        public string UpdateUrl
        {
            get { return _UpdateUrl; }
            set { _UpdateUrl = value; }
        }

        [
          CategoryAttribute("Add or Remove Programs Information"),
          DescriptionAttribute("Enter comments about your application in this field."),
          XmlAttribute("Comments")
        ]
        public string Comments
        {
            get { return _Comments; }
            set { _Comments = value; }
        }

        [
         CategoryAttribute("Add or Remove Programs Information"),
         DescriptionAttribute(@"Enter Icon for your application in this field. Example: [APPLICATIONFOLDER]\WarSetup.ico"),
         XmlAttribute("Icon")
        ]
        public string Icon
        {
            get { return _Icon; }
            set { _Icon = value; }
        }

        [
         CategoryAttribute("Cab files"),
         DescriptionAttribute("Embed the cab-file in the target. If false, external cab files will be created."),
         XmlAttribute("EmbedCab")
        ]
        public bool EmbedCab
        {
            get { return _EmbedCab; }
            set { _EmbedCab = value; }
        }

        [
         CategoryAttribute("Cab files"),
         DescriptionAttribute("Max size of a sinbgle cab-file. If 0, no limit is enforced and only one cab-file will be used."),
         XmlAttribute("MaxCabSize")
        ]
        public Int64 MaxCabSize
        {
            get { return _MaxCabSize; }
            set { _MaxCabSize = value; }
        }

        [
         CategoryAttribute("Cab files"),
         DescriptionAttribute("Compression-level. The default is mszip."),
         XmlAttribute("CabCompressionLevel")
        ]
        public CabCompressionLevelE CabCompressionLevel
        {
            get { return _CabCompressionLevel; }
            set { _CabCompressionLevel = value; }
        }

        [
         CategoryAttribute("Windows Installer"),
         DescriptionAttribute("The minimum version of Microsoft Installer required to install this package. (Ex: 300 means Windows Installer 3.0)"),
         XmlAttribute("InstallerVersion")
        ]
        public int InstallerVersion
        {
            get { return _InstallerVersion; }
            set { _InstallerVersion = value; }
        }
      
        #endregion

        #region Methods

        public ProjectProperties()
        {
            _UiCultures = new List<UiCulture>();
            _UiCultures.Add(new UiCulture("en-US", true));
        }

        public ProjectProperties.VariablePair[] GetVariables()
        {
            List<ProjectProperties.VariablePair> rval = new List<VariablePair>();

            if ("" != TopBanner)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUIBannerBmp";
                v.mValue = TopBanner;
                rval.Add(v);
            }

            if ("" != BigBanner)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUIDialogBmp";
                v.mValue = BigBanner;
                rval.Add(v);
            }

            if ("" != ExclamationIcon)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUIExclamationIco";
                v.mValue = ExclamationIcon;
                rval.Add(v);
            }

            if ("" != InfoIcon)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUIInfoIco";
                v.mValue = InfoIcon;
                rval.Add(v);
            }

            if ("" != NewIcon)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUINewIco";
                v.mValue = NewIcon;
                rval.Add(v);
            }

            if ("" != UpIcon)
            {
                VariablePair v = new VariablePair();
                v.mName = "WixUIUpIco";
                v.mValue = UpIcon;
                rval.Add(v);
            }

            return rval.ToArray();
        }

        public void AddAllUiCulture()
        {
            string[] loc = { "en-US", "de-de", "es-es", "nl-nl" };

            foreach (string myloc in loc)
            {
                bool found = false;
                foreach (UiCulture probe in UiCultures)
                {
                    if (probe.Name == myloc)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    UiCultures.Add(new UiCulture(myloc, ("en-US" == myloc)));
            }
        }

        public void RemoveUiCultureDuplicates()
        {
            again:
            foreach (UiCulture cult in _UiCultures)
            {
                foreach (UiCulture probe in _UiCultures)
                {
                    if ((probe.Name == cult.Name)
                        && (!probe.Equals(cult)))
                    {
                        _UiCultures.Remove(cult);
                        goto again;
                    }
                }
            }
        }

        #endregion
    }

    [XmlRoot("UiCulture")]
    public class UiCulture
    {
        private string _Name = "";
        private bool _Enabled = false;

        [
            CategoryAttribute("Interface"),
            DescriptionAttribute("Include the WixUI module."),
            XmlAttribute("Name")
        ]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        [
            CategoryAttribute("Interface"),
            DescriptionAttribute("Include the WixUI module."),
            XmlAttribute("Enabled")
        ]
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }

        public UiCulture()
        {
        }

        public UiCulture(string name, bool enable)
        {
            _Name = name;
            _Enabled = enable;
        }

        
    }
}
