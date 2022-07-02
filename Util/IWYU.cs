using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IncludeMinimizer
{
    internal class IWYU
    {
        Process process = new Process();
        string output = "";

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

            
            process.StartInfo.Arguments = string.Join(" ", args.Select(x => " -Xiwyu " + x));
            settings.ClearFlag();
        }

        public string Start(string file)
        {
            output = "";

            Output.WriteLineAsync(string.Format("Running command '{0}' with following arguments:\n{1}\n\n", process.StartInfo.FileName, process.StartInfo.Arguments)).FireAndForget();

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();
            return output;
        }
        public async Task CancelAsync()
        {
            await Task.Run(delegate { process.Kill(); });
        }
    }
}
