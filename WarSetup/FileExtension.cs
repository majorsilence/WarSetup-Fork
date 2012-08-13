using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;
using System.Globalization;

namespace WarSetup
{

    [
        TypeConverterAttribute(typeof(FileExtensionConverter)),
        DefaultPropertyAttribute("Extension"),
        DescriptionAttribute("File extension registration."),
        XmlRoot("SetupFile")
    ]
    public class FileExtension
    {
        #region Variables
        private string _Id = "";
        private string _Extension = "";
        private string _MimeType = "";
        private string _Description = "";
        private int _IconIndex = 0;
        #endregion

        #region Properties

        [
            CategoryAttribute("File Extension"),
            DescriptionAttribute("Unique ID for the file-extension."),
            XmlAttribute("Id")
        ]
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [
            CategoryAttribute("File Extension"),
            DescriptionAttribute("File extension to register. Just the extension, like \"doc\""),
            XmlAttribute("Extension")
        ]
        public string Extension
        {
            get { return _Extension; }
            set { _Extension = value; }
        }

        [
            CategoryAttribute("File Extension"),
            DescriptionAttribute("Mime-type to use for the extension; like \"Binary/YourAppName\""),
            XmlAttribute("MimeType")
        ]
        public string MimeType
        {
            get { return _MimeType; }
            set { _MimeType = value; }
        }

        [
            CategoryAttribute("File Extension"),
            DescriptionAttribute("Description to associate with thw shortcut"),
            XmlAttribute("Description")
        ]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        [
            CategoryAttribute("File Extension"),
            DescriptionAttribute("Index to the icon in the file."),
            XmlAttribute("IconIndex")
        ]
        public int IconIndex
        {
            get { return _IconIndex; }
            set { _IconIndex = value; }
        }

        #endregion

        #region Methods

        FileExtension()
        {
            _Id = MainFrame.CurrentProject.GetUniqueId();
        }

        #endregion
    }

    public class FileExtensionConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(FileExtension))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is FileExtension)
            {
                FileExtension fe = (FileExtension)value;
                return fe.Extension;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
