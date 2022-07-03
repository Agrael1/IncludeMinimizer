using Community.VisualStudio.Toolkit;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static IncludeMinimizer.OptionsProvider;

namespace IncludeMinimizer
{
    internal class IWYU
    {
        Process process = new Process();
        string output = "";
        string command_line = "";
        string support_path = "";

        public IWYU()
        {
            process.EnableRaisingEvents = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (s, args) =>
            {
                output += args.Data + "\n";
            };
            process.ErrorDataReceived += (s, args) =>
            {
                output += args.Data + "\n";
            };
        }

        public void BuildCommandLine(IWYUOptions settings)
        {
            process.StartInfo.FileName = settings.Executable;

            List<string> args = new List<string>();

            switch (settings.Comms)
            {
                case Comment.Always: args.Add("--update_comments"); break;
                case Comment.Never: args.Add("--no_comments"); break;
                case Comment.Default: break;
            }
            args.Add(string.Format("--verbose={0}", settings.Verbosity));

            if (settings.Precompiled)
                args.Add("--pch_in_code");
            if (settings.Transitives)
                args.Add("--transitive_includes_only");
            if (settings.NoDefault)
                args.Add("--no_default_mappings");
            if (settings.MappingFile != "")
                args.Add(string.Format("--mapping_file=\"{0}\"", settings.MappingFile));
            args.Add("--max_line_length=256"); // output line for commentaries

            command_line =
                string.Join(" ", args.Select(x => " -Xiwyu " + x));

            if (settings.ClangOptions != null && settings.ClangOptions?.Count() != 0)
                command_line += " " + string.Join(" ", settings.ClangOptions);
            if (settings.Options != null && settings.Options.Count() != 0)
                command_line += " " + string.Join(" ", settings.Options.Select(x => " -Xiwyu " + x));
            settings.ClearFlag();
        }

        public async Task<string> StartAsync(string file)
        {
            output = "";
            var cmd = await VCUtil.GetCommandLineAsync();
            if (cmd == null) return null;
            if (cmd != "")
            {
                support_path = Path.GetTempFileName();
                File.WriteAllText(support_path, cmd);
            }
            process.StartInfo.Arguments = $"{command_line} \"@{support_path}\" {file}";

            Output.WriteLineAsync(string.Format("Running command '{0}' with following arguments:\n{1}\n\n", process.StartInfo.FileName, process.StartInfo.Arguments)).FireAndForget();

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            Output.WriteLineAsync(output).FireAndForget();
            return output;
        }
        public async Task CancelAsync()
        {
            await Task.Run(delegate { process.Kill(); });
        }
    }
}
