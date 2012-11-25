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
    [XmlRoot("SetupFile")]
    public class SetupFile
    {
        #region Variables
        private string _fileId = "";
        private string _srcName = "";
        private string _srcDirectory = "";
        private string _dstName = "";
        private string _dstPath = "";
        private string _menuName = "";
        private string _shortcutWorkingDirectory = "";
        private string _shortcutCommandArguments = "";
        private string _shortcutDescription = "";
        private string _typelibGuid = "";
        private bool _shortcutInProgramFilesMenu = false;
        private bool _shortcutOnDesktop = false;
        private bool _shortcutInQuickLaunch = false;
        private bool _shortcutInStartupFolder = false;
        private bool _isComModule = false;
        private List<FileExtension> _fileExtensions;
        private ServiceInstall _service;
        private bool _isTrueTypeFont = false;
        private bool _executeOnInstall=false;
        private string _executeOnInstallParameters = "";
        #endregion

        #region Properties
        [
            CategoryAttribute("Installer Internals"),
            DescriptionAttribute("Unique ID for the file"),
            XmlAttribute("fileId")
        ]
        public string fileId
        {
            get { return _fileId; }
            set { _fileId = value; }
        }

        [
            CategoryAttribute("Source object"),
            DescriptionAttribute("Name of the file on the source system (the file to copy to the installer project)."),
            XmlAttribute("srcName")
        ]
        public string srcName
        {
            get { return _srcName; }
            set { _srcName = value; }
        }

        [
            CategoryAttribute("Source object"),
            DescriptionAttribute("Path to the directory on the source system (the directory where the srcName file resides)."),
            XmlAttribute("srcDirectory")
        ]
        public string srcDirectory
        {
            get { return _srcDirectory; }
            set { _srcDirectory = value; }
        }

        [
            CategoryAttribute("Target object"),
            DescriptionAttribute("Name of the file on the destination system (after the file is installed)."),
            XmlAttribute("dstName")
        ]
        public string dstName
        {
            get { return _dstName; }
            set { _dstName = value; }
        }

        [
            CategoryAttribute("Target object"),
            DescriptionAttribute("Folder on the destination system. Use square brackets to reference any "
                + "declared directories. If no path is given, the configurable path to the parent "
                + "feature is assumed. If no such paths exists, the [APPLICATIONFOLDER] is used."),
            TypeConverterAttribute(typeof(DefaultDirectoryConverter)), 
            XmlAttribute("dstPath")
        ]
        public string dstPath
        {
            get 
            {
                if (_isTrueTypeFont)
                    return "[FontsFolder]";

                return _dstPath; 
            }
            set 
            {
                _dstPath = value; 
            }
        }

        [
            CategoryAttribute("Font"),
            DescriptionAttribute("True if this is a TrueType font"),
            XmlAttribute("isTrueTypeFont")
        ]
        public bool isTrueTypeFont
        {
            get { return _isTrueTypeFont; }
            set 
            { 
                _isTrueTypeFont = value; 
            }
        }

        [
            CategoryAttribute("COM Object/Typelib"),
            DescriptionAttribute("True if this file must be registered as a COM/Typelib object"),
            XmlAttribute("isComModule")
        ]
        public bool isComModule
        {
            get { return _isComModule; }
            set { _isComModule = value; }
        }

        [
            CategoryAttribute("COM Object/Typelib"),
            DescriptionAttribute("GUID for the COM/Typelib object."),
            XmlAttribute("typelibGuid")
        ]
        public string typelibGuid
        {
            get { return _typelibGuid; }
            set { _typelibGuid = value; }
        }



        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("Name of the Menu/Desktop shortcut referencing this object. "
                + "If empty, the file-name will be used."),
            XmlAttribute("menuName")
        ]
        public string menuName
        {
            get
            {
                if ((_menuName != null) && (_menuName != ""))
                    return _menuName;
                return Path.GetFileNameWithoutExtension(dstName);
            }
            set { _menuName = value; }
        }

        [
            CategoryAttribute("Target object"),
            DescriptionAttribute("If true, the file will appear under Program Files."),
            XmlAttribute("shortcutInProgramFilesMenu")
        ]
        public bool shortcutInProgramFilesMenu
        {
            get { return _shortcutInProgramFilesMenu; }
            set { _shortcutInProgramFilesMenu = value; }
        }

        [
            CategoryAttribute("Target object"),
            DescriptionAttribute("If true, a shortcut will appear on the Desktop."),
            XmlAttribute("shortcutOnDesktop")
        ]
        public bool shortcutOnDesktop
        {
            get { return _shortcutOnDesktop; }
            set { _shortcutOnDesktop = value; }
        }

        [
           CategoryAttribute("Target object"),
           DescriptionAttribute("If true, a shortcut will appear on the Quick Launch bar (if it is enabled by the target-user)."),
           XmlAttribute("shortcutInQuickLaunch")
        ]
        public bool shortcutInQuickLaunch
        {
            get { return _shortcutInQuickLaunch; }
            set { _shortcutInQuickLaunch = value; }
        }

        [
           CategoryAttribute("Target object"),
           DescriptionAttribute("If true, a shortcut will appear on the Startup Program-Files menu. "
            + "This will execute the program each time the user signs in to Windows."),
           XmlAttribute("shortcutInStartupFolder")
        ]
        public bool shortcutInStartupFolder
        {
            get { return _shortcutInStartupFolder; }
            set { _shortcutInStartupFolder = value; }
        }

        [
           CategoryAttribute("Shortcut"),
           DescriptionAttribute("Optional Working directory for this command on the target system."),
           TypeConverterAttribute(typeof(DefaultDirectoryConverter)),
           XmlAttribute("shortcutWorkingDirectory")
        ]
        public string shortcutWorkingDirectory
        {
            get { return _shortcutWorkingDirectory; }
            set { _shortcutWorkingDirectory = value; }
        }

        [
           CategoryAttribute("Shortcut"),
           DescriptionAttribute("Optional command-line arguments for the shortcuts."),
           XmlAttribute("shortcutCommandArguments")
        ]
        public string shortcutCommandArguments
        {
            get { return _shortcutCommandArguments; }
            set { _shortcutCommandArguments = value; }
        }

        [
           CategoryAttribute("Shortcut"),
           DescriptionAttribute("Optional description for the shortcuts. This is dispølayed when the mouse "
                + "moves above the shortcut in the target system."),
           XmlAttribute("shortcutDescription")
        ]
        public string shortcutDescription
        {
            get { return _shortcutDescription; }
            set { _shortcutDescription = value; }
        }

        [
           CategoryAttribute("File Extensions"),
           DescriptionAttribute("Optional file-extensions to register for this file."),
           XmlElement("fileExtensions")
        ]
        public List<FileExtension> fileExtensions
        {
            get { return _fileExtensions; }
            set { _fileExtensions = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Windows Service"),
           XmlElement("service")
        ]
        public ServiceInstall service
        {
            get { return _service; }
            set { _service = value; }
        }


        [
           CategoryAttribute("Execute"),
           DescriptionAttribute("Execute this executable on finish of installation."),
           XmlElement("executeOnInstall")
        ]
        public bool ExecuteOnInstall
        {
            get { return _executeOnInstall; }
            set { _executeOnInstall = value; }
        }

        [
           CategoryAttribute("Execute"),
           DescriptionAttribute("Parameters that are passed to this executable."),
           XmlElement("executeOnInstallParameters")
        ]
        public string ExecuteOnInstallParameters
        {
            get { return _executeOnInstallParameters; }
            set { _executeOnInstallParameters = value; }
        }

        #endregion

        #region Methods
        public SetupFile()
        {
            _shortcutInProgramFilesMenu = false;
            _shortcutOnDesktop = false;
            _shortcutInQuickLaunch = false;
            _isComModule = false;
            _fileId = "ID-" + Guid.NewGuid().ToString();
            _fileId = _fileId.Replace('-', '_');
            _shortcutWorkingDirectory = "";
            _shortcutCommandArguments = "";
            _shortcutDescription = "";
            _fileExtensions = new List<FileExtension>();
            _service = new ServiceInstall();
        }

        public static string GetShortPath(string path)
        {
            string short_path = "";
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890";

            int col = 0;
            foreach(char ch in path)
            {
                if (valid.IndexOf(ch) == -1)
                    short_path = short_path + "X";
                else
                    short_path = short_path + ch;

                if (++col >= 8)
                    break;
            }

            return short_path;
        }
        #endregion
    }
}
