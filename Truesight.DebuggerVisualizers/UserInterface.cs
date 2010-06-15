using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Truesight.DebuggerVisualizers
{
    internal class UserInterface
    {
        [STAThread]
        private static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DotViewerForm(args));
        }

        private class DotViewerForm : Form
        {
            public DotViewerForm(String[] args)
            {
                if (args.Count() == 1)
                {
                    var arg = args.Single();
                    if (String.Compare(arg, "/register", false) == 0 ||
                        String.Compare(arg, "/reg", false) == 0)
                    {
                        Register();
                    }
                    else if (String.Compare(arg, "/unregister", false) == 0 ||
                        String.Compare(arg, "/unreg", false) == 0)
                    {
                        Unregister();
                    }
                    else
                    {
                        Open(new FileInfo(arg));
                    }
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                Close();
            }

            private void Register()
            {
                Unregister();

                // open the classes root
                var classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes",
                    RegistryKeyPermissionCheck.ReadWriteSubTree,
                    RegistryRights.EnumerateSubKeys | RegistryRights.CreateSubKey | RegistryRights.SetValue);

                // redirect .dotgraph to dotgraphfile
                var dotgraph = classes.CreateSubKey(".dotgraph");
                dotgraph.SetValue("", "dotgraphfile");

                // create dotgraphfile registration
                var dotgraphfile = classes.CreateSubKey(@"dotgraphfile\shell\Open with GraphViz\command");
                dotgraphfile.SetValue("", Assembly.GetExecutingAssembly().Location + " \"%1\"");
            }

            private void Unregister()
            {
                var classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes",
                    RegistryKeyPermissionCheck.ReadWriteSubTree,
                    RegistryRights.EnumerateSubKeys | RegistryRights.Delete);

                var dotgraph = classes.OpenSubKey(".dotgraph");
                var dotgraphfile = classes.OpenSubKey("dotgraphfile");
                if (dotgraph != null) classes.DeleteSubKeyTree(".dotgraph");
                if (dotgraphfile != null) classes.DeleteSubKeyTree("dotgraphfile");
            }

            private void Open(FileInfo dotFile)
            {
                var pngFile = dotFile.RenderAsPng();
                if (pngFile != null)
                {
                    Process.Start(pngFile.FullName);
                }
            }
        }
    }
}
