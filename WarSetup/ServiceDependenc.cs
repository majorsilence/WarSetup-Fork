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
    [
        TypeConverterAttribute(typeof(ServiceDependencyConverter)),
        DefaultPropertyAttribute("Id"),
        XmlRoot("ServiceDependency")
    ]
    public class ServiceDependency
    {
        #region Variables
        private string _Id = "";
        private bool _IsGroup = false;
        #endregion


        #region Attributes

        [
           CategoryAttribute("Service Dependency"),
           DescriptionAttribute("The nameof a previously installed service, "
                + "A foreign key referring to another ServiceInstall/@Id or A group of services"),
           XmlAttribute("Id")
        ]
        public string Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [
           CategoryAttribute("Service Dependency"),
           DescriptionAttribute("Enable to indicate that the value in the Id attribute is the name of a group of services."),
           XmlAttribute("IsGroup")
        ]
        public bool IsGroup
        {
            get { return _IsGroup; }
            set { _IsGroup = value; }
        }

        #endregion
    }

    public class ServiceDependencyConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(ServiceDependency))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is ServiceDependency)
            {
                ServiceDependency fe = (ServiceDependency)value;
                return fe.Id;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
