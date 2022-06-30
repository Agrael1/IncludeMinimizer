using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;

namespace IncludeMinimizer.Commands
{
    [Command(PackageIds.RemMap)]
    internal sealed class RemoveMap : BaseCommand<RemoveMap>
    {
        protected override Task InitializeCompletedAsync()
        {
            return base.InitializeCompletedAsync();
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            Command.Visible = General.Instance.IsInMapAsync().Result;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = await General.GetLiveInstanceAsync();
            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            var file = Util.GetRelativePath(doc.FilePath, settings.Prefix).Replace('\\', '/');

            settings.Map.TryRemoveValue(file);
            settings.Map.WriteMapAsync(settings).FireAndForget();
        }
    }
}
