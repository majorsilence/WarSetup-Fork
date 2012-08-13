using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Globalization;


namespace WarSetup
{
    [XmlRoot("SetupDirectory")]
    public class SetupDirectory
    {
        #region Variables
        private string _srcPath;
        private string _dstPath;
        private string _patterns = ".*";
        private string _excludePatterns = "";
        private string _dirId;
    
        bool _recurse = false;
        bool _addToPath = false;
        bool _ideHeaderFiles = false;
        bool _ideExecutables = false;
        bool _ideLibrary = false;
        bool _ideMicrosoftVisualStudio2003 = false;
        bool _ideMicrosoftVisualStudio2005 = false;
        bool _preventEmptyDirectories = false;
        SetupComponent _pathComponent;
        #endregion

        #region Properties
        [
           CategoryAttribute("Installer Internals"),
           DescriptionAttribute("Unique ID for the directory"),
           XmlAttribute("dirId")
        ]
        public string dirId
        {
            get { return _dirId; }
            set { _dirId = value; }
        }

        [
            Browsable(false),
            XmlElement("pathComponent")
        ]
        public SetupComponent pathComponent
        {
            get { return _pathComponent; }
            set { _pathComponent = value; }
        }

        [
            CategoryAttribute("Source object"),
            DescriptionAttribute("Path to the directory on the source system."),
            XmlAttribute("srcPath")
        ]
        public string srcPath
        {
            get { return _srcPath; }
            set { _srcPath = value; }
        }

        [
            CategoryAttribute("Destination Location"),
            DescriptionAttribute("Path to the directory on the destination system. "
                + "This name may also include a path, relative to APPLICATIONFOLDER, or you can "
                + "reference another Configurable Directory by it's name enclosed in square brackets."),
            TypeConverterAttribute(typeof(DefaultDirectoryConverter)), 
            XmlAttribute("dstPath")
        ]
        public string dstPath
        {
            get { return _dstPath; }
            set { _dstPath = value; }
        }

        [
            CategoryAttribute("Filter"),
            DescriptionAttribute(@"Regular expression filtering what files to extract from the directory. Use *. to copy all files."),
            XmlAttribute("patterns")
        ]
        public string patterns
        {
            get { return _patterns; }
            set { _patterns = value; }
        }

        [
            CategoryAttribute("Filter"),
            DescriptionAttribute(@"Regular expression filtering files and directoris to exclude (even if they math the pattern)."),
            XmlAttribute("excludePatterns")
        ]
        public string excludePatterns
        {
            get { return _excludePatterns; }
            set { _excludePatterns = value; }
        }
        

        [
            CategoryAttribute("Filter"),
            DescriptionAttribute(@"If true, subdirectories will be scanned as well"),
            XmlAttribute("recurse")
        ]
        public bool recurse
        {
            get { return _recurse; }
            set { _recurse = value; }
        }

        [
            CategoryAttribute("Filter"),
            DescriptionAttribute(@"If true, only add directory-structures with files."),
            XmlAttribute("preventEmptyDirectories")
        ]
        public bool preventEmptyDirectories
        {
            get { return _preventEmptyDirectories; }
            set { _preventEmptyDirectories = value; }
        }

        [
            CategoryAttribute("Options"),
            DescriptionAttribute(@"If true, the directory will be added to the target systems PATH "
                + "environment-variable. NEVER use this option unless you have a really good reason "
                + "to do so, and you know what you are doing!"),
            XmlAttribute("addToPath")
        ]
        public bool addToPath
        {
            get { return _addToPath; }
            set { _addToPath = value; }
        }

        [
            CategoryAttribute("Microsoft Visual Studio Integration"),
            DescriptionAttribute("If an IDE is selected, add this path to the IDE's header-file path."),
            XmlAttribute("ideHeaderFiles")
        ]
        public bool ideHeaderFiles
        {
            get { return _ideHeaderFiles; }
            set { _ideHeaderFiles = value; }
        }

        [
            CategoryAttribute("Microsoft Visual Studio Integration"),
            DescriptionAttribute("If an IDE is selected, add this path to the IDE's execute path."),
            XmlAttribute("ideExecutables")
        ]
        public bool ideExecutables
        {
            get { return _ideExecutables; }
            set { _ideExecutables = value; }
        }

