using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace WarSetup
{
    public class Shell
    {
        public static void ShellExecute(string command)
        {
            //using (Process prc = new Process())
            {
                try
                {
                    //ProcessStartInfo psi = new ProcessStartInfo(command);
                    //psi.UseShellExecute = true;
                    //psi.RedirectStandardError = false;
                    //prc.StartInfo = psi;
                    //prc.Start();
                    Process.Start(command);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to run command:\r\n\""
                        + command + "\".\r\n" + ex.Message);
                }
            }
        }

        public static bool Execute(string command, IList<string> args)
        {
            bool rval = false;

            using (Process prc = new Process())
            {
                StreamReader stdout;
                Cursor.Current = Cursors.WaitCursor;

                ProcessStartInfo psi = new ProcessStartInfo(command);
                psi.UseShellExecute = false;
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;

                string arguments = "";
                foreach (string arg in args)
                {
                    arguments += "\"" + arg + "\" ";
                }

                arguments.TrimEnd();
                psi.Arguments = arguments;

                psi.Arguments = arguments;
                prc.StartInfo = psi;

                //if (output != null)
                MainFrame.AddToCompilerOutputWindow("Executing: \"" + command + "\" " + arguments + "\r\n");

                prc.Start();
                stdout = prc.StandardError;
                stdout = prc.StandardOutput;

                //if (output == null)
                //    prc.WaitForExit();
                //else
                    MainFrame.AddToCompilerOutputWindow(stdout.ReadToEnd());

                rval = prc.ExitCode == 0;
            }

            return rval;
        }

        public static void Browse(string path)
        {
            using (Process prc = new Process())
            {
                prc.StartInfo.FileName = path;
                prc.StartInfo.UseShellExecute = true;
                prc.Start();
            }
        }

        public static string wixPath
        {
            get
            {
                string rval = Properties.Settings.Default.wixPath;
                if ((rval == null) || (rval == ""))
                {
                    rval = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                    if ((rval == null) || (rval == ""))
                        rval = System.Environment.GetEnvironmentVariable("ProgramFiles");
                    if ((rval == null) || (rval == ""))
                        rval = @"C:\Program Files";

                    if (System.IO.Directory.Exists(System.IO.Path.Combine(rval, "WiX Toolset v3.6")))
                    {
                        // Attempt to use version 3.6 new directory
                        rval = System.IO.Path.Combine(rval, "WiX Toolset v3.6");
                    }
                    else if (System.IO.Directory.Exists(System.IO.Path.Combine(rval, "Windows Installer XML v3.6")))
                    {
                        // Attempt to use version 3.6
                        rval = System.IO.Path.Combine(rval, "Windows Installer XML v3.6");
                    }
                    else if (System.IO.Directory.Exists(System.IO.Path.Combine(rval, "Windows Installer XML v3.5")))
                    {
                        // Attempt to use version 3.5
                        rval = System.IO.Path.Combine(rval, "Windows Installer XML v3.5");
                    }
                    else
                    {
                        // Fall back to version 3
                        rval = Path.Combine(rval, "Windows Installer XML v3");
                    }

                   
                    rval = Path.Combine(rval, "bin");
                }

                return rval;
            }
        }

        public static string mergeModulesPathX64
        {
            get
            {
                string path = Properties.Settings.Default.mergePathX32;
                if ("" != path)
                    return path;

                if ((path == null) || (path == ""))
                    path = System.Environment.GetEnvironmentVariable("CommonProgramFiles");

                if ((path == null) || (path == ""))
                    path = @"C:\Program Files\Common Files";

                path = Path.Combine(path, "Merge Modules");
                return path;
            }
        }

        public static string mergeModulesPathX32
        {
            get
            {
                string path = Properties.Settings.Default.mergePathX32;
                if ("" != path)
                    return path;

                path = System.Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");

                if ((path == null) || (path == ""))
                    path = System.Environment.GetEnvironmentVariable("CommonProgramFiles");

                if ((path == null) || (path == ""))
                    path = @"C:\Program Files\Common Files";

                path = Path.Combine(path, "Merge Modules");
                return path;
            }
        }

        public static string GetWixBinary(string name)
        {
            return Path.Combine(Shell.wixPath, name);
        }

        public static string warSetupResPath
        {
            get
            {
                string path = Properties.Settings.Default.warSetupResPath;
                if ((null == path) || ("" == path))
                {
                    path = Application.StartupPath;
                }

                return path;
            }
        }

        // Get a relative path from rootPath to fullPath
        // If rootPath = "C:\test" and fullPath = "C:\test\subdir\readme.txt"
        // the result is "subdir\readme.txt"
        // Allow the relative path to start one level down from root
        //
        // If rootPath is null, the current directory is used
        static public string GetRelativePath(string rootPath, string fullPath)
        {
            if (null == rootPath)
                rootPath = Environment.CurrentDirectory;

            char[] sep = { '\\', '/' };
            string[] root = rootPath.Split(sep);
            string[] full = fullPath.Split(sep);

            if (full.Length < root.Length)
                return fullPath;

            string rval = @".\";

            int i;
            for (i = 0; (i < root.Length); i++ )
            {
                if (root[i].ToLower() != full[i].ToLower())
                {
                    if ((root.Length > 2) && (i == (root.Length - 1)))
                    {
                        rval = @"..\";
                        break;
                    }
                    else
                        return fullPath; // Not relative
                }
            }

            
            for (; i < full.Length; i++)
            {
                rval = Path.Combine(rval, full[i]);
            }

            return rval;
        }

        static public string GetRelativePath(string fullPath)
        {
            return GetRelativePath(null, fullPath);
        }
    }
}
