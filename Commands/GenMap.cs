using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.VisualStudio.Threading.AsyncReaderWriterLock;

namespace IncludeMinimizer
{
    [Command(PackageIds.GenMap)]
    internal sealed class GenMap : BaseCommand<GenMap>
    {
        readonly Regex regex = new("\\s*#include\\s*([<\"].*[\">])");


        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = await General.GetLiveInstanceAsync();
            if (settings.MapFile == "") { VS.MessageBox.ShowErrorAsync("Map output error", "Map output file is empty, go to Tools->Options->Include Minimizer->General and set the output file!").FireAndForget(); return; }

            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            if (doc == null) return;

            var path = doc.FilePath;
            var relative_path = settings.Prefix != "" ? Util.GetRelativePath(path, settings.Prefix) : path;
            relative_path = relative_path.Replace('\\', '/');

            string text = doc.TextBuffer.CurrentSnapshot.GetText();
            var sresult = regex.Matches(text).OfType<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).Distinct();

            if (!sresult.Any()) return;

            string file_map = "";
            foreach (var match in sresult)
                file_map += string.Format("\t{{ include: [ \"{0}\", public, \"<{1}>\", public ] }},\n", match.Replace('\\', '/'), relative_path);
            settings.Map.Map[relative_path] = file_map;

            settings.Map.WriteMapAsync(settings).FireAndForget();
        }
    }
}
