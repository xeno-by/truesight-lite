using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Truesight.DebuggerVisualizers
{
    public static class GraphRenderer
    {
        public static FileInfo RenderAsPng(this String dotFile)
        {
            return new FileInfo(dotFile).RenderAsPng();
        }

        public static FileInfo RenderAsPng(this FileInfo dotFile)
        {
            var pathToAsm = MethodInfo.GetCurrentMethod().DeclaringType.Assembly.Location;
            var cfgXml = XDocument.Load(pathToAsm + ".config");
            var x_dotExePath = cfgXml.XPathSelectElement("//appSettings/add[@key='DotExePath']");
            var a_dotExePath = x_dotExePath == null ? null : x_dotExePath.Attribute("value");
            var dotExePath = a_dotExePath == null ? null : a_dotExePath.Value;
            if (dotExePath != null)
            {
                dotExePath = dotExePath.Replace("%ProgramFiles%", "$ProgramFiles$");
                dotExePath = Environment.ExpandEnvironmentVariables(dotExePath);

                var pfiles = Environment.GetEnvironmentVariable("ProgramFiles");
                var probe = dotExePath.Replace("$ProgramFiles$", pfiles);
                if (File.Exists(probe)) dotExePath = probe;
                else
                {
                    var pfiles_x86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                    probe = dotExePath.Replace("$ProgramFiles$", pfiles_x86);
                    if (File.Exists(probe)) dotExePath = probe;
                    else
                    {
                        var s_x86 = " (x86)";
                        if (pfiles != null && pfiles.EndsWith(s_x86))
                            pfiles = pfiles.Substring(0, pfiles.Length - s_x86.Length);
                        probe = dotExePath.Replace("$ProgramFiles$", pfiles);
                        if (File.Exists(probe)) dotExePath = probe;
                        else
                        {
                            // do nothing - we're out of guesses
                        }
                    }
                }
            }
            var x_cmdline = cfgXml.XPathSelectElement("//appSettings/add[@key='DotExeCommandLine']");
            var a_cmdline = x_cmdline == null ? null : x_cmdline.Attribute("value");
            var cmdline = a_cmdline == null ? null : a_cmdline.Value;

            if (File.Exists(dotExePath))
            {
                var pngFile = Path.ChangeExtension(dotFile.FullName, "png");
                var pngExists = File.Exists(pngFile);
                var pngAintYoungerThanDot = pngExists &&
                    File.GetLastWriteTime(dotFile.FullName) <= File.GetLastWriteTime(pngFile);

                if (pngExists && pngAintYoungerThanDot)
                {
                    return new FileInfo(pngFile);
                }
                else
                {
                    var pngBak = Path.ChangeExtension(pngFile, "bak");
                    if (pngExists)
                    {
                        if (File.Exists(pngBak)) File.Delete(pngBak);
                        File.Move(pngFile, pngBak);
                    }

                    try
                    {
                        var psi = new ProcessStartInfo(dotExePath);
                        psi.Arguments = String.Format(cmdline, pngFile, dotFile);
                        psi.UseShellExecute = false;
                        psi.RedirectStandardError = true;
                        psi.CreateNoWindow = true;

                        var p = Process.Start(psi);
                        var stderr = p.StandardError.ReadToEnd();
                        p.WaitForExit();

                        if (!File.Exists(pngFile))
                        {
                            if (File.Exists(pngBak)) File.Move(pngBak, pngFile);
                            var msg = String.Format(
                                "{0} has completed with the following message in stderr:{2}{1}",
                                dotExePath, stderr, Environment.NewLine);

                            if (File.Exists(pngFile))
                            {
                                var askUser = String.Format("{0}{1}" +
                                    "However, there seems to exist an image " +
                                    "that corresponds to an older version of the graph. " +
                                    "Do you wish to open it?",
                                    msg, Environment.NewLine);
                                var shouldOpenPng = MessageBox.Show(askUser, "An error has occurred",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button2) == DialogResult.Yes;
                                return shouldOpenPng ? new FileInfo(pngFile) : null;
                            }
                            else
                            {
                                MessageBox.Show(msg, "An error has occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                        }
                        else
                        {
                            return new FileInfo(pngFile);
                        }
                    }
                    finally
                    {
                        if (File.Exists(pngBak)) File.Delete(pngBak);
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}