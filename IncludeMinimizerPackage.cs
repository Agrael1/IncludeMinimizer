global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;

namespace IncludeMinimizer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.IncludeMinimizerString)]
    [ProvideOptionPage(typeof(OptionsProvider.General1Options), "Include Minimizer", "General", 0, 0, true, SupportsProfiles = true)]
    [ProvideAutoLoad(PackageGuids.GHeaderOnlyString, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(PackageGuids.GHeaderOnlyString, "UIOnlyHeader",
    expression: "userWantsToSeeIt",
    termNames: new[] { "userWantsToSeeIt" },
    termValues: new[] { @"HierSingleSelectionName:.(h|hpp|hxx)$" }
)]
    public sealed class IncludeMinimizerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
        }
    }
}