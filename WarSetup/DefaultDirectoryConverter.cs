using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace WarSetup
{
    public class DefaultDirectoryConverter : StringConverter 
    {
        public override bool GetStandardValuesSupported(
                           ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection
                     GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> rval = new List<string>();

            MainFrame.CurrentProject.EnumTargetDirs(rval);

            rval.Add("-- SYSTEM folders below --");

            foreach(string path in SetupProject.WindowsInstallerKnownDirs)
            {
                rval.Add("[" + path + "]");
            }
           
            return new StandardValuesCollection(rval);
        }

    }
}
