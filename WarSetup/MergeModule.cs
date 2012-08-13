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
    [XmlRoot("MergeModule")]
    public class MergeModule
    {
        public class Properties
        {
            public enum Types
            {
                Text,
                Key,
                Integer,
                Bitfield
            };

            public string _Id;
            public string _Name;
            public Types _Type;
            public string _DefaultValue;
            public string _Value = "";
            public bool _Nullable = true;
            public string _Description;

            [ ReadOnlyAttribute(true) ]
            public string Description
            {
                get { return _Description; }
                set { _Description = value; }
            }

            [ReadOnlyAttribute(true)]
            public string Id
            {
                get { return _Id; }
                set { _Id = value; }
            }

            [ReadOnlyAttribute(true)]
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            [ReadOnlyAttribute(true)]
            public Types Type
            {
                get { return _Type; }
                set { _Type = value; }
            }

            [ReadOnlyAttribute(true)]
            public string DefaultValue
            {
                get { return _DefaultValue; }
                set { _DefaultValue = value; }
            }

            public string Value
            {
                get { return _Value; }
                set { _Value = value; }
            }

            [ReadOnlyAttribute(true)]
            public bool Nullable
            {
                get { return _Nullable; }
                set { _Nullable = value; }
            }
        
            public Properties()
            {
            }
        }

        private string _srcPath;

        // Module-data are fetched on demand
        private string _moduleId;
        private string _moduleVersion;
        private string _mouleDescription;
        private string _moduleComments;
        private string _moduleManufacturer;
        private string _moduleInstallerVersion;
        private string _modulePlatforms;
        private string _moduleLanguage;
        private bool _excludeFromBuild = false;

        private List<Properties> _moduleProperties;

        [CategoryAttribute("Module Properties"),
        DescriptionAttribute("Configurable properties within the module")]
        public List<Properties> ModuleProperties
        {
            get { return _moduleProperties; }
            set { _moduleProperties = value; }
        }


        [CategoryAttribute("Build options"),
         DescriptionAttribute("Exclude from build. The merge-module is ignored."), 
        XmlAttribute("excludeFromBuild")]
        public bool ExcludeFromBuild
        {
            get { return _excludeFromBuild; }
            set { _excludeFromBuild = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Source path"), 
         XmlAttribute("srcPath"),
         ReadOnlyAttribute(true)]
        public string SrcPath
        {
            get { return _srcPath; }
            set { _srcPath = value; /*GetInfo();*/ }
        }

        public string FileName
        {
            get { return Path.GetFileName(SrcPath); }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Module ID"),
         ReadOnlyAttribute(true)]
        public string ModuleId
        {
            get { return _moduleId; }
            set { _moduleId = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("SLanguage of the module"),
         ReadOnlyAttribute(true)]
        public string ModuleLanguage
        {
            get 
            {
                if ((null == _moduleLanguage) || ("" == _moduleLanguage))
                    return "0";
                return _moduleLanguage; 
            }
            set { _moduleLanguage = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Varsion of the module"),
         ReadOnlyAttribute(true)]
        public string ModuleVersion
        {
            get { return _moduleVersion; }
            set { _moduleVersion = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Description of the module"),
         ReadOnlyAttribute(true)]
        public string MouleDescription
        {
            get { return _mouleDescription; }
            set { _mouleDescription = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Comments about the module"),
         ReadOnlyAttribute(true)]
        public string ModuleComments
        {
            get { return _moduleComments; }
            set { _moduleComments = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Manifacturer"),
         ReadOnlyAttribute(true)]
        public string ModuleManufacturer
        {
            get { return _moduleManufacturer; }
            set { _moduleManufacturer = value; }
        }

        [CategoryAttribute("Module Properties"),
         DescriptionAttribute("Platforms"),
         ReadOnlyAttribute(true)]
        public string ModulePlatforms
        {
            get { return _modulePlatforms; }
            set { _modulePlatforms = value; }
        }

        public MergeModule()
        {
            _moduleProperties = new List<Properties>();
        }

        // Get information about the merge-module
        public void LoadInfo()
        {
            string tmp_file = Path.GetTempFileName();
            string cmd = "";

            try
            {
                string[] args = new string[3];

                cmd = Shell.GetWixBinary("dark.exe");
                args[0] = SrcPath;
                args[1] = "-xo";
                args[2] = tmp_file;

                if (!Shell.Execute(cmd, args))
                {
                    MessageBox.Show("Decompilation of \"" + SrcPath + "\" failed.");
                    return;
                }

                ParseXml(tmp_file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Decompilation of \"" + SrcPath + "\" failed.\r\n" + ex.ToString());
                return;
            }
            finally
            {
                File.Delete(tmp_file);
            }
        }

        // Obsolete - parse from WIX format. 
        // Some msi files (like Crystal Reports XI) are invalid, and
        // cannot be deparsed to MSI format.
        private void Parse(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList modules = doc.GetElementsByTagName("Module");
            XmlNodeList packages = doc.GetElementsByTagName("Package");
            if ((null == modules) 
                || (0 == modules.Count)
                || (null == packages) 
                || (0 == packages.Count))
            {
                MessageBox.Show("Invalid merge-module: " + SrcPath);
                return;
            }

            XmlElement module = (XmlElement)modules[0];
            XmlElement package = (XmlElement)packages[0];

            _moduleId = module.GetAttribute("Id");
            _mouleDescription = package.GetAttribute("Description");
            _moduleComments = package.GetAttribute("Comments");
            _moduleManufacturer = package.GetAttribute("Manufacturer");
            _moduleInstallerVersion = package.GetAttribute("InstallerVersion");
            _modulePlatforms = package.GetAttribute("Platforms");

        }

        // Parse from raw XML (-xo switch to dark.exe) table format.
        // Fagile implementation that assumes that the format is as
        // in inspected xml files.
        private void ParseXml(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            // Get basic stuff
            {
                XmlNodeList rows = GetTable("ModuleSignature", doc).GetElementsByTagName("row");
                XmlElement row = (XmlElement)rows[0];
                XmlNodeList fields = row.GetElementsByTagName("field");
                _moduleId = fields[0].InnerText;
                _moduleLanguage = fields[1].InnerText;
                _moduleVersion = fields[2].InnerText;
            }

            // Get information
            // Format in: http://msdn2.microsoft.com/en-us/library/aa372045.aspx
            // Article: "Summary Information Stream Property Set"
            {
                XmlNodeList rows = GetTable("_SummaryInformation", doc).GetElementsByTagName("row");
                _mouleDescription = GetFieldData(rows, 2);
                _moduleComments = GetFieldData(rows, 6);
                _moduleManufacturer = GetFieldData(rows, 4);
                _moduleInstallerVersion = "";
                _modulePlatforms = GetFieldData(rows, 7);
            }


            // Get properties
            {
                XmlNodeList rows = GetTable("ModuleConfiguration", doc).GetElementsByTagName("row");

                foreach (XmlElement row in rows)
                {
                    Properties prop = new Properties();

                    XmlNodeList fields = row.GetElementsByTagName("field");

                    prop._Id = fields[0].InnerText;

                    string value = fields[1].InnerText;
                    if (value == "0")
                        prop._Type = Properties.Types.Text;
                    else if (value == "1")
                        prop._Type = Properties.Types.Key;
                    else if (value == "2")
                        prop._Type = Properties.Types.Integer;
                    else
                        prop._Type = Properties.Types.Bitfield;

                    if ((null != fields[5].InnerText) && ("" != fields[5].InnerText))
                        prop._Nullable = ((int.Parse(fields[5].InnerText) & 2) == 2);
                    prop._Name = fields[0].InnerText;
                    prop._DefaultValue = fields[4].InnerText;
                    prop._Value = fields[4].InnerText;
                    prop._Description = fields[7].InnerText;

                    // Remove the attributes if it exists, but preserve the value in the replacement
                    {
                        foreach (Properties existing in _moduleProperties)
                        {
                            if ((null != existing.Id) && (prop.Id == existing.Id))
                            {
                                prop.Value = existing.Value;
                                _moduleProperties.Remove(existing);
                                break;
                            }
                        }
                    }

                    _moduleProperties.Add(prop);
                }
            }
        }

        private string GetFieldData(XmlNodeList rows, int filedNo)
        {
            string field_no = filedNo.ToString();

            foreach (XmlElement row in rows)
            {
                XmlNodeList fields = row.GetElementsByTagName("field");
                string value = fields[0].InnerText;
                if (value == field_no)
                    return fields[1].InnerText;
            }

            return "";
            //XmlElement[] fields = row.GetElementsByTagName("field");
            //return fields[1].Value;
        }

        private XmlElement GetTable(string name, XmlDocument doc)
        {
            //XmlNodeList content = doc.GetElementsByTagName("wixOutput");
            //XmlElement content_el = (XmlElement)content[0];
            XmlNodeList tables = doc.GetElementsByTagName("table");

            foreach (XmlElement table in tables)
            {
                string tbl_name = table.GetAttribute("name");
                if (name == tbl_name)
                    return table;
            }

            throw new Exception("Cannot find table with name = \"" + name + "\".");
        }
    }
}
