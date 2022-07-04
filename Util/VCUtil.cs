using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncludeMinimizer
{
    public class VCUtil
    {
        static EnvDTE.Project cached_project;
        public static EnvDTE80.DTE2 GetDTE()
        {
            var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                throw new System.Exception("Failed to retrieve DTE2!");
            }
            return dte;
        }
        public static async Task<EnvDTE.Project> GetProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var doc = GetDTE().ActiveDocument;
            return doc.ProjectItem?.ContainingProject;
        }

        static List<string> GetIncludes(VCCLCompilerTool compiler)
        {
            List<string> includes = new List<string>();
            includes.AddRange(compiler.AdditionalIncludeDirectories.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            return includes.Select(x => Path.GetFullPath(x.Trim()).Replace('\\', '/')).ToList();
        }

        public static async Task<string> GetCommandLineAsync(bool rebuild)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var prj = await GetProjectAsync();

            if (cached_project == prj && !rebuild)
                return "";

            cached_project = prj;

            var proj = prj.Object as VCProject;
            if (proj == null) { VS.MessageBox.ShowErrorAsync("IWYU Error:", "The project is not a Visual Studio C/C++ type.").FireAndForget(); return null; }

            var cfg = proj.ActiveConfiguration;
            var cl = cfg?.Rules;
            if (cl == null) { VS.MessageBox.ShowErrorAsync("IWYU Error:", "Failed to gather Compiler info.").FireAndForget(); return null; }

            var com = (IVCRulePropertyStorage2)cl.Item("CL");
            var xstandard = com.GetEvaluatedPropertyValue("LanguageStandard");
            var defs = com.GetEvaluatedPropertyValue("PreprocessorDefinitions")
                .Split(';').Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(x => "-D" + x);

            //old fashion, but works
            var tools = (IVCCollection)cfg.Tools;
            var compiler = (VCCLCompilerTool)tools?.Item("VCCLCompilerTool");
            if (compiler == null)
            {
                VS.MessageBox.ShowErrorAsync("IWYU Error:", "Failed to gather Compiler info.").FireAndForget(); return null;
            }
            var includes = GetIncludes(compiler).Select(x => "-I \"" + x + "\"");

            string standard;
            switch (xstandard)
            {
                default:
                case "stdcpp20":
                    standard = "-std=c++20";
                    break;
                case "stdcpp17":
                    standard = "-std=c++17";
                    break;
                case "stdcpp14":
                case "Default":
                    standard = "-std=c++14";
                    break;
            }

            var inc_string = string.Join(" ", includes);
            var def_string = string.Join(" ", defs);


            return inc_string + ' ' + def_string + ' ' + standard;
        }

        //public static async Task<EnvDTE.Project> Get(EnvDTE.Project proj)
        //{
        //    VS.Solutions.GetActiveProjectAsync().Result
        //    return GetDTE().ActiveDocument.ProjectItem?.ContainingProject;
        //}
    }
}
