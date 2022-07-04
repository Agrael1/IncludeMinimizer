global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Threading;

namespace IncludeMinimizer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.IncludeMinimizerString)]
    [ProvideOptionPage(typeof(OptionsProvider.XMapGenOptions), "Include Minimizer", "Map Generator", 0, 0, true, SupportsProfiles = true)]
    [ProvideOptionPage(typeof(OptionsProvider.XIWYUOptions), "Include Minimizer", "Include-What-You-Use", 0, 0, true, SupportsProfiles = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideUIContextRule(PackageGuids.GHeaderOnlyString, "UIOnlyHeader",
    expression: "userWantsToSeeIt",
    termNames: new[] { "userWantsToSeeIt" },
    termValues: new[] { @"HierSingleSelectionName:.(h|hpp|hxx)$" }
)]
    [ProvideUIContextRule(PackageGuids.GOnlyVCString, "UIOnlyVC",
    expression: "one & two",
    termNames: new[] { "one", "two" },
    termValues: new[] { @"ActiveProjectCapability:VisualC", @"HierSingleSelectionName:.(h|hpp|hxx|cpp|c|cxx)$" }
)]
    public sealed class IncludeMinimizerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            await Output.InitializeAsync();
        }
    }
}