        [
            CategoryAttribute("Microsoft Visual Studio Integration"),
            DescriptionAttribute("If an IDE is selected, add this path to the IDE's library path."),
            XmlAttribute("ideLibrary")
        ]
        public bool ideLibrary
        {
            get { return _ideLibrary; }
            set { _ideLibrary = value; }
        }


        [
            CategoryAttribute("Microsoft Visual Studio Integration"),
            DescriptionAttribute("This directory should be integrated with Microsoft Visual Studio 2003."),
            XmlAttribute("ideMicrosoftVisualStudio2003")
        ]
        public bool ideMicrosoftVisualStudio2003
        {
            get { return _ideMicrosoftVisualStudio2003; }
            set { _ideMicrosoftVisualStudio2003 = value; }
        }

        [
           CategoryAttribute("Microsoft Visual Studio Integration"),
           DescriptionAttribute("This directory should be integrated with Microsoft Visual Studio 2005."),
           XmlAttribute("ideMicrosoftVisualStudio2005")
        ]
        public bool ideMicrosoftVisualStudio2005
        {
            get { return _ideMicrosoftVisualStudio2005; }
            set { _ideMicrosoftVisualStudio2005 = value; }
        }
       

        public void Clear()
        {
            _recurse = false;
            _addToPath = false;
            _ideHeaderFiles = false;
            _ideExecutables = false;
            _ideLibrary = false;
            _pathComponent = new SetupComponent();
            _dirId = MainFrame.CurrentProject.GetUniqueId();
            _patterns = ".*";
        }

        public SetupDirectory()
        {
            Clear();
        }

        public SetupDirectory(string path)
        {
            Clear();
            _srcPath = path;
            _dstPath = Path.GetFileName(path);
        }

        #endregion

        #region Methods
        public static string GetMd5Hash(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }

        /* Add one or more components to the components list, based on the contents */
        /* Return true if files were added at any sublevel */
        public bool MakeComponents(List<SetupComponent> components, 
            string srcDirectory,
            string targetDirectory)
        {
            bool rval = false;
            // For now, we use one component per directory
            SetupComponent component = new SetupComponent();
            component.targetDirectory = targetDirectory;
            components.Add(component);

            Regex exclude = null;
            if ("" != excludePatterns)
                exclude = new Regex(excludePatterns);

            //component.componentId = component.componentGuid = "Component_"
            // + GetMd5Hash(srcDirectory + "::" + targetDirectory);

            // Build pattern
            if ((null != patterns) && ("" != patterns))
            {
                string my_pattern = patterns.Replace("\r\n", "");
                Regex regex = new Regex(my_pattern);

                // Add files
                foreach (string path in Directory.GetFiles(srcDirectory))
                {
                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & (FileAttributes.Device
                        | FileAttributes.Hidden
                        | FileAttributes.System
                        | FileAttributes.Temporary)) != 0)
                    {
                        // Don't use
                    }
                    else
                    {
                        if ((null != exclude) && exclude.Match(path).Success)
                            continue;

                        if (regex.Match(Path.GetFileName(path)).Success)
                        {
                            SetupFile file = new SetupFile();
                            file.srcDirectory = Path.GetDirectoryName(path);
                            file.srcName = Path.GetFileName(path);
                            file.dstName = file.srcName;
                            //file.fileId = "ID_" + GetMd5Hash(path + "::"
                            //    + Path.Combine(file.dstDirectory, file.dstName));

                            component.componentFiles.Add(file);
                            rval = true;
                        }
                    }
                }
            }

            // Recurse
            if (recurse)
            {
                foreach (string path in Directory.GetDirectories(srcDirectory))
                {
                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & (FileAttributes.Device
                        | FileAttributes.Hidden
                        | FileAttributes.System
                        | FileAttributes.Temporary)) != 0)
                    {
                        // Dont use
                    }
                    else if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if ((null != exclude) && (exclude.Match(path + @"\").Success))
                            continue; // Excluded

                        bool probe = MakeComponents(components,
                            Path.Combine(srcDirectory,  Path.GetFileName(path)),
                            Path.Combine(targetDirectory, Path.GetFileName(path)));

                        if (!rval && probe)
                            rval = true; // We found files
                    }
                }
            }

            component.addToCreateFolder = true;

            if (preventEmptyDirectories && !rval)
                components.Remove(component); // Prevent empty dirs/structures

            return rval;
        }

        

        #endregion
    }
}
