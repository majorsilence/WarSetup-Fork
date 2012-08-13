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
        TypeConverterAttribute(typeof(ServiceInstallConverter)),
        DefaultPropertyAttribute("serviceName"),
        DescriptionAttribute("Windows Servicen."),
        XmlRoot("ServiceInstall")
    ]
    public class ServiceInstall
    {
        #region Enums
        public enum ErrorControlE
        {
            ignore,
            normal,
            critical
        };

        public enum StartE
        {
            auto,
            demand,
            disabled
        };

        public enum RemoveE
        {
            install,
            uninstall,
            both
        };

       
        #endregion 

        #region Variables
        private string _id;
        private bool _isService = false;
        private string _userAccount = "";
        private string _userPassword = "";
        private string _cmdLineArguments = "";
        private string _description;
        private ErrorControlE _errorControl = ErrorControlE.normal;
        private bool _interactive = false;
        private string _loadOrderGroup = "";
        private string _serviceName = "";
        private StartE _startMode = StartE.auto;
        private bool _vital = true;
        private List<ServiceDependency> _dependencies;
        private RemoveE _Remove = RemoveE.both;
        private bool _StartWhenInstalled = true;
        #endregion

        #region Attributes

        [
            CategoryAttribute("Install Options"),
            DescriptionAttribute("Specifies whether the service should be Stared when it is installed."),
            XmlAttribute("StartWhenInstalled")
        ]
        public bool StartWhenInstalled
        {
            get { return _StartWhenInstalled; }
            set { _StartWhenInstalled = value; }
        }

        [
            CategoryAttribute("Install Options"),
            DescriptionAttribute("Specifies whether the service should be removed on install, uninstall or both."),
            XmlAttribute("Remove")
        ]
        public RemoveE Remove
        {
            get { return _Remove; }
            set { _Remove = value; }
        }

        [
           CategoryAttribute("Setup internals"),
           DescriptionAttribute("Unique ID for the service."),
           XmlAttribute("id")
        ]
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }


        [
           CategoryAttribute("Service"),
           DescriptionAttribute("If enabled, this file is installed as a Windows Service."),
           XmlAttribute("isService")
        ]
        public bool isService
        {
            get { return _isService; }
            set { _isService = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("The acount under which to start the service."),
           XmlAttribute("userAccount")
        ]
        public string userAccount
        {
            get { return _userAccount; }
            set { _userAccount = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("The password for the account. Valid only when the account has a password"),
           XmlAttribute("userPassword")
        ]
        public string userPassword
        {
            get { return _userPassword; }
            set { _userPassword = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Contains any command line arguments or properties required to run the service."),
           XmlAttribute("cmdLineArguments")
        ]
        public string cmdLineArguments
        {
            get { return _cmdLineArguments; }
            set { _cmdLineArguments = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Sets the description of the service"),
           XmlAttribute("description")
        ]
        public string description
        {
            get { return _description; }
            set { _description = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Determines what action should be taken on an error."),
           XmlAttribute("errorControl")
        ]
        public ErrorControlE errorControl
        {
            get { return _errorControl; }
            set { _errorControl = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Whether or not the service interacts with the desktop."),
           XmlAttribute("interactive")
        ]
        public bool interactive
        {
            get { return _interactive; }
            set { _interactive = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("The load ordering group that this service should be a part of."),
           XmlAttribute("loadOrderGroup")
        ]
        public string loadOrderGroup
        {
            get { return _loadOrderGroup; }
            set { _loadOrderGroup = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("The name of the service."),
           XmlAttribute("serviceName")
        ]
        public string serviceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Determines when the service should be started."),
           XmlAttribute("startMode")
        ]
        public StartE startMode
        {
            get { return _startMode; }
            set { _startMode = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("The overall install should fail if this service fails to install."),
           XmlAttribute("vital")
        ]
        public bool vital
        {
            get { return _vital; }
            set { _vital = value; }
        }

        [
           CategoryAttribute("Service"),
           DescriptionAttribute("Services this service depends on (must be running before this service is started)."),
           XmlElement("dependencies")
        ]
        public List<ServiceDependency> dependencies
        {
            get { return _dependencies; }
            set { _dependencies = value; }
        }

        #endregion

        public ServiceInstall()
        {
            _dependencies = new List<ServiceDependency>();
            _id = MainFrame.CurrentProject.GetUniqueId();
        }
    }

    public class ServiceInstallConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(ServiceInstall))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is ServiceInstall)
            {
                ServiceInstall fe = (ServiceInstall)value;
                return fe.serviceName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
