using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace WarSetup
{
    [XmlRoot("SetupComponent")]
    public class SetupComponent
    {
        private string _componentId;
        private string _componentGuid;
        private string _targetDirectory;
        private string _pathEntry;
        private bool _addToCreateFolder = false;

        public string pathEntry
        {
            get { return _pathEntry; }
            set { _pathEntry = value; }
        }

        [XmlAttribute("targetDirectory")]
        public string targetDirectory
        {
            get { return _targetDirectory; }
            set { _targetDirectory = value; }
        }
        private List<WarSetup.SetupFile> _componentFiles;

        [XmlAttribute("componentId")]
        public string componentId
        {
            get { return _componentId; }
            set { _componentId = value; }
        }

        [XmlAttribute("componentGuid")]
        public string componentGuid
        {
            get { return _componentGuid; }
            set { _componentGuid = value; }
        }

        [XmlElement("componentFiles")]
        public List<WarSetup.SetupFile> componentFiles
        {
            get { return _componentFiles; }
            set { _componentFiles = value; }
        }

        [XmlElement("addToCreateFolder")]
        public bool addToCreateFolder
        {
            get { return _addToCreateFolder; }
            set { _addToCreateFolder = value; }
        }

        public SetupComponent()
        {
            _componentId = MainFrame.CurrentProject.GetUniqueId();
            _componentGuid = Guid.NewGuid().ToString();
            _componentFiles = new List<WarSetup.SetupFile>();
            _pathEntry = null;
        }

        // Return the file-id for the component's (first) file.
        // Normally, a component only have one file.
        public string GetFirstFileId()
        {
            if (_componentFiles.Count > 0)
                return _componentFiles[0].fileId;

            return "";
        }
    }
}
