namespace IncludeMinimizer.Commands
{
    [Command(PackageIds.RemMap)]
    internal sealed class RemoveMap : BaseCommand<RemoveMap>
    {
        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var settings = await General.GetLiveInstanceAsync();
            var file = Util.GetRelativePath(settings.MapFile, settings.Prefix);

            settings.
            Map.TryRemoveValue(file);
            settings.Map.WriteMapAsync(settings).FireAndForget();
        }
    }
}
