using System;
using System.IO;
using System.Reflection;
using System.Text;
using XenoGears;

namespace Truesight.TextGenerators.Core
{
    public static class Helpers
    {
        public static void GenerateIntoFile(String targetFile, Func<String> logic)
        {
            GenerateIntoFile(targetFile, buffer => buffer.Append(logic()));
        }

        public static void GenerateIntoFile(String targetFile, Action<StringBuilder> logic)
        {
            var buffer = new StringBuilder();
            logic(buffer);

            using (var targetFileWriter = new StreamWriter(targetFile))
            {
                targetFileWriter.WriteLine(Constants.CodegenDisclaimer);
                targetFileWriter.WriteLine();
                targetFileWriter.Write(buffer.ToString());
            }
        }

        public static void GenerateIntoClass(
            String targetFile, String @namespace, String classDeclaration, Func<String> logic)
        {
            GenerateIntoClass(targetFile, @namespace, classDeclaration, buffer => buffer.Append(logic()));
        }

        public static void GenerateIntoClass(
            String targetFile, String @namespace, String classDeclaration, Action<StringBuilder> logic)
        {
            var buffer = new StringBuilder();
            logic(buffer);

            using (var targetFileWriter = new StreamWriter(targetFile))
            {
                targetFileWriter.WriteLine(Constants.CodegenDisclaimer);
                targetFileWriter.WriteLine();

                var textGeneratedIntoClass = typeof(Helpers).Assembly.ReadAllText("Truesight.TextGenerators.Core.TextGeneratedIntoClass.template");
                textGeneratedIntoClass = textGeneratedIntoClass
                    .Replace("%NAMESPACE_NAME%", @namespace)
                    .Replace("%CLASS_DECLARATION%", classDeclaration)
                    .Replace("%GENERATED_TEXT%", buffer.ToString());

                if (classDeclaration.Contains("enum") || classDeclaration.Contains("interface"))
                {
                    textGeneratedIntoClass = textGeneratedIntoClass
                        .Replace("    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]" + Environment.NewLine, "");
                }

                targetFileWriter.Write(textGeneratedIntoClass);
            }
        }

        public static void GenerateIntoNamespace(
            String targetFile, String @namespace, Func<String> logic)
        {
            GenerateIntoNamespace(targetFile, @namespace, buffer => buffer.Append(logic()));
        }

        public static void GenerateIntoNamespace(
            String targetFile, String @namespace, Action<StringBuilder> logic)
        {
            var buffer = new StringBuilder();
            logic(buffer);

            using (var targetFileWriter = new StreamWriter(targetFile))
            {
                targetFileWriter.WriteLine(Constants.CodegenDisclaimer);
                targetFileWriter.WriteLine();

                var textGeneratedIntoNamespace = typeof(Helpers).Assembly.ReadAllText("Truesight.TextGenerators.Core.TextGeneratedIntoNamespace.template");
                textGeneratedIntoNamespace = textGeneratedIntoNamespace
                    .Replace("%NAMESPACE_NAME%", @namespace)
                    .Replace("%GENERATED_TEXT%", buffer.ToString());

                targetFileWriter.Write(textGeneratedIntoNamespace);
            }
        }

        public static String ReadAllText(this Assembly asm, String resource)
        {
            using (var res = asm.GetManifestResourceStream(resource))
            {
                return new StreamReader(res).ReadToEnd();
            }
        }

        public static String FixupMethodDeclarations(String res)
        {
            res = res.Trim();
            var eoln = Environment.NewLine;
            while (res.Contains(eoln + eoln))
            {
                res = res.Replace(eoln + eoln, eoln);
            }

            res = res.Replace("}" + eoln + "[", "}" + eoln + eoln + "[");
            res = res.Replace("}" + eoln + "public", "}" + eoln + eoln + "public");
            const string indent = "        ";
            return indent + res.Replace(eoln, eoln + indent);
        }
    }
}
