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
    [XmlRoot("SetupShortcut")]
    public class SetupShortcut
    {
        #region Variables
        private string _srcPath = "";
        private string _dstPath = "";
        private string _workingDir = "";
        private string _commandArguments = "";
        private string _description = "";
        private bool _showOnProgramMenu = false;
        private bool _showOnQuickLaunchBar = false;
        private bool _showOnDesktop = false;
        private string _shortcutId;
        #endregion

        #region Properties
        
        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("The path (relative for the target installation) " +
                "that the shortcut points to."),
            TypeConverterAttribute(typeof(DefaultDirectoryConverter)), 
            XmlAttribute("srcPath")
        ]
        public string srcPath
        {
            get { return _srcPath; }
            set { _srcPath = value; }
        }

        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("The path (relative for the target installation) "+
                "to the shortcut. This is the combined name and directory " +
                "listed in the shortcut-list above."),
            TypeConverterAttribute(typeof(DefaultDirectoryConverter)), 
            XmlAttribute("dstPath")
        ]
        public string dstPath
        {
            get { return _dstPath; }
            set { _dstPath = value; }
        }

        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("Default working directory for the target when " +
                "opened by the shortcut."),
            TypeConverterAttribute(typeof(DefaultDirectoryConverter)), 
            XmlAttribute("workingDir")
        ]
        public string workingDir
        {
            get { return _workingDir; }
            set { _workingDir = value; }
        }

        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("Command-arguments for the target."),
            XmlAttribute("commandArguments")
        ]
        public string commandArguments
        {
            get { return _commandArguments; }
            set { _commandArguments = value; }
        }

        [
            CategoryAttribute("Shortcut"),
            DescriptionAttribute("Description to show when the mouse are moved " +
                "over the shortcut on the target-machine."),
            XmlAttribute("description")
        ]
        public string description
        {
            get { return _description; }
            set { _description = value; }
        }

        [
           CategoryAttribute("Menus"),
           DescriptionAttribute("If enabled, the shourtcut will be added to the Program Menu."),
           XmlAttribute("showOnProgramMenu")
        ]
        public bool showOnProgramMenu
        {
            get { return _showOnProgramMenu; }
            set { _showOnProgramMenu = value; }
        }

        [
           CategoryAttribute("Menus"),
           DescriptionAttribute("If enabled, the shourtcut will be added to the Quick Launch Bar."),
           XmlAttribute("showOnQuickLaunchBar")
        ]
        public bool showOnQuickLaunchBar
        {
            get { return _showOnQuickLaunchBar; }
            set { _showOnQuickLaunchBar = value; }
        }

        [
           CategoryAttribute("Menus"),
           DescriptionAttribute("If enabled, the shourtcut will be added to the Desktop."),
           XmlAttribute("showOnDesktop")
        ]
        public bool showOnDesktop
        {
            get { return _showOnDesktop; }
            set { _showOnDesktop = value; }
        }


        [
           CategoryAttribute("Setup Internals"),
           DescriptionAttribute("Unique ID for the shortcut. Don't modify unless you know what you do!"),
           XmlAttribute("shortcutId")
        ]
        public string shortcutId
        {
            get { return _shortcutId; }
            set { _shortcutId = value; }
        }

        #endregion

        #region Methods
        
        public SetupShortcut()
        {
            _shortcutId = MainFrame.CurrentProject.GetUniqueId();
        }
        
        #endregion
    }
}
