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
        Dictionary<string, string> files = new();
        HashSet<string> fileNames = new();

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            Command.BeforeQueryStatus += UpdateVisibility;
            return base.InitializeCompletedAsync();
        }

        private void UpdateVisibility(object sender, EventArgs e)
        {
            try
            {
                var doc = VS.Documents.GetActiveDocumentViewAsync().Result;
                var ext = Path.GetExtension(doc.FilePath);
                Command.Visible = ext == ".h" || ext == ".hpp" || ext == ".hxx";
            }
            catch (Exception) {
                
            }
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = await General1.GetLiveInstanceAsync();
            if (settings.MapFile == "") return;

            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            if (doc == null) return;

            var path = doc.FilePath;
            var relative_path = settings.Prefix != "" ? GetRelativePath(path, settings.Prefix) : path;

            string text = doc.TextBuffer.CurrentSnapshot.GetText();
            var sresult = regex.Matches(text).OfType<System.Text.RegularExpressions.Match>().Select(m => m.Groups[1].Value).Distinct();

            string file_map = "#" + relative_path + '\n';
            relative_path = relative_path.Replace('\\', '/');
            fileNames.Add(relative_path);


            foreach (var match in sresult)
                file_map += string.Format("\t{{ include: [ \"{0}\", public, \"<{1}>\", public ] }},\n", match.Replace('\\', '/'), relative_path);
            files[path] = file_map;


            FileStream file = File.OpenWrite(settings.MapFile);
            List<Task> array = new();

            await file.WriteAsync(Encoding.ASCII.GetBytes("[\n"), 0, 2);

            foreach (var f in files)
                array.Append(file.WriteAsync(Encoding.ASCII.GetBytes(f.Value), 0, f.Value.Length));

            foreach (var f in array)
                await f;

            string x = "";
            foreach (var f in fileNames)
                x += string.Format("\t{{ include: [ \"\\\"{0}\\\"\", private, \"<{0}>\", public ] }},\n", f);

            await file.WriteAsync(Encoding.ASCII.GetBytes(x), 0, x.Length);
            await file.WriteAsync(Encoding.ASCII.GetBytes("]\n"), 0, 2);

            file.Close();
        }
    }
}
