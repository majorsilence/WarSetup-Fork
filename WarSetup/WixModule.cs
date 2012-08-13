using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;

namespace WarSetup
{
    [XmlRoot("WixModule")]
    public class WixModule
    {

        #region Enums
        public enum CompileModeE
        {
            Auto,
            Yes,
            No
        };

        public enum RefModeE
        {
            None,
            CustomAction, 
            Feature, 
            Property, 
            UI
        };

        #endregion

        #region Variables
        private string _Id;
        private string _Name = "";
        private string _Path = "";
        private RefModeE _ReferenceType = RefModeE.None;
        private CompileModeE _Compile = CompileModeE.Auto;
        private string _ReferenceId = "";
        #endregion

        #region Properties

        [
            CategoryAttribute("Installer Internals"),
            DescriptionAttribute("Unique ID for the module"),
            BrowsableAttribute(false),
            XmlAttribute("Id")
        ]
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [
            CategoryAttribute("Build action"),
            DescriptionAttribute("Determines if the .wxs file is to be compiled before it is used."),
            XmlAttribute("Compile")
        ]
        public CompileModeE Compile
        {
            get { return _Compile; }
            set { _Compile = value; }
        }

        [
           CategoryAttribute("Fragment"),
           DescriptionAttribute("Fragment name"),
           XmlAttribute("Name")
        ]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        [
           CategoryAttribute("Module"),
           DescriptionAttribute("Path to the module"),
           XmlAttribute("Path")
        ]
        public string Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        [
           CategoryAttribute("Reference"),
           DescriptionAttribute("Make a reference to the modules fragment-name in the project. Select the appropriate reference type or None."),
           XmlAttribute("ReferenceType")
        ]
        public RefModeE ReferenceType
        {
            get { return _ReferenceType; }
            set { _ReferenceType = value; }
        }

        [
          CategoryAttribute("Reference"),
          DescriptionAttribute("Object-id to add the reference to. This must be an object of the same type that is specified in the ReferenceType field."),
          XmlAttribute("ReferenceId")
        ]
        public string ReferenceId
        {
            get { return _ReferenceId; }
            set { _ReferenceId = value; }
        }

        [
          CategoryAttribute("File System"),
          DescriptionAttribute("Source file.")
        ]
        public string SrcPath
        {
            get
            {
                string src_file = System.IO.Path.GetDirectoryName(_Path);
                if ("." == src_file)
                    src_file = System.IO.Path.GetFileNameWithoutExtension(_Path); // No path
                else
                    src_file += System.IO.Path.Combine(src_file, System.IO.Path.GetFileNameWithoutExtension(_Path));
                src_file += ".wxs";

                return src_file;
            }
        }

        [
          CategoryAttribute("File System"),
          DescriptionAttribute("Object file.")
        ]
        public string DstPath
        {
            get
            {
                string dst_file = System.IO.Path.GetDirectoryName(_Path);
                if ("." == dst_file)
                    dst_file = System.IO.Path.GetFileNameWithoutExtension(_Path);
                else
                    dst_file += System.IO.Path.Combine(dst_file, System.IO.Path.GetFileNameWithoutExtension(_Path));
                dst_file += ".wixobj";

                return dst_file;
            }
        }

        #endregion 

        #region Methods

        public WixModule()
        {
            _Id = MainFrame.CurrentProject.GetUniqueId();
        }

        public void GetInfo()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_Path);
            XmlNodeList fragments = doc.GetElementsByTagName("Fragment");
            if (fragments.Count != 1)
            {
                 throw new ApplicationException(
                    "The Wix module \"" + _Path + "\" don't have a \"Fragment\" XML node. This is required in Wix modules.");
            }
            XmlElement fragment = (XmlElement)fragments[0];
            Name = fragment.GetAttribute("Id");
        }

        #endregion

    }
}
