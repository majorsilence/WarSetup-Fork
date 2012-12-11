using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WarSetup
{
    [XmlRoot("SetupBootstrap")]
    public class SetupBootstrap
    {
        private string _bundlePath = "";
        private string _bundleName = "";


        [
            CategoryAttribute("Bundle"),
            DescriptionAttribute("The path to the bundle file for use with wix burn"),
            XmlAttribute("BundlePath")
        ]
        public string BundlePath
        {
            get { return _bundlePath; }
            set { _bundlePath = value; }
        }

        [
            CategoryAttribute("Bundle"),
            DescriptionAttribute("The name of the bundle file"),
            XmlAttribute("BundleName")
        ]
        public string BundleName
        {
            get { return _bundleName; }
            set { _bundleName = value; }
        }
    }
}
