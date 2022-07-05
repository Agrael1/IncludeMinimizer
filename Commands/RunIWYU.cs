using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace IncludeMinimizer.Commands
{
    internal class CancelCallback : IVsThreadedWaitDialogCallback
    {
        public delegate void Cancel();
        public Cancel cancel;
        void IVsThreadedWaitDialogCallback.OnCanceled()
        {
            cancel();
        }
    }

    [Command(PackageIds.RunIWYU)]
    internal sealed class RunIWYU : BaseCommand<RunIWYU>
    {
        IWYU proc = new();
        CancelCallback cancelCallback = new();

        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            cancelCallback.cancel = delegate { proc.CancelAsync().FireAndForget(); };
            return base.InitializeCompletedAsync();
        }

        async Task SaveAllDocumentsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                VCUtil.GetDTE().Documents.SaveAll();
            }
            catch (Exception e)
            {
                Output.WriteLineAsync($"Failed to get save all documents: {e.Message}").FireAndForget();
            }
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = await IWYUOptions.GetLiveInstanceAsync();
            if(settings.Executable == "" || !File.Exists(settings.Executable))return;
            var doc = await VS.Documents.GetActiveDocumentViewAsync();
            if (doc == null) return;


            if (settings.Dirty) proc.BuildCommandLine(settings);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dlg = (IVsThreadedWaitDialogFactory)await VS.Services.GetThreadedWaitDialogAsync();

            await SaveAllDocumentsAsync();

            IVsThreadedWaitDialog2 xdialog;
            dlg.CreateInstance(out xdialog);
            IVsThreadedWaitDialog4 dialog = xdialog as IVsThreadedWaitDialog4;
            
            dialog.StartWaitDialogWithCallback("Include Minimizer", "Running include-what-you-use", null, null, "Running include-what-you-use", true, 0, true, 0, 0, cancelCallback);

            var result = await proc.StartAsync(doc.FilePath, settings.AlwaysRebuid);

            if(dialog.EndWaitDialog() || result == false)return;

            await proc.ApplyAsync();
        }
    }
}